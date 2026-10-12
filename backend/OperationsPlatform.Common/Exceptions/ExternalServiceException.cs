namespace OperationsPlatform.Common.Exceptions;

public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message)
    {
    }
}
