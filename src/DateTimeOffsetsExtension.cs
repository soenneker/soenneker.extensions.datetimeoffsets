using Soenneker.Enums.UnitOfTime;
using Soenneker.Extensions.CultureInfos;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.DateTimeOffsets;

/// <summary>
/// A collection of helpful <see cref="DateTimeOffset"/> extension methods.
/// </summary>
public static class DateTimeOffsetExtension
{
    private const long _ticksPerMicrosecond = 10; // 1 tick = 100ns, so 1µs = 10 ticks
    private const long _ticksPerMillisecond = TimeSpan.TicksPerMillisecond; // 10,000
    private const double _ticksPerNanosecond = 0.01d; // 1ns = 0.01 ticks (cannot be represented; we truncate to ticks)

    /// <summary>
    /// Converts the value to a UTC <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>A UTC <see cref="DateTime"/> representing the same instant.</returns>
    [Pure]
    public static DateTime ToUtcDateTime(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime;
    }

    /// <summary>
    /// Converts the value to the specified time zone.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <param name="tz">The target <see cref="TimeZoneInfo"/>.</param>
    /// <returns>A <see cref="DateTimeOffset"/> representing the same instant in the specified time zone.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tz"/> is <see langword="null"/>.</exception>
    [Pure]
    public static DateTimeOffset ToTz(this DateTimeOffset dateTimeOffset, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        return TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
    }

    /// <summary>
    /// Converts the value to UTC (offset <c>+00:00</c>).
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>A <see cref="DateTimeOffset"/> representing the same instant with offset <c>+00:00</c>.</returns>
    [Pure]
    public static DateTimeOffset ToUtc(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToOffset(TimeSpan.Zero);
    }

    /// <summary>
    /// Calculates the elapsed time ("age") from <paramref name="fromDateTimeOffset"/> until <paramref name="utcNow"/>.
    /// </summary>
    /// <param name="fromDateTimeOffset">The starting instant.</param>
    /// <param name="unitOfTime">The unit to return the elapsed time in.</param>
    /// <param name="utcNow">
    /// The current instant in UTC. If <see langword="null"/>, <see cref="DateTimeOffset.UtcNow"/> is used.
    /// </param>
    /// <returns>The elapsed time expressed in <paramref name="unitOfTime"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="unitOfTime"/> is not supported.</exception>
    [Pure]
    public static double ToAge(this DateTimeOffset fromDateTimeOffset, UnitOfTime unitOfTime, DateTimeOffset? utcNow = null)
    {
        utcNow ??= DateTimeOffset.UtcNow;
        TimeSpan timeSpan = utcNow.Value - fromDateTimeOffset;

        return unitOfTime.Value switch
        {
            UnitOfTime.TickValue => timeSpan.Ticks,
            UnitOfTime.MicrosecondValue => timeSpan.Ticks / (double)_ticksPerMicrosecond,
            UnitOfTime.MillisecondValue => timeSpan.TotalMilliseconds,
            UnitOfTime.SecondValue => timeSpan.TotalSeconds,
            UnitOfTime.MinuteValue => timeSpan.TotalMinutes,
            UnitOfTime.HourValue => timeSpan.TotalHours,
            UnitOfTime.DayValue => timeSpan.TotalDays,
            UnitOfTime.WeekValue => timeSpan.TotalDays / 7D,

            // Calendar-exact (whole + fractional based on actual next interval length)
            UnitOfTime.MonthValue => MonthsBetween(fromDateTimeOffset, utcNow.Value),
            UnitOfTime.QuarterValue => QuartersBetween(fromDateTimeOffset, utcNow.Value),
            UnitOfTime.YearValue => YearsBetween(fromDateTimeOffset, utcNow.Value),

            _ => throw new NotSupportedException("UnitOfTime is not supported for this method")
        };
    }

    /// <summary>
    /// Calculates the number of quarters between two instants as a calendar-exact value
    /// (whole quarters plus a fractional remainder based on the length of the next quarter interval).
    /// </summary>
    /// <param name="from">The start instant.</param>
    /// <param name="to">The end instant.</param>
    /// <returns>The number of quarters between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static double QuartersBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int whole = WholeQuartersBetween(from, to);
        DateTimeOffset start = from.AddMonths(whole * 3);
        DateTimeOffset end = start.AddMonths(3);

        if (start == to)
            return whole;

        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return whole + frac;
    }

    /// <summary>
    /// Calculates the number of years between two instants as a calendar-exact value
    /// (whole years plus a fractional remainder based on the length of the next year interval).
    /// </summary>
    /// <param name="from">The start instant.</param>
    /// <param name="to">The end instant.</param>
    /// <returns>The number of years between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static double YearsBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int whole = WholeYearsBetween(from, to);
        DateTimeOffset start = from.AddYears(whole);
        DateTimeOffset end = start.AddYears(1);

        if (start == to)
            return whole;

        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return whole + frac;
    }

    /// <summary>
    /// Calculates the number of months between two instants as a calendar-exact value
    /// (whole months plus a fractional remainder based on the length of the next month interval).
    /// </summary>
    /// <param name="from">The start instant.</param>
    /// <param name="to">The end instant.</param>
    /// <returns>The number of months between <paramref name="from"/> and <paramref name="to"/>.</returns>
    [Pure]
    public static double MonthsBetween(DateTimeOffset from, DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int whole = WholeMonthsBetween(from, to);
        DateTimeOffset start = from.AddMonths(whole);
        DateTimeOffset end = start.AddMonths(1);

        if (start == to)
            return whole;

        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return whole + frac;
    }

    /// <summary>
    /// Calculates the whole number of months between two instants (calendar months).
    /// </summary>
    /// <param name="from">The start instant.</param>
    /// <param name="to">The end instant.</param>
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
    /// Calculates the whole number of years between two instants (calendar years).
    /// </summary>
    /// <param name="from">The start instant.</param>
    /// <param name="to">The end instant.</param>
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
    /// Calculates the whole number of quarters between two instants (calendar quarters).
    /// </summary>
    /// <param name="from">The start instant.</param>
    /// <param name="to">The end instant.</param>
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
    /// Determines whether the date is a business day (i.e., not a weekend day) in the supplied time zone and culture.
    /// </summary>
    /// <param name="dateTimeOffset">The instant to evaluate.</param>
    /// <param name="zone">
    /// The time zone whose local calendar should be used to determine the day-of-week.
    /// If <see langword="null"/>, the value's existing offset is used (no additional conversion).
    /// </param>
    /// <param name="culture">
    /// The culture used to determine weekend days. If <see langword="null"/>, <see cref="CultureInfo.CurrentCulture"/> is used.
    /// </param>
    /// <returns><see langword="true"/> if the local date is a business day; otherwise <see langword="false"/>.</returns>
    [Pure]
    public static bool IsBusinessDay(this DateTimeOffset dateTimeOffset, TimeZoneInfo? zone = null, CultureInfo? culture = null)
    {
        DateTimeOffset local = zone is null ? dateTimeOffset : TimeZoneInfo.ConvertTime(dateTimeOffset, zone);
        DayOfWeek day = local.DayOfWeek;

        IReadOnlySet<DayOfWeek> weekendDays = (culture ?? CultureInfo.CurrentCulture).GetWeekendDays();
        return !weekendDays.Contains(day);
    }

    /// <summary>
    /// Adds (or subtracts) business days, skipping weekend days in the supplied time zone and culture.
    /// </summary>
    /// <param name="dateTimeOffset">The starting instant.</param>
    /// <param name="businessDays">Positive to add, negative to subtract.</param>
    /// <param name="zone">
    /// The time zone whose local calendar should be used when deciding if a day is a weekend.
    /// If <see langword="null"/>, the value's existing offset is used (no additional conversion).
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

        int direction = businessDays > 0 ? 1 : -1;
        int remaining = Math.Abs(businessDays);
        DateTimeOffset current = dateTimeOffset;

        while (remaining > 0)
        {
            current = current.AddDays(direction);

            if (current.IsBusinessDay(zone, culture))
                remaining--;
        }

        return current;
    }

    /// <summary>
    /// Checks whether the value is between <paramref name="start"/> and <paramref name="end"/>.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <param name="start">The range start.</param>
    /// <param name="end">The range end.</param>
    /// <param name="inclusive">Whether the bounds are inclusive.</param>
    /// <returns><see langword="true"/> if the value is within the range; otherwise <see langword="false"/>.</returns>
    [Pure]
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
    /// The existing <see cref="DateTimeOffset.Offset"/> is preserved; trimming is performed in the value's local offset.
    /// </remarks>
    /// <param name="dateTimeOffset">The value to trim.</param>
    /// <param name="unitOfTime">The unit to trim to.</param>
    /// <returns>A new <see cref="DateTimeOffset"/> representing the start of the specified unit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unitOfTime"/> is unsupported.</exception>
    [Pure]
    public static DateTimeOffset Trim(this DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        TimeSpan offset = dateTimeOffset.Offset;

        return unitOfTime.Value switch
        {
            UnitOfTime.MicrosecondValue => new DateTimeOffset(dateTimeOffset.Ticks - dateTimeOffset.Ticks % _ticksPerMicrosecond, offset),
            UnitOfTime.MillisecondValue => new DateTimeOffset(dateTimeOffset.Ticks - dateTimeOffset.Ticks % _ticksPerMillisecond, offset),

            UnitOfTime.SecondValue => new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour,
                dateTimeOffset.Minute, dateTimeOffset.Second, 0, offset),
            UnitOfTime.MinuteValue => new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour,
                dateTimeOffset.Minute, 0, 0, offset),
            UnitOfTime.HourValue => new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, 0, 0, 0, offset),
            UnitOfTime.DayValue => new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, 0, offset),

            // ISO week (Monday start)
            UnitOfTime.WeekValue => new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, 0, offset).AddDays(
                -((7 + (int)dateTimeOffset.DayOfWeek - (int)DayOfWeek.Monday) % 7)),

            UnitOfTime.MonthValue => new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, 1, 0, 0, 0, 0, offset),

            UnitOfTime.QuarterValue => new DateTimeOffset(dateTimeOffset.Year, (dateTimeOffset.Month - 1) / 3 * 3 + 1, 1, 0, 0, 0, 0, offset),

            UnitOfTime.YearValue => new DateTimeOffset(dateTimeOffset.Year, 1, 1, 0, 0, 0, 0, offset),

            UnitOfTime.DecadeValue => new DateTimeOffset(dateTimeOffset.Year - dateTimeOffset.Year % 10, 1, 1, 0, 0, 0, 0, offset),

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
    /// <param name="unitOfTime">The unit to trim to.</param>
    /// <returns>A new <see cref="DateTimeOffset"/> representing the end of the specified unit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="unitOfTime"/> is unsupported.</exception>
    [Pure]
    public static DateTimeOffset TrimEnd(this DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        DateTimeOffset startOfPeriod = dateTimeOffset.Trim(unitOfTime);

        startOfPeriod = unitOfTime.Value switch
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

        return startOfPeriod.AddTicks(-1);
    }

    /// <summary>
    /// Adds a value expressed in the specified unit of time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For <see cref="UnitOfTime.NanosecondValue"/>, <see cref="DateTimeOffset"/> cannot represent 1ns precision (it is 100ns/tick),
    /// so the value is truncated to whole ticks.
    /// </para>
    /// <para>
    /// For <see cref="UnitOfTime.MonthValue"/> and <see cref="UnitOfTime.YearValue"/>, fractional values are converted to days
    /// using the length of the resulting month/year after applying the whole-month/year portion.
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
                return dateTimeOffset.AddDays(value * 7);

            case UnitOfTime.MonthValue:
            {
                int wholeMonths = (int)value;
                double fractionalMonths = value - wholeMonths;

                dateTimeOffset = dateTimeOffset.AddMonths(wholeMonths);

                if (fractionalMonths == 0)
                    return dateTimeOffset;

                int daysInMonth = DateTime.DaysInMonth(dateTimeOffset.Year, dateTimeOffset.Month);
                return dateTimeOffset.AddDays(fractionalMonths * daysInMonth);
            }

            case UnitOfTime.QuarterValue:
                return dateTimeOffset.AddMonths((int)(value * 3));

            case UnitOfTime.YearValue:
            {
                int wholeYears = (int)value;
                double fractionalYears = value - wholeYears;

                dateTimeOffset = dateTimeOffset.AddYears(wholeYears);

                if (fractionalYears == 0)
                    return dateTimeOffset;

                int daysInYear = DateTime.IsLeapYear(dateTimeOffset.Year) ? 366 : 365;
                return dateTimeOffset.AddDays(fractionalYears * daysInYear);
            }

            case UnitOfTime.DecadeValue:
                return dateTimeOffset.AddYears((int)(value * 10));

            default:
                throw new ArgumentOutOfRangeException(nameof(unitOfTime), $"Unsupported UnitOfTime: {unitOfTime.Name}");
        }
    }

    /// <summary>
    /// Subtracts a value expressed in the specified unit of time.
    /// </summary>
    /// <param name="dateTimeOffset">The starting value.</param>
    /// <param name="value">The amount to subtract (may be fractional depending on <paramref name="unitOfTime"/>).</param>
    /// <param name="unitOfTime">The unit of time.</param>
    /// <returns>A new <see cref="DateTimeOffset"/> with the adjustment applied.</returns>
    [Pure]
    public static DateTimeOffset Subtract(this DateTimeOffset dateTimeOffset, double value, UnitOfTime unitOfTime)
    {
        return dateTimeOffset.Add(-value, unitOfTime);
    }

    /// <inheritdoc cref="Trim(DateTimeOffset, UnitOfTime)"/>
    [Pure]
    public static DateTimeOffset ToStartOf(this DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        return dateTimeOffset.Trim(unitOfTime);
    }

    /// <inheritdoc cref="TrimEnd(DateTimeOffset, UnitOfTime)"/>
    [Pure]
    public static DateTimeOffset ToEndOf(this DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        return dateTimeOffset.TrimEnd(unitOfTime);
    }

    /// <summary>
    /// Converts the value to an integer in the format <c>yyyyMMdd</c>.
    /// </summary>
    /// <param name="dateTimeOffset">The value to convert.</param>
    /// <returns>An integer representing the date portion of the value in the format <c>yyyyMMdd</c>.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToDateAsInteger(this DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.Year * 10000 + dateTimeOffset.Month * 100 + dateTimeOffset.Day;

    /// <summary>
    /// Gets the UTC offset for the specified instant in the provided time zone.
    /// </summary>
    /// <param name="utcNow">A UTC instant used to evaluate the time zone's offset (including DST) at that moment.</param>
    /// <param name="tz">The time zone.</param>
    /// <returns>The UTC offset (including DST) at <paramref name="utcNow"/> in <paramref name="tz"/>.</returns>
    [Pure]
    public static TimeSpan ToTzOffset(this DateTimeOffset utcNow, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        return tz.GetUtcOffset(utcNow.UtcDateTime);
    }

    /// <summary>
    /// Gets the UTC offset in hours for the specified instant in the provided time zone.
    /// </summary>
    /// <param name="utcNow">A UTC instant used to evaluate the time zone's offset (including DST) at that moment.</param>
    /// <param name="tz">The time zone.</param>
    /// <returns>The UTC offset in hours (may be fractional for non-whole-hour offsets).</returns>
    [Pure]
    public static double ToTzOffsetHours(this DateTimeOffset utcNow, TimeZoneInfo tz)
    {
        if (tz is null)
            throw new ArgumentNullException(nameof(tz));

        return tz.GetUtcOffset(utcNow.UtcDateTime)
                 .TotalHours;
    }

    /// <summary>
    /// Converts a local hour (0-23) in a given time zone on the local date corresponding to <paramref name="utcNow"/>
    /// into the corresponding UTC hour (0-23).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The local date is determined by converting <paramref name="utcNow"/> into <paramref name="tz"/> and using that local date.
    /// </para>
    /// <para>
    /// If the requested local time falls into a DST "gap" (invalid local time), this method advances minute-by-minute
    /// to the next valid local time.
    /// </para>
    /// <para>
    /// If the requested local time is ambiguous (DST "fold"), this method chooses the earlier UTC instant.
    /// </para>
    /// </remarks>
    /// <param name="utcNow">A UTC instant used to choose the local date in <paramref name="tz"/>.</param>
    /// <param name="tzHour">The hour in the target time zone (0-23).</param>
    /// <param name="tz">The time zone whose local hour should be converted to UTC.</param>
    /// <returns>The UTC hour (0-23) corresponding to <paramref name="tzHour"/> on that local date.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tz"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tzHour"/> is not in the range 0-23.</exception>
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

        // If invalid (spring-forward gap), advance until valid.
        if (tz.IsInvalidTime(local))
        {
            do
            {
                local = local.AddMinutes(1);
            }
            while (tz.IsInvalidTime(local));
        }

        // If ambiguous (fall-back fold), choose the earlier UTC instant (i.e., subtract the larger offset).
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
    /// Produces a time window ending <paramref name="delay"/> before the current instant and spanning <paramref name="subtraction"/>.
    /// </summary>
    /// <param name="utcNow">The reference instant (commonly "now").</param>
    /// <param name="delay">How far back from <paramref name="utcNow"/> the window end should be.</param>
    /// <param name="subtraction">How large the window should be (subtracted from the window end).</param>
    /// <param name="unitOfTime">The unit used for <paramref name="delay"/> and <paramref name="subtraction"/>.</param>
    /// <returns>
    /// A tuple where <c>endAt</c> is <paramref name="delay"/> before <paramref name="utcNow"/>, and
    /// <c>startAt</c> is <paramref name="subtraction"/> before <c>endAt</c>.
    /// </returns>
    [Pure]
    public static (DateTimeOffset startAt, DateTimeOffset endAt) ToWindow(this DateTimeOffset utcNow, int delay, int subtraction, UnitOfTime unitOfTime)
    {
        DateTimeOffset endAt = utcNow.Subtract(delay, unitOfTime);
        DateTimeOffset startAt = endAt.Subtract(subtraction, unitOfTime);

        return (startAt, endAt);
    }

    /// <summary>
    /// Converts the value to a <see cref="DateOnly"/> by stripping the time component.
    /// </summary>
    /// <param name="dateTimeOffset">The value to convert.</param>
    /// <returns>A <see cref="DateOnly"/> representing the date portion of the value.</returns>
    [Pure]
    public static DateOnly ToDateOnly(this DateTimeOffset dateTimeOffset)
    {
        return DateOnly.FromDateTime(dateTimeOffset.Date);
    }
}