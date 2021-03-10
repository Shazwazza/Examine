using System;
using System.Diagnostics;

namespace Examine.Logging
{
    public class TraceLoggingService : ILoggingService
    {
        public void Log(LogEntry logEntry)
        {
            Trace.WriteLine($"{Enum.GetName(typeof(LogLevel),logEntry.Level)}: {logEntry.Message} {logEntry.Exception?.ToString()}");
        }
    }
}