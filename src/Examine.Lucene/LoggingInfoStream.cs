using Lucene.Net.Util;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene
{
    internal class LoggingInfoStream<T> : InfoStream
    {
        private readonly LogLevel _logLevel;

        public LoggingInfoStream(ILogger<T> logger, LogLevel logLevel)
        {
            Logger = logger;
            _logLevel = logLevel;
        }

        public ILogger<T> Logger { get; }

        public override bool IsEnabled(string component) => Logger.IsEnabled(_logLevel);

        public override void Message(string component, string message)
        {
            if (Logger.IsEnabled(_logLevel))
            {
                Logger.LogDebug("{Component} - {Message}", component, message);
            }
        }
    }
}
