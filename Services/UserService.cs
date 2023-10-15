using AdminBot.Entities;
using AdminBot.MenuStates;
using AdminBot.Repository.UserRepository;
using AdminBot.Services.ServiceInterfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.Services;

public class UserService : IUserService
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepository;

    public UserService(ITelegramBotClient bot, IUserRepository userRepository)
    {
        _bot = bot;
        _userRepository = userRepository;
    }

    public async Task<Roles> GetUserRole(long userId)
    {
        var user = await _userRepository.GetUserById(userId, _bot);
        return user.UserRole;
    }

    public async Task ResetRole(long userId)
    {
        await _userRepository.ResetRole(userId);
    }
    public async Task SaveUser(UserBot user, IStateMenu newState)
    {
        await _userRepository.SaveUser(user, newState);
    }
    public async Task<UserBot> GetUser(User? user)
    {
        return await _userRepository.GetUser(user, _bot);
    }

    public async Task<List<long>> GetAllUserIdsBot()
    {
        var usersIds = new List<long>();
        for (var i = Roles.Employer; i <= Roles.Administrator; i++)
        {
            usersIds.AddRange(await _userRepository.GetUsersIdByRole(i));
        }
        return usersIds;
    }
}