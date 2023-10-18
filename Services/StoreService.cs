using AdminBot.Entities;
using AdminBot.Repository.StoreRepository;
using AdminBot.Services.ServiceInterfaces;
using System.Text;

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
        if (store == null)
        {
            sb.AppendLine($"Код ОП: {userStoreCode}");
            sb.AppendLine($"Проблема при загрузке, проверьте наличие магазина в загружаемом отчете");
            return sb.ToString();
        }
        sb.AppendLine(new string('_', 30));
        sb.AppendLine($"Код ОП: {store.CodeStore}");
        sb.AppendLine($"Количество ЛП: {store.CountLP}");
        foreach (var economicDirection in store.EconomicDirections)
        {
            var economicCalculation = new DirectionCalculation(economicDirection, store.CountLP);
            sb.AppendLine(new string('_', 20));
            sb.AppendLine($"{economicDirection.DirectionName}");
            sb.AppendLine($"План: {economicDirection.Plan}");
            sb.AppendLine($"Факт: {economicDirection.Fact}");
            sb.AppendLine($"Процент выполнения: {economicCalculation.DirectionProgress,0:P1}");
            sb.AppendLine($"Процент выполнения прогноз: {economicCalculation.DirectionProgressForecast,0:P1}");
            sb.AppendLine($"Остаток: {economicCalculation.DirectionRemainingTotal}");
            sb.AppendLine($"Дневной план по направлению: {economicCalculation.DailyPlan, 0:F2}");
        }
        sb.AppendLine(new string('_', 30));
        return sb.ToString();
    }
}