namespace OperationsPlatform.Common.Exceptions;

/// <summary>The thing the caller asked for does not exist. It becomes HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
