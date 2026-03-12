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
/// Uses <see cref="CultureInfo.InvariantCulture"/> and precompiled format strings for stable, allocation-friendly output.
/// Where a time zone abbreviation is appended, the output is built with <see cref="string.Concat(string, string, string)"/>
/// to avoid intermediate string allocations.
/// </remarks>
public static class DateTimeOffsetExtensionFormat
{
    private static readonly CultureInfo _inv = CultureInfo.InvariantCulture;

    // Precompile common formats (reduces repeated parsing work inside DateTime formatting).
    private static readonly DateTimeFormatInfo _invDtf = DateTimeFormatInfo.InvariantInfo;

    private const string _hourFormat = "hh tt";
    private const string _preciseFormat = "yyyy-MM-ddTHH:mm:ss.fffffff";
    private const string _monthFirstDateFormat = "MM-dd-yyyy";
    private const string _preciseUtcFormat = "yyyy-MM-ddTHH:mm:ss.fffffff'Z'";
    private const string _iso8601UtcMillisFormat = "yyyy-MM-ddTHH:mm:ss.fff'Z'";
    private const string _tzDateTimeFormat = "MM/dd/yyyy hh:mm:ss tt";
    private const string _tzDateFormat = "MM/dd/yyyy";
    private const string _tzDateHourFormat = "MM/dd/yyyy h tt";
    private const string _utcDateTimeFormat = "MM/dd/yyyy hh:mm:ss tt 'UTC'";
    private const string _fileNameFormat = "yyyy-MM-dd--HH-mm-ss";
    private const string _shortMonthDayYear = "MMM dd, yyyy";
    private const string _longMonthDayYear = "MMMM d, yyyy";

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>hh tt {tz}</c>.
    /// </summary>
    /// <param name="dateTimeOffset">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>hh tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHourFormat(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        string abbr = tz.ToSimpleAbbreviation();

        // Avoid intermediate "+" concatenation string.
        return string.Concat(converted.ToString(_hourFormat, _invDtf), " ", abbr);
    }

    /// <summary>
    /// Formats as <c>yyyy-MM-ddTHH:mm:ss.fffffff</c> using invariant culture.
    /// Intended for admin/debug use rather than UI display.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToPreciseFormat(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.ToString(_preciseFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MM-dd-yyyy</c> using invariant culture.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToMonthFirstDateFormat(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.ToString(_monthFirstDateFormat, _invDtf);

    /// <summary>
    /// Converts to UTC and formats as <c>yyyy-MM-ddTHH:mm:ss.fffffffZ</c>.
    /// Intended for admin/debug use rather than UI display.
    /// </summary>
    /// <param name="dateTimeOffset">The value to format (any offset; it will be converted to UTC).</param>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToPreciseUtcFormat(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.UtcDateTime.ToString(_preciseUtcFormat, _invDtf);

    /// <summary>
    /// Converts to UTC and formats as <c>yyyy-MM-ddTHH:mm:ss.fffZ</c> (UTC with millisecond precision).
    /// Useful for lexicographic string comparisons/queries (e.g., Cosmos).
    /// </summary>
    /// <param name="dateTimeOffset">The value to format (any offset; it will be converted to UTC).</param>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToIso8601(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.UtcDateTime.ToString(_iso8601UtcMillisFormat, _invDtf);

    /// <summary>
    /// Alias for <see cref="ToIso8601(DateTimeOffset)"/>.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToWebString(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.ToIso8601();

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy hh:mm:ss tt {tz}</c>.
    /// </summary>
    /// <param name="dateTimeOffset">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy hh:mm:ss tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateTimeFormat(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return converted.ToDateTimeFormatAsTz(tz);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy</c>.
    /// </summary>
    /// <param name="dateTimeOffset">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateFormat(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return converted.ToString(_tzDateFormat, _invDtf);
    }

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as <c>MM/dd/yyyy h tt {tz}</c>.
    /// </summary>
    /// <param name="dateTimeOffset">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy h tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzDateHourFormat(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        string abbr = tz.ToSimpleAbbreviation();

        return string.Concat(converted.ToString(_tzDateHourFormat, _invDtf), " ", abbr);
    }

    /// <summary>
    /// Does not convert; formats as <c>MM/dd/yyyy hh:mm:ss tt {tz}</c> using the provided <paramref name="tz"/> abbreviation.
    /// </summary>
    /// <remarks>
    /// This method assumes <paramref name="dateTimeOffset"/> already represents the desired local time; it only appends the
    /// abbreviation derived from <paramref name="tz"/>.
    /// </remarks>
    /// <param name="dateTimeOffset">The value to format (no conversion performed).</param>
    /// <param name="tz">The time zone used only for its abbreviation.</param>
    /// <returns>A string in the form <c>MM/dd/yyyy hh:mm:ss tt {tz}</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDateTimeFormatAsTz(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        string abbr = tz.ToSimpleAbbreviation();
        return string.Concat(dateTimeOffset.ToString(_tzDateTimeFormat, _invDtf), " ", abbr);
    }

    /// <summary>
    /// Converts to UTC and formats as <c>MM/dd/yyyy hh:mm:ss tt UTC</c>.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToUtcDateTimeFormat(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.UtcDateTime.ToString(_utcDateTimeFormat, _invDtf);

    /// <summary>
    /// Converts to <paramref name="tz"/> and formats as a file-name friendly string: <c>yyyy-MM-dd--HH-mm-ss</c>.
    /// </summary>
    /// <param name="dateTimeOffset">The instant to format.</param>
    /// <param name="tz">The time zone to convert into.</param>
    /// <returns>A file-name friendly time stamp string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTzFileName(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
        return converted.ToFileName();
    }

    /// <summary>
    /// Formats as a file-name friendly string: <c>yyyy-MM-dd--HH-mm-ss</c>.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToFileName(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.ToString(_fileNameFormat, _invDtf);

    /// <summary>
    /// Formats as <c>MMM dd, yyyy</c> using invariant culture.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToShortMonthDayYearString(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.ToString(_shortMonthDayYear, _invDtf);

    /// <summary>
    /// Formats as <c>MMMM d, yyyy</c> using invariant culture.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToLongMonthDayYearString(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.ToString(_longMonthDayYear, _invDtf);
}