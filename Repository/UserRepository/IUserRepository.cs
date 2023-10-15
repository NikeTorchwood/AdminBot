using AdminBot.Entities;
using AdminBot.MenuStates;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.Repository.UserRepository;

public interface IUserRepository
{
    public Task SaveUser(UserBot user, IStateMenu newState);
    public Task<List<long>> GetUsersIdByRole(Roles role);
    Task ResetRole(long userId);
    Task<List<long>> GetStoreDirectorId(string? storeCode);
    Task ChangeRole(ChangeRoleRequest request);
    Task<UserBot> GetUser(User user, ITelegramBotClient bot);
    Task<UserBot> GetUserById(long userId, ITelegramBotClient bot);
}