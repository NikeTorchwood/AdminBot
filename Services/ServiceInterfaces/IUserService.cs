using AdminBot.Entities;
using AdminBot.MenuStates;
using Telegram.Bot.Types;

namespace AdminBot.Services.ServiceInterfaces;

public interface IUserService
{
    Task<Roles> GetUserRole(long userId);
    Task ResetRole(long userId);
    Task SaveUser(UserBot user, IStateMenu newState);
    Task<UserBot> GetUser(User? user);
}