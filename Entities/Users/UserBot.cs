using AdminBot.MenuStates;

namespace AdminBot.Entities.Users;

public class UserBot
{
    public UserBot(long userId, IStateMenu state, Roles roleResult)
    {
        Id = userId;
        StateMenu = state;
        Role = roleResult;
    }

    public long Id { get; }
    public IStateMenu StateMenu { get; }
    public Roles Role { get;}
}