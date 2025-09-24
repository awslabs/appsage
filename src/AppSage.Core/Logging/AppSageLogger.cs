using Serilog;

namespace AppSage.Core.Logging
{
    public class AppSageLogger : IAppSageLogger
    {
        private readonly ILogger _logger;

        public AppSageLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogInformation(string message)
        {
            _logger.Information(message);
        }

        public void LogInformation(string message, params object?[]? propertyValues)
        {
            _logger.Information(message, propertyValues);
        }

        public void LogWarning(string message)
        {
            _logger.Warning(message);
        }
        public void LogWarning(string message, params object?[]? propertyValues)
        {
            _logger.Warning(message,propertyValues);
        }

        public void LogError(string message, Exception exception = null)
        {
            if (exception != null)
                _logger.Error(exception, message);
            else
                _logger.Error(message);
        }

        public void LogError(string message, params object?[]? propertyValues)
        {
                _logger.Error(message, propertyValues);
        }

        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        public void LogDebug(string message, params object?[]? propertyValues)
        {
            _logger.Debug(message, propertyValues);
        }
    }
}
