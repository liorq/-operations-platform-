using OperationsPlatform.Common.Constants;

namespace OperationsPlatform.Core.Time;

/// <summary>
/// The platform's clock. Its local time zone is Israel on every machine, so "the start of the month"
/// means midnight of the 1st in Israel, not in the server's own zone and not in UTC.
/// </summary>
public class IsraelTimeProvider : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneIds.Israel);
}
