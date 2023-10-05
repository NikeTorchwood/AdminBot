﻿using System.Data.SqlClient;
using AdminBot.Entities.Users;
using AdminBot.MenuStates;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.Repository.UserRepository;

public class UserRepositoryMsSQL : IUserRepository
{
    private readonly IDbConnectionProvider _connectionProvider;

    public UserRepositoryMsSQL(IDbConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task SaveUser(UserBot user)
    {
        var resultState = (int)StateMenuConverter.ConvertToStatesMenu(user.StateMenu);
        Console.WriteLine($@"
            Сохранено в БД:
                UserId = {user.Id},
                UserState = {(StatesMenu)resultState},
                UserRole = {user.Role}");
        Console.WriteLine(new string('_', 50));
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = $@"
            MERGE INTO UserRepository AS Target
            USING (SELECT @UserId AS [UserId], @State AS [State], @UserRole AS [Role]) AS Source
            ON (Target.[UserId] = Source.[UserId])
            WHEN MATCHED THEN
                UPDATE SET [State] = Source.[State],
                [Role] = Source.[Role]
            WHEN NOT MATCHED THEN
                INSERT ([UserId], [State], [Role])
                VALUES (Source.[UserId], Source.[State], Source.[Role]);
        ";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", user.Id);
        command.Parameters.AddWithValue("@State", resultState);
        command.Parameters.AddWithValue("@UserRole", user.Role);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }
    public UserBot GetUserById(long userId, ITelegramBotClient bot)
    {
        using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "SELECT State, Role FROM UserRepository WHERE UserId = @UserId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        connection.Open();
        var stateResult = 0;
        var roleResult = Roles.None;
        using (var reader = command.ExecuteReader())
        {

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    stateResult = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    roleResult = reader.IsDBNull(1) ? 0 : (Roles)reader.GetInt32(1);
                }
            }
        }
        Console.WriteLine($@"
            Получено из БД:
                UserId = {userId},
                UserState = {(StatesMenu)stateResult},
                UserRole = {(Roles)roleResult}");
        Console.WriteLine(new string('_', 50));
        var state = StateMenuConverter.ConvertToIStateMenu((StatesMenu)stateResult, bot);
        return new UserBot(userId, state, roleResult);
    }

}