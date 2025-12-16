namespace AutoWise.CommonUtilities.ExtensionMethods;

public static class LoggerExtensionMethods
{
    public static void LogInformationMessage(this ILogger logger, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(message, args);
        }
    }

    public static void LogErrorMessage(this ILogger logger, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(message, args);
        }
    }
}
