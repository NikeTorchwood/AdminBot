using AdminBot.Entities.Users;
using AdminBot.MenuStates;
using AdminBot.Repository;
using AdminBot.Repository.RequestRepository;
using AdminBot.Repository.UserRepository;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Threading.Tasks.Task;

namespace AdminBot.Services;

public class TelegramService
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepository;
    private TelegramBotMenuContext? _context;
    private readonly IRequestRepository _requestRepository;
    private RequestHandler _requestHandler;

    public TelegramService(ITelegramBotClient bot, SqlConnectionProvider sqlConnectionProvider)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _userRepository = new UserRepositoryMsSQL(sqlConnectionProvider) ??
                          throw new ArgumentNullException(nameof(sqlConnectionProvider));
        _requestRepository = new RequestRepositoryMSSql(sqlConnectionProvider) ?? throw new ArgumentNullException(nameof(sqlConnectionProvider));
    }

    public void StartListening()
    {
        _context = new TelegramBotMenuContext(_bot, _userRepository, _requestRepository);
        _requestHandler = new RequestHandler(_bot, _userRepository, _requestRepository);
        _bot.StartReceiving(UpdateHandler, ErrorHandler);
    }


    private async Task ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        switch (update.Type)
        {
            case UpdateType.CallbackQuery:
                Console.WriteLine($"{DateTime.Now}: {update.CallbackQuery.Data}");
                Console.WriteLine(update.CallbackQuery.Message.Text);
                Console.WriteLine(new string('_', 50));
                Console.WriteLine(update.CallbackQuery.From.Id);
                if (update.CallbackQuery != null)
                {
                    Run(async () =>
                        {
                            await _requestHandler.ProcessMessage(update);
                        }, ct);
                }
                break;
            case UpdateType.Message:
                Console.WriteLine(update.Message.From.Username);
                Console.WriteLine($"{DateTime.Now}: {update.Message.Chat.Id}");
                Console.WriteLine(new string('_', 50));
                var message = update.Message;
                if (message != null)
                {
                    Run(async () =>
                    {
                        await _context.ProcessMessage(update);
                    }, ct);
                };
                break;
        }

        return CompletedTask;
    }
}