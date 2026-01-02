using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using Soenneker.Enums.UnitOfTime;
using Soenneker.Extensions.CultureInfos;

namespace Soenneker.Extensions.DateTimeOffsets;

/// <summary>
/// A collection of helpful DateTimeOffset extension methods
/// </summary>
public static class DateTimeOffsetExtension
{
    private const double _nanosecondsPerTick = 100.0;
    private const double _ticksPerMicrosecond = 10.0;

    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to a UTC <see cref="System.DateTime"/>.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <returns>A UTC <see cref="System.DateTime"/> equivalent to the provided <see cref="DateTimeOffset"/>.</returns>
    [Pure]
    public static DateTime ToUtcDateTime(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime;
    }

    /// <summary>
    /// Converts a <see cref="DateTimeOffset"/> to the specified time zone.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <param name="tzInfo">The target <see cref="TimeZoneInfo"/> representing the time zone to convert to.</param>
    /// <returns>A <see cref="System.DateTimeOffset"/> instance representing the converted time in the specified time zone.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="tzInfo"/> is null.</exception>
    [Pure]
    public static System.DateTimeOffset ToTz(this System.DateTimeOffset dateTimeOffset, TimeZoneInfo tzInfo)
    {
        return TimeZoneInfo.ConvertTime(dateTimeOffset, tzInfo);
    }

    /// <summary>
    /// Converts a <see cref="DateTimeOffset"/> to UTC.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert to UTC.</param>
    /// <returns>A <see cref="System.DateTimeOffset"/> value that represents the same point in time, expressed in UTC.</returns>
    [Pure]
    public static System.DateTimeOffset ToUtc(this System.DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToOffset(TimeSpan.Zero);
    }

    /// <summary>
    /// Calculates the age between the specified date and the current date and time.
    /// </summary>
    /// <param name="fromDateTimeOffset">The specified date and time.</param>
    /// <param name="unitOfTime">The unit of time to calculate the age in.</param>
    /// <param name="utcNow">The current date and time in UTC. If not provided, the current UTC date and time will be used.</param>
    /// <returns>The age in the specified unit of time.</returns>
    /// <exception cref="NotSupportedException"></exception>
    [Pure]
    public static double ToAge(this System.DateTimeOffset fromDateTimeOffset, UnitOfTime unitOfTime, System.DateTimeOffset? utcNow = null)
    {
        utcNow ??= System.DateTimeOffset.UtcNow;
        TimeSpan timeSpan = utcNow.Value - fromDateTimeOffset;

        return unitOfTime.Value switch
        {
            UnitOfTime.TickValue => timeSpan.Ticks,
            UnitOfTime.NanosecondValue => timeSpan.Ticks * _nanosecondsPerTick,
            UnitOfTime.MicrosecondValue => timeSpan.Ticks / _ticksPerMicrosecond,
            UnitOfTime.MillisecondValue => timeSpan.TotalMilliseconds,
            UnitOfTime.SecondValue => timeSpan.TotalSeconds,
            UnitOfTime.MinuteValue => timeSpan.TotalMinutes,
            UnitOfTime.HourValue => timeSpan.TotalHours,
            UnitOfTime.DayValue => timeSpan.TotalDays,
            UnitOfTime.WeekValue => timeSpan.TotalDays / 7D,

            // calendar-exact (whole + fractional based on actual next interval length)
            UnitOfTime.MonthValue => MonthsBetween(fromDateTimeOffset, utcNow.Value),
            UnitOfTime.QuarterValue => QuartersBetween(fromDateTimeOffset, utcNow.Value),
            UnitOfTime.YearValue => YearsBetween(fromDateTimeOffset, utcNow.Value),

            _ => throw new NotSupportedException("UnitOfTime is not supported for this method")
        };
    }

    [Pure]
    public static double QuartersBetween(System.DateTimeOffset from, System.DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int whole = WholeQuartersBetween(from, to);
        System.DateTimeOffset start = from.AddMonths(whole * 3);
        System.DateTimeOffset end = start.AddMonths(3);

        if (start == to)
            return whole;

        // fraction of the next quarter interval
        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return whole + frac;
    }

    [Pure]
    public static double YearsBetween(System.DateTimeOffset from, System.DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int whole = WholeYearsBetween(from, to);
        System.DateTimeOffset start = from.AddYears(whole);
        System.DateTimeOffset end = start.AddYears(1);

        if (start == to)
            return whole;

        // fraction of the next year interval
        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return whole + frac;
    }

    [Pure]
    public static double MonthsBetween(System.DateTimeOffset from, System.DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int whole = WholeMonthsBetween(from, to);
        System.DateTimeOffset start = from.AddMonths(whole);
        System.DateTimeOffset end = start.AddMonths(1);

        if (start == to)
            return whole;

        // fraction of the next month interval
        double frac = (to - start).Ticks / (double)(end - start).Ticks;
        return whole + frac;
    }

    [Pure]
    public static int WholeMonthsBetween(System.DateTimeOffset from, System.DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int months = (to.Year - from.Year) * 12 + (to.Month - from.Month);

        // If "to" hasn't reached the day/time of "from" within that month, back up 1
        System.DateTimeOffset candidate = from.AddMonths(months);

        if (candidate > to)
            months--;

        return months;
    }

    [Pure]
    public static int WholeYearsBetween(System.DateTimeOffset from, System.DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int years = to.Year - from.Year;
        System.DateTimeOffset candidate = from.AddYears(years);
        if (candidate > to)
            years--;

        return years;
    }

    [Pure]
    public static int WholeQuartersBetween(System.DateTimeOffset from, System.DateTimeOffset to)
    {
        if (to < from)
            (from, to) = (to, from);

        int fromQ = (from.Month - 1) / 3;
        int toQ = (to.Month - 1) / 3;

        int quarters = (to.Year - from.Year) * 4 + (toQ - fromQ);
        System.DateTimeOffset candidate = from.AddMonths(quarters * 3);
        if (candidate > to)
            quarters--;

        return quarters;
    }

    /// <summary>
    /// Determines whether the date is a business day (Mon–Fri) in the supplied time-zone.
    /// If <paramref name="zone"/> is <see langword="null"/>, the date’s own offset is used.
    /// </summary>
    [Pure]
    public static bool IsBusinessDay(this System.DateTimeOffset dateTimeOffset, TimeZoneInfo? zone = null, CultureInfo? culture = null)
    {
        System.DateTimeOffset local = zone is null ? dateTimeOffset : TimeZoneInfo.ConvertTime(dateTimeOffset, zone);
        DayOfWeek d = local.DayOfWeek;

        IReadOnlySet<DayOfWeek> weekendDays = (culture ?? CultureInfo.CurrentCulture).GetWeekendDays();

        return !weekendDays.Contains(d);
    }

    /// <summary>
    /// Adds (or subtracts) a number of business days, skipping weekends in the given time-zone.
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <param name="businessDays">Positive to add, negative to subtract.</param>
    /// <param name="zone">
    /// Time-zone whose calendar should be used when deciding if a day is a weekend.
    /// If <see langword="null"/>, the date’s own offset is used.
    /// </param>
    /// <param name="culture"></param>
    [Pure]
    public static System.DateTimeOffset AddBusinessDays(this System.DateTimeOffset dateTimeOffset, int businessDays, TimeZoneInfo? zone = null,
        CultureInfo? culture = null)
    {
        if (businessDays == 0)
            return dateTimeOffset;

        int direction = businessDays > 0 ? 1 : -1;
        int remaining = Math.Abs(businessDays);
        System.DateTimeOffset current = dateTimeOffset;

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
    [Pure]
    public static bool IsBetween(this System.DateTimeOffset value, System.DateTimeOffset start, System.DateTimeOffset end, bool inclusive = true)
    {
        if (start > end)
            (start, end) = (end, start);

        return inclusive ? value >= start && value <= end : value > start && value < end;
    }

    /// <summary>
    /// Trims a <see cref="System.DateTimeOffset"/> object to a specified level of precision.
    /// </summary>
    /// <remarks>
    /// This method adjusts a <see cref="System.DateTimeOffset"/> object to the nearest lower value of the specified precision. For example, trimming to <see cref="UnitOfTime.Minute"/> 
    /// will result in a <see cref="System.DateTimeOffset"/> object set to the beginning of the minute, with seconds and milliseconds set to zero.
    /// The method supports various levels of precision, such as Year, Month, Day, Hour, Minute, and Second. Any time components finer than the specified precision are set to zero.
    /// The offset is preserved from the original <see cref="System.DateTimeOffset"/>.
    /// </remarks>
    /// <param name="dateTimeOffset">The <see cref="System.DateTimeOffset"/> to trim.</param>
    /// <param name="unitOfTime">The precision to which the <paramref name="dateTimeOffset"/> should be trimmed. This should be one of the values defined in <see cref="UnitOfTime"/>.</param>
    /// <returns>A new <see cref="System.DateTimeOffset"/> object trimmed to the specified <paramref name="unitOfTime"/>.</returns>
    [Pure]
    public static System.DateTimeOffset Trim(this System.DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        System.DateTimeOffset trimmed;
        TimeSpan offset = dateTimeOffset.Offset;

        switch (unitOfTime.Value)
        {
            case UnitOfTime.MicrosecondValue:
                {
                    long ticks = dateTimeOffset.Ticks;
                    long truncatedTicks = ticks - ticks % (long)_ticksPerMicrosecond;
                    trimmed = new System.DateTimeOffset(truncatedTicks, offset);
                    break;
                }
            case UnitOfTime.MillisecondValue:
                {
                    long ticks = dateTimeOffset.Ticks;
                    long truncatedTicks = ticks - ticks % 10000;
                    trimmed = new System.DateTimeOffset(truncatedTicks, offset);
                    break;
                }
            case UnitOfTime.SecondValue:
                trimmed = new System.DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute,
                    dateTimeOffset.Second, 0, offset);
                break;
            case UnitOfTime.MinuteValue:
                trimmed = new System.DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute,
                    0, 0, offset);
                break;
            case UnitOfTime.HourValue:
                trimmed = new System.DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, 0, 0, 0, offset);
                break;
            case UnitOfTime.DayValue:
                trimmed = new System.DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, 0, offset);
                break;
            case UnitOfTime.WeekValue: // Considering Monday is the first day - ISO 8601
                {
                    int daysToSubtract = (int)dateTimeOffset.DayOfWeek - (int)DayOfWeek.Monday;
                    if (daysToSubtract < 0)
                    {
                        daysToSubtract += 7;
                    }

                    trimmed = new System.DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, 0, offset).AddDays(-daysToSubtract);
                    break;
                }
            case UnitOfTime.MonthValue:
                trimmed = new System.DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, 1, 0, 0, 0, 0, offset);
                break;
            case UnitOfTime.QuarterValue:
                // Determine the start month of the quarter
                int quarterNumber = (dateTimeOffset.Month - 1) / 3;
                int startMonthOfQuarter = quarterNumber * 3 + 1;
                trimmed = new System.DateTimeOffset(dateTimeOffset.Year, startMonthOfQuarter, 1, 0, 0, 0, 0, offset);
                break;
            case UnitOfTime.YearValue:
                trimmed = new System.DateTimeOffset(dateTimeOffset.Year, 1, 1, 0, 0, 0, 0, offset);
                break;
            case UnitOfTime.DecadeValue:
                // Calculate the start year of the decade
                int startYearOfDecade = dateTimeOffset.Year - dateTimeOffset.Year % 10;
                trimmed = new System.DateTimeOffset(startYearOfDecade, 1, 1, 0, 0, 0, 0, offset);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(unitOfTime), $"Unsupported UnitOfTime: {unitOfTime.Name}");
        }

        return trimmed;
    }

    /// <summary>
    /// Adjusts the provided <see cref="System.DateTimeOffset"/> object to the end of the specified period, minus one tick.
    /// </summary>
    /// <param name="dateTimeOffset">The date and time value to adjust.</param>
    /// <param name="unitOfTime">The precision level to which the date and time should be adjusted. This determines the period (e.g., Year, Month, Day, etc.) to which the <paramref name="dateTimeOffset"/> will be trimmed.</param>
    /// <returns>
    /// A new <see cref="System.DateTimeOffset"/> object representing the last moment of the specified period, just before it transitions to the next period, according to the specified <paramref name="unitOfTime"/>.
    /// </returns>
    /// <remarks>
    /// This method first calculates the start of the next period based on the specified <paramref name="unitOfTime"/>. It then subtracts one tick from this calculated start time to get the precise end of the current period.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported <paramref name="unitOfTime"/> is provided.</exception>
    [Pure]
    public static System.DateTimeOffset TrimEnd(this System.DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        System.DateTimeOffset startOfPeriod = dateTimeOffset.Trim(unitOfTime);

        startOfPeriod = unitOfTime.Value switch
        {
            UnitOfTime.MicrosecondValue => startOfPeriod.AddTicks((long)_ticksPerMicrosecond), // Add 10 ticks to move to the start of the next microsecond
            UnitOfTime.MillisecondValue => startOfPeriod.AddMilliseconds(1),
            UnitOfTime.SecondValue => startOfPeriod.AddSeconds(1),
            UnitOfTime.MinuteValue => startOfPeriod.AddMinutes(1),
            UnitOfTime.HourValue => startOfPeriod.AddHours(1),
            UnitOfTime.DayValue => startOfPeriod.AddDays(1),
            UnitOfTime.WeekValue => startOfPeriod.AddDays(7),
            UnitOfTime.MonthValue => startOfPeriod.AddMonths(1),
            UnitOfTime.QuarterValue => startOfPeriod.AddMonths(3), // Quarters consist of 3 months
            UnitOfTime.YearValue => startOfPeriod.AddYears(1),
            UnitOfTime.DecadeValue => startOfPeriod.AddYears(10),
            _ => throw new ArgumentOutOfRangeException(nameof(unitOfTime), $"Unsupported UnitOfTime: {unitOfTime.Name}")
        };

        // Subtract one tick to get the last moment of the current period
        return startOfPeriod.AddTicks(-1);
    }

    /// <summary>
    /// Adds a specified amount of time to the given <see cref="System.DateTimeOffset"/> object based on the provided <see cref="UnitOfTime"/>.
    /// </summary>
    /// <param name="dateTimeOffset">The original <see cref="System.DateTimeOffset"/> object to which time will be added.</param>
    /// <param name="value">The amount of time to add. Can be a fractional value for finer granularity.</param>
    /// <param name="unitOfTime">The unit of time to add, specified as a <see cref="UnitOfTime"/>.</param>
    /// <returns>A new <see cref="System.DateTimeOffset"/> object that is the result of adding the specified amount of time to the original date and time.</returns>
    [Pure]
    public static System.DateTimeOffset Add(this System.DateTimeOffset dateTimeOffset, double value, UnitOfTime unitOfTime)
    {
        switch (unitOfTime.Value)
        {
            case UnitOfTime.TickValue:
                return dateTimeOffset.AddTicks((long)value);
            case UnitOfTime.NanosecondValue:
                double totalTicksForNanoseconds = value / _nanosecondsPerTick;
                var wholeTicksForNanoseconds = (long)totalTicksForNanoseconds;
                double fractionalTicksForNanoseconds = totalTicksForNanoseconds - wholeTicksForNanoseconds;
                dateTimeOffset = dateTimeOffset.AddTicks(wholeTicksForNanoseconds);
                return dateTimeOffset.AddTicks((long)(fractionalTicksForNanoseconds * _nanosecondsPerTick));
            case UnitOfTime.MicrosecondValue:
                double totalTicksForMicroseconds = value * _ticksPerMicrosecond;
                var wholeTicksForMicroseconds = (long)totalTicksForMicroseconds;
                double fractionalTicksForMicroseconds = totalTicksForMicroseconds - wholeTicksForMicroseconds;
                dateTimeOffset = dateTimeOffset.AddTicks(wholeTicksForMicroseconds);
                return dateTimeOffset.AddTicks((long)(fractionalTicksForMicroseconds * _ticksPerMicrosecond));
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
                var wholeMonths = (int)value;
                double fractionalMonths = value - wholeMonths;
                dateTimeOffset = dateTimeOffset.AddMonths(wholeMonths);
                return dateTimeOffset.AddDays(fractionalMonths * DateTime.DaysInMonth(dateTimeOffset.Year, dateTimeOffset.Month));
            case UnitOfTime.QuarterValue:
                return dateTimeOffset.AddMonths((int)(value * 3));
            case UnitOfTime.YearValue:
                var wholeYears = (int)value;
                double fractionalYears = value - wholeYears;
                dateTimeOffset = dateTimeOffset.AddYears(wholeYears);
                return dateTimeOffset.AddDays(fractionalYears * (DateTime.IsLeapYear(dateTimeOffset.Year) ? 366 : 365));
            case UnitOfTime.DecadeValue:
                return dateTimeOffset.AddYears((int)(value * 10));
            default:
                throw new ArgumentOutOfRangeException(nameof(unitOfTime), $"Unsupported UnitOfTime: {unitOfTime.Name}");
        }
    }

    [Pure]
    public static System.DateTimeOffset Subtract(this System.DateTimeOffset dateTimeOffset, double value, UnitOfTime unitOfTime)
    {
        return dateTimeOffset.Add(-value, unitOfTime);
    }

    /// <inheritdoc cref="Trim(System.DateTimeOffset, UnitOfTime)"/>
    [Pure]
    public static System.DateTimeOffset ToStartOf(this System.DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        return Trim(dateTimeOffset, unitOfTime);
    }

    /// <inheritdoc cref="TrimEnd(System.DateTimeOffset, UnitOfTime)"/>
    [Pure]
    public static System.DateTimeOffset ToEndOf(this System.DateTimeOffset dateTimeOffset, UnitOfTime unitOfTime)
    {
        return TrimEnd(dateTimeOffset, unitOfTime);
    }

    /// <summary>
    /// Converts a <see cref="System.DateTimeOffset"/> instance to an integer in the format yyyyMMdd.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="System.DateTimeOffset"/> instance to convert.</param>
    /// <returns>An integer representing the <paramref name="dateTimeOffset"/> in the format yyyyMMdd.</returns>
    /// <exception cref="FormatException">Thrown when the conversion to the integer format fails, which should not occur with valid dates.</exception>
    /// <remarks>
    /// This method extends <see cref="System.DateTimeOffset"/> and allows for a compact representation of a date as an integer. This can be useful for comparisons, sorting, or storing dates in a condensed format.
    /// <example>
    /// <code>
    /// var date = new DateTimeOffset(2023, 3, 15, 0, 0, 0, TimeSpan.Zero);
    /// int dateInt = date.ToDateAsInteger();
    /// Console.WriteLine(dateInt); // Outputs "20230315"
    /// </code>
    /// </example>
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToDateAsInteger(this System.DateTimeOffset dateTimeOffset) =>
        dateTimeOffset.Year * 10000 + dateTimeOffset.Month * 100 + dateTimeOffset.Day;

    /// <summary>
    /// Calculates the whole hour part of the time zone offset for a given DateTimeOffset in the specified time zone.
    /// </summary>
    /// <remarks>
    /// This method provides the time zone offset in hours for the specified DateTimeOffset, accounting for any applicable daylight saving time changes.
    /// The offset is determined by converting the DateTimeOffset to the target time zone and then calculating the offset from UTC.
    /// </remarks>
    /// <param name="dateTimeOffset">The DateTimeOffset to calculate the offset for.</param>
    /// <param name="timeZoneInfo">The time zone to calculate the offset against.</param>
    /// <returns>The time zone offset in hours from UTC. Time zones west of UTC return negative values. i.e. Eastern returns a negative value (-4, or -5)</returns>
    [Pure]
    public static int ToTzOffsetHours(this System.DateTimeOffset dateTimeOffset, TimeZoneInfo timeZoneInfo)
    {
        return dateTimeOffset.ToTzOffset(timeZoneInfo)
                             .Hours;
    }

    /// <summary>
    /// Determines the time zone offset as a TimeSpan for a given DateTimeOffset in the specified time zone, considering daylight saving time.
    /// </summary>
    /// <remarks>
    /// This method calculates the exact time zone offset, including minutes and seconds, for the specified DateTimeOffset.
    /// It accounts for the time zone's daylight saving rules, which can cause the offset to vary throughout the year.
    /// </remarks>
    /// <param name="dateTimeOffset">The DateTimeOffset to calculate the offset for.</param>
    /// <param name="timeZoneInfo">The time zone to calculate the offset for.</param>
    [Pure]
    public static TimeSpan ToTzOffset(this System.DateTimeOffset dateTimeOffset, TimeZoneInfo timeZoneInfo)
    {
        System.DateTimeOffset converted = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZoneInfo);
        return timeZoneInfo.GetUtcOffset(converted.DateTime);
    }

    /// <summary>
    /// Converts a specific hour in a given time zone to its corresponding hour in UTC.
    /// </summary>
    /// <remarks>
    /// This method calculates the UTC equivalent of a specified hour in a given time zone, considering the time zone's offset from UTC, including any daylight saving time adjustments.
    /// It is designed to handle time zone differences and daylight saving time, ensuring that the conversion always produces a valid hour in the 24-hour format.
    /// Special Case: Due to the modulo operation, a conversion can result in 24, which represents midnight at the start of a new day.
    /// </remarks>
    /// <param name="dateTimeOffset">The current DateTimeOffset, used to determine the time zone's current offset, including daylight saving time.</param>
    /// <param name="tzHour">The hour in the specified time zone to be converted to UTC. Must be in 24-hour format.</param>
    /// <param name="timeZoneInfo">The time zone of the original hour.</param>
    /// <returns>The hour in UTC after conversion. This is always a positive number in the 24-hour format, where 24 may indicate midnight.</returns>
    [Pure]
    public static int ToUtcHoursFromTz(this System.DateTimeOffset dateTimeOffset, int tzHour, TimeZoneInfo timeZoneInfo)
    {
        int utcHoursOffset = dateTimeOffset.ToTzOffsetHours(timeZoneInfo);

        int v = tzHour - utcHoursOffset;
        v %= 24;

        if (v < 0)
            v += 24;
        return v;
    }

    /// <summary>
    /// Subtracts an amount (delay) of time (endAt), and then subtracts another amount (subtraction) of time (startAt).
    /// </summary>
    [Pure]
    public static (System.DateTimeOffset startAt, System.DateTimeOffset endAt) ToWindow(this System.DateTimeOffset dateTimeOffset, int delay, int subtraction,
        UnitOfTime unitOfTime)
    {
        System.DateTimeOffset endAt = dateTimeOffset.Subtract(delay, unitOfTime);
        System.DateTimeOffset startAt = endAt.Subtract(subtraction, unitOfTime);

        return (startAt, endAt);
    }

    /// <summary>
    /// Converts a <see cref="System.DateTimeOffset"/> to a <see cref="DateOnly"/> by stripping the time component.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="System.DateTimeOffset"/> to convert.</param>
    /// <returns>A <see cref="DateOnly"/> representing the date portion of the input.</returns>
    [Pure]
    public static DateOnly ToDateOnly(this System.DateTimeOffset dateTimeOffset)
    {
        return DateOnly.FromDateTime(dateTimeOffset.Date);
    }
}