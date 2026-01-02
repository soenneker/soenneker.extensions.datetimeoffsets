using System.Diagnostics.Contracts;
using System.Globalization;
using Soenneker.Extensions.TimeZoneInfos;

namespace Soenneker.Extensions.DateTimeOffsets;

public static class DateTimeOffsetExtensionFormat
{
    /// <summary><code>hh tt {timezone}</code></summary>
    [Pure]
    public static string ToHourFormat(this System.DateTimeOffset dateTimeOffset, System.TimeZoneInfo timeZoneInfo)
    {
        System.DateTimeOffset converted = System.TimeZoneInfo.ConvertTime(dateTimeOffset, timeZoneInfo);
        return converted.DateTime.ToString($"hh tt {timeZoneInfo.ToSimpleAbbreviation()}");
    }

    /// <summary>
    /// Not typically for UI display, for admin/debug purposes
    /// </summary>
    /// <code>"yyyy-MM-ddTHH:mm:ss.fffffff"</code>
    [Pure]
    public static string ToPreciseFormat(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
    }

    /// <summary>"MM-dd-yyyy"</summary>
    [Pure]
    public static string ToMonthFirstDateFormat(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MM-dd-yyyy");
    }

    /// <summary>
    /// Not typically for UI display, for admin/debug purposes. Appends Zulu ("Z") to string. Does not do any conversion.
    /// </summary>
    /// <param name="dateTimeOffset">Should be UTC for best results.</param>
    /// <code>"yyyy-MM-ddTHH:mm:ss.fffffffZ"</code>
    [Pure]
    public static string ToPreciseUtcFormat(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
    }

    /// <summary>
    /// yyyy-MM-ddTHH:mm:ss.fffZ. ISO 8601. Can be used for Cosmos queries.
    /// </summary>
    /// <param name="dateTimeOffset">Should be UTC for best results.</param>
    /// <code>"yyyy-MM-ddTHH:mm:ss.fffZ"</code>
    [Pure]
    public static string ToIso8601(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    ///<inheritdoc cref="ToIso8601"/>
    [Pure]
    public static string ToWebString(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToIso8601();
    }

    /// <summary>
    /// Converts DateTimeOffset into the specified timezone, and then appends a timezone display (i.e. 'ET') <para/>
    /// <code>MM/dd/yyyy hh:mm:ss tt ET</code>
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to convert.</param>
    /// <param name="tzInfo">The target timezone.</param>
    [Pure]
    public static string ToTzDateTimeFormat(this System.DateTimeOffset dateTimeOffset, System.TimeZoneInfo tzInfo)
    {
        System.DateTimeOffset converted = System.TimeZoneInfo.ConvertTime(dateTimeOffset, tzInfo);
        return converted.ToDateTimeFormatAsTz(tzInfo);
    }

    /// <summary>
    /// Converts DateTimeOffset into the specified timezone first<para/>
    /// <code>MM/dd/yyyy</code>
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to convert.</param>
    /// <param name="tzInfo">The target timezone.</param>
    [Pure]
    public static string ToTzDateFormat(this System.DateTimeOffset dateTimeOffset, System.TimeZoneInfo tzInfo)
    {
        System.DateTimeOffset converted = System.TimeZoneInfo.ConvertTime(dateTimeOffset, tzInfo);
        return converted.ToString("MM/dd/yyyy");
    }

    /// <summary>
    /// Essentially <see cref="ToTzDateTimeFormat"/> but doesn't include minutes or seconds <para/>
    /// <code>MM/dd/yyyy h tt ET</code>
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to convert.</param>
    /// <param name="tzInfo">The target timezone.</param>
    [Pure]
    public static string ToTzDateHourFormat(this System.DateTimeOffset dateTimeOffset, System.TimeZoneInfo tzInfo)
    {
        System.DateTimeOffset converted = System.TimeZoneInfo.ConvertTime(dateTimeOffset, tzInfo);
        return converted.ToString($"MM/dd/yyyy h tt {tzInfo.ToSimpleAbbreviation()}");
    }

    /// <summary>
    /// Does NOT convert to Tz. Formats and appends a timezone display (i.e. 'ET') <para/>
    /// <code>MM/dd/yyyy hh:mm:ss tt ET</code>
    /// </summary>
    [Pure]
    public static string ToDateTimeFormatAsTz(this System.DateTimeOffset dateTimeOffset, System.TimeZoneInfo tzInfo)
    {
        return dateTimeOffset.ToString($"MM/dd/yyyy hh:mm:ss tt {tzInfo.ToSimpleAbbreviation()}");
    }

    /// <summary>
    /// Does NOT convert.<para/>
    /// <code>MM/dd/yyyy hh:mm:ss tt UTC</code>
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to format.</param>
    [Pure]
    public static string ToUtcDateTimeFormat(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime.ToString("MM/dd/yyyy hh:mm:ss tt UTC");
    }

    /// <summary>
    /// Converts to tzTime and then formats <para/>
    /// <code>yyyy-MM-dd--HH-mm-ss</code>
    /// </summary>
    [Pure]
    public static string ToTzFileName(this System.DateTimeOffset dateTimeOffset, System.TimeZoneInfo tzInfo)
    {
        System.DateTimeOffset converted = System.TimeZoneInfo.ConvertTime(dateTimeOffset, tzInfo);
        return converted.ToFileName();
    }

    /// <summary>
    /// Simply formats <para/>
    /// <code>yyyy-MM-dd--HH-mm-ss</code>
    /// </summary>
    [Pure]
    public static string ToFileName(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("yyyy-MM-dd--HH-mm-ss");
    }

    /// <summary>
    /// Formats the DateTimeOffset object to a string in the format "MMM dd, yyyy".
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset object to format.</param>
    /// <returns>A string representing the formatted DateTimeOffset in the "MMM dd, yyyy" format.</returns>
    /// <example>
    /// <code>
    /// DateTimeOffset date = new DateTimeOffset(2017, 1, 5, 0, 0, 0, TimeSpan.Zero);
    /// string formattedDate = date.ToShortMonthDayYearString();
    /// // formattedDate will be "Jan 05, 2017"
    /// </code>
    /// </example>
    [Pure]
    public static string ToShortMonthDayYearString(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats the DateTimeOffset object to a string in the format "MMMM d, yyyy".
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset object to format.</param>
    /// <returns>A string representing the formatted DateTimeOffset in the "MMMM d, yyyy" format.</returns>
    /// <example>
    /// <code>
    /// DateTimeOffset date = new DateTimeOffset(2017, 1, 5, 0, 0, 0, TimeSpan.Zero);
    /// string formattedDate = date.ToLongMonthDayYearString();
    /// // formattedDate will be "January 5, 2017"
    /// </code>
    /// </example>
    [Pure]
    public static string ToLongMonthDayYearString(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);
    }
}
