using AdminBot.Entities;
using AdminBot.MenuStates;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.Services.ServiceInterfaces;

public interface IMessageHandlerService
{
    public Task ProcessMessage(Update update, ITelegramBotClient bot, UpdateHandlerService updateHandlerService);
    public Task SetState(UserBot user, IStateMenu newState, UpdateHandlerService updateHandlerService);

}