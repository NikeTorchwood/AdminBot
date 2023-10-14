using AdminBot.Entities;
using Telegram.Bot.Types;

namespace AdminBot.Services.ServiceInterfaces;

public interface IRequestService
{
    Task<UserBot> ApplyRequest(CallbackQuery callbackQuery);
    Task CreateDirectorSectorRequest(User user);
    Task CreateEmployerRequest(Roles applicantRole, User user, string? applicantStoreCode);
}