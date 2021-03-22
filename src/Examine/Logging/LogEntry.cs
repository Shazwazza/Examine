using System;
namespace Examine.Logging
{
    public struct LogEntry
    {
        public LogLevel Level { get; }
        public string Message { get; }
        public Exception Exception { get; }

        public LogEntry(LogLevel level, Exception exception, string message)
        {
            Level = level;
            Message = message;
            Exception = exception;
        }
    }
}