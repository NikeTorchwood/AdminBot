using AdminBot.Entities.Users;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.MenuStates;

public interface IStateMenu
{
    public Task ProcessMessage(Update update, UserBot userBot, TelegramBotMenuContext context);
    Task SendStateMessage(UserBot userBot);
}