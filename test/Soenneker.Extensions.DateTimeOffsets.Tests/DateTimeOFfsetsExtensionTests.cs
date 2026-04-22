using System;
using System.Globalization;
using AwesomeAssertions;
using Soenneker.Enums.UnitOfTime;
using Soenneker.Extensions.DateTimeOffsets;
using Soenneker.Tests.Unit;

namespace Soenneker.Extensions.DateTimeOffsets.Tests;

public sealed class DateTimeOFfsetsExtensionTests : UnitTest
{
    private static TimeZoneInfo GetEastern() =>
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Eastern Standard Time" : "America/New_York");

    [Test]
    public void ToTz_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToTz(null!));
    }

    [Test]
    public void ToUtc_preserves_instant()
    {
        var withOffset = new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.FromHours(-4));
        DateTimeOffset utc = withOffset.ToUtc();
        utc.Offset.Should().Be(TimeSpan.Zero);
        utc.Year.Should().Be(2024);
        utc.Hour.Should().Be(12);
    }

    [Test]
    public void ToAge_same_instant_returns_zero()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        double age = dto.ToAge(UnitOfTime.Day, dto);
        age.Should().Be(0);
    }

    [Test]
    public void ToAge_reversed_returns_negative()
    {
        var from = new DateTimeOffset(2024, 6, 20, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        double age = from.ToAge(UnitOfTime.Day, to);
        age.Should().BeLessThan(0);
        age.Should().BeApproximately(-5, 0.5);
    }

    [Test]
    public void ToAge_month_unit_across_leap_year_february()
    {
        var from = new DateTimeOffset(2024, 2, 15, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero);
        double months = from.ToAge(UnitOfTime.Month, to);
        months.Should().BeGreaterThan(0);
        months.Should().BeLessThanOrEqualTo(1.1);
    }

    [Test]
    public void MonthsBetween_same_instant_returns_zero()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 30, 0, TimeSpan.Zero);
        DateTimeOffsetExtension.MonthsBetween(dto, dto).Should().Be(0d);
    }

    [Test]
    public void MonthsBetween_reversed_order_returns_negative()
    {
        var from = new DateTimeOffset(2024, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
        double m = DateTimeOffsetExtension.MonthsBetween(from, to);
        m.Should().BeLessThan(0);
    }

    [Test]
    public void WholeMonthsBetween_jan_31_to_mar_30_returns_one()
    {
        var from = new DateTimeOffset(2024, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 3, 30, 0, 0, 0, TimeSpan.Zero);
        int whole = DateTimeOffsetExtension.WholeMonthsBetween(from, to);
        whole.Should().Be(1);
    }

    [Test]
    public void YearsBetween_same_instant_returns_zero()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffsetExtension.YearsBetween(dto, dto).Should().Be(0d);
    }

    [Test]
    public void QuartersBetween_reversed_returns_negative()
    {
        var from = new DateTimeOffset(2024, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);
        double q = DateTimeOffsetExtension.QuartersBetween(from, to);
        q.Should().BeLessThan(0);
    }

    [Test]
    public void IsBusinessDay_with_null_zone_uses_offset_day_of_week()
    {
        var saturday = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero); // Saturday
        bool isBusiness = saturday.IsBusinessDay(zone: null, CultureInfo.InvariantCulture);
        isBusiness.Should().BeFalse();
    }

    [Test]
    public void AddBusinessDays_zero_returns_same_value()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = dto.AddBusinessDays(0);
        result.Should().Be(dto);
    }

    [Test]
    public void AddBusinessDays_negative_goes_backward()
    {
        var friday = new DateTimeOffset(2024, 6, 14, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = friday.AddBusinessDays(-1);
        (result < friday).Should().BeTrue();
        result.DayOfWeek.Should().Be(DayOfWeek.Thursday);
    }

    [Test]
    public void IsBetween_reversed_start_end_swaps_internally()
    {
        var start = new DateTimeOffset(2024, 6, 20, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 6, 10, 0, 0, 0, TimeSpan.Zero);
        var value = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        value.IsBetween(start, end, inclusive: true).Should().BeTrue();
    }

    [Test]
    public void IsBetween_exclusive_excludes_boundaries()
    {
        var start = new DateTimeOffset(2024, 6, 10, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 6, 20, 0, 0, 0, TimeSpan.Zero);
        start.IsBetween(start, end, inclusive: false).Should().BeFalse();
        end.IsBetween(start, end, inclusive: false).Should().BeFalse();
        new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero).IsBetween(start, end, inclusive: false).Should().BeTrue();
    }

    [Test]
    public void Trim_decade_rounds_down_year()
    {
        var dto = new DateTimeOffset(2025, 7, 15, 12, 30, 0, TimeSpan.Zero);
        DateTimeOffset trimmed = dto.Trim(UnitOfTime.Decade);
        trimmed.Year.Should().Be(2020);
        trimmed.Month.Should().Be(1);
        trimmed.Day.Should().Be(1);
        trimmed.Hour.Should().Be(0);
    }

    [Test]
    public void Trim_quarter_first_month_of_quarter()
    {
        var dto = new DateTimeOffset(2024, 5, 20, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset trimmed = dto.Trim(UnitOfTime.Quarter);
        trimmed.Year.Should().Be(2024);
        trimmed.Month.Should().Be(4);
        trimmed.Day.Should().Be(1);
    }

    [Test]
    public void TrimEnd_month_last_tick_of_month()
    {
        var dto = new DateTimeOffset(2024, 2, 15, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset endOfMonth = dto.TrimEnd(UnitOfTime.Month);
        endOfMonth.Year.Should().Be(2024);
        endOfMonth.Month.Should().Be(2);
        endOfMonth.Day.Should().Be(29); // 2024 leap year
        endOfMonth.Hour.Should().Be(23);
        endOfMonth.Minute.Should().Be(59);
        endOfMonth.Second.Should().Be(59);
    }

    [Test]
    public void Trim_week_monday_start()
    {
        var wednesday = new DateTimeOffset(2024, 6, 12, 14, 0, 0, TimeSpan.Zero);
        DateTimeOffset startOfWeek = wednesday.Trim(UnitOfTime.Week);
        startOfWeek.DayOfWeek.Should().Be(DayOfWeek.Monday);
        startOfWeek.Hour.Should().Be(0);
        startOfWeek.Minute.Should().Be(0);
    }

    [Test]
    public void Add_zero_returns_same()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        dto.Add(0, UnitOfTime.Day).Should().Be(dto);
        dto.Add(0, UnitOfTime.Month).Should().Be(dto);
    }

    [Test]
    public void Subtract_positive_value_goes_backward()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = dto.Subtract(2, UnitOfTime.Day);
        (result < dto).Should().BeTrue();
        result.Day.Should().Be(13);
    }

    [Test]
    public void Add_fractional_month_uses_days_in_month()
    {
        var dto = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = dto.Add(0.5, UnitOfTime.Month);
        result.Year.Should().Be(2024);
        result.Month.Should().Be(1);
        // 0.5 * 31 days = 15.5 days → Jan 16 12:00
        result.Day.Should().Be(16);
        result.Hour.Should().Be(12);
    }

    [Test]
    public void ToDateAsInteger_min_value()
    {
        var dto = DateTimeOffset.MinValue;
        int ymd = dto.ToDateAsInteger();
        ymd.Should().Be(10101);
    }

    [Test]
    public void ToDateAsInteger_max_value()
    {
        var dto = DateTimeOffset.MaxValue;
        int ymd = dto.ToDateAsInteger();
        ymd.Should().Be(99991231);
    }

    [Test]
    public void ToTzOffset_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToTzOffset(null!));
    }

    [Test]
    public void ToUtcHoursFromTz_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToUtcHoursFromTz(14, null!));
    }

    [Test]
    public void ToUtcHoursFromTz_throws_when_tzHour_out_of_range()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() => dto.ToUtcHoursFromTz(24, TimeZoneInfo.Utc));
        Assert.Throws<ArgumentOutOfRangeException>(() => dto.ToUtcHoursFromTz(-1, TimeZoneInfo.Utc));
    }

    [Test]
    public void ToUtcHoursFromTz_UTC_returns_same_hour()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero);
        int utcHour = dto.ToUtcHoursFromTz(14, TimeZoneInfo.Utc);
        utcHour.Should().Be(14);
    }

    [Test]
    public void ToWindow_delay_and_subtraction_applied()
    {
        var now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        (DateTimeOffset startAt, DateTimeOffset endAt) = now.ToWindow(2, 3, UnitOfTime.Day);
        startAt.Day.Should().Be(10);
        endAt.Day.Should().Be(13);
    }

    [Test]
    public void ToDateOnly_strips_time()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 23, 59, 59, TimeSpan.Zero);
        DateOnly date = dto.ToDateOnly();
        date.Year.Should().Be(2024);
        date.Month.Should().Be(6);
        date.Day.Should().Be(15);
    }

    [Test]
    public void ToStartOf_and_ToEndOf_match_Trim_and_TrimEnd()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 14, 30, 45, TimeSpan.Zero);
        dto.ToStartOf(UnitOfTime.Day).Should().Be(dto.Trim(UnitOfTime.Day));
        dto.ToEndOf(UnitOfTime.Day).Should().Be(dto.TrimEnd(UnitOfTime.Day));
    }
}
