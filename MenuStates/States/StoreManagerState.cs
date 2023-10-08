using AdminBot.Entities.Users;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class StoreManagerState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string ShowStoreButton = "Показать все магазины";
    private const string AddStoreButton = "Добавить магазины";
    private const string DeleteStoreButton = "Удалить магазины";

    public StoreManagerState(ITelegramBotClient bot)
    {
        _bot = bot?? throw new ArgumentNullException(nameof(bot));
    }

    public Task ProcessMessage(Update update, UserBot userBot, TelegramBotMenuContext context)
    {
        throw new NotImplementedException();
    }

    public async Task SendStateMessage(UserBot userBot)
    {
        await _bot.SendTextMessageAsync(userBot.Id, GetStateTitle(), replyMarkup: GetKeyboard());
    }

    private string GetStateTitle()
    {
        return "Меню выбора Магазинов";
    }

    private IReplyMarkup GetKeyboard()
    {
        var keyList = new List<List<KeyboardButton>>()
        {
            new List<KeyboardButton>()
            {
                new KeyboardButton(ShowStoreButton),
            },
            new List<KeyboardButton>()
            {
                new KeyboardButton(AddStoreButton),
            },
            new List<KeyboardButton>()
            {
                new KeyboardButton(DeleteStoreButton)
            }
        };
        return new ReplyKeyboardMarkup(keyList);
    }
}