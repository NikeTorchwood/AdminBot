using System.Data;
using System.Data.SqlClient;
using AdminBot.Entities;
using AdminBot.MenuStates;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace AdminBot.Repository.UserRepository;

public class UserRepositoryMsSql : IUserRepository
{
    private readonly IDbConnectionProvider _connectionProvider;

    public UserRepositoryMsSql(IDbConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task SaveUser(UserBot user, IStateMenu newState)
    {
        var resultState = (int)StateMenuConverter.ConvertToStatesMenu(newState);
        Console.WriteLine($@"
        Сохранено в БД:
        UserId = {user.Id},
        UserState = {(StatesMenu)resultState}");
        Console.WriteLine(new string('_', 50));
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = $@"
        MERGE INTO UserRepository AS Target
        USING (SELECT @UserId AS [UserId], @Role as [Role], @State AS [State], @Username as [Username], @StoreCode as [StoreCode]) AS Source
        ON (Target.[UserId] = Source.[UserId])
        WHEN MATCHED THEN
            UPDATE SET Target.[State] = Source.[State],
                Target.[Username] = Source.[Username],
                Target.[Role] = Source.[Role],
                Target.[StoreCode] = Source.[StoreCode]
        WHEN NOT MATCHED THEN
            INSERT ([UserId], [State], [Username], [Role], [StoreCode]) 
            VALUES (Source.[UserId], Source.[State], Source.[Username], Source.[Role], Source.[StoreCode]);
        ";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", user.Id);
        command.Parameters.AddWithValue("@State", resultState);
        command.Parameters.AddWithValue("@Role", user.UserRole);
        command.Parameters.AddWithValue("@Username", user.Username ?? string.Empty);
        command.Parameters.AddWithValue("@StoreCode", user.StoreCode ?? string.Empty);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }
    public async Task<UserBot> GetUser(User user, ITelegramBotClient bot)
    {
        await SetUsername(user);
        return await GetUserById(user.Id, bot);
    }

    public async Task<UserBot> GetUserById(long userId, ITelegramBotClient bot)
    {
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "SELECT [State], [Role], [StoreCode], [Username] FROM UserRepository WHERE UserId = @UserId";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        await connection.OpenAsync();
        var stateResult = 0;
        var roleResult = Roles.None;
        var storeCodeResult = string.Empty;
        var usernameResult = string.Empty;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    stateResult = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    roleResult = reader.IsDBNull(1) ? 0 : (Roles)reader.GetInt32(1);
                    storeCodeResult = reader.IsDBNull(2) ? default : reader.GetString(2);
                    usernameResult = reader.IsDBNull(3) ? default : reader.GetString(3);
                }
            }
        }
        await connection.CloseAsync();
        Console.WriteLine($@"
            Получено из БД:
                UserId = {userId},
                UserState = {(StatesMenu)stateResult},
                UserRole = {(Roles)roleResult},
                UserStoreCode = {storeCodeResult}");
        Console.WriteLine(new string('_', 50));
        var state = StateMenuConverter.ConvertToIStateMenu((StatesMenu)stateResult, bot);
        return new UserBot(userId, state, roleResult, storeCodeResult, usernameResult);
    }

    public async Task<List<long>> GetUsersByRole(Roles role)
    {
        var result = new List<long>();
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "SELECT UserId FROM UserRepository WHERE Role = @UsersRole";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsersRole", role);
        connection.Open();
        await using var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows) return result;
        while (reader.Read())
        {
            var userId = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            result.Add(userId);
            Console.WriteLine($@"
                        Получен из БД список для запроса:
                        UserRole = {role},
                        ChatId = {userId}");
            Console.WriteLine(new string('_', 50));
        }
        await connection.CloseAsync();
        return result;
    }

    public async Task ChangeRole(ChangeRoleRequest request)
    {
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "UPDATE UserRepository SET [Role] = @UserRole, [StoreCode] = @StoreCode WHERE UserId = @UserId;";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", request.ApplicantId);
        command.Parameters.AddWithValue("@UserRole", request.NewRole);
        command.Parameters.AddWithValue("@StoreCode", request.StoreCode ?? string.Empty);
        await connection.OpenAsync();
        var result = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    private async Task SetUsername(User user)
    {
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "UPDATE UserRepository SET [Username] = @Username WHERE UserId = @UserId;";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@UserId", user.Id);
        await connection.OpenAsync();
        var result = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    public async Task ResetRole(long userId)
    {
        var request = new ChangeRoleRequest(userId, Roles.None, null);
        await ChangeRole(request);
    }

    public async Task<List<long>> GetStoreDirectorId(string? storeCode)
    {
        var result = new List<long>();
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "SELECT UserId FROM UserRepository WHERE Role = @UsersRole and StoreCode = @StoreCode";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UsersRole", Roles.StoreDirector);
        command.Parameters.AddWithValue("@StoreCode", storeCode);
        connection.Open();
        await using var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows) return result;
        while (reader.Read())
        {
            var userId = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            result.Add(userId);
            Console.WriteLine($@"
                        Получен из БД список для запроса:
                        ChatId = {userId}");
            Console.WriteLine(new string('_', 50));
        }
        await connection.CloseAsync();
        return result;
    }
}