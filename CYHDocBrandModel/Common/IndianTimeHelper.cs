namespace CYHDocBrandModel.Common;

/// <summary>
/// Helper to handle Indian Standard Time (IST - UTC+05:30) consistently across all environments
/// (Windows, Linux, Docker, and Cloud Database).
/// </summary>
public static class IndianTimeHelper
{
    public static readonly TimeSpan IstOffset = new TimeSpan(5, 30, 0);

    public static readonly TimeZoneInfo IndianTimeZone = InitializeIndianTimeZone();

    private static TimeZoneInfo InitializeIndianTimeZone()
    {
        try
        {
            // Windows ID
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
        catch
        {
            try
            {
                // Linux / IANA / Docker ID
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
            catch
            {
                // Fallback custom timezone: UTC+05:30
                return TimeZoneInfo.CreateCustomTimeZone("IST", IstOffset, "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi", "India Standard Time");
            }
        }
    }

    /// <summary>
    /// Current DateTime in Indian Standard Time (IST).
    /// </summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndianTimeZone);

    /// <summary>
    /// Ensures DateTime is in Indian Standard Time (IST).
    /// If the DateTime is UTC, converts to IST.
    /// If already stored in Indian Time (Unspecified/Local), returns as-is.
    /// </summary>
    public static DateTime ToIndianTime(this DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dt, IndianTimeZone);
        }
        return dt;
    }

    /// <summary>
    /// Converts a nullable DateTime to Indian Standard Time (IST).
    /// </summary>
    public static DateTime? ToIndianTime(this DateTime? dt)
    {
        if (!dt.HasValue) return null;
        return dt.Value.ToIndianTime();
    }

    /// <summary>
    /// Formats an IST DateTime into standard readable format with IST indicator.
    /// E.g. "27 Oct 2026, 04:30 PM IST"
    /// </summary>
    public static string FormatIndianTime(this DateTime dt, string format = "dd MMM yyyy, hh:mm tt")
    {
        return $"{dt.ToString(format)} IST";
    }
}
