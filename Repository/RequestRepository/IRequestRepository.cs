using AdminBot.Entities.Users;

namespace AdminBot.Repository.RequestRepository;

public interface IRequestRepository
{
    //to do: сделать по патерну строитель
    Task CreateChangeRoleRequest(int messageMessageId, RequestTypes requestType, long responsibleId, long applicantId, Roles newRole);
    Task<ChangeRoleRequest> GetChangeRoleRequest(long id, long responsibleId);
}