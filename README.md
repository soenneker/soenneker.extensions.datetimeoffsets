[![](https://img.shields.io/nuget/v/soenneker.extensions.datetimeoffsets.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.datetimeoffsets/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.datetimeoffsets/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.datetimeoffsets/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.datetimeoffsets.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.datetimeoffsets/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.datetimeoffsets/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.datetimeoffsets/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.DateTimeOffsets
A collection of helpful DateTimeOffset extension methods.

## Installation

```bash
dotnet add package Soenneker.Extensions.DateTimeOffsets
```

## Quick start

```csharp
using Soenneker.Extensions.DateTimeOffsets;

DateTimeOffset value = DateTimeOffset.UtcNow;
var result = value.ToHourFormat();
```

## Common operations

- `ToHourFormat()` - Formats as `hh tt`. Returns a string in the form `hh tt`.
- `ToHourMinuteFormat()` - Formats as `h:mm tt`. Returns a string in the form `h:mm tt`.
- `ToHourMinuteSecondFormat()` - Formats as `h:mm:ss tt`. Returns a string in the form `h:mm:ss tt`.
- `To24HourFormat()` - Formats as `HH`. Returns a string in the form `HH`.
- `To24HourMinuteFormat()` - Formats as `HH:mm`. Returns a string in the form `HH:mm`.
- `To24HourMinuteSecondFormat()` - Formats as `HH:mm:ss`. Returns a string in the form `HH:mm:ss`.
- `ToDateFormat()` - Formats as `MM/dd/yyyy`. Returns a string in the form `MM/dd/yyyy`.
- `ToDateDashFormat()` - Formats as `MM-dd-yyyy`. Returns a string in the form `MM-dd-yyyy`.
- `ToYearMonthDayFormat()` - Formats as `yyyy-MM-dd`. Returns a string in the form `yyyy-MM-dd`.
- `ToDateHourFormat()` - Formats as `MM/dd/yyyy h tt`. Returns a string in the form `MM/dd/yyyy h tt`.
- `ToDateHourMinuteFormat()` - Formats as `MM/dd/yyyy h:mm tt`. Returns a string in the form `MM/dd/yyyy h:mm tt`.
- `ToDateHourMinuteSecondFormat()` - Formats as `MM/dd/yyyy h:mm:ss tt`. Returns a string in the form `MM/dd/yyyy h:mm:ss tt`.

The package also includes 70 additional operations for more specialized cases.
