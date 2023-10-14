using System.Text;
using AdminBot.Entities;
using AdminBot.Repository.StoreRepository;
using AdminBot.Services.ServiceInterfaces;

namespace AdminBot.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;
    public StoreService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    }

    public async Task<List<string>> GetStoreNameList()
    {
        return await _storeRepository.GetStoreNameList();
    }
    public async Task AddStore(string codeStore)
    {
        await _storeRepository.AddStore(codeStore);
    }

    public async Task DeleteStore(string codeStore)
    {
        await _storeRepository.DeleteStore(codeStore);
    }
    public async Task UpdateStores(List<Store> storeList)
    {
        await _storeRepository.AddStoresFromReport(storeList);
    }

    public async Task<string> GetStoreData(string? userStoreCode)
    {
        var store = await _storeRepository.GetStore(userStoreCode);
        var sb = new StringBuilder();
        sb.AppendLine(new string('_', 30));
        sb.AppendLine($"Код ОП: {store.CodeStore}");
        foreach (var economicDirection in store.EconomicDirections)
        {
            var percentage = (double)economicDirection.Fact / (double) economicDirection.Plan;
            var balance = economicDirection.Plan - economicDirection.Fact;
            sb.AppendLine(new string('_', 20));
            sb.AppendLine($"{economicDirection.DirectionName}");
            sb.AppendLine($"План: {economicDirection.Plan}");
            sb.AppendLine($"Факт: {economicDirection.Fact}");
            sb.AppendLine($"Процент выполнения: {percentage, 0:P1}");
            sb.AppendLine($"Остаток: {balance}");
        }
        return sb.ToString();
    }
}