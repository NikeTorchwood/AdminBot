using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.Services.ServiceInterfaces;

public interface ICallbackQueryHandler
{
    public Task ProcessMessage(Update update, ITelegramBotClient bot, UpdateHandlerService updateHandlerService);
}