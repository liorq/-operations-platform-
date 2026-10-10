using OperationsPlatform.Common.Constants;
using OperationsPlatform.Domain.Entities;
using OperationsPlatform.Domain.Enums;

namespace OperationsPlatform.Core.Data;

/// <summary>The operations that exist from the first run. The first EF migration inserts them.</summary>
public static class OperationSeed
{
    private const string WeatherDefinition =
        "{\"url\":\"https://api.open-meteo.com/v1/forecast?latitude={a}&longitude={b}&current=temperature_2m\"," +
        "\"path\":\"current.temperature_2m\"}";

    public static readonly Operation[] Operations =
    {
        Create(1, "add", "חיבור", OperationKind.Expression, InputType.Number, "a + b"),
        Create(2, "subtract", "חיסור", OperationKind.Expression, InputType.Number, "a - b"),
        Create(3, "multiply", "כפל", OperationKind.Expression, InputType.Number, "a * b"),
        Create(4, "divide", "חילוק", OperationKind.Expression, InputType.Number, "a / b"),
        Create(5, "power", "חזקה", OperationKind.Expression, InputType.Number, "Pow(a, b)"),
        Create(6, "hypotenuse", "אלכסון (פיתגורס)", OperationKind.Expression, InputType.Number, "Sqrt(a*a + b*b)"),
        Create(7, "concat", "חיבור מחרוזות", OperationKind.Text, InputType.Text, TextFunctionNames.Concat),
        Create(8, "levenshtein", "מרחק עריכה בין מילים", OperationKind.Text, InputType.Text, TextFunctionNames.Levenshtein),
        Create(9, "anagram", "האם אנגרמה?", OperationKind.Text, InputType.Text, TextFunctionNames.Anagram),
        Create(10, "weather", "טמפרטורה עכשיו (קו רוחב, קו אורך)", OperationKind.Http, InputType.Coordinates, WeatherDefinition)
    };

    private static Operation Create(int id, string key, string name, OperationKind kind, InputType inputType, string definition)
    {
        return new Operation
        {
            Id = id,
            Key = key,
            Name = name,
            Kind = kind,
            InputType = inputType,
            Definition = definition
        };
    }
}
