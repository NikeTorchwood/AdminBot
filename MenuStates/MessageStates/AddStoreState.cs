using AdminBot.Entities;
using AdminBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class AddStoreState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string MainMenuButton = "Главное меню";
    public AddStoreState(ITelegramBotClient bot)
    {
        _bot = bot ?? throw new NullReferenceException(nameof(bot));
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
                        if (update.Message.Text.Length == 4)
                        {
                            await _bot.SendTextMessageAsync(userBot.Id, $"\"{update.Message.Text}\" - добавить код оп?",
                                replyMarkup: new InlineKeyboardMarkup(new List<InlineKeyboardButton>()
                                {
                                    InlineKeyboardButton.WithCallbackData("Да", $"{(int)RequestTypes.AddStore}.Apply"),
                                    InlineKeyboardButton.WithCallbackData("Нет", $"{(int)RequestTypes.AddStore}.Decline")
                                }), replyToMessageId: update.Message.MessageId
                            );
                            await updateHandlerService.SetState(userBot, new StoreManagerState(_bot));
                        }
                        else
                        {
                            await _bot.SendTextMessageAsync(userBot.Id, "Ваш код ОП не соответсует стандартам");
                        }
                        break;
                }
                break;

        }
    }
    public async Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService)
    {
        await _bot.SendTextMessageAsync(user.Id, "Отправь код ОП на добавление в формате \"{Буква}ХХХ\"", replyMarkup: new ReplyKeyboardRemove());
    }
}