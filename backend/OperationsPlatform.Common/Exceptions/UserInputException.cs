namespace OperationsPlatform.Common.Exceptions;

/// <summary>A mistake made by the caller (bad input, unknown operation). It becomes HTTP 400, never 500.</summary>
public class UserInputException : Exception
{
    public UserInputException(string message) : base(message)
    {
    }
}
