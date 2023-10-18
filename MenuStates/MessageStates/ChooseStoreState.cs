using AdminBot.Entities;
using AdminBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class ChooseStoreState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string? MainMenuButton = "Главное меню";
    public ChooseStoreState(ITelegramBotClient bot)
    {
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
    }

    public async Task ProcessMessage(Update update, UserBot userBot, UpdateHandlerService updateHandlerService)
    {
        switch (update.Message.Type)
        {
            case MessageType.Text:
                switch (update.Message.Text)
                {
                    case MainMenuButton:
                        await updateHandlerService.SetState(userBot, new StartState(_bot));
                        break;
                    default:
                        var storeList = await updateHandlerService.GetStoreNameList();
                        var messageText = update.Message.Text;
                        if (storeList.Contains(messageText))
                        {
                            switch (userBot.UserRole)
                            {
                                case Roles.None:
                                case Roles.Employer:
                                case Roles.StoreDirector:
                                    await _bot.SendTextMessageAsync(userBot.Id, $"Выберите свою позицию на офисе {messageText}",
                                        replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                                        {
                                    InlineKeyboardButton.WithCallbackData("Начальник офиса",
                                        $"{(int)RequestTypes.ChangeRoleEmployer}.Director"),
                                    InlineKeyboardButton.WithCallbackData("Специалист офиса",
                                        $"{(int)RequestTypes.ChangeRoleEmployer}.Specialist"),
                                        }),
                                        replyToMessageId: update.Message.MessageId);
                                    await _bot.SendTextMessageAsync(userBot.Id,
                                        "Дождитесь подтверждения со стороны ответственного, после подтверждения ваше меню изменится");
                                    await updateHandlerService.SetState(userBot, new StartState(_bot));
                                    break;
                                case Roles.Administrator:
                                case Roles.SectorDirector:
                                    await updateHandlerService.SetState(new UserBot(
                                        userBot.Id,
                                        userBot.StateMenu,
                                        userBot.UserRole,
                                        messageText,
                                        userBot.Username), new StartState(_bot));
                                    break;
                            }
                        }
                        break;
                }
                break;
            case MessageType.Document:
                break;
        }
    }

    public async Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService)
    {
        var storeList = await updateHandlerService.GetStoreNameList();
        if (storeList.Count == 0)
        {
            await _bot.SendTextMessageAsync(user.Id,
                "Список магазинов пуст. Скорей всего магазины еще не добавили.");
            return;
        }
        var keyboard = new List<List<KeyboardButton>>();
        var rowButtons = new List<KeyboardButton>();
        for (var i = 0; i < storeList.Count; i++)
            if (i % 3 == 0)
            {
                rowButtons = new List<KeyboardButton> { new(storeList[i]) };
                keyboard.Add(rowButtons);
            }
            else
            {
                rowButtons.Add(new KeyboardButton(storeList[i]));
            }
        keyboard.Add(new List<KeyboardButton>
        {
            new(MainMenuButton)
        });
        await _bot.SendTextMessageAsync(user.Id,
            $"Выберите свой магазин. Текущий выбранный магазин - {user.StoreCode ?? "\"Пусто\""}. Магазин изменится только после подтверждения со стороны ответственного лица.",
            replyMarkup: new ReplyKeyboardMarkup(keyboard));
    }
}