using AdminBot.Entities;

namespace AdminBot.Repository.StoreRepository;

public interface IStoreRepository
{
    //Task<List<Store>> GetStoreList();
    Task DeleteStore(string codeStore);
    Task AddStore(string codeStore);
    Task<List<string>> GetStoreNameList();
    Task AddStoresFromReport(List<Store> storeList);
    Task<Store> GetStore(string? userStoreCode);
}