namespace Stampcard.UI;

/// <summary>
/// Formats values for display in German. The backend returns points in time in UTC,
/// they are shown in the time zone of the club.
/// </summary>
public static class DisplayFormat
{
    private static readonly TimeZoneInfo ClubTimeZone = FindClubTimeZone();

    public static string Date(DateOnly date)
    {
        return date.ToString("dd.MM.yyyy");
    }

    public static string Date(DateTimeOffset pointInTime)
    {
        return TimeZoneInfo.ConvertTime(pointInTime, ClubTimeZone).ToString("dd.MM.yyyy");
    }

    public static string DateTime(DateTimeOffset pointInTime)
    {
        return TimeZoneInfo.ConvertTime(pointInTime, ClubTimeZone).ToString("dd.MM.yyyy HH:mm");
    }

    // container images without time zone data fall back to UTC instead of failing
    private static TimeZoneInfo FindClubTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
