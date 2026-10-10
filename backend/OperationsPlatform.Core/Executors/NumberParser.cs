using System.Globalization;

namespace OperationsPlatform.Core.Executors;

/// <summary>The one way the platform reads a number typed by a caller: invariant culture, finite values only.</summary>
public static class NumberParser
{
    public static bool TryParse(string text, out double number) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number) && double.IsFinite(number);
}
