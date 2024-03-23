using System.Globalization;

namespace Budget.App.Common;

public static class DateExtensions
{
    public static int ToIsoWeekNumber(this DateTime date)
    {
        var day = (int)CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date.AddDays(4 - (day == 0 ? 7 : day)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
    public static int ToIsoWeekNumber(this DateOnly date)
    {
        return date.ToDateTime(TimeOnly.MinValue).ToIsoWeekNumber();
    }

    public static string TimePassed(this DateTime date, TimeProvider dateProvider)
    {
        var now = dateProvider.GetUtcNow().DateTime;
        var timePassed = now - date;

        if (timePassed.Days > 1)
        {
            return $"{timePassed.Days} days";
        }

        if (timePassed.Days == 1)
        {
            return $"{timePassed.Days} day";
        }

        if (timePassed.Hours > 1)
        {
            return $"{timePassed.Hours} hours";
        }

        if (timePassed.Hours == 1)
        {
            return $"{timePassed.Hours} hour";
        }

        if (timePassed.Minutes > 1)
        {
            return $"{timePassed.Minutes} minutes";
        }

        if (timePassed.Minutes == 1)
        {
            return $"{timePassed.Minutes} minute";
        }

        if (timePassed.Seconds > 1)
        {
            return $"{timePassed.Seconds} seconds";
        }

        return "Just now";
    }

    public static string TimePassedShort(this DateTime date, TimeProvider dateProvider)
    {
        var now = dateProvider.GetUtcNow().DateTime;
        var timePassed = now - date;

        if (timePassed.Days > 1)
        {
            return $"{timePassed.Days}d";
        }

        if (timePassed.Days == 1)
        {
            return $"{timePassed.Days}d";
        }

        if (timePassed.Hours > 1)
        {
            return $"{timePassed.Hours}h";
        }

        if (timePassed.Hours == 1)
        {
            return $"{timePassed.Hours}h";
        }

        if (timePassed.Minutes > 1)
        {
            return $"{timePassed.Minutes}m";
        }

        if (timePassed.Minutes == 1)
        {
            return $"{timePassed.Minutes}m";
        }

        if (timePassed.Seconds > 1)
        {
            return $"{timePassed.Seconds}s";
        }

        return "Now";
    }
}