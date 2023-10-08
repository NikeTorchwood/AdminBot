using AdminBot.Entities.Users;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.Repository.UserRepository;

public interface IUserRepository
{
    public Task SaveUser(Update update, UserBot user);

    public Task<UserBot> GetUserById(long id, ITelegramBotClient bot);
    public Task<List<long>> GetUsersByRole(Roles role);
    public Task<bool> ChangeRole(long userId, Roles newRole);
}