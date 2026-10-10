using OperationsPlatform.Core.Executors;
using Xunit;

namespace OperationsPlatform.Tests;

public class TextFunctionsTests
{
    [Theory]
    [InlineData("", "", 0)]
    [InlineData("abc", "", 3)]
    [InlineData("", "abc", 3)]
    [InlineData("שלום", "שלום", 0)]
    public void Levenshtein_IsTheEditDistance(string a, string b, int expected)
    {
        Assert.Equal(expected, TextFunctions.Levenshtein(a, b));
    }

    [Theory]
    [InlineData("Dormitory", "Dirty room!", true)]
    [InlineData("abc", "abd", false)]
    public void IsAnagram_IgnoresCaseSpacesAndPunctuation(string a, string b, bool expected)
    {
        Assert.Equal(expected, TextFunctions.IsAnagram(a, b));
    }

    [Fact]
    public void EveryListedName_CanBeRun()
    {
        foreach (string name in TextFunctions.Names)
        {
            Assert.NotNull(TextFunctions.Run(name, "ab", "ba"));
        }
    }
}
