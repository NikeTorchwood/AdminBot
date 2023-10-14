using AdminBot.Entities;
using AdminBot.MenuStates;
using AdminBot.Repository.UserRepository;
using AdminBot.Services.ServiceInterfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.Services;

public class MessageProcessService : IMessageHandlerService
{
    private readonly IUserService _userService;
    private IStateMenu? _currentState;

    public MessageProcessService(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task ProcessMessage(Update update, ITelegramBotClient bot, UpdateHandlerService service)
    {
        var user = await _userService.GetUser(update.Message.From);
        _currentState = user.StateMenu;
        await _currentState?.ProcessMessage(update, user, service)!;
    }
    public async Task SetState(UserBot user, IStateMenu newState, UpdateHandlerService updateHandlerService)
    {
        await _userService.SaveUser(user, newState);
        _currentState = newState;
        await _currentState?.SendStateMessage(user, updateHandlerService)!;
    }
}