using AdminBot.Entities;
using AdminBot.MenuStates;
using AdminBot.Repository.RequestRepository;
using AdminBot.Repository.StoreRepository;
using AdminBot.Repository.UserRepository;
using AdminBot.Services.ServiceInterfaces;
using Aspose.Cells;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AdminBot.Services;

public class UpdateHandlerService
{
    private readonly ITelegramBotClient _bot;
    private readonly IRequestService _changeRoleService;
    private readonly IMessageHandlerService _messageHandlerService;
    private readonly ICallbackQueryHandler _callbackQueryHandler;
    private readonly IStoreService _storeService;
    private readonly IUserService _userService;

    public UpdateHandlerService(
        ITelegramBotClient bot,
        IUserRepository userRepository,
        IRequestRepository requestRepository,
        IStoreRepository storeRepository)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _userService = new UserService(bot, userRepository);
        _changeRoleService = new ChangeRoleRequestService(userRepository, requestRepository, bot);
        _storeService = new StoreService(storeRepository);
        _callbackQueryHandler = new CallbackQueryProcessService(_changeRoleService, _storeService, _bot, _userService);
        _messageHandlerService = new MessageProcessService(_userService);
    }


    public async Task SetState(UserBot user, IStateMenu newState)
    {
        await _messageHandlerService.SetState(user, newState, this);
    }

    public async Task ProcessMessage(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.CallbackQuery:
                await _callbackQueryHandler.ProcessMessage(update, _bot, this);
                break;
            case UpdateType.Message:
                await _messageHandlerService.ProcessMessage(update, _bot, this);
                break;
        }
    }

    public async Task CreateChangeRoleRequest(Roles applicantRole, User user, string? applicantStoreCode = default)
    {
        switch (applicantRole)
        {
            case Roles.SectorDirector:
                await _changeRoleService.CreateDirectorSectorRequest(user);
                break;
            case Roles.Employer:
                await _changeRoleService.CreateEmployerRequest(Roles.Employer, user, applicantStoreCode);
                break;
            case Roles.StoreDirector:
                await _changeRoleService.CreateEmployerRequest(Roles.StoreDirector, user, applicantStoreCode);
                break;
        }
    }

    public async Task<List<string>> GetStoreNameList()
    {
        return await _storeService.GetStoreNameList();
    }

    public async Task<Roles> GetUserRole(long userId)
    {
        return await _userService.GetUserRole(userId);
    }

    public async Task<ReportService> CreateReport(Workbook workbook)
    {
        return new ReportService(_storeService, workbook);
    }

    public async Task PrintStoreReport(UserBot user)
    {
        var reportData = await _storeService.GetStoreData(user.StoreCode);
        await _bot.SendTextMessageAsync(user.Id, reportData);
    }

    public async Task<List<long>> GetAllUsers()
    {
        return await _userService.GetAllUserIdsBot();
    }
}