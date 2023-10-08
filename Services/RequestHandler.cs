using AdminBot.Entities.Users;
using AdminBot.Repository.RequestRepository;
using AdminBot.Repository.UserRepository;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.Services;

public class RequestHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepository;
    private readonly IRequestRepository _requestRepository;
    private const string Apply = "apply";
    private const string Decline = "decline";

    public RequestHandler(ITelegramBotClient bot, IUserRepository userRepository, IRequestRepository requestRepository)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
    }

    public async Task ProcessMessage(Update update)
    {
        switch (update.CallbackQuery.Data)
        {
            case Apply:
                await ApplyRequest(update);
                break;
            case Decline:
                await DeclineRequest(update);
                break;
        }
    }

    private async Task DeclineRequest(Update update)
    {
        await _bot.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
    }

    private async Task ApplyRequest(Update update)
    {
        var responsibleId = update.CallbackQuery.From.Id;
        var confirmedMessageId = update.CallbackQuery.Message.MessageId;
        var request = await _requestRepository.GetChangeRoleRequest(responsibleId, confirmedMessageId);
        switch (request.RequestType)
        {
            case RequestTypes.ChangeRole:
                await ChangeRole(request);
                await _bot.DeleteMessageAsync(responsibleId, confirmedMessageId);
                break;
            case RequestTypes.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task ChangeRole(ChangeRoleRequest changeRoleRequest)
    {
        var keyboard = new ReplyKeyboardMarkup("Главное меню");
        var isChanged = await _userRepository.ChangeRole(changeRoleRequest.ApplicantId, changeRoleRequest.NewRole);
        if (isChanged)
        {
            await _bot.SendTextMessageAsync(changeRoleRequest.ApplicantId, $"Роль была изменена на {changeRoleRequest.NewRole}", replyMarkup:keyboard);
            var user = await _userRepository.GetUserById(changeRoleRequest.ApplicantId, _bot);
            await user.StateMenu.SendStateMessage(user);
        }
    }
}