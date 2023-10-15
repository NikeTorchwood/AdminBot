namespace AdminBot.Entities;

public class Store
{
    public string CodeStore { get; }
    public List<EconomicDirection> EconomicDirections { get; }
    public int CountLP { get;}

    public Store(string codeStore, List<EconomicDirection> economicDirections, int countLP)
    {
        CodeStore = codeStore ?? throw new ArgumentNullException(nameof(codeStore));
        EconomicDirections = economicDirections;
        CountLP = countLP;
    }
}