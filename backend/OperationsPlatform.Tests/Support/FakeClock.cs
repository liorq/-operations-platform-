using OperationsPlatform.Core.Time;

namespace OperationsPlatform.Tests.Support;

/// <summary>A clock the test can move, with the same Israel time zone as the real one, so "start of the month" is easy to test.</summary>
public sealed class FakeClock : IsraelTimeProvider
{
    public FakeClock(DateTimeOffset start)
    {
        Now = start;
    }

    public DateTimeOffset Now { get; set; }

    public override DateTimeOffset GetUtcNow() => Now;
}
