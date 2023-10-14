using AdminBot.Entities;
using AdminBot.Services;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.MenuStates.States;

public class StoreManagerState : IStateMenu
{
    private readonly ITelegramBotClient _bot;
    private const string ShowStoreButton = "Показать все магазины";
    private const string MainMenuButton = "Главное меню";
    private const string AddStoreButton = "Добавить магазины";
    private const string DeleteStoreButton = "Удалить магазины";

    public StoreManagerState(ITelegramBotClient bot)
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
                    case ShowStoreButton:
                        await ShowStores(userBot, updateHandlerService);
                        break;
                    case AddStoreButton:
                        await updateHandlerService.SetState(userBot, new AddStoreState(_bot));
                        break;
                    case DeleteStoreButton:
                        await updateHandlerService.SetState(userBot, new DeleteStoreState(_bot));
                        break;
                }
                break;
            case MessageType.Document:
                break;
        }
    }

    private async Task ShowStores(UserBot userBot, UpdateHandlerService updateHandlerService)
    {
        var storeList = await updateHandlerService.GetStoreNameList();
        var sb = new StringBuilder();
        sb.AppendLine("Список магазинов:");
        foreach (var store in storeList)
        {
            sb.AppendLine(store);
        }
        await _bot.SendTextMessageAsync(userBot.Id, sb.ToString());
    }

    public async Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService)
    {
        await _bot.SendTextMessageAsync(user.Id, GetStateTitle(), replyMarkup: GetKeyboard());
    }

    private static string GetStateTitle()
    {
        return "Менеджер магазинов";
    }

    private static IReplyMarkup GetKeyboard()
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
                new KeyboardButton(DeleteStoreButton)
            },
            new List<KeyboardButton>()
            {
                new KeyboardButton(MainMenuButton)
            }
        };
        return new ReplyKeyboardMarkup(keyList);
    }



}