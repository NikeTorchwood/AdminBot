using AdminBot.Entities.Users;
using AdminBot.MenuStates;
using AdminBot.Repository;
using AdminBot.Repository.UserRepository;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Threading.Tasks.Task;

namespace AdminBot.Services;

public class TelegramService
{
    private readonly ITelegramBotClient _bot;
    private readonly IUserRepository _userRepository;
    private TelegramBotMenuContext? _context;
    public TelegramService(ITelegramBotClient bot, SqlConnectionProvider sqlConnectionProvider)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _userRepository = new UserRepositoryMsSQL(sqlConnectionProvider) ??
                          throw new ArgumentNullException(nameof(sqlConnectionProvider));
    }

    public void StartListening()
    {
        _context = new TelegramBotMenuContext(_bot, _userRepository);
        _bot.StartReceiving(UpdateHandler, ErrorHandler);
    }


    private async Task ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        Console.WriteLine($"{DateTime.Now}: {update.Message.Chat.Id}");
        Console.WriteLine(new string('_', 50));
        var message = update.Message;
        if (message != null)
        {
            Run(async () =>
            {
                await _context.ProcessMessage(update);
            }, ct);
        }

        return CompletedTask;
    }
}