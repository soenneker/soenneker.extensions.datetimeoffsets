using Soenneker.Enums.UnitOfTime;
using Soenneker.Extensions.CultureInfos;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.DateTimeOffsets;

/// <summary>
/// Extension methods for <see cref="DateTimeOffset"/> providing:
/// <list type="bullet">
/// <item><description>Time zone conversion helpers.</description></item>
/// <item><description>Elapsed-time (“age”) calculations in multiple units (including signed calendar units).</description></item>
/// <item><description>Business-day logic with culture-defined weekend rules.</description></item>
/// <item><description>Trimming to unit boundaries and computing end-of-unit instants.</description></item>
/// <item><description>Adding/subtracting in a specified <see cref="UnitOfTime"/> (including fractional values where applicable).</description></item>
/// </list>
/// </summary>
public static class DateTimeOffsetExtension
{
    private const long _ticksPerMicrosecond = 10; // 1 tick = 100ns, so 1µs = 10 ticks
    private const long _ticksPerMillisecond = TimeSpan.TicksPerMillisecond; // 10,000
    private const double _ticksPerNanosecond = 0.01d; // 1ns = 0.01 ticks (cannot be represented; we truncate to ticks)

    private const long _ticksPerSecond = TimeSpan.TicksPerSecond;
    private const long _ticksPerMinute = TimeSpan.TicksPerMinute;
    private const long _ticksPerHour = TimeSpan.TicksPerHour;
    private const long _ticksPerDay = TimeSpan.TicksPerDay;

    /// <summary>
    /// Converts the value to a UTC <see cref="DateTime"/> representing the same instant.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>A UTC <see cref="DateTime"/> representing the same instant.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToUtcDateTime(this DateTimeOffset dateTimeOffset) => dateTimeOffset.UtcDateTime;

    /// <summary>
    /// Converts the value to the specified time zone while preserving the instant in time.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <param name="tz">The target time zone.</param>
    /// <returns>A <see cref="DateTimeOffset"/> representing the same instant in <paramref name="tz"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ToTz(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        return TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
    }

    /// <summary>
    /// Converts the value to UTC (<c>+00:00</c>) while preserving the instant in time.
    /// </summary>
    /// <param name="dateTimeOffset">The value to convert.</param>
    /// <returns>The same instant with offset <c>+00:00</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ToUtc(this DateTimeOffset dateTimeOffset) => dateTimeOffset.ToOffset(TimeSpan.Zero);

    /// <summary>
    /// Calculates the elapsed time from <paramref name="fromDateTimeOffset"/> until <paramref name="utcNow"/>
    /// expressed in <paramref name="unitOfTime"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For tick-through-week units, this uses <see cref="TimeSpan"/> totals (signed).
    /// </para>
    /// <para>
    /// For calendar units (month/quarter/year), this returns a calendar-exact, signed value:
    /// whole intervals plus a fractional remainder based on the actual length of the next interval
    /// (e.g., month length varies). This preserves sign to match the behavior of <see cref="TimeSpan"/>-based units.
    /// </para>
    /// </remarks>
    /// <param name="fromDateTimeOffset">The starting instant.</param>
    /// <param name="unitOfTime">The unit used to express elapsed time.</param>
    /// <param name="utcNow">
    /// The ending instant in UTC. If <see langword="null"/>, <see cref="DateTimeOffset.UtcNow"/> is used.
    /// </param>
    /// <returns>The elapsed time expressed in <paramref name="unitOfTime"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="unitOfTime"/> is not supported.</exception>
    [Pure]
    public static double ToAge(this DateTimeOffset fromDateTimeOffset, UnitOfTime unitOfTime, DateTimeOffset? utcNow = null)
    {
        utcNow ??= DateTimeOffset.UtcNow;
        DateTimeOffset to = utcNow.Value;

        TimeSpan timeSpan = to - fromDateTimeOffset;

        return unitOfTime.Value switch
        {
            UnitOfTime.TickValue => timeSpan.Ticks,
            UnitOfTime.MicrosecondValue => timeSpan.Ticks / (double)_ticksPerMicrosecond,
            UnitOfTime.MillisecondValue => timeSpan.TotalMilliseconds,
            UnitOfTime.SecondValue => timeSpan.TotalSeconds,
            UnitOfTime.MinuteValue => timeSpan.TotalMinutes,
            UnitOfTime.HourValue => timeSpan.TotalHours,
            UnitOfTime.DayValue => timeSpan.TotalDays,
            UnitOfTime.WeekValue => timeSpan.TotalDays / 7d,

            UnitOfTime.MonthValue => MonthsBetween(fromDateTimeOffset, to),
            UnitOfTime.QuarterValue => QuartersBetween(fromDateTimeOffset, to),
            UnitOfTime.YearValue => YearsBetween(fromDateTimeOffset, to),

            _ => throw new NotSupportedException("UnitOfTime is not supported for this method")
        };
    }

    /// <summary>
    /// Calculates the number of calendar quarters between two instants as a calendar-exact, signed value:
    /// whole quarters plus a fractional remainder based on the length of the next quarter interval.
    /// </summary>
    /// <param name="from">The starting instant.</param>
    /// <param name="to">The ending instant.</param>
    /// <returns>A signed number of quarters between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static double QuartersBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (from == to)
            return 0d;

        int sign = to >= from ? 1 : -1;
        if (sign < 0)
            (from, to) = (to, from);

        int whole = WholeQuartersBetween(from, to);
        DateTimeOffset start = from.AddMonths(whole * 3);
        DateTimeOffset end = start.AddMonths(3);

        if (start == to)
            return sign * whole;

        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return sign * (whole + frac);
    }

    /// <summary>
    /// Calculates the number of calendar years between two instants as a calendar-exact, signed value:
    /// whole years plus a fractional remainder based on the length of the next year interval.
    /// </summary>
    /// <param name="from">The starting instant.</param>
    /// <param name="to">The ending instant.</param>
    /// <returns>A signed number of years between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static double YearsBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (from == to)
            return 0d;

        int sign = to >= from ? 1 : -1;
        if (sign < 0)
            (from, to) = (to, from);

        int whole = WholeYearsBetween(from, to);
        DateTimeOffset start = from.AddYears(whole);
        DateTimeOffset end = start.AddYears(1);

        if (start == to)
            return sign * whole;

        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return sign * (whole + frac);
    }

    /// <summary>
    /// Calculates the number of calendar months between two instants as a calendar-exact, signed value:
    /// whole months plus a fractional remainder based on the length of the next month interval.
    /// </summary>
    /// <param name="from">The starting instant.</param>
    /// <param name="to">The ending instant.</param>
    /// <returns>A signed number of months between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static double MonthsBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (from == to)
            return 0d;

        int sign = to >= from ? 1 : -1;
        if (sign < 0)
            (from, to) = (to, from);

        int whole = WholeMonthsBetween(from, to);
        DateTimeOffset start = from.AddMonths(whole);
        DateTimeOffset end = start.AddMonths(1);

        if (start == to)
            return sign * whole;

        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return sign * (whole + frac);
    }

    /// <summary>
    /// Calculates the whole number of calendar months between two instants.
    /// </summary>
    /// <remarks>
    /// This returns the greatest integer <c>m</c> such that <c>from.AddMonths(m) &lt;= to</c> (after ordering).
    /// </remarks>
    /// <param name="from">The starting instant.</param>
    /// <param name="to">The ending instant.</param>
    /// <returns>The whole number of calendar months between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static int WholeMonthsBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int months = (to.Year - from.Year) * 12 + (to.Month - from.Month);

        DateTimeOffset candidate = from.AddMonths(months);
        if (candidate > to)
            months--;

        return months;
    }

    /// <summary>
    /// Calculates the whole number of calendar years between two instants.
    /// </summary>
    /// <remarks>
    /// This returns the greatest integer <c>y</c> such that <c>from.AddYears(y) &lt;= to</c> (after ordering).
    /// </remarks>
    /// <param name="from">The starting instant.</param>
    /// <param name="to">The ending instant.</param>
    /// <returns>The whole number of calendar years between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static int WholeYearsBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int years = to.Year - from.Year;

        DateTimeOffset candidate = from.AddYears(years);
        if (candidate > to)
            years--;

        return years;
    }

    /// <summary>
    /// Calculates the whole number of calendar quarters between two instants.
    /// </summary>
    /// <remarks>
    /// This returns the greatest integer <c>q</c> such that <c>from.AddMonths(q * 3) &lt;= to</c> (after ordering).
    /// </remarks>
    /// <param name="from">The starting instant.</param>
    /// <param name="to">The ending instant.</param>
    /// <returns>The whole number of calendar quarters between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static int WholeQuartersBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int fromQ = (from.Month - 1) / 3;
        int toQ = (to.Month - 1) / 3;

        int quarters = (to.Year - from.Year) * 4 + (toQ - fromQ);

        DateTimeOffset candidate = from.AddMonths(quarters * 3);
        if (candidate > to)
            quarters--;

        return quarters;
    }

    /// <summary>
    /// Determines whether the instant falls on a business day (i.e., not a weekend day) according to the supplied
    /// time zone and culture.
    /// </summary>
    /// <remarks>
    /// The day-of-week is evaluated in the local calendar day of <paramref name="zone"/> (if provided); otherwise the
    /// day-of-week is taken from the input value as-is.
    /// </remarks>
    /// <param name="dateTimeOffset">The instant to evaluate.</param>
    /// <param name="zone">
    /// The time zone whose local calendar should be used to determine the day-of-week.
    /// If <see langword="null"/>, no conversion is performed.
    /// </param>
    /// <param name="culture">
    /// The culture used to determine weekend days. If <see langword="null"/>, <see cref="CultureInfo.CurrentCulture"/> is used.
    /// </param>
    /// <returns><see langword="true"/> if the local date is a business day; otherwise <see langword="false"/>.</returns>
    [Pure]
    public static bool IsBusinessDay(this DateTimeOffset dateTimeOffset, TimeZoneInfo? zone = null, CultureInfo? culture = null)
    {
        // Pull weekend definition once (culture extension may allocate; do not call per-loop in AddBusinessDays).
        IReadOnlySet<DayOfWeek> weekendDays = (culture ?? CultureInfo.CurrentCulture).GetWeekendDays();

        DayOfWeek day = zone is null
            ? dateTimeOffset.DayOfWeek
            : TimeZoneInfo.ConvertTime(dateTimeOffset, zone).DayOfWeek;

        return !weekendDays.Contains(day);
    }

    /// <summary>
    /// Adds (or subtracts) business days, skipping weekend days according to the supplied time zone and culture.
    /// </summary>
    /// <remarks>
    /// This method advances by one day at a time and checks whether each candidate day is a business day.
    /// Weekend definitions can vary by culture, so this intentionally uses the culture-provided weekend set rather
    /// than hard-coding Saturday/Sunday.
    /// </remarks>
    /// <param name="dateTimeOffset">The starting instant.</param>
    /// <param name="businessDays">Positive to add; negative to subtract; zero returns the original value.</param>
    /// <param name="zone">
    /// The time zone whose local calendar should be used when deciding if a day is a weekend.
    /// If <see langword="null"/>, no conversion is performed.
    /// </param>
    /// <param name="culture">
    /// The culture used to determine weekend days. If <see langword="null"/>, <see cref="CultureInfo.CurrentCulture"/> is used.
    /// </param>
    /// <returns>The resulting <see cref="DateTimeOffset"/> after adding business days.</returns>
    [Pure]
    public static DateTimeOffset AddBusinessDays(this DateTimeOffset dateTimeOffset, int businessDays, TimeZoneInfo? zone = null, CultureInfo? culture = null)
    {
        if (businessDays == 0)
            return dateTimeOffset;

        IReadOnlySet<DayOfWeek> weekendDays = (culture ?? CultureInfo.CurrentCulture).GetWeekendDays();

        int direction = businessDays > 0 ? 1 : -1;
        int remaining = Math.Abs(businessDays);

        DateTimeOffset current = dateTimeOffset;

        while (remaining > 0)
        {
            current = current.AddDays(direction);

            DayOfWeek day = zone is null
                ? current.DayOfWeek
                : TimeZoneInfo.ConvertTime(current, zone).DayOfWeek;

            if (!weekendDays.Contains(day))
                remaining--;
        }

        return current;
    }

    /// <summary>
    /// Determines whether <paramref name="value"/> is between <paramref name="start"/> and <paramref name="end"/>.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <param name="inclusive">
    /// If <see langword="true"/>, bounds are inclusive; otherwise strict.
    /// </param>
    /// <returns><see langword="true"/> if the value lies within the range; otherwise <see langword="false"/>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(this DateTimeOffset value, DateTimeOffset start, DateTimeOffset end, bool inclusive = true)
    {
        if (start > end)
            (start, end) = (end, start);

        return inclusive ? value >= start && value <= end : value > start && value < end;
    }

    /// <summary>
    /// Trims the value down to the start of the specified unit of time.
    /// </summary>
    /// <remarks>
    /// Trimming is performed in the value’s local (offset) representation; the existing <see cref="DateTimeOffset.Offset"/>
    /// is preserved.
    /// </remarks>
    /// <param name="dateTimeOffset">The value to trim.</param>
    /// <param name="unitOfTime">The unit boundary to trim to.</param>
    /// <returns>A value representing the start of the requested unit in the original offset.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unitOfTime"/> is unsupported.</exception>
    [Pure]
    public static DateTimeOffset Trim(this DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        TimeSpan offset = dateTimeOffset.Offset;

        // DateTimeOffset.Ticks are the ticks of the "clock time" component, so modulo trimming works in the local-offset view.
        long ticks = dateTimeOffset.Ticks;

        return unitOfTime.Value switch
        {
            UnitOfTime.MicrosecondValue => new DateTimeOffset(ticks - (ticks % _ticksPerMicrosecond), offset),
            UnitOfTime.MillisecondValue => new DateTimeOffset(ticks - (ticks % _ticksPerMillisecond), offset),
            UnitOfTime.SecondValue => new DateTimeOffset(ticks - (ticks % _ticksPerSecond), offset),
            UnitOfTime.MinuteValue => new DateTimeOffset(ticks - (ticks % _ticksPerMinute), offset),
            UnitOfTime.HourValue => new DateTimeOffset(ticks - (ticks % _ticksPerHour), offset),
            UnitOfTime.DayValue => new DateTimeOffset(ticks - (ticks % _ticksPerDay), offset),

            // Monday-based week start (not ISO week-numbering).
            UnitOfTime.WeekValue => new DateTimeOffset(ticks - (ticks % _ticksPerDay), offset).AddDays(
                -((7 + (int)dateTimeOffset.DayOfWeek - (int)DayOfWeek.Monday) % 7)),

            UnitOfTime.MonthValue => new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, 1, 0, 0, 0, 0, offset),
            UnitOfTime.QuarterValue => new DateTimeOffset(dateTimeOffset.Year, ((dateTimeOffset.Month - 1) / 3 * 3) + 1, 1, 0, 0, 0, 0, offset),
            UnitOfTime.YearValue => new DateTimeOffset(dateTimeOffset.Year, 1, 1, 0, 0, 0, 0, offset),
            UnitOfTime.DecadeValue => new DateTimeOffset(dateTimeOffset.Year - (dateTimeOffset.Year % 10), 1, 1, 0, 0, 0, 0, offset),

            _ => throw new ArgumentOutOfRangeException(nameof(unitOfTime), $"Unsupported UnitOfTime: {unitOfTime.Name}")
        };
    }

    /// <summary>
    /// Trims the value to the end of the specified unit of time (start of the next unit minus one tick).
    /// </summary>
    /// <remarks>
    /// The existing <see cref="DateTimeOffset.Offset"/> is preserved; trimming is performed in the value's local offset.
    /// </remarks>
    /// <param name="dateTimeOffset">The value to trim.</param>
    /// <param name="unitOfTime">The unit boundary to trim to.</param>
    /// <returns>A value representing the end of the requested unit in the original offset.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unitOfTime"/> is unsupported.</exception>
    [Pure]
    public static DateTimeOffset TrimEnd(this DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        DateTimeOffset startOfPeriod = dateTimeOffset.Trim(unitOfTime);

        DateTimeOffset startOfNext = unitOfTime.Value switch
        {
            UnitOfTime.MicrosecondValue => startOfPeriod.AddTicks(_ticksPerMicrosecond),
            UnitOfTime.MillisecondValue => startOfPeriod.AddTicks(_ticksPerMillisecond),
            UnitOfTime.SecondValue => startOfPeriod.AddSeconds(1),
            UnitOfTime.MinuteValue => startOfPeriod.AddMinutes(1),
            UnitOfTime.HourValue => startOfPeriod.AddHours(1),
            UnitOfTime.DayValue => startOfPeriod.AddDays(1),
            UnitOfTime.WeekValue => startOfPeriod.AddDays(7),
            UnitOfTime.MonthValue => startOfPeriod.AddMonths(1),
            UnitOfTime.QuarterValue => startOfPeriod.AddMonths(3),
            UnitOfTime.YearValue => startOfPeriod.AddYears(1),
            UnitOfTime.DecadeValue => startOfPeriod.AddYears(10),
            _ => throw new ArgumentOutOfRangeException(nameof(unitOfTime), $"Unsupported UnitOfTime: {unitOfTime.Name}")
        };

        return startOfNext.AddTicks(-1);
    }

    /// <summary>
    /// Adds a value expressed in the specified <see cref="UnitOfTime"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For <see cref="UnitOfTime.NanosecondValue"/>, <see cref="DateTimeOffset"/> has 100ns resolution (ticks),
    /// so nanoseconds are truncated to whole ticks.
    /// </para>
    /// <para>
    /// For month/quarter/year/decade, fractional parts are applied as a fraction of the resulting month/year length
    /// after applying the whole-month/year portion. This makes the operation deterministic and sign-consistent.
    /// </para>
    /// </remarks>
    /// <param name="dateTimeOffset">The starting value.</param>
    /// <param name="value">The amount to add (may be fractional depending on <paramref name="unitOfTime"/>).</param>
    /// <param name="unitOfTime">The unit of time.</param>
    /// <returns>A new <see cref="DateTimeOffset"/> with the adjustment applied.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unitOfTime"/> is unsupported.</exception>
    [Pure]
    public static DateTimeOffset Add(this DateTimeOffset dateTimeOffset, double value, UnitOfTime unitOfTime)
    {
        switch (unitOfTime.Value)
        {
            case UnitOfTime.TickValue:
                return dateTimeOffset.AddTicks((long)value);

            case UnitOfTime.NanosecondValue:
                return dateTimeOffset.AddTicks((long)(value * _ticksPerNanosecond));

            case UnitOfTime.MicrosecondValue:
                return dateTimeOffset.AddTicks((long)(value * _ticksPerMicrosecond));

            case UnitOfTime.MillisecondValue:
                return dateTimeOffset.AddMilliseconds(value);

            case UnitOfTime.SecondValue:
                return dateTimeOffset.AddSeconds(value);

            case UnitOfTime.MinuteValue:
                return dateTimeOffset.AddMinutes(value);

            case UnitOfTime.HourValue:
                return dateTimeOffset.AddHours(value);

            case UnitOfTime.DayValue:
                return dateTimeOffset.AddDays(value);

            case UnitOfTime.WeekValue:
                return dateTimeOffset.AddDays(value * 7d);

            case UnitOfTime.MonthValue:
                return AddMonthsWithFraction(dateTimeOffset, value);

            case UnitOfTime.QuarterValue:
                // 1 quarter = 3 months (preserve fractional part consistently; no truncation).
                return AddMonthsWithFraction(dateTimeOffset, value * 3d);

            case UnitOfTime.YearValue:
                return AddYearsWithFraction(dateTimeOffset, value);

            case UnitOfTime.DecadeValue:
                // 1 decade = 10 years (preserve fractional part consistently; no truncation).
                return AddYearsWithFraction(dateTimeOffset, value * 10d);

            default:
                throw new ArgumentOutOfRangeException(nameof(unitOfTime), $"Unsupported UnitOfTime: {unitOfTime.Name}");
        }
    }

    /// <summary>
    /// Subtracts a value expressed in the specified <see cref="UnitOfTime"/>.
    /// </summary>
    /// <param name="dateTimeOffset">The starting value.</param>
    /// <param name="value">The amount to subtract (may be fractional depending on <paramref name="unitOfTime"/>).</param>
    /// <param name="unitOfTime">The unit of time.</param>
    /// <returns>A new <see cref="DateTimeOffset"/> with the adjustment applied.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset Subtract(this DateTimeOffset dateTimeOffset, double value, UnitOfTime unitOfTime) =>
        dateTimeOffset.Add(-value, unitOfTime);

    /// <summary>
    /// Alias for <see cref="Trim(DateTimeOffset, UnitOfTime)"/>.
    /// </summary>
    /// <param name="dateTimeOffset">The value to trim.</param>
    /// <param name="unitOfTime">The unit boundary to trim to.</param>
    /// <returns>The start of the specified unit.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ToStartOf(this DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime) =>
        dateTimeOffset.Trim(unitOfTime);

    /// <summary>
    /// Alias for <see cref="TrimEnd(DateTimeOffset, UnitOfTime)"/>.
    /// </summary>
    /// <param name="dateTimeOffset">The value to trim.</param>
    /// <param name="unitOfTime">The unit boundary to trim to.</param>
    /// <returns>The end of the specified unit.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ToEndOf(this DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime) =>
        dateTimeOffset.TrimEnd(unitOfTime);

    /// <summary>
    /// Converts the value to an integer in the form <c>yyyyMMdd</c> using the value’s local date components.
    /// </summary>
    /// <param name="dateTimeOffset">The value to convert.</param>
    /// <returns>An integer representing the date portion in <c>yyyyMMdd</c> format.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToDateAsInteger(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.Year * 10000 + dateTimeOffset.Month * 100 + dateTimeOffset.Day;

    /// <summary>
    /// Gets the UTC offset for <paramref name="utcNow"/> in <paramref name="tz"/> (including DST at that instant).
    /// </summary>
    /// <remarks>
    /// The supplied <paramref name="utcNow"/> is treated as an instant; its UTC representation is used for offset lookup.
    /// </remarks>
    /// <param name="utcNow">A UTC instant used to evaluate the time zone’s offset at that moment.</param>
    /// <param name="tz">The time zone.</param>
    /// <returns>The UTC offset in <paramref name="tz"/> at <paramref name="utcNow"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ToTzOffset(this DateTimeOffset utcNow, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        return tz.GetUtcOffset(utcNow.UtcDateTime);
    }

    /// <summary>
    /// Gets the UTC offset (in hours) for <paramref name="utcNow"/> in <paramref name="tz"/> (including DST at that instant).
    /// </summary>
    /// <param name="utcNow">A UTC instant used to evaluate the time zone’s offset at that moment.</param>
    /// <param name="tz">The time zone.</param>
    /// <returns>The UTC offset in hours (may be fractional for non-whole-hour offsets).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToTzOffsetHours(this DateTimeOffset utcNow, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        return tz.GetUtcOffset(utcNow.UtcDateTime).TotalHours;
    }

    /// <summary>
    /// Converts a local hour (0–23) in a given time zone, on the local date corresponding to <paramref name="utcNow"/>,
    /// into the corresponding UTC hour (0–23).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The local date is determined by converting <paramref name="utcNow"/> into <paramref name="tz"/> and using that local date.
    /// </para>
    /// <para>
    /// If the requested local time falls into a DST gap (invalid local time), this advances minute-by-minute to the next valid time
    /// with a safety cap to avoid pathological looping.
    /// </para>
    /// <para>
    /// If the requested local time is ambiguous (DST fold), this chooses the earlier UTC instant (which corresponds to the larger offset).
    /// </para>
    /// <para>
    /// Note: This returns only the UTC hour-of-day. If you need the full UTC instant (including day rollover), prefer returning
    /// a <see cref="DateTimeOffset"/> from a dedicated helper.
    /// </para>
    /// </remarks>
    /// <param name="utcNow">A UTC instant used to choose the local date in <paramref name="tz"/>.</param>
    /// <param name="tzHour">The hour in the target time zone (0–23).</param>
    /// <param name="tz">The time zone whose local hour should be converted to UTC.</param>
    /// <returns>The UTC hour (0–23) corresponding to <paramref name="tzHour"/> on that local date.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tz"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tzHour"/> is not in the range 0–23.</exception>
    [Pure]
    public static int ToUtcHoursFromTz(this DateTimeOffset utcNow, int tzHour, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        if ((uint)tzHour > 23)
            throw new ArgumentOutOfRangeException(nameof(tzHour), "Hour must be in range [0, 23].");

        // Anchor on the local DATE for the provided UTC instant.
        DateTimeOffset localNow = TimeZoneInfo.ConvertTime(utcNow, tz);

        // Construct the local wall-clock time (Kind must be Unspecified for TZ conversion APIs).
        DateTime local = new(localNow.Year, localNow.Month, localNow.Day, tzHour, 0, 0, DateTimeKind.Unspecified);

        // If invalid (spring-forward gap), advance until valid (cap to avoid pathological loops).
        if (tz.IsInvalidTime(local))
        {
            const int maxMinutes = 180; // DST gaps are typically <= 120 minutes; cap provides safety.
            int i = 0;

            do
            {
                local = local.AddMinutes(1);
                i++;
                if (i > maxMinutes)
                    break;
            }
            while (tz.IsInvalidTime(local));
        }

        // If ambiguous (fall-back fold), choose the earlier UTC instant.
        // Earlier UTC instant corresponds to the *larger* offset (local - offset = utc).
        if (tz.IsAmbiguousTime(local))
        {
            TimeSpan[] offsets = tz.GetAmbiguousTimeOffsets(local);
            TimeSpan chosenOffset = offsets[0] >= offsets[1] ? offsets[0] : offsets[1];

            DateTime utcAmbiguous = DateTime.SpecifyKind(local - chosenOffset, DateTimeKind.Utc);
            return utcAmbiguous.Hour;
        }

        DateTime utc = TimeZoneInfo.ConvertTimeToUtc(local, tz);
        return utc.Hour;
    }

    /// <summary>
    /// Produces a time window that ends <paramref name="delay"/> before <paramref name="utcNow"/> and spans
    /// <paramref name="subtraction"/> before that end.
    /// </summary>
    /// <param name="utcNow">The reference instant (commonly “now”).</param>
    /// <param name="delay">How far back from <paramref name="utcNow"/> the window end should be.</param>
    /// <param name="subtraction">How large the window should be (subtracted from the window end).</param>
    /// <param name="unitOfTime">The unit used for <paramref name="delay"/> and <paramref name="subtraction"/>.</param>
    /// <returns>
    /// A tuple where <c>endAt</c> is <paramref name="delay"/> before <paramref name="utcNow"/>, and
    /// <c>startAt</c> is <paramref name="subtraction"/> before <c>endAt</c>.
    /// </returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (DateTimeOffset startAt, DateTimeOffset endAt) ToWindow(this DateTimeOffset utcNow, int delay, int subtraction, UnitOfTime unitOfTime)
    {
        DateTimeOffset endAt = utcNow.Subtract(delay, unitOfTime);
        DateTimeOffset startAt = endAt.Subtract(subtraction, unitOfTime);
        return (startAt, endAt);
    }

    /// <summary>
    /// Converts the value to a <see cref="DateOnly"/> by stripping the time component using the value’s local date.
    /// </summary>
    /// <param name="dateTimeOffset">The value to convert.</param>
    /// <returns>The <see cref="DateOnly"/> corresponding to the value’s local calendar date.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateOnly ToDateOnly(this DateTimeOffset dateTimeOffset) =>
        DateOnly.FromDateTime(dateTimeOffset.Date);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTimeOffset AddMonthsWithFraction(DateTimeOffset dto, double months)
    {
        if (months == 0d)
            return dto;

        // Use floor so the fractional component is always in [0, 1) and sign behaves consistently.
        int whole = (int)Math.Floor(months);
        double frac = months - whole;

        dto = dto.AddMonths(whole);

        if (frac == 0d)
            return dto;

        int daysInMonth = DateTime.DaysInMonth(dto.Year, dto.Month);
        return dto.AddDays(frac * daysInMonth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateTimeOffset AddYearsWithFraction(DateTimeOffset dto, double years)
    {
        if (years == 0d)
            return dto;

        int whole = (int)Math.Floor(years);
        double frac = years - whole;

        dto = dto.AddYears(whole);

        if (frac == 0d)
            return dto;

        int daysInYear = DateTime.IsLeapYear(dto.Year) ? 366 : 365;
        return dto.AddDays(frac * daysInYear);
    }
}