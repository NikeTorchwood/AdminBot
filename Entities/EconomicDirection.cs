namespace AdminBot.Entities;

public class EconomicDirection
{
    public EconomicDirection(string directionName, int plan, int fact)
    {
        DirectionName = directionName;
        Plan = plan;
        Fact = fact;
    }
    public string DirectionName { get; }
    public int Plan { get; }
    public int Fact { get; }
}