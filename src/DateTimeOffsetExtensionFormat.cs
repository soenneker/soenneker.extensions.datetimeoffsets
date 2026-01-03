using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using Soenneker.Extensions.TimeZoneInfos;

namespace Soenneker.Extensions.DateTimeOffsets;

public static class DateTimeOffsetExtensionFormat
{
    private static readonly CultureInfo _inv = CultureInfo.InvariantCulture;

    /// <summary>Formats as <c>hh tt {tz}</c> using the provided time zone.</summary>
    [Pure]
    public static string ToHourFormat(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return converted.ToString("hh tt", _inv) + " " + tz.ToSimpleAbbreviation();
    }

    /// <summary>Not typically for UI display; for admin/debug purposes. Format: <c>yyyy-MM-ddTHH:mm:ss.fffffff</c>.</summary>
    [Pure]
    public static string ToPreciseFormat(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", _inv);
    }

    /// <summary>Formats as <c>MM-dd-yyyy</c>.</summary>
    [Pure]
    public static string ToMonthFirstDateFormat(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MM-dd-yyyy", _inv);
    }

    /// <summary>
    /// Not typically for UI display; for admin/debug purposes. Converts to UTC and appends <c>Z</c>.
    /// Format: <c>yyyy-MM-ddTHH:mm:ss.fffffffZ</c>.
    /// </summary>
    /// <param name="dateTimeOffset">The value to format (any offset; it will be converted to UTC).</param>
    [Pure]
    public static string ToPreciseUtcFormat(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffff'Z'", _inv);
    }

    /// <summary>
    /// Converts to UTC and formats as <c>yyyy-MM-ddTHH:mm:ss.fffZ</c>. Useful for Cosmos string comparisons/queries.
    /// </summary>
    /// <param name="dateTimeOffset">The value to format (any offset; it will be converted to UTC).</param>
    [Pure]
    public static string ToIso8601(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'", _inv);
    }

    ///<inheritdoc cref="ToIso8601"/>
    [Pure]
    public static string ToWebString(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToIso8601();
    }

    /// <summary>
    /// Converts to the specified time zone and formats as <c>MM/dd/yyyy hh:mm:ss tt {tz}</c>.
    /// </summary>
    [Pure]
    public static string ToTzDateTimeFormat(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return converted.ToDateTimeFormatAsTz(tz);
    }

    /// <summary>
    /// Converts to the specified time zone and formats as <c>MM/dd/yyyy</c>.
    /// </summary>
    [Pure]
    public static string ToTzDateFormat(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return converted.ToString("MM/dd/yyyy", _inv);
    }

    /// <summary>
    /// Converts to the specified time zone and formats as <c>MM/dd/yyyy h tt {tz}</c>.
    /// </summary>
    [Pure]
    public static string ToTzDateHourFormat(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return converted.ToString("MM/dd/yyyy h tt", _inv) + " " + tz.ToSimpleAbbreviation();
    }

    /// <summary>
    /// Does not convert; formats the value as <c>MM/dd/yyyy hh:mm:ss tt {tz}</c>.
    /// </summary>
    [Pure]
    public static string ToDateTimeFormatAsTz(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        return dateTimeOffset.ToString("MM/dd/yyyy hh:mm:ss tt", _inv) + " " + tz.ToSimpleAbbreviation();
    }

    /// <summary>
    /// Converts to UTC and formats as <c>MM/dd/yyyy hh:mm:ss tt UTC</c>.
    /// </summary>
    [Pure]
    public static string ToUtcDateTimeFormat(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime.ToString("MM/dd/yyyy hh:mm:ss tt 'UTC'", _inv);
    }

    /// <summary>
    /// Converts to the specified time zone and formats as <c>yyyy-MM-dd--HH-mm-ss</c>.
    /// </summary>
    [Pure]
    public static string ToTzFileName(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return converted.ToFileName();
    }

    /// <summary>
    /// Formats as <c>yyyy-MM-dd--HH-mm-ss</c>.
    /// </summary>
    [Pure]
    public static string ToFileName(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("yyyy-MM-dd--HH-mm-ss", _inv);
    }

    /// <summary>Formats as <c>MMM dd, yyyy</c> using invariant culture.</summary>
    [Pure]
    public static string ToShortMonthDayYearString(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MMM dd, yyyy", _inv);
    }

    /// <summary>Formats as <c>MMMM d, yyyy</c> using invariant culture.</summary>
    [Pure]
    public static string ToLongMonthDayYearString(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MMMM d, yyyy", _inv);
    }
}