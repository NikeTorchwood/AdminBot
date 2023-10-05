using AdminBot.Entities.Users;
using AdminBot.MenuStates;
using Telegram.Bot;

namespace AdminBot.Repository.UserRepository;

public interface IUserRepository
{
    public Task SaveUser(UserBot user);

    public UserBot GetUserById(long id, ITelegramBotClient bot);
}