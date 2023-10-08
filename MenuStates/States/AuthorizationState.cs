using AdminBot.Entities.Users;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class AuthorizationState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string MainMenuButton = "Главное меню";
    private const string ResetRoleButton = "Сбросить роль";
    private const string EmployerButton = "Специалист офиса";
    private const string StoreDirectorButton = "Начальник офиса";
    private const string SectorDirectorButton = "Директор сектора";
    private const string AdministratorButton = "Администратор";

    private const string RequestCreated =
        "Запрос был отправлен отвественному лицу, ожидайте. Если в течении 48 часов доступ не появился - попробуйте снова";
    public AuthorizationState(ITelegramBotClient bot)
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
                    case EmployerButton:
                        if (userBot.Role >= Roles.Employer)
                        {
                            await _bot.SendTextMessageAsync(userBot.Id,
                                "На данный момент ваша роль Специалист офиса или выше");
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                    new StartState(_bot),
                                    userBot.Role));
                        }
                        else
                        {
                            await context.CreateChangeRoleRequest(Roles.StoreDirector, update, Roles.Employer);
                            await _bot.SendTextMessageAsync(userBot.Id, RequestCreated);
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                    new StartState(_bot),
                                    userBot.Role));
                        }
                        break;
                    case ResetRoleButton:
                        await _bot.SendTextMessageAsync(userBot.Id, "Ваша роль была сброшена");
                        await context.SetState(update,
                            new UserBot(userBot.Id,
                                new StartState(_bot),
                                Roles.None));
                        break;
                    case MainMenuButton:
                        await context.SetState(update,
                            new UserBot(userBot.Id,
                            new StartState(_bot),
                            userBot.Role));
                        break;
                    case StoreDirectorButton:
                        if (userBot.Role >= Roles.StoreDirector)
                        {
                            await _bot.SendTextMessageAsync(userBot.Id,
                                "На данный момент ваша роль Директор Магазина или выше");
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                    new StartState(_bot),
                                    userBot.Role));
                        }
                        else
                        {
                            await context.CreateChangeRoleRequest(Roles.SectorDirector, update, Roles.StoreDirector);
                            await _bot.SendTextMessageAsync(userBot.Id, RequestCreated);
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                    new StartState(_bot),
                                    userBot.Role));
                        }
                        break;
                    case SectorDirectorButton:
                        if (userBot.Role >= Roles.SectorDirector)
                        {
                            await _bot.SendTextMessageAsync(userBot.Id,
                                "На данный момент ваша роль Директор Сектора или выше");
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                    new StartState(_bot),
                                    userBot.Role));
                        }
                        else
                        {
                            await context.CreateChangeRoleRequest(Roles.Administrator, update, Roles.SectorDirector);
                            await _bot.SendTextMessageAsync(userBot.Id, RequestCreated);
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                new StartState(_bot),
                                userBot.Role));
                        }
                        break;
                    case AdministratorButton:
                        if (userBot.Id == 6410857523)
                        {
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                new StartState(_bot),
                                Roles.Administrator));
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(update.Message.Chat.Id,
                                "К сожалению вы не можете стать Администратором, пройдите авторизацию повторно");
                            await context.SetState(update,
                                new UserBot(userBot.Id, new StartState(_bot), userBot.Role));
                        }
                        break;
                }
                break;
            case MessageType.Document:
                await _bot.SendTextMessageAsync(update.Message.Chat.Id,
                    "Возможно ты отправил отчет? Посмотри внимательно на кнопки или нажми инструкцию.");
                break;
        }
    }

    public async Task SendStateMessage(UserBot userBot)
    {
        await _bot.SendTextMessageAsync(userBot.Id, GetStateTitle(), replyMarkup: GetKeyboard());
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
                        new KeyboardButton(ResetRoleButton),
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(MainMenuButton),
                    }
                };
        return new ReplyKeyboardMarkup(keyList);

    }
}