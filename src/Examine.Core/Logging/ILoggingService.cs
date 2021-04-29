namespace Examine.Logging
{
    // TODO: Kill this and just use MS Logging

    public interface ILoggingService
    {
        void Log(LogEntry logEntry);
    }
}