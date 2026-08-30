[![](https://img.shields.io/nuget/v/soenneker.extensions.datetimeoffsets.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.datetimeoffsets/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.datetimeoffsets/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.datetimeoffsets/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.datetimeoffsets.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.datetimeoffsets/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.datetimeoffsets/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.datetimeoffsets/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.DateTimeOffsets

`DateTimeOffset` helpers for time-zone conversion, elapsed and calendar differences, business days, range checks, period boundaries, unit-based arithmetic, offsets, and invariant formatting.

## Installation

```bash
dotnet add package Soenneker.Extensions.DateTimeOffsets
```

## Convert an instant

```csharp
using Soenneker.Extensions.DateTimeOffsets;

DateTimeOffset value = new(2026, 8, 29, 18, 0, 0, TimeSpan.Zero);
TimeZoneInfo eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

DateTimeOffset easternValue = value.ToTz(eastern);
DateTimeOffset utcValue = easternValue.ToUtc();
DateTime utcDateTime = easternValue.ToUtcDateTime();
```

`ToTz()` and `ToUtc()` preserve the instant while changing its displayed offset. `ToUtcDateTime()` returns the same instant as a `DateTime` with `Kind` set to `Utc`.

## Elapsed and calendar differences

```csharp
DateTimeOffset from = new(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
DateTimeOffset to = new(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);

double elapsedDays = from.ToAge(UnitOfTime.Day, to);
double months = DateTimeOffsetExtension.MonthsBetween(from, to);
int completeYears = DateTimeOffsetExtension.WholeYearsBetween(from, to);
```

`ToAge()` supports ticks, microseconds, milliseconds, seconds, minutes, hours, days, weeks, months, quarters, and years. Fixed units use elapsed duration. Calendar units use whole intervals plus fractional progress through the next actual month, quarter, or year.

`MonthsBetween()`, `QuartersBetween()`, and `YearsBetween()` are signed: reversing the endpoints reverses the sign. Their `Whole...Between()` counterparts return a non-negative complete-interval count regardless of argument order.

## Business days

```csharp
CultureInfo culture = CultureInfo.GetCultureInfo("en-US");

bool isBusinessDay = value.IsBusinessDay(eastern, culture);
DateTimeOffset threeBusinessDaysLater = value.AddBusinessDays(3, eastern, culture);
```

Business-day methods skip weekends only; they do not know public holidays. Weekend selection comes from `Soenneker.Extensions.CultureInfos`, whose fixed rules map `ar-*`, `he-IL`, `fa-IR`, and `ur-PK` to Friday–Saturday and every other culture to Saturday–Sunday.

When a time zone is supplied, it is used to decide each candidate instant's local weekday. `AddBusinessDays()` still advances the original `DateTimeOffset` by 24-hour increments and preserves its stored offset; it does not preserve a target-zone wall-clock time across DST or replace the result's offset with the zone's offset.

## Boundaries and ranges

```csharp
DateTimeOffset startOfMonth = value.ToStartOf(UnitOfTime.Month);
DateTimeOffset endOfMonth = value.ToEndOf(UnitOfTime.Month);
bool inside = value.IsBetween(startOfMonth, endOfMonth);
```

`Trim()` / `ToStartOf()` return the first tick of a period. `TrimEnd()` / `ToEndOf()` return one tick before the next period. Supported boundaries are microsecond through decade; weeks begin Monday and quarters begin in January, April, July, and October.

Boundary methods operate in the value's existing offset and preserve that offset. They do not consult a `TimeZoneInfo`, so a fixed offset cannot automatically follow DST changes across a long period.

`IsBetween()` accepts endpoints in either order. Bounds are inclusive by default; pass `inclusive: false` for a strict comparison.

## Unit-based arithmetic

```csharp
DateTimeOffset delayed = value.Add(1.5, UnitOfTime.Hour);
DateTimeOffset previousQuarter = value.Subtract(1, UnitOfTime.Quarter);
(DateTimeOffset startAt, DateTimeOffset endAt) =
    value.ToWindow(delay: 5, subtraction: 30, UnitOfTime.Minute);
```

`Add()` and `Subtract()` support ticks through decades. `DateTimeOffset` stores 100-nanosecond ticks, so sub-tick nanosecond and microsecond portions are truncated. Fractional months, quarters, years, and decades are converted using the length of the calendar month or year reached after the whole portion is applied.

`ToWindow()` subtracts `delay` to produce `endAt`, then subtracts `subtraction` from that endpoint to produce `startAt`.

## Time-zone offsets and local hours

```csharp
TimeSpan offset = value.ToTzOffset(eastern);
double offsetHours = value.ToTzOffsetHours(eastern);
int utcHour = value.ToUtcHoursFromTz(tzHour: 9, eastern);
```

Offset lookup uses the supplied instant, including the zone's daylight-saving rule at that instant. `ToTzOffsetHours()` returns `double`, preserving half-hour and quarter-hour offsets.

`ToUtcHoursFromTz()` chooses the local calendar date corresponding to `value`, interprets `tzHour` on that date, and returns only the UTC hour from `0` through `23`. Invalid local times advance to the first valid minute; ambiguous times choose the earlier UTC instant. Because only an hour is returned, date rollover and minutes are not represented.

## Formatting

Formatting is invariant-culture. Methods without `Tz` format the value's existing clock fields; `ToTz...()` methods first convert the instant to the supplied zone.

Common formats include:

| Methods | Output shape |
| --- | --- |
| `ToHourFormat()`, `ToHourMinuteFormat()`, `ToHourMinuteSecondFormat()` | `hh tt`, `h:mm tt`, `h:mm:ss tt` |
| `To24HourFormat()`, `To24HourMinuteFormat()`, `To24HourMinuteSecondFormat()` | `HH`, `HH:mm`, `HH:mm:ss` |
| `ToDateFormat()`, `ToDateDashFormat()`, `ToYearMonthDayFormat()` | `MM/dd/yyyy`, `MM-dd-yyyy`, `yyyy-MM-dd` |
| `ToSortableMinuteFormat()`, `ToSortableSecondFormat()` | `yyyy-MM-dd HH:mm[:ss]` |
| `ToPreciseFormat()` | `yyyy-MM-ddTHH:mm:ss.fffffff` using the stored offset's clock fields |
| `ToPreciseUtcFormat()` | UTC with seven fractional digits and literal `Z` |
| `ToIso8601()` / `ToWebString()` | UTC with milliseconds and literal `Z` |
| `ToIso8601SecondFormat()`, `ToIso8601MillisFormat()` | Stored clock fields without an offset suffix |
| `ToFileName()`, `ToFileNameMillis()` | File-safe timestamps |

Time-zone display methods that append an abbreviation use `Soenneker.Extensions.TimeZoneInfos`. `ToDateTimeFormatAsTz()` is the exception to conversion: it formats the existing clock fields and uses the supplied zone only for its abbreviation.

`ToDateAsInteger()` and `ToDateOnly()` use the value's stored local calendar fields; they do not normalize to UTC first.
