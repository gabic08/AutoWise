namespace AutoWise.CommonUtilities.Exceptions;

public class BadRequestWithMultipleFailuresException : Exception
{
    public BadRequestWithMultipleFailuresException(string message) : base(message)
    {
    }

    public BadRequestWithMultipleFailuresException(string message, Exception innerException) : base(message, innerException)
    {
    }
}