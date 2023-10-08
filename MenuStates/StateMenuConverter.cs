using AdminBot.MenuStates.States;
using Telegram.Bot;

namespace AdminBot.MenuStates;

public static class StateMenuConverter
{
    private static readonly Dictionary<Type, StatesMenu> StateToEnumMap = new()
    {
        { typeof(StartState), StatesMenu.StartState},
        { typeof(DownloadFileState), StatesMenu.DownloadFileState },
        { typeof(AuthorizationState), StatesMenu.AuthorizationState },
        { typeof(StoreManagerState), StatesMenu.StoreManagerState},
    };
    private static readonly Dictionary<StatesMenu, Type> EnumToStateMap = new()
    {
        { StatesMenu.StartState, typeof(StartState) },
        { StatesMenu.DownloadFileState, typeof(DownloadFileState) },
        { StatesMenu.AuthorizationState , typeof(AuthorizationState)},
        { StatesMenu.StoreManagerState, typeof(StoreManagerState)},
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