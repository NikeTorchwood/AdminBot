using AdminBot.Entities.Users;
using AdminBot.Repository.RequestRepository;
using AdminBot.Repository.UserRepository;
using AdminBot.Services;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates;

public class TelegramBotMenuContext
{
    private readonly ITelegramBotClient _bot;
    private IStateMenu _currentState;
    private readonly IUserRepository _userRepository;
    private readonly IRequestRepository _requestRepository;

    public TelegramBotMenuContext(ITelegramBotClient bot,
        IUserRepository userRepository,
        IRequestRepository requestRepository)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
    }
    public async Task SetState(Update update, UserBot user)
    {
        await _userRepository.SaveUser(update, user);
        _currentState = user.StateMenu;
        await _currentState.SendStateMessage(user);
    }
    public async Task ProcessMessage(Update update)
    {
        var userId = update.Message.From.Id;
        var user = await _userRepository.GetUserById(userId, _bot);
        _currentState = user.StateMenu;
        await _currentState.ProcessMessage(update, user, this);
    }

    public async Task CreateChangeRoleRequest(Roles responsibleRole, Update update, Roles applicantRole)
    {
        var employers = new List<long>();
        var senderUserId = update.Message.From.Id;
        var senderUserName = update.Message.From.Username;
        while (employers.Count == 0)
        {
            employers = await _userRepository.GetUsersByRole(responsibleRole);
            responsibleRole++;
        }
        foreach (var employer in employers)
        {
            var message = await _bot.SendTextMessageAsync(employer, $"Привет, тебе нужно решить: @{senderUserName} хочет получить {applicantRole}",
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Разрешаю", $"apply"),
                    InlineKeyboardButton.WithCallbackData("Не разрешаю", $"decline"),
                }));
            await _requestRepository.CreateChangeRoleRequest(message.MessageId, RequestTypes.ChangeRole, employer, senderUserId, applicantRole);
        }
    }
}