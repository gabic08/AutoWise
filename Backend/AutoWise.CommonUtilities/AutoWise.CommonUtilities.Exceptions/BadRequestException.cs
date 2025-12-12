namespace AutoWise.CommonUtilities.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception? innereException) : base(message, innereException)
    {
    }
}
