using AdminBot.Entities.Users;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class AuthorizationMenu : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string MainMenuButton = "Главное меню";
    private const string EmployerButton = "Специалист офиса";
    private const string StoreDirectorButton = "Начальник офиса";
    private const string SectorDirectorButton = "Директор сектора";
    private const string AdministratorButton = "Администратор";
    public AuthorizationMenu(ITelegramBotClient bot)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    }

    public async Task ProcessMessage(Update update, UserBot userBot, TelegramBotMenuContext context)
    {
        switch (update.Message.Type)
        {
            case MessageType.Text:
                switch (update.Message.Text)
                {
                    case MainMenuButton:
                        await context.SetState(update,
                            new UserBot(userBot.Id,
                            new StartState(_bot),
                            userBot.Role));
                        break;
                    case EmployerButton:
                        await context.SetState(update, 
                            new UserBot(userBot.Id,
                            new StartState(_bot),
                            Roles.Employer));
                        break;
                    case StoreDirectorButton:
                        await context.SetState(update, 
                            new UserBot(userBot.Id,
                            new StartState(_bot),
                            Roles.StoreDirector));
                        break;
                    case SectorDirectorButton:
                        await context.SetState(update, 
                            new UserBot(userBot.Id,
                            new StartState(_bot),
                            Roles.SectorDirector));
                        break;
                    case AdministratorButton:
                        await context.SetState(update, 
                            new UserBot(userBot.Id,
                            new StartState(_bot),
                            Roles.Administrator));
                        break;
                }
                break;
            case MessageType.Document:
                await _bot.SendTextMessageAsync(update.Message.Chat.Id,
                    "Возможно ты отправил отчет? Посмотри внимательно на кнопки или нажми инструкцию.");
                break;
        }
    }

    public async Task SendStateMessage(Update update, UserBot userBot)
    {
        await _bot.SendTextMessageAsync(update.Message.Chat.Id, GetStateTitle(), replyMarkup: GetKeyboard());
    }
    private static string GetStateTitle()
    {
        return "Меню выбора роли. Выбери свою роль по кнопочке.";
    }

    private static IReplyMarkup GetKeyboard()
    {
        List<List<KeyboardButton>>? keyList;
        keyList = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(EmployerButton),
                        new KeyboardButton(StoreDirectorButton)
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(SectorDirectorButton),
                        new KeyboardButton(AdministratorButton)
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(MainMenuButton),
                    }
                };
        return new ReplyKeyboardMarkup(keyList);

    }
}