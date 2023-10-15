namespace AdminBot.Entities;

public class DirectionCalculation
{
    public decimal DirectionProgress { get; }
    public int DirectionRemainingTotal { get; }
    public decimal DailyPlan { get; }
    public decimal DirectionProgressForecast { get; }
    public DirectionCalculation(EconomicDirection economicDirection, int countLP)
    {
        DirectionProgress = economicDirection.Fact / (decimal)economicDirection.Plan;
        DirectionRemainingTotal = economicDirection.Plan - economicDirection.Fact;
        var daysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
        var today = DateTime.Today.Day;
        var daysRemainingInMonth = daysInMonth - today;
        DirectionProgressForecast = DirectionProgress / countLP * daysInMonth;
        DailyPlan = Math.Round(DirectionRemainingTotal / (decimal)daysRemainingInMonth, 2);
    }
}