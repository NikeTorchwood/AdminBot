using AdminBot.Entities;

namespace AdminBot.Repository.RequestRepository;

public interface IRequestRepository
{
    //to do: сделать по патерну строитель
    Task<ChangeRoleRequest> GetChangeRoleRequest(long responsibleId, long messageId);
    Task CreateChangeRoleRequest(long applicantId, long responsibleId, int messageId, Roles newRole, string? applicantStoreCode = default);
}