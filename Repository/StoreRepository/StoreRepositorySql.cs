using AdminBot.Entities;
using System.Data.SqlClient;
using System.Diagnostics;

namespace AdminBot.Repository.StoreRepository;

public class StoreRepositorySql : IStoreRepository
{
    private readonly IDbConnectionProvider _connectionProvider;

    public StoreRepositorySql(IDbConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task DeleteStore(string codeStore)
    {
        var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "DELETE FROM [StoreTable] WHERE CodeStore = @CodeStore";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CodeStore", codeStore);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    public async Task AddStore(string codeStore)
    {
        var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = @"
            MERGE INTO [StoreTable] AS Target
                USING (SELECT @CodeStore AS [CodeStore]) AS Source
            ON (Target.[CodeStore] = Source.[CodeStore])
            WHEN MATCHED THEN
                UPDATE SET Target.[CodeStore] = Source.[CodeStore]
            WHEN NOT MATCHED THEN
                INSERT ([CodeStore]) VALUES (Source.[CodeStore]);";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CodeStore", codeStore.ToUpper());
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    public async Task<List<string>> GetStoreNameList()
    {
        var result = new List<string>();
        var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = "SELECT CodeStore FROM StoreTable";
        await using var command = new SqlCommand(query, connection);
        await connection.OpenAsync();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    var codeStore = reader.GetString(0);
                    result.Add(codeStore);
                }
            }
        }

        await connection.CloseAsync();
        return result;
    }

    public async Task AddStoresFromReport(List<Store> storeList)
    {
        Console.WriteLine("Загрузка данных в БД");
        var sw = new Stopwatch();
        sw.Start();
        var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = @"
UPDATE [StoreTable]
SET [CountLP] = @CountLP
WHERE [CodeStore] = @StoreCode;

MERGE INTO [EconomicDirections] AS Target
USING  
    (VALUES (@StoreCode, @EconomicDirectionName, @EconomicDirectionPlan, @EconomicDirectionFact))
    AS Source ([StoreCode], [EconomicDirectionName], [EconomicDirectionPlan], [EconomicDirectionFact])
ON  
    (Target.[StoreCode] = Source.[StoreCode] AND Target.[EconomicDirectionName] = Source.[EconomicDirectionName])
WHEN MATCHED THEN
    UPDATE SET
        Target.[EconomicDirectionPlan] = Source.[EconomicDirectionPlan],
        Target.[EconomicDirectionFact] = Source.[EconomicDirectionFact]
WHEN NOT MATCHED THEN
    INSERT
        ([StoreCode], [EconomicDirectionName], [EconomicDirectionPlan], [EconomicDirectionFact])
    VALUES
        (Source.[StoreCode], Source.[EconomicDirectionName], Source.[EconomicDirectionPlan], Source.[EconomicDirectionFact]);
";
        await using var command = new SqlCommand(query, connection);
        await connection.OpenAsync();
        foreach (var store in storeList)
        {
            foreach (var direction in store.EconomicDirections)
            {
                command.Parameters.Clear(); // Очистить параметры перед каждой итерацией
                command.Parameters.AddWithValue("@StoreCode", store.CodeStore);
                command.Parameters.AddWithValue("@CountLP", store.CountLP);
                command.Parameters.AddWithValue("@EconomicDirectionName", direction.DirectionName);
                command.Parameters.AddWithValue("@EconomicDirectionPlan", direction.Plan);
                command.Parameters.AddWithValue("@EconomicDirectionFact", direction.Fact);
                await command.ExecuteNonQueryAsync();
            }
        }

        sw.Stop();
        Console.WriteLine($"Потраченное время на обновление БД : {sw.Elapsed}");
    }


    public async Task<Store> GetStore(string? userStoreCode)
    {
        var countLP = await GetCountLP(userStoreCode);
        if (countLP == null)
        {
            return null;
        }

        var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = @"
Select 
    [EconomicDirectionName], [EconomicDirectionPlan], [EconomicDirectionFact]
from [EconomicDirections] 
    where [StoreCode] = @userStoreCode";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@userStoreCode", userStoreCode);
        var result = new List<EconomicDirection>();
        await connection.OpenAsync();
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    var directionName = reader.IsDBNull(0) ? null : reader.GetString(0);
                    var directionPlan = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    var directionFact = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    result.Add(new EconomicDirection(directionName, directionPlan, directionFact));
                }
            }
        }
        return new Store(userStoreCode, result, (int)countLP);
    }

    private async Task<int?> GetCountLP(string? userStoreCode)
    {
        await using var connection = (SqlConnection)_connectionProvider.GetConnection();
        const string query = @"
SELECT
    [CountLP]
FROM [StoreTable]
WHERE [CodeStore] = @userStoreCode";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@userStoreCode", userStoreCode);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != DBNull.Value ? (int?)result : null;
    }
}