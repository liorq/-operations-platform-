using OperationsPlatform.Common.Constants;

namespace OperationsPlatform.Core.Time;

public class IsraelTimeProvider : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneIds.Israel);
}
