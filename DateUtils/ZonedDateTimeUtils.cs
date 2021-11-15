using NodaTime;
using TimeZoneConverter;

namespace DateUtils;

public static class ZonedDateTimeUtils
{
    public static ZonedDateTime GetZonedDateTime()
    {
        var dotNetDateTime = DateTime.Now;
        var localDate = LocalDateTime.FromDateTime(dotNetDateTime);
        var zone = DateTimeZoneProviders.Tzdb[GetTimeZoneId()];
        return zone.AtStrictly(localDate);
    }

    private static string GetTimeZoneId()
    {
        if (TZConvert.TryWindowsToIana(TimeZoneInfo.Local.Id, out string result))
            return result;
        else
            return TimeZoneInfo.Local.Id;
    }

    public static ZonedDateTime GetZonedDateTime(int hour, int minute, int second)
    {
        var dotNetDateTime = new DateTime(1970, 1, 1, hour, minute, second);
        var localDate = LocalDateTime.FromDateTime(dotNetDateTime);
        var localTime = DateTimeZoneProviders.Tzdb[GetTimeZoneId()];
        return localTime.AtStrictly(localDate);
    }

    public static ZonedDateTime GetZonedDateTime(int year)
    {
        var dotNetDateTime = new DateTime(year, 1, 1, 0, 0, 0);
        var localDate = LocalDateTime.FromDateTime(dotNetDateTime);
        var localTime = DateTimeZoneProviders.Tzdb[GetTimeZoneId()];
        return localTime.AtStrictly(localDate);
    }

    public static ZonedDateTime GetZonedDateTime(DateTimeOffset dateTime)
    {
        var localDate = LocalDateTime.FromDateTime(dateTime.UtcDateTime);
        var localTime = DateTimeZoneProviders.Tzdb[GetTimeZoneId()];
        return localTime.AtStrictly(localDate);
    }

    public static bool IsInBetween(this ZonedDateTime zonedDateTime, ZonedDateTime zonedDateTime1, ZonedDateTime zonedDateTime2) =>
        (zonedDateTime - zonedDateTime1).Milliseconds > 0 && (zonedDateTime2 - zonedDateTime).Milliseconds > 0;

    public static bool IsTimeInBetween(this ZonedDateTime zonedDateTime, ZonedDateTime zonedDateTime1, ZonedDateTime zonedDateTime2) =>
        (zonedDateTime.TimeOfDay - zonedDateTime1.TimeOfDay).Milliseconds > 0 && (zonedDateTime2.TimeOfDay - zonedDateTime.TimeOfDay).Milliseconds > 0;
}
