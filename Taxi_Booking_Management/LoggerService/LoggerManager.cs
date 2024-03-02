using NLog;

namespace Taxi_Booking_Management.LoggerService
{
    public class LoggerManager : ILoggerManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void LogInfo(string message)
        {
            _logger.Info(message);
        }

        public void LogError(string message, Exception exception = null)
        {
            _logger.Error(exception, message);
        }

        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        public void LogWarning(string message)
        {
            _logger.Warn(message);
        }
    }
}
