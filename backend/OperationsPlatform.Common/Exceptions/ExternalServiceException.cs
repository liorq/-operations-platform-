namespace OperationsPlatform.Common.Exceptions;

/// <summary>An external service failed or answered something unusable. It becomes HTTP 502, because the caller did nothing wrong.</summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message)
    {
    }
}
