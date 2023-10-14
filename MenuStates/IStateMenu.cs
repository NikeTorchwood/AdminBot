using AdminBot.Entities;
using AdminBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminBot.MenuStates;

public interface IStateMenu
{
    // todo : метод вывода сообщения с инлайн кнопками
    // todo : сделать метод для генерации кнопок для стейт сообщения
    public Task ProcessMessage(Update update, UserBot userBot, UpdateHandlerService updateHandlerService);
    Task SendStateMessage(UserBot user, UpdateHandlerService updateHandlerService);
}