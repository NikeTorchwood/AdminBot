using System.Diagnostics;
using AdminBot.Entities;
using AdminBot.MenuStates.States;
using AdminBot.Repository.UserRepository;
using AdminBot.Services.ServiceInterfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.Services;

public class CallbackQueryProcessService : ICallbackQueryHandler
{
    private readonly IRequestService _requestHandlerService;
    private readonly IStoreService _storeService;
    private readonly ITelegramBotClient _bot;
    private readonly IUserService _userService;
    private const string Apply = "Apply";
    private const string Decline = "Decline";
    private const string Director = "Director";
    private const string Specialist = "Specialist";
    private string[] _callback;
    private RequestTypes _cbCathegory;
    private string _cbData;
    public CallbackQueryProcessService(IRequestService requestService, IStoreService storeService,
        ITelegramBotClient bot, IUserService userService)
    {
        _bot = bot;
        _userService = userService;
        _requestHandlerService = requestService;
        _storeService = storeService;
    }
    public async Task ProcessMessage(Update update, ITelegramBotClient bot, UpdateHandlerService updateHandlerService)
    {
        var callbackQuery = update.CallbackQuery;
        if (callbackQuery != null)
        {
            _callback = update.CallbackQuery.Data.Split('.');
            _cbCathegory = (RequestTypes)Enum.Parse(typeof(RequestTypes), _callback[0]);
            _cbData = _callback[1];
        }
        switch (_cbCathegory)
        {
            case RequestTypes.ChangeRole:
                await ProcessChangeRoleRequest(callbackQuery, updateHandlerService);
                break;
            case RequestTypes.AddStore:
                await ProcessAddStore(callbackQuery);
                break;
            case RequestTypes.DeleteStore:
                await ProcessDeleteStore(callbackQuery);
                break;
            case RequestTypes.ResetUserRole:
                await ProcessResetUserRole(callbackQuery, updateHandlerService);
                break;
            case RequestTypes.ChangeRoleEmployer:
                await ProcessChooseStoreRole(callbackQuery, updateHandlerService);
                break;
        }
    }

    private async Task ProcessChooseStoreRole(CallbackQuery callbackQuery, UpdateHandlerService updateHandlerService)
    {
        var userId = callbackQuery.From.Id;
        var messageOnDelete = callbackQuery.Message.MessageId;
        string storeCode;
        switch (_cbData)
        {
            case Director:
                storeCode = callbackQuery.Message.ReplyToMessage.Text;
                await _bot.DeleteMessageAsync(userId, messageOnDelete);
                await updateHandlerService.CreateChangeRoleRequest(Roles.StoreDirector, callbackQuery.From, storeCode);
                break;
            case Specialist:
                storeCode = callbackQuery.Message.ReplyToMessage.Text;
                await _bot.DeleteMessageAsync(userId, messageOnDelete);
                await updateHandlerService.CreateChangeRoleRequest(Roles.Employer, callbackQuery.From, storeCode);
                break;
        }
    }

    private async Task ProcessResetUserRole(CallbackQuery callbackQuery, UpdateHandlerService updateHandlerService)
    {
        switch (_cbData)
        {
            case Apply:
                var user = callbackQuery.From;
                await _userService.ResetRole(user.Id);
                var newUserBot = await _userService.GetUser(callbackQuery.From);
                await _bot.SendTextMessageAsync(newUserBot.Id, $"Ваша роль была сброшена.");
                await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                await updateHandlerService.SetState(newUserBot, new StartState(_bot));
                break;
            case Decline:
                await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                break;
        }
    }

    private async Task ProcessDeleteStore(CallbackQuery callbackQuery)
    {
        switch (_cbData)
        {
            case Apply:
                var codeStore = callbackQuery.Message.ReplyToMessage.Text;
                await _storeService.DeleteStore(codeStore);
                await _bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Был удален магазин {codeStore}");
                await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                break;
            case Decline:
                await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                break;
        }
    }

    private async Task ProcessAddStore(CallbackQuery callbackQuery)
    {
        switch (_cbData)
        {
            case Apply:
                var codeStore = callbackQuery.Message.ReplyToMessage.Text;
                await _storeService.AddStore(codeStore);
                await _bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Был добавлен магазин {codeStore}");
                await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                break;
            case Decline:
                await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                break;
        }
    }
    private async Task ProcessChangeRoleRequest(CallbackQuery callbackQuery, UpdateHandlerService updateHandlerService)
    {
        switch (_cbData)
        {
            case Apply:
                var newUserBot = await _requestHandlerService.ApplyRequest(callbackQuery);
                await _bot.SendTextMessageAsync(newUserBot.Id, $"Роль была изменена.");
                await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                await updateHandlerService.SetState(newUserBot, new StartState(_bot));
                break;
            case Decline:
                await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                break;
        }
    }
}