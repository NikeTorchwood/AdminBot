using AdminBot.Repository;
using AdminBot.Repository.RequestRepository;
using AdminBot.Repository.StoreRepository;
using AdminBot.Repository.UserRepository;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Threading.Tasks.Task;

namespace AdminBot.Services;

public class TelegramService
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepository;
    private UpdateHandlerService? _updateHandlerService;
    private readonly IRequestRepository _requestRepository;
    private readonly IStoreRepository _storeRepository;

    public TelegramService(ITelegramBotClient bot, SqlConnectionProvider sqlConnectionProvider)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _userRepository = new UserRepositoryMsSql(sqlConnectionProvider) ??
                          throw new ArgumentNullException(nameof(sqlConnectionProvider));
        _requestRepository = new RequestRepositoryMSSql(sqlConnectionProvider) ?? throw new ArgumentNullException(nameof(sqlConnectionProvider));
        _storeRepository = new StoreRepositorySql(sqlConnectionProvider);
    }

    public void StartListening()
    {
        _updateHandlerService = new UpdateHandlerService(_bot, _userRepository, _requestRepository, _storeRepository);
        _bot.StartReceiving(UpdateHandler, ErrorHandler);
    }


    private async Task ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        Run(async () =>
        {
            await _updateHandlerService.ProcessMessage(update);
        }, ct);
        return CompletedTask;
    }
}