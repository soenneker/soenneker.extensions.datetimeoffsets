using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using Soenneker.Extensions.TimeZoneInfos;

namespace Soenneker.Extensions.DateTimeOffsets;

/// <summary>
/// Formatting helpers for <see cref="DateTimeOffset"/>.
/// </summary>
/// <remarks>
/// Uses <see cref="CultureInfo.InvariantCulture"/> through <see cref="DateTimeFormatInfo.InvariantInfo"/>
/// for stable, culture-independent output. Time zone methods convert the instant into the supplied
/// <see cref="TimeZoneInfo"/> unless explicitly documented otherwise.
/// </remarks>
public static class DateTimeOffsetExtensionFormat
{
    private static readonly DateTimeFormatInfo _invDtf = DateTimeFormatInfo.InvariantInfo;

    private const string _hour12Format = "h tt";
    private const string _hour12PaddedFormat = "hh tt";
    private const string _hour12MinuteFormat = "h:mm tt";
    private const string _hour12MinuteSecondFormat = "h:mm:ss tt";

    private const string _hour24Format = "HH";
    private const string _hour24MinuteFormat = "HH:mm";
    private const string _hour24MinuteSecondFormat = "HH:mm:ss";

    private const string _dateSlashFormat = "MM/dd/yyyy";
    private const string _dateDashFormat = "MM-dd-yyyy";
    private const string _yearMonthDayFormat = "yyyy-MM-dd";

    private const string _dateHour12Format = "MM/dd/yyyy h tt";
    private const string _dateHour12MinuteFormat = "MM/dd/yyyy h:mm tt";
    private const string _dateHour12MinuteSecondFormat = "MM/dd/yyyy h:mm:ss tt";

    private const string _dateHour24MinuteFormat = "MM/dd/yyyy HH:mm";
    private const string _dateHour24MinuteSecondFormat = "MM/dd/yyyy HH:mm:ss";

    private const string _sortableMinuteFormat = "yyyy-MM-dd HH:mm";
    private const string _sortableSecondFormat = "yyyy-MM-dd HH:mm:ss";

    private const string _shortMonthDayYearFormat = "MMM d, yyyy";
    private const string _shortMonthDayYearPaddedFormat = "MMM dd, yyyy";
    private const string _longMonthDayYearFormat = "MMMM d, yyyy";
    private const string _longWeekdayMonthDayYearFormat = "dddd, MMMM d, yyyy";

    private const string _preciseFormat = "yyyy-MM-ddTHH:mm:ss.fffffff";
    private const string _preciseUtcFormat = "yyyy-MM-ddTHH:mm:ss.fffffff'Z'";
    private const string _iso8601UtcMillisFormat = "yyyy-MM-ddTHH:mm:ss.fff'Z'";
    private const string _iso8601SecondFormat = "yyyy-MM-ddTHH:mm:ss";
    private const string _iso8601MillisFormat = "yyyy-MM-ddTHH:mm:ss.fff";

    private const string _utcDateTimeFormat = "MM/dd/yyyy h:mm:ss tt 'UTC'";
    private const string _utcDateHourMinuteFormat = "MM/dd/yyyy h:mm tt 'UTC'";

    private const string _fileNameFormat = "yyyy-MM-dd--HH-mm-ss";
    private const string _fileNameMillisFormat = "yyyy-MM-dd--HH-mm-ss-fff";

    /// <summary>
    /// Formats as <c>hh tt</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>hh tt</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHourFormat(this DateTimeOffset value) =>
        value.ToString(_hour12PaddedFormat, _invDtf);

    /// <summary>
    /// Formats as <c>h:mm tt</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>h:mm tt</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHourMinuteFormat(this DateTimeOffset value) =>
        value.ToString(_hour12MinuteFormat, _invDtf);

    /// <summary>
    /// Formats as <c>h:mm:ss tt</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>h:mm:ss tt</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHourMinuteSecondFormat(this DateTimeOffset value) =>
        value.ToString(_hour12MinuteSecondFormat, _invDtf);

    /// <summary>
    /// Formats as <c>HH</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>HH</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string To24HourFormat(this DateTimeOffset value) =>
        value.ToString(_hour24Format, _invDtf);

    /// <summary>
    /// Formats as <c>HH:mm</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>HH:mm</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string To24HourMinuteFormat(this DateTimeOffset value) =>
        value.ToString(_hour24MinuteFormat, _invDtf);

    /// <summary>
    /// Formats as <c>HH:mm:ss</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>HH:mm:ss</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string To24HourMinuteSecondFormat(this DateTimeOffset value) =>
        value.ToString(_hour24MinuteSecondFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MM/dd/yyyy</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDateFormat(this DateTimeOffset value) =>
        value.ToString(_dateSlashFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MM-dd-yyyy</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MM-dd-yyyy</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDateDashFormat(this DateTimeOffset value) =>
        value.ToString(_dateDashFormat, _invDtf);

    /// <summary>
    /// Formats as <c>yyyy-MM-dd</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>yyyy-MM-dd</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToYearMonthDayFormat(this DateTimeOffset value) =>
        value.ToString(_yearMonthDayFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MM/dd/yyyy h tt</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h tt</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDateHourFormat(this DateTimeOffset value) =>
        value.ToString(_dateHour12Format, _invDtf);

    /// <summary>
    /// Formats as <c>MM/dd/yyyy h:mm tt</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h:mm tt</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDateHourMinuteFormat(this DateTimeOffset value) =>
        value.ToString(_dateHour12MinuteFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MM/dd/yyyy h:mm:ss tt</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h:mm:ss tt</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDateHourMinuteSecondFormat(this DateTimeOffset value) =>
        value.ToString(_dateHour12MinuteSecondFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MM/dd/yyyy HH:mm</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy HH:mm</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDate24HourMinuteFormat(this DateTimeOffset value) =>
        value.ToString(_dateHour24MinuteFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MM/dd/yyyy HH:mm:ss</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy HH:mm:ss</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDate24HourMinuteSecondFormat(this DateTimeOffset value) =>
        value.ToString(_dateHour24MinuteSecondFormat, _invDtf);

    /// <summary>
    /// Formats as a sortable value with minute precision: <c>yyyy-MM-dd HH:mm</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>yyyy-MM-dd HH:mm</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToSortableMinuteFormat(this DateTimeOffset value) =>
        value.ToString(_sortableMinuteFormat, _invDtf);

    /// <summary>
    /// Formats as a sortable value with second precision: <c>yyyy-MM-dd HH:mm:ss</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>yyyy-MM-dd HH:mm:ss</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToSortableSecondFormat(this DateTimeOffset value) =>
        value.ToString(_sortableSecondFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MMM dd, yyyy</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MMM dd, yyyy</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToShortMonthDayYearString(this DateTimeOffset value) =>
        value.ToString(_shortMonthDayYearPaddedFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MMM d, yyyy</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MMM d, yyyy</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToShortMonthDayYearFormat(this DateTimeOffset value) =>
        value.ToString(_shortMonthDayYearFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MMMM d, yyyy</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MMMM d, yyyy</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToLongMonthDayYearString(this DateTimeOffset value) =>
        value.ToString(_longMonthDayYearFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MMMM d, yyyy</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MMMM d, yyyy</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToLongMonthDayYearFormat(this DateTimeOffset value) =>
        value.ToString(_longMonthDayYearFormat, _invDtf);

    /// <summary>
    /// Formats as <c>dddd, MMMM d, yyyy</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>dddd, MMMM d, yyyy</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToLongWeekdayMonthDayYearFormat(this DateTimeOffset value) =>
        value.ToString(_longWeekdayMonthDayYearFormat, _invDtf);

    /// <summary>
    /// Formats as <c>yyyy-MM-ddTHH:mm:ss.fffffff</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>yyyy-MM-ddTHH:mm:ss.fffffff</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToPreciseFormat(this DateTimeOffset value) =>
        value.ToString(_preciseFormat, _invDtf);

    /// <summary>
    /// Converts to UTC and formats as <c>yyyy-MM-ddTHH:mm:ss.fffffffZ</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A UTC string in the form <c>yyyy-MM-ddTHH:mm:ss.fffffffZ</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToPreciseUtcFormat(this DateTimeOffset value) =>
        value.UtcDateTime.ToString(_preciseUtcFormat, _invDtf);

    /// <summary>
    /// Converts to UTC and formats as ISO-8601 with millisecond precision: <c>yyyy-MM-ddTHH:mm:ss.fffZ</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A UTC string in the form <c>yyyy-MM-ddTHH:mm:ss.fffZ</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToIso8601(this DateTimeOffset value) =>
        value.UtcDateTime.ToString(_iso8601UtcMillisFormat, _invDtf);

    /// <summary>
    /// Converts to UTC and formats as a web-safe ISO-8601 string with millisecond precision.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A UTC string in the form <c>yyyy-MM-ddTHH:mm:ss.fffZ</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToWebString(this DateTimeOffset value) =>
        value.ToIso8601();

    /// <summary>
    /// Formats as ISO-8601 without an offset and with second precision: <c>yyyy-MM-ddTHH:mm:ss</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>yyyy-MM-ddTHH:mm:ss</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToIso8601SecondFormat(this DateTimeOffset value) =>
        value.ToString(_iso8601SecondFormat, _invDtf);

    /// <summary>
    /// Formats as ISO-8601 without an offset and with millisecond precision: <c>yyyy-MM-ddTHH:mm:ss.fff</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>yyyy-MM-ddTHH:mm:ss.fff</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToIso8601MillisFormat(this DateTimeOffset value) =>
        value.ToString(_iso8601MillisFormat, _invDtf);

    /// <summary>
    /// Converts to UTC and formats as <c>MM/dd/yyyy h:mm:ss tt UTC</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A UTC string in the form <c>MM/dd/yyyy h:mm:ss tt UTC</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToUtcDateTimeFormat(this DateTimeOffset value) =>
        value.UtcDateTime.ToString(_utcDateTimeFormat, _invDtf);

    /// <summary>
    /// Converts to UTC and formats as <c>MM/dd/yyyy h:mm tt UTC</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A UTC string in the form <c>MM/dd/yyyy h:mm tt UTC</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToUtcDateHourMinuteFormat(this DateTimeOffset value) =>
        value.UtcDateTime.ToString(_utcDateHourMinuteFormat, _invDtf);

    /// <summary>
    /// Formats as a file-name friendly string: <c>yyyy-MM-dd--HH-mm-ss</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A file-name friendly timestamp string.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToFileName(this DateTimeOffset value) =>
        value.ToString(_fileNameFormat, _invDtf);

    /// <summary>
    /// Formats as a file-name friendly string with millisecond precision: <c>yyyy-MM-dd--HH-mm-ss-fff</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A file-name friendly timestamp string with millisecond precision.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToFileNameMillis(this DateTimeOffset value) =>
        value.ToString(_fileNameMillisFormat, _invDtf);

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>hh tt {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>hh tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzHourFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_hour12PaddedFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>h:mm tt {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>h:mm tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzHourMinuteFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_hour12MinuteFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>h:mm:ss tt {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>h:mm:ss tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzHourMinuteSecondFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_hour12MinuteSecondFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>HH {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>HH {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTz24HourFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_hour24Format, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>HH:mm {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>HH:mm {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTz24HourMinuteFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_hour24MinuteFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>HH:mm:ss {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>HH:mm:ss {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTz24HourMinuteSecondFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_hour24MinuteSecondFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_dateSlashFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM-dd-yyyy</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM-dd-yyyy</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateDashFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_dateDashFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>yyyy-MM-dd</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>yyyy-MM-dd</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzYearMonthDayFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_yearMonthDayFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy h tt {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateHourFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_dateHour12Format, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy h:mm tt {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h:mm tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateHourMinuteFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_dateHour12MinuteFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy h:mm:ss tt {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h:mm:ss tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateHourMinuteSecondFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_dateHour12MinuteSecondFormat, _invDtf), " ",
            tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy HH:mm {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy HH:mm {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDate24HourMinuteFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_dateHour24MinuteFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy HH:mm:ss {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy HH:mm:ss {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDate24HourMinuteSecondFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_dateHour24MinuteSecondFormat, _invDtf), " ",
            tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as a sortable value with minute precision:
    /// <c>yyyy-MM-dd HH:mm {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>yyyy-MM-dd HH:mm {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzSortableMinuteFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_sortableMinuteFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as a sortable value with second precision:
    /// <c>yyyy-MM-dd HH:mm:ss {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>yyyy-MM-dd HH:mm:ss {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzSortableSecondFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return string.Concat(converted.ToString(_sortableSecondFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MMM d, yyyy</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MMM d, yyyy</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzShortMonthDayYearFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_shortMonthDayYearFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MMMM d, yyyy</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MMMM d, yyyy</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzLongMonthDayYearFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_longMonthDayYearFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>dddd, MMMM d, yyyy</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>dddd, MMMM d, yyyy</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzLongWeekdayMonthDayYearFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_longWeekdayMonthDayYearFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as ISO-8601 without an offset and with second precision:
    /// <c>yyyy-MM-ddTHH:mm:ss</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>yyyy-MM-ddTHH:mm:ss</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzIso8601SecondFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_iso8601SecondFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as ISO-8601 without an offset and with millisecond precision:
    /// <c>yyyy-MM-ddTHH:mm:ss.fff</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>yyyy-MM-ddTHH:mm:ss.fff</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzIso8601MillisFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_iso8601MillisFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>yyyy-MM-ddTHH:mm:ss.fffffff</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>yyyy-MM-ddTHH:mm:ss.fffffff</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzPreciseFormat(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToString(_preciseFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as a file-name friendly string:
    /// <c>yyyy-MM-dd--HH-mm-ss</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A file-name friendly timestamp string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzFileName(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToFileName();
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as a file-name friendly string with millisecond precision:
    /// <c>yyyy-MM-dd--HH-mm-ss-fff</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A file-name friendly timestamp string with millisecond precision.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzFileNameMillis(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(value, tz);
        return converted.ToFileNameMillis();
    }

    /// <summary>
    /// Does not convert. Formats as <c>MM/dd/yyyy h:mm:ss tt {tz}</c> using the abbreviation from <paramref name="tz"/>.
    /// </summary>
    /// <remarks>
    /// This method assumes <paramref name="value"/> already represents the desired local time.
    /// The supplied <paramref name="tz"/> is used only for its abbreviation.
    /// </remarks>
    /// <param name="value">The value to format. No conversion is performed.</param>
    /// <param name="tz">The time zone used only for its abbreviation.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h:mm:ss tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDateTimeFormatAsTz(this DateTimeOffset value, TimeZoneInfo tz)
    {
        ArgumentNullException.ThrowIfNull(tz);

        return string.Concat(value.ToString(_dateHour12MinuteSecondFormat, _invDtf), " ", tz.ToSimpleAbbreviation());
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy h:mm:ss tt {tz}</c>.
    /// </summary>
    /// <param name="value">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h:mm:ss tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateTimeFormat(this DateTimeOffset value, TimeZoneInfo tz) =>
        value.ToTzDateHourMinuteSecondFormat(tz);

    /// <summary>
    /// Formats as <c>MM-dd-yyyy</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A string in the form <c>MM-dd-yyyy</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToMonthFirstDateFormat(this DateTimeOffset value) =>
        value.ToDateDashFormat();
}