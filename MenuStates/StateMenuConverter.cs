using AdminBot.MenuStates.States;
using Telegram.Bot;

namespace AdminBot.MenuStates;

public static class StateMenuConverter
{
    private static readonly Dictionary<Type, StatesMenu> StateToEnumMap = new()
    {
        { typeof(StartState), StatesMenu.StartMenu},
        { typeof(DownloadFileMenu), StatesMenu.DownloadMenu },
        { typeof(AuthorizationMenu), StatesMenu.AuthorizationMenu },
    };
    private static readonly Dictionary<StatesMenu, Type> EnumToStateMap = new()
    {
        { StatesMenu.StartMenu, typeof(StartState) },
        { StatesMenu.DownloadMenu, typeof(DownloadFileMenu) },
        { StatesMenu.AuthorizationMenu , typeof(AuthorizationMenu)}
    };
    public static StatesMenu ConvertToStatesMenu(IStateMenu state)
    {
        var stateType = state.GetType();

        if (StateToEnumMap.TryGetValue(stateType, out StatesMenu result))
        {
            return result;
        }
        throw new ArgumentException("Invalid state type");
    }

    public static IStateMenu ConvertToIStateMenu(StatesMenu state, ITelegramBotClient bot)
    {
        if (bot == null)
        {
            throw new ArgumentNullException(nameof(bot));
        }
        if (EnumToStateMap.TryGetValue(state, out var resultType))
        {
            return Activator.CreateInstance(resultType, bot) as IStateMenu;
        }
        throw new ArgumentException("Invalid state type");
    }
}