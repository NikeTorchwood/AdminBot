using System.Data.SqlClient;
using AdminBot.Entities.Users;
using AdminBot.Repository;

namespace AdminBot.Repository.RequestRepository;

public class RequestRepositoryMSSql : IRequestRepository
{
    private readonly IDbConnectionProvider _connectionProvider;

    public RequestRepositoryMSSql(IDbConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
    }

    public async Task CreateChangeRoleRequest(int messageMessageId, RequestTypes requestType, long responsibleId, long applicantId, Roles newRole)
    {
        Console.WriteLine($@"
            Сохранено в БД Запрос:
                RequestId = {messageMessageId},
                RequestType = {requestType},
                ResponsibleId = {responsibleId},
                ApplicantId = {applicantId},
                NewRole = {newRole}");
        Console.WriteLine(new string('_', 50));
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = $@"
            INSERT INTO [ChangeRoleRequestRepository] ([MessageId],[RequestType],[ResponsibleId],[ApplicantId],[NewRole])
            VALUES (@MessageId,@RequestType,@ResponsibleId,@ApplicantId,@NewRole);
        ";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@MessageId", messageMessageId);
        command.Parameters.AddWithValue("@RequestType", (int)requestType);
        command.Parameters.AddWithValue("@ResponsibleId", responsibleId);
        command.Parameters.AddWithValue("@ApplicantId", applicantId);
        command.Parameters.AddWithValue("@NewRole", newRole);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    

    public async Task<ChangeRoleRequest> GetChangeRoleRequest(long responsibleId, long confirmedMessageId)
    {
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "SELECT [RequestType],[ApplicantId], [NewRole] FROM [ChangeRoleRequestRepository] WHERE [ResponsibleId] = @ResponsibleId and [MessageId] = @MessageId";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ResponsibleId", responsibleId);
        command.Parameters.AddWithValue("@MessageId", confirmedMessageId);
        connection.Open();
        var requestType = RequestTypes.None;
        long applicantId = 0;
        var newRole = Roles.None;
        await using (var reader = await command.ExecuteReaderAsync())
        {

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    requestType = reader.IsDBNull(0) ? 0 : (RequestTypes)reader.GetInt32(0);
                    applicantId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                    newRole = reader.IsDBNull(2) ? 0 : (Roles)reader.GetInt32(2);
                }
            }
        }
        Console.WriteLine($@"
            Получено Из БД Запрос:
                RequestType = {requestType},
                ResponsibleId = {responsibleId},
                ApplicantId = {applicantId},
                MessageId = {confirmedMessageId}");
        Console.WriteLine(new string('_', 50));
        await connection.CloseAsync();
        return new ChangeRoleRequest(applicantId, requestType, newRole);
    }
}

public class ChangeRoleRequest
{
    public  long ApplicantId { get; }
    public RequestTypes RequestType { get; }
    public Roles NewRole { get; }

    public ChangeRoleRequest(long applicantId, RequestTypes requestType, Roles newRole)
    {
        ApplicantId = applicantId;
        RequestType = requestType;
        NewRole = newRole;
    }
}