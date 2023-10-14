using AdminBot.MenuStates;

namespace AdminBot.Entities;

public class UserBot
{
    public UserBot(long userId, IStateMenu? state, Roles userRole, string? storeCode, string username)
    {
        Id = userId;
        StateMenu = state;
        UserRole = userRole;
        StoreCode = storeCode;
        Username = username;
    }


    public long Id { get; }
    public IStateMenu? StateMenu { get; }
    public Roles UserRole { get; }
    public string? StoreCode { get; }
    public string Username { get; }
}