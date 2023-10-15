using AdminBot.Entities;
using AdminBot.Services;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class StartState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
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
        UpdateHandlerService updateHandlerService)
    {
        switch (update.Message.Type)
        {
            case MessageType.Text:
                switch (update.Message.Text)
                {
                    case DownloadManagerButton:
                        if (userBot.UserRole >= Roles.SectorDirector)
                        {
                            await updateHandlerService.SetState(userBot, new DownloadFileState(_bot));
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(userBot.Id,
                                "Для загрузки отчета нужна роль Администратор\\Директор сектора");
                        }
                        break;
                    case InstructionButton:
                        await PrintInstructions(userBot);
                        break;
                    case AuthorizationButton:
                        await updateHandlerService.SetState(userBot, new AuthorizationState(_bot));
                        break;
                    case StoreManagerButton:
                        if (userBot.UserRole >= Roles.SectorDirector)
                        {
                            await updateHandlerService.SetState(userBot, new StoreManagerState(_bot));
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(userBot.Id,
                                "Для управлением списка магазинов нужна роль Администратор\\Директор сектора");
                        }
                        break;
                    case ChooseStoreButton: 
                        await updateHandlerService.SetState(userBot, new ChooseStoreState(_bot));
                        break;
                    case PrintReportButton:
                        if (string.IsNullOrWhiteSpace(userBot.StoreCode) || userBot.UserRole == Roles.None)
                        {
                            await _bot.SendTextMessageAsync(userBot.Id,
                                "У вас не выбран магазин, пожалуйста выберите магазин и попробуйте снова");
                        }
                        else
                        {
                            await updateHandlerService.PrintStoreReport(userBot);
                        }
                        break;
                    default:
                        await updateHandlerService.SetState(userBot, new StartState(_bot));
                        break;
                }
                break;
            case MessageType.Document:
                await _bot.SendTextMessageAsync(userBot.Id,
                    "Возможно ты отправил отчет? Посмотри внимательно на кнопки или нажми инструкцию.");
                break;
        }
    }

    private async Task PrintInstructions(UserBot userBot)
    {
        await _bot.SendTextMessageAsync(userBot.Id, GetInstruction(userBot.UserRole), replyMarkup: GetKeyboard(userBot.UserRole));
    }

    private static string GetInstruction(Roles userRole)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Инструкция:");
        sb.AppendLine("Привет, здесь инструкция по использованию бота");
        sb.AppendLine(
            "Для начала нужно пройти авторизацию, выбери свою роль и свой магазин. После подтверждения со стороны ответственного у тебя обновится ме" +
            "" +
            "" +
            "" +
            "ню.");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("Немного о кнопочках");
        sb.AppendLine("=> Печать отчета: Печатает отчет по выбранному магазину;");
        sb.AppendLine(
            "=> Выбрать магазин: Берет актуальный список магазинов и дает возможность выбрать магазин;");
        sb.AppendLine(
            "=> Загрузить отчет: Меню куда мы скидываем детализацию. !Важно! Если ты отправишь что-то кроме детализации продаж - то скорей всего бот сломается");
        return sb.ToString();
    }

    public async Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService)
    {
        var role = updateHandlerService.GetUserRole(user.Id);
        await _bot.SendTextMessageAsync(user.Id, GetStateTitle(), replyMarkup: GetKeyboard(await role));
    }
    private static string GetStateTitle()
    {
        return "Главное меню бота";
    }

    public static IReplyMarkup GetKeyboard(Roles userRole)
    {
        List<List<KeyboardButton>>? keyList;
        switch (userRole)
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
                    },new List<KeyboardButton>()
                    {
                        new KeyboardButton(PrintReportButton)
                    },
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
                    },
                    //new List<KeyboardButton>()
                    //{
                    //    new KeyboardButton(EmployerManagerButton),
                    //},
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
                    new List<KeyboardButton>() {
                        new KeyboardButton(ChooseStoreButton),
                        new KeyboardButton(PrintReportButton),
                        new KeyboardButton(DownloadManagerButton)
                    },
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(StoreManagerButton),
                    }
                };
                return new ReplyKeyboardMarkup(keyList);
        }
        return null;
    }

}

//public class EmployerManagerState : IStateMenu
//{
//    private const string ShowEmployerList = "Показать список моих сотрудников";
//    private const string ChangeEmployerList = "Изменить список сотрудников";
//    private readonly ITelegramBotClient _bot;

//    public EmployerManagerState(ITelegramBotClient bot)
//    {
//        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
//    }

//    public async Task ProcessMessage(Update update, UserBot userBot, UpdateHandlerService updateHandlerService)
//    {
//        switch (update.Message.Type)
//        {
//            case MessageType.Text:
//                switch (update.Message.Text)
//                {
//                    case ShowEmployerList:
//                        await ShowUserEmployerList(userBot, updateHandlerService);
//                        break;
//                    case ChangeEmployerList:
//                        await updateHandlerService.SetState(userBot, new ChangeEmployerList(_bot));
//                        break;
//                }
//                break;
//        }
//    }

//    private async Task ShowUserEmployerList(UserBot userBot, UpdateHandlerService updateHandlerService)
//    {
//        var subEmployerList = await updateHandlerService.GetSubEmployersList(userBot);
//        var sb = new StringBuilder();
//        sb.AppendLine("Список подчиненных:");
//        sb.AppendLine("UserId/UserRole/StoreCode");
//        foreach (var employer in subEmployerList)
//        {
//            var employerRole = string.Empty;
//            switch (employer.UserRole)
//            {
//                case Roles.Employer:
//                    employerRole = "Спец";
//                    break;
//                case Roles.StoreDirector:
//                    employerRole = "НОП";
//                    break;
//            }
//            sb.AppendLine($"@{employer.Username} / {employerRole} / {employer.StoreCode}");
//        }
//        await _bot.SendTextMessageAsync(userBot.Id, sb.ToString());
//    }

//    public async Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService)
//    {
//        await _bot.SendTextMessageAsync(user.Id, "Меню менеджера сотрудников.", replyMarkup: new ReplyKeyboardMarkup(new List<KeyboardButton>()
//        {
//            new KeyboardButton(ShowEmployerList),
//            new KeyboardButton(ChangeEmployerList)
//        }));
//    }
//}

//public class ChangeEmployerList : IStateMenu
//{
//    public ChangeEmployerList(ITelegramBotClient bot)
//    {
//        throw new NotImplementedException();
//    }

//    public Task ProcessMessage(Update update, UserBot userBot, UpdateHandlerService updateHandlerService)
//    {
//        throw new NotImplementedException();
//    }

//    public Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService)
//    {
//        throw new NotImplementedException();
//    }
//}