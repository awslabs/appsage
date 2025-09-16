namespace AppSage.Core.Logging
{
    public interface IAppSageLogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception = null);
        void LogDebug(string message);
    }
}
