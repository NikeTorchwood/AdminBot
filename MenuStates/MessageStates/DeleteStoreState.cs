using AdminBot.Entities;
using AdminBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class DeleteStoreState : IStateMenu
{
    private const string? MainMenuButton = "Главное меню";
    private const string BackButton = "Назад";
    private readonly ITelegramBotClient _bot;


    public DeleteStoreState(ITelegramBotClient bot)
    {
        _bot = bot;
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
                    case BackButton:
                        await updateHandlerService.SetState(userBot, new StoreManagerState(_bot));
                        break;
                    default:
                        var storeList = await updateHandlerService.GetStoreNameList();
                        var messageText = update.Message.Text;
                        if (storeList.Contains(messageText))
                        {
                            await _bot.SendTextMessageAsync(userBot.Id, $"Ты хочешь удалить {messageText}",
                                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Да",
                                        $"{(int)RequestTypes.DeleteStore}.Apply"),
                                    InlineKeyboardButton.WithCallbackData("Нет",
                                        $"{(int)RequestTypes.DeleteStore}.Decline"),
                                }),
                                replyToMessageId: update.Message.MessageId);
                            await updateHandlerService.SetState(userBot, new StoreManagerState(_bot));
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
                "Список магазинов пуст. Скорей всего вы не добавили еще магазины.");
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
            new("Назад")
        });
        await _bot.SendTextMessageAsync(user.Id, 
            "Список магазинов на клавиатуре:\nВыберите какой магазин нужно удалить?",
            replyMarkup: new ReplyKeyboardMarkup(keyboard));
    }
}