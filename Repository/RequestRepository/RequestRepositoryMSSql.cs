using System.Data.SqlClient;
using AdminBot.Entities;
using Telegram.Bot.Types;

namespace AdminBot.Repository.RequestRepository;

public class RequestRepositoryMSSql : IRequestRepository
{
    private readonly IDbConnectionProvider _connectionProvider;

    public RequestRepositoryMSSql(IDbConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
    }

    public async Task CreateChangeRoleRequest(long applicantId, long responsibleId, int messageId, Roles newRole,
        string? applicantStoreCode = default)
    {
        Console.WriteLine($@"
        Сохранено в БД Запрос:
        RequestId = {messageId},
        ResponsibleId = {responsibleId},
        ApplicantId = {applicantId},
        NewRole = {newRole},
        ApplicantStore = {applicantStoreCode}");
        Console.WriteLine(new string('_', 50));
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = $@"
            INSERT INTO [ChangeRoleRequestRepository] ([MessageId],[ResponsibleId],[ApplicantId],[NewRole], [ApplicantStore])
            VALUES (@MessageId,@ResponsibleId,@ApplicantId,@NewRole, @ApplicantStore);
        ";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@MessageId", messageId);
        command.Parameters.AddWithValue("@ResponsibleId", responsibleId);
        command.Parameters.AddWithValue("@ApplicantId", applicantId);
        command.Parameters.AddWithValue("@NewRole", newRole);
        command.Parameters.AddWithValue("@ApplicantStore", applicantStoreCode ?? string.Empty);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    

    public async Task<ChangeRoleRequest> GetChangeRoleRequest(long responsibleId, long confirmedMessageId)
    {
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "SELECT [ApplicantId], [NewRole], [ApplicantStore] FROM [ChangeRoleRequestRepository] WHERE [ResponsibleId] = @ResponsibleId and [MessageId] = @MessageId;";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ResponsibleId", responsibleId);
        command.Parameters.AddWithValue("@MessageId", confirmedMessageId);
        await connection.OpenAsync();
        long applicantId = 0;
        var newRole = Roles.None;
        var storeCode = string.Empty;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    applicantId = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                    newRole = reader.IsDBNull(1) ? 0 : (Roles)reader.GetInt32(1);
                    storeCode = reader.IsDBNull(2) ? default : reader.GetString(2);
                }
            }
        }
        Console.WriteLine($@"
            Получено Из БД Запрос:
                ResponsibleId = {responsibleId},
                ApplicantId = {applicantId},
                MessageId = {confirmedMessageId}");
        Console.WriteLine(new string('_', 50));
        await connection.CloseAsync();
        return new ChangeRoleRequest(applicantId, newRole, storeCode);
    }
}