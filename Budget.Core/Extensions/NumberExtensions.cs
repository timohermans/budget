namespace Budget.Core.Extensions;

public static class NumberExtensions
{
    public static bool IsCurrentWeek(this int weekNumber, TimeProvider timeProvider)
    {
        return weekNumber == timeProvider.GetUtcNow().DateTime.ToIsoWeekNumber();
    }
}
