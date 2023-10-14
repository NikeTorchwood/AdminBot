using AdminBot.Entities;
using AdminBot.Repository.RequestRepository;
using AdminBot.Repository.UserRepository;
using AdminBot.Services.ServiceInterfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.Services;

public class ChangeRoleRequestService : IRequestService
{
    private readonly IUserRepository _userRepository;
    private readonly IRequestRepository _requestRepository;
    private readonly ITelegramBotClient _bot;

    public ChangeRoleRequestService(IUserRepository userRepository, IRequestRepository requestRepository, ITelegramBotClient bot)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    }

    public async Task<UserBot> ApplyRequest(CallbackQuery callbackQuery)
    {
        var responsibleId = callbackQuery.From.Id;
        var confirmedMessageId = callbackQuery.Message.MessageId;
        var request = await _requestRepository.GetChangeRoleRequest(responsibleId, confirmedMessageId);
        await _userRepository.ChangeRole(request);
        return await _userRepository.GetUserById(request.ApplicantId, _bot);
    }

    public async Task CreateDirectorSectorRequest(User user)
    {
        var senderUserId = user.Id;
        var senderUserName = user.Username;
        var responsibleIds = await _userRepository.GetUsersByRole(Roles.Administrator);
        foreach (var responsibleId in responsibleIds)
        {
            var message = await _bot.SendTextMessageAsync(responsibleId, $"Привет, вам нужно принять решение: @{senderUserName} хочет получить права Директора сектора.",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Разрешаю", $"{(int)RequestTypes.ChangeRole}.Apply"),
                            InlineKeyboardButton.WithCallbackData("Не разрешаю", $"{(int)RequestTypes.ChangeRole}.Decline"),
                        }));
            var messageId = message.MessageId;
            await _requestRepository.CreateChangeRoleRequest(senderUserId, responsibleId, messageId, Roles.SectorDirector);
        }
    }

    public async Task CreateEmployerRequest(Roles applicantRole, User user, string? applicantStoreCode)
    {
        var responsibleRole = Roles.SectorDirector;
        var senderUserId = user.Id;
        var senderUserName = user.Username;
        List<long> responsibleIds = new();
        if (applicantRole == Roles.Employer)
        {
            responsibleIds = await _userRepository.GetStoreDirectorId(applicantStoreCode);
        }
        while (responsibleIds.Count == 0)
        {
            responsibleIds = await _userRepository.GetUsersByRole(responsibleRole++);
        }
        foreach (var responsibleId in responsibleIds)
        {
            string role = null;
            switch (applicantRole)
            {
                case Roles.Employer:
                    role = "Специалиста офиса";
                    break;
                case Roles.StoreDirector:
                    role = "Начальника офиса продаж";
                    break;
            }
            var message = await _bot.SendTextMessageAsync(responsibleId, $"Привет, вам нужно принять решение: @{senderUserName} хочет получить права {role} - {applicantStoreCode}.",
               replyMarkup: new InlineKeyboardMarkup(new[]
               {
                    InlineKeyboardButton.WithCallbackData("Разрешаю", $"{(int)RequestTypes.ChangeRole}.Apply"),
                    InlineKeyboardButton.WithCallbackData("Не разрешаю", $"{(int)RequestTypes.ChangeRole}.Decline"),
               }));
            var messageId = message.MessageId;
            await _requestRepository.CreateChangeRoleRequest(senderUserId, responsibleId, messageId, applicantRole, applicantStoreCode);
        }
    }

}