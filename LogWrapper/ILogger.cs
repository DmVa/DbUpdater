using System;

namespace LogWrapper
{
   public interface ILogger
   {
       void Log(LogLevel level, string message);
       void LogError(string message, Exception ex);
   }
}
