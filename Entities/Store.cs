namespace AdminBot.Entities;

public class Store
{
    public string CodeStore { get; }
    public List<EconomicDirection> EconomicDirections { get; }

    public Store(string codeStore, List<EconomicDirection> economicDirections)
    {
        CodeStore = codeStore ?? throw new ArgumentNullException(nameof(codeStore));
        EconomicDirections = economicDirections;
    }
}