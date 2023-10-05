using AdminBot.Entities.Users;
using AdminBot.Repository.UserRepository;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.MenuStates;

public class TelegramBotMenuContext
{
    private readonly ITelegramBotClient _bot;
    private IStateMenu _currentState;
    private readonly IUserRepository _userRepository;
    public TelegramBotMenuContext(ITelegramBotClient bot, IUserRepository userRepository)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    public async Task SetState(Update update, UserBot user)
    {
        await _userRepository.SaveUser(user);
        _currentState = user.StateMenu;
        await _currentState.SendStateMessage(update, user);
    }
    public async Task ProcessMessage(Update update)
    {
        var userId = update.Message.From.Id;
        var user = _userRepository.GetUserById(userId, _bot);
        _currentState = user.StateMenu;
        await _currentState.ProcessMessage(update, user, this);
    }
}