using AdminBot.Entities;

namespace AdminBot.Services.ServiceInterfaces;

public interface IStoreService
{
    public Task<List<string>> GetStoreNameList();
    Task AddStore(string codeStore);
    Task DeleteStore(string codeStore);
    Task UpdateStores(List<Store> storeList);
    Task<string> GetStoreData(string? userStoreCode);
}