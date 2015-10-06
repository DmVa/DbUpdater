using System;

namespace LogWrapper
{
    public class NLogWrapper: ILogger
    {
        private readonly NLog.Logger _logger;
        
        public NLogWrapper(NLog.Logger logger)
        {
            _logger = logger;
        }

        #region ILogger Members

        public void Log(LogLevel level, string message)
        {
            if (_logger == null)
                return;

            _logger.Log(ToNlogLevel(level), message);
        }

        public void LogError(string message, Exception ex)
        {
            if (_logger == null)
                return;

            _logger.Error(message, ex);
        }

        #endregion

        private static NLog.LogLevel ToNlogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Info:
                    return NLog.LogLevel.Info;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
            }

            // Null should never happen. Exception will raise in switch if do not any value is valid.
            return null;
        }
    }
}
