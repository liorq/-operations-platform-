namespace OperationsPlatform.Common.Exceptions;

/// <summary>The request clashes with data that already exists (for example a duplicate key). It becomes HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
