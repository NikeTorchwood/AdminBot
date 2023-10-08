using AdminBot.Entities.Users;
using AdminBot.MenuStates;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class StartState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string DirectionManagerButton = "Перейти в менеджер КПИ";
    private const string StoreManagerButton = "Перейти в менеджер Магазинов";
    private const string PrintReportButton = "Печатать отчет";
    private const string ChooseStoreButton = "Выбрать магазин";
    private const string DownloadManagerButton = "Загрузить отчет";
    private const string InstructionButton = "Печать инструкции";
    private const string AuthorizationButton = "Авторизация";
    public StartState(ITelegramBotClient bot)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    }

    public async Task ProcessMessage(Update update,
        UserBot userBot,
        TelegramBotMenuContext context)
    {
        switch (update.Message.Type)
        {
            case MessageType.Text:
                switch (update.Message.Text)
                {
                    case DownloadManagerButton:
                        if (userBot.Role >= Roles.SectorDirector)
                        {
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                new DownloadFileState(_bot),
                                userBot.Role));
                        }
                        await _bot.SendTextMessageAsync(update.Message.Chat.Id,
                            "Для загрузки отчета нужна роль Администратор\\Директор сектора");
                        break;
                    case InstructionButton:
                        await PrintInstructions(update, userBot);
                        break;
                    case AuthorizationButton:
                        await context.SetState(update,
                            new UserBot(userBot.Id,
                            new AuthorizationState(_bot),
                            userBot.Role));
                        break;
                    case StoreManagerButton:
                        if (userBot.Role >= Roles.SectorDirector)
                        {
                            await context.SetState(update,
                                new UserBot(userBot.Id,
                                    new StoreManagerState(_bot),
                                    userBot.Role));
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(update.Message.Chat.Id,
                                "Для управлением списка магазинов нужна роль Администратор\\Директор сектора");
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

    private async Task PrintInstructions(Update update, UserBot userBot)
    {
        await _bot.SendTextMessageAsync(update.Message.Chat.Id, GetInstruction(), replyMarkup: GetKeyboard(userBot));
    }

    private static string GetInstruction()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Инструкция:");
        sb.AppendLine("Привет, здесь инструкция по использованию");
        sb.AppendLine(
            "Бот может печатать отчет по магазину, основывается он только на тех данных, которые мы туда загрузили.");
        sb.AppendLine("Если у тебя он печатает отчет не по твоему магазину - выбери заново свой магазин и повтори.");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Немного о кнопочках");
        sb.AppendLine("=> Печать отчета: Печатает отчет по выбранному магазину;");
        sb.AppendLine(
            "=> Выбрать магазин: После загрузки файла берет актуальный список магазинов и выбирает магазин, который можешь выбрать;");
        sb.AppendLine(
            "=> Загрузить отчет: Меню куда мы скидываем детализацию. !Важно! Если ты отправишь что-то кроме детализации продаж - то скорей всего бот сломается");
        return sb.ToString();
    }

    public async Task SendStateMessage(UserBot userBot)
    {
        await _bot.SendTextMessageAsync(userBot.Id, GetStateTitle(), replyMarkup: GetKeyboard(userBot));
    }
    private static string GetStateTitle()
    {
        return "Главное меню бота";
    }

    public static IReplyMarkup GetKeyboard(UserBot userBot)
    {
        List<List<KeyboardButton>>? keyList;
        switch (userBot.Role)
        {
            case Roles.None:
                keyList = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(InstructionButton),
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(AuthorizationButton),
                    }
                };
                return new ReplyKeyboardMarkup(keyList);
            case Roles.Employer:
                keyList = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(InstructionButton),
                        new KeyboardButton(AuthorizationButton),
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(ChooseStoreButton)
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(PrintReportButton)
                    }
                };
                return new ReplyKeyboardMarkup(keyList);
            case Roles.StoreDirector:
                keyList = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(InstructionButton),
                        new KeyboardButton(AuthorizationButton),
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(ChooseStoreButton),
                        new KeyboardButton(DownloadManagerButton)
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(PrintReportButton)
                    }
                };
                return new ReplyKeyboardMarkup(keyList);
            case Roles.SectorDirector:
                keyList = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(InstructionButton),
                        new KeyboardButton(AuthorizationButton),
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(ChooseStoreButton),
                        new KeyboardButton(PrintReportButton),
                        new KeyboardButton(DownloadManagerButton)
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(StoreManagerButton),
                        new KeyboardButton(DirectionManagerButton)
                    }
                };
                return new ReplyKeyboardMarkup(keyList);
            case Roles.Administrator:
                keyList = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(InstructionButton),
                        new KeyboardButton(AuthorizationButton),
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(ChooseStoreButton),
                        new KeyboardButton(PrintReportButton),
                        new KeyboardButton(DownloadManagerButton)
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(StoreManagerButton),
                        new KeyboardButton(DirectionManagerButton)
                    }
                };
                return new ReplyKeyboardMarkup(keyList);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}