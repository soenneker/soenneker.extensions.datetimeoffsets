using System;
using System.Globalization;
using Soenneker.Enums.UnitOfTime;
using Soenneker.Extensions.DateTimeOffsets;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Extensions.DateTimeOffsets.Tests;

public sealed class DateTimeOFfsetsExtensionTests : UnitTest
{
    private static TimeZoneInfo GetEastern() =>
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Eastern Standard Time" : "America/New_York");

    [Fact]
    public void ToTz_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToTz(null!));
    }

    [Fact]
    public void ToUtc_preserves_instant()
    {
        var withOffset = new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.FromHours(-4));
        DateTimeOffset utc = withOffset.ToUtc();
        Assert.Equal(TimeSpan.Zero, utc.Offset);
        Assert.Equal(2024, utc.Year);
        Assert.Equal(12, utc.Hour);
    }

    [Fact]
    public void ToAge_same_instant_returns_zero()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        double age = dto.ToAge(UnitOfTime.Day, dto);
        Assert.Equal(0, age);
    }

    [Fact]
    public void ToAge_reversed_returns_negative()
    {
        var from = new DateTimeOffset(2024, 6, 20, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        double age = from.ToAge(UnitOfTime.Day, to);
        Assert.True(age < 0);
        Assert.Equal(-5, age, precision: 0);
    }

    [Fact]
    public void ToAge_month_unit_across_leap_year_february()
    {
        var from = new DateTimeOffset(2024, 2, 15, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero);
        double months = from.ToAge(UnitOfTime.Month, to);
        Assert.True(months > 0);
        Assert.True(months <= 1.1);
    }

    [Fact]
    public void MonthsBetween_same_instant_returns_zero()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 30, 0, TimeSpan.Zero);
        Assert.Equal(0d, DateTimeOffsetExtension.MonthsBetween(dto, dto));
    }

    [Fact]
    public void MonthsBetween_reversed_order_returns_negative()
    {
        var from = new DateTimeOffset(2024, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
        double m = DateTimeOffsetExtension.MonthsBetween(from, to);
        Assert.True(m < 0);
    }

    [Fact]
    public void WholeMonthsBetween_jan_31_to_mar_30_returns_one()
    {
        var from = new DateTimeOffset(2024, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 3, 30, 0, 0, 0, TimeSpan.Zero);
        int whole = DateTimeOffsetExtension.WholeMonthsBetween(from, to);
        Assert.Equal(1, whole);
    }

    [Fact]
    public void YearsBetween_same_instant_returns_zero()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(0d, DateTimeOffsetExtension.YearsBetween(dto, dto));
    }

    [Fact]
    public void QuartersBetween_reversed_returns_negative()
    {
        var from = new DateTimeOffset(2024, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);
        double q = DateTimeOffsetExtension.QuartersBetween(from, to);
        Assert.True(q < 0);
    }

    [Fact]
    public void IsBusinessDay_with_null_zone_uses_offset_day_of_week()
    {
        var saturday = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero); // Saturday
        bool isBusiness = saturday.IsBusinessDay(zone: null, CultureInfo.InvariantCulture);
        Assert.False(isBusiness);
    }

    [Fact]
    public void AddBusinessDays_zero_returns_same_value()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = dto.AddBusinessDays(0);
        Assert.Equal(dto, result);
    }

    [Fact]
    public void AddBusinessDays_negative_goes_backward()
    {
        var friday = new DateTimeOffset(2024, 6, 14, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = friday.AddBusinessDays(-1);
        Assert.True(result < friday);
        Assert.Equal(DayOfWeek.Thursday, result.DayOfWeek);
    }

    [Fact]
    public void IsBetween_reversed_start_end_swaps_internally()
    {
        var start = new DateTimeOffset(2024, 6, 20, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 6, 10, 0, 0, 0, TimeSpan.Zero);
        var value = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        Assert.True(value.IsBetween(start, end, inclusive: true));
    }

    [Fact]
    public void IsBetween_exclusive_excludes_boundaries()
    {
        var start = new DateTimeOffset(2024, 6, 10, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 6, 20, 0, 0, 0, TimeSpan.Zero);
        Assert.False(start.IsBetween(start, end, inclusive: false));
        Assert.False(end.IsBetween(start, end, inclusive: false));
        Assert.True(new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero).IsBetween(start, end, inclusive: false));
    }

    [Fact]
    public void Trim_decade_rounds_down_year()
    {
        var dto = new DateTimeOffset(2025, 7, 15, 12, 30, 0, TimeSpan.Zero);
        DateTimeOffset trimmed = dto.Trim(UnitOfTime.Decade);
        Assert.Equal(2020, trimmed.Year);
        Assert.Equal(1, trimmed.Month);
        Assert.Equal(1, trimmed.Day);
        Assert.Equal(0, trimmed.Hour);
    }

    [Fact]
    public void Trim_quarter_first_month_of_quarter()
    {
        var dto = new DateTimeOffset(2024, 5, 20, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset trimmed = dto.Trim(UnitOfTime.Quarter);
        Assert.Equal(2024, trimmed.Year);
        Assert.Equal(4, trimmed.Month);
        Assert.Equal(1, trimmed.Day);
    }

    [Fact]
    public void TrimEnd_month_last_tick_of_month()
    {
        var dto = new DateTimeOffset(2024, 2, 15, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset endOfMonth = dto.TrimEnd(UnitOfTime.Month);
        Assert.Equal(2024, endOfMonth.Year);
        Assert.Equal(2, endOfMonth.Month);
        Assert.Equal(29, endOfMonth.Day); // 2024 leap year
        Assert.Equal(23, endOfMonth.Hour);
        Assert.Equal(59, endOfMonth.Minute);
        Assert.Equal(59, endOfMonth.Second);
    }

    [Fact]
    public void Trim_week_monday_start()
    {
        var wednesday = new DateTimeOffset(2024, 6, 12, 14, 0, 0, TimeSpan.Zero);
        DateTimeOffset startOfWeek = wednesday.Trim(UnitOfTime.Week);
        Assert.Equal(DayOfWeek.Monday, startOfWeek.DayOfWeek);
        Assert.Equal(0, startOfWeek.Hour);
        Assert.Equal(0, startOfWeek.Minute);
    }

    [Fact]
    public void Add_zero_returns_same()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Equal(dto, dto.Add(0, UnitOfTime.Day));
        Assert.Equal(dto, dto.Add(0, UnitOfTime.Month));
    }

    [Fact]
    public void Subtract_positive_value_goes_backward()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = dto.Subtract(2, UnitOfTime.Day);
        Assert.True(result < dto);
        Assert.Equal(13, result.Day);
    }

    [Fact]
    public void Add_fractional_month_uses_days_in_month()
    {
        var dto = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = dto.Add(0.5, UnitOfTime.Month);
        Assert.Equal(2024, result.Year);
        Assert.Equal(1, result.Month);
        // 0.5 * 31 days = 15.5 days → Jan 16 12:00
        Assert.Equal(16, result.Day);
        Assert.Equal(12, result.Hour);
    }

    [Fact]
    public void ToDateAsInteger_min_value()
    {
        var dto = DateTimeOffset.MinValue;
        int ymd = dto.ToDateAsInteger();
        Assert.Equal(10101, ymd);
    }

    [Fact]
    public void ToDateAsInteger_max_value()
    {
        var dto = DateTimeOffset.MaxValue;
        int ymd = dto.ToDateAsInteger();
        Assert.Equal(99991231, ymd);
    }

    [Fact]
    public void ToTzOffset_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToTzOffset(null!));
    }

    [Fact]
    public void ToUtcHoursFromTz_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToUtcHoursFromTz(14, null!));
    }

    [Fact]
    public void ToUtcHoursFromTz_throws_when_tzHour_out_of_range()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() => dto.ToUtcHoursFromTz(24, TimeZoneInfo.Utc));
        Assert.Throws<ArgumentOutOfRangeException>(() => dto.ToUtcHoursFromTz(-1, TimeZoneInfo.Utc));
    }

    [Fact]
    public void ToUtcHoursFromTz_UTC_returns_same_hour()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero);
        int utcHour = dto.ToUtcHoursFromTz(14, TimeZoneInfo.Utc);
        Assert.Equal(14, utcHour);
    }

    [Fact]
    public void ToWindow_delay_and_subtraction_applied()
    {
        var now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        (DateTimeOffset startAt, DateTimeOffset endAt) = now.ToWindow(2, 3, UnitOfTime.Day);
        Assert.Equal(10, startAt.Day);
        Assert.Equal(13, endAt.Day);
    }

    [Fact]
    public void ToDateOnly_strips_time()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 23, 59, 59, TimeSpan.Zero);
        DateOnly date = dto.ToDateOnly();
        Assert.Equal(2024, date.Year);
        Assert.Equal(6, date.Month);
        Assert.Equal(15, date.Day);
    }

    [Fact]
    public void ToStartOf_and_ToEndOf_match_Trim_and_TrimEnd()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 14, 30, 45, TimeSpan.Zero);
        Assert.Equal(dto.Trim(UnitOfTime.Day), dto.ToStartOf(UnitOfTime.Day));
        Assert.Equal(dto.TrimEnd(UnitOfTime.Day), dto.ToEndOf(UnitOfTime.Day));
    }
}
