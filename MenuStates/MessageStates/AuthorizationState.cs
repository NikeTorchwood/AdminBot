using AdminBot.Entities;
using AdminBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class AuthorizationState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string MainMenuButton = "Главное меню";
    private const string ResetRoleButton = "Мне нужно сбросить роль";
    private const string EmployerButton = "Я сотрудник офиса";
    private const string SectorDirectorButton = "Я директор сектора";
    private const string AdministratorButton = "Я администратор";
    public AuthorizationState(ITelegramBotClient bot)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    }

    public async Task ProcessMessage(Update update, UserBot userBot, UpdateHandlerService updateHandlerService)
    {
        if (update.Message != null)
            switch (update.Message.Type)
            {
                case MessageType.Text:
                    switch (update.Message.Text)
                    {
                        case EmployerButton:
                            if (userBot.UserRole >= Roles.SectorDirector)
                            {
                                await _bot.SendTextMessageAsync(userBot.Id,
                                    "На данный момент ваша роль Директор сектора или выше, для перехода в дальнейшее меню сбросьте свою роль.");
                                await updateHandlerService.SetState(userBot, new AuthorizationState(_bot));
                            }
                            else
                            {
                                await updateHandlerService.SetState(userBot, new ChooseStoreState(_bot));
                            }
                            break;
                        case ResetRoleButton:
                            await _bot.SendTextMessageAsync(userBot.Id,
                                "Вы уверены, что хотите сбросить роль? Для возврата к своей роли, нужно проходить авторизацию повторно",
                                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Да",
                                        $"{(int)RequestTypes.ResetUserRole}.Apply"),
                                    InlineKeyboardButton.WithCallbackData("Нет",
                                        $"{(int)RequestTypes.ResetUserRole}.Decline"),
                                }),
                                replyToMessageId: update.Message.MessageId);
                            await updateHandlerService.SetState(userBot, new AuthorizationState(_bot));
                            break;
                        case MainMenuButton:
                            await updateHandlerService.SetState(userBot, new StartState(_bot));
                            break;
                        case SectorDirectorButton:
                            if (userBot.UserRole >= Roles.SectorDirector)
                            {
                                await _bot.SendTextMessageAsync(userBot.Id,
                                    "На данный момент ваша роль Директор сектора или выше, для перехода в дальнейшее меню сбросьте свою роль.");
                                await updateHandlerService.SetState(userBot, new AuthorizationState(_bot));
                            }
                            else
                            {
                                await updateHandlerService.CreateChangeRoleRequest(Roles.SectorDirector, update.Message.From);
                                await _bot.SendTextMessageAsync(userBot.Id,
                                    "Дождитесь подтверждения со стороны ответственного, после подтверждения ваше меню изменится");
                                await updateHandlerService.SetState(userBot, new StartState(_bot));
                            }

                            break;
                        case AdministratorButton:
                            if (userBot.Id == 6410857523)
                            {
                                await updateHandlerService.SetState(new UserBot(userBot.Id,
                                    userBot.StateMenu,
                                    Roles.Administrator,
                                    userBot.StoreCode,
                                    userBot.Username),
                                    new StartState(_bot));
                            }
                            else
                            {
                                await _bot.SendTextMessageAsync(update.Message.Chat.Id,
                                    "К сожалению вы не можете стать Администратором, пройдите авторизацию повторно");
                                await updateHandlerService.SetState(userBot, new AuthorizationState(_bot));
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

    public async Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService)
    {
        await _bot.SendTextMessageAsync(user.Id, GetStateTitle(), replyMarkup: GetKeyboard());
    }
    private static string GetStateTitle()
    {
        return "Меню выбора роли. Выберите свою роль.";
    }

    private static IReplyMarkup GetKeyboard()
    {
        List<List<KeyboardButton>>? keyList;
        keyList = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        new KeyboardButton(EmployerButton),
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