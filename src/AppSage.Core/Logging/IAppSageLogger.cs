namespace AppSage.Core.Logging
{
    public interface IAppSageLogger
    {
        void LogInformation(string message);
        void LogInformation(string message, params object?[]? propertyValues);
        void LogWarning(string message);
        void LogWarning(string message, params object?[]? propertyValues);
        void LogError(string message, Exception exception = null);
        void LogError(string message, params object?[]? propertyValues);
        void LogDebug(string message);
        void LogDebug(string message, params object?[]? propertyValues);
    }
}
