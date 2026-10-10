namespace OperationsPlatform.Common.Constants;

/// <summary>Size limits for what a caller may send. One place, so the validators, the database and the middleware always agree.</summary>
public static class InputLimits
{
    /// <summary>Longest allowed value for field A or field B, in characters.</summary>
    public const int MaxInputLength = 500;

    /// <summary>Longest result that is saved in the history, in characters.</summary>
    public const int MaxResultLength = 2000;

    public const int MinKeyLength = 2;
    public const int MaxKeyLength = 50;
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 300;
    public const int MaxDefinitionLength = 4000;
    public const int MaxEnumTextLength = 20;
}
