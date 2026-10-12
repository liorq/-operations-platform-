using System.Globalization;

namespace OperationsPlatform.Core.Executors;

public static class NumberParser
{
    public static bool TryParse(string text, out double number) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number) && double.IsFinite(number);
}
