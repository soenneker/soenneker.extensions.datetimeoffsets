using System;
using Soenneker.Extensions.DateTimeOffsets;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Extensions.DateTimeOffsets.Tests;

public sealed class DateTimeOffsetExtensionFormatTests : UnitTest
{
    private static TimeZoneInfo GetEastern() =>
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Eastern Standard Time" : "America/New_York");

    [Fact]
    public void ToHourFormat_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 14, 30, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToHourFormat(null!));
    }

    [Fact]
    public void ToPreciseFormat_handles_DateTimeOffset_MinValue()
    {
        var dto = DateTimeOffset.MinValue;
        string result = dto.ToPreciseFormat();
        Assert.Contains("0001-01-01", result);
    }

    [Fact]
    public void ToPreciseFormat_handles_DateTimeOffset_MaxValue()
    {
        var dto = DateTimeOffset.MaxValue;
        string result = dto.ToPreciseFormat();
        Assert.Contains("9999-12-31", result);
    }

    [Fact]
    public void ToMonthFirstDateFormat_midnight()
    {
        var dto = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal("01-01-2024", dto.ToMonthFirstDateFormat());
    }

    [Fact]
    public void ToPreciseUtcFormat_preserves_instant_across_offset()
    {
        var utc = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var est = new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.FromHours(-4));
        Assert.Equal(utc.ToPreciseUtcFormat(), est.ToPreciseUtcFormat());
    }

    [Fact]
    public void ToIso8601_and_ToWebString_are_equal()
    {
        var dto = new DateTimeOffset(2024, 3, 2, 10, 30, 45, 123, TimeSpan.Zero);
        Assert.Equal(dto.ToIso8601(), dto.ToWebString());
    }

    [Fact]
    public void ToIso8601_has_Z_suffix()
    {
        var dto = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.FromHours(5));
        string result = dto.ToIso8601();
        Assert.EndsWith("Z", result);
    }

    [Fact]
    public void ToTzDateTimeFormat_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToTzDateTimeFormat(null!));
    }

    [Fact]
    public void ToTzDateFormat_converts_to_target_zone()
    {
        var utc = new DateTimeOffset(2024, 6, 16, 3, 0, 0, TimeSpan.Zero); // 16th 03:00 UTC
        TimeZoneInfo tz = GetEastern();
        string result = utc.ToTzDateFormat(tz);
        // In EDT (UTC-4), 03:00 UTC = 23:00 on 15th
        Assert.Equal("06/15/2024", result);
    }

    [Fact]
    public void ToTzDateHourFormat_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToTzDateHourFormat(null!));
    }

    [Fact]
    public void ToDateTimeFormatAsTz_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToDateTimeFormatAsTz(null!));
    }

    [Fact]
    public void ToUtcDateTimeFormat_contains_UTC()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.FromHours(-4));
        string result = dto.ToUtcDateTimeFormat();
        Assert.Contains("UTC", result);
    }

    [Fact]
    public void ToTzFileName_throws_when_tz_is_null()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentNullException>(() => dto.ToTzFileName(null!));
    }

    [Fact]
    public void ToFileName_has_no_spaces_or_colons()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 30, 45, TimeSpan.Zero);
        string result = dto.ToFileName();
        Assert.DoesNotContain(" ", result);
        Assert.DoesNotContain(":", result);
        Assert.Contains("--", result);
    }

    [Fact]
    public void ToShortMonthDayYearString_invariant_month_name()
    {
        var dto = new DateTimeOffset(2024, 3, 2, 0, 0, 0, TimeSpan.Zero);
        string result = dto.ToShortMonthDayYearString();
        Assert.StartsWith("Mar", result);
    }

    [Fact]
    public void ToLongMonthDayYearString_invariant_month_name()
    {
        var dto = new DateTimeOffset(2024, 3, 2, 0, 0, 0, TimeSpan.Zero);
        string result = dto.ToLongMonthDayYearString();
        Assert.StartsWith("March", result);
    }

    [Fact]
    public void ToHourFormat_noon_in_UTC_shows_12_PM()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        string result = dto.ToHourFormat(TimeZoneInfo.Utc);
        Assert.Contains("12", result);
        Assert.Contains("PM", result);
    }

    [Fact]
    public void ToHourFormat_midnight_in_UTC_shows_12_AM()
    {
        var dto = new DateTimeOffset(2024, 6, 16, 0, 0, 0, TimeSpan.Zero);
        string result = dto.ToHourFormat(TimeZoneInfo.Utc);
        Assert.Contains("12", result);
        Assert.Contains("AM", result);
    }
}
