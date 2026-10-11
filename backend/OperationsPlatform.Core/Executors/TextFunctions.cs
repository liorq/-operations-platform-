using OperationsPlatform.Common.Constants;

namespace OperationsPlatform.Core.Executors;

/// <summary>
/// Pure static string functions: (A, B) in, text out.
/// To offer a new text operation, add its name to <see cref="Names"/> and a case to <see cref="Run"/>.
/// </summary>
public static class TextFunctions
{
    private const string AnagramYes = "Yes, an anagram";
    private const string AnagramNo = "Not an anagram";

    public static readonly string[] Names = { TextFunctionNames.Concat, TextFunctionNames.Reverse, TextFunctionNames.Levenshtein, TextFunctionNames.Anagram };

    public static string Run(string name, string a, string b)
        => name.ToLowerInvariant() switch
        {
            TextFunctionNames.Concat => a + b,
            TextFunctionNames.Reverse => Reverse(a),
            TextFunctionNames.Levenshtein => Levenshtein(a, b).ToString(),
            TextFunctionNames.Anagram => IsAnagram(a, b) ? AnagramYes : AnagramNo,
            _ => throw new ArgumentException($"Unknown text function: {name}")
        };

    private static string Reverse(string text) => new string(text.Reverse().ToArray());

    /// <summary>
    /// Edit distance: the fewest inserts, deletes and replacements that turn a into b.
    /// Dynamic programming that keeps only the previous row of the table.
    /// </summary>
    public static int Levenshtein(string a, string b)
    {
        int[] previousRow = new int[b.Length + 1];

        for (int j = 0; j <= b.Length; j++)
        {
            previousRow[j] = j;
        }

        for (int i = 1; i <= a.Length; i++)
        {
            int[] currentRow = new int[b.Length + 1];
            currentRow[0] = i;

            for (int j = 1; j <= b.Length; j++)
            {
                int replaceCost = a[i - 1] == b[j - 1] ? 0 : 1;
                int insert = currentRow[j - 1] + 1;
                int delete = previousRow[j] + 1;
                int replace = previousRow[j - 1] + replaceCost;

                currentRow[j] = Math.Min(Math.Min(insert, delete), replace);
            }

            previousRow = currentRow;
        }

        return previousRow[b.Length];
    }

    /// <summary>True when both texts have the same letters and digits, ignoring case, spaces and punctuation.</summary>
    public static bool IsAnagram(string a, string b) => Normalize(a) == Normalize(b);

    private static string Normalize(string text)
        => new string(text.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).Order().ToArray());
}
