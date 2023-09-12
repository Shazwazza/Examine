using Lucene.Net.Util;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene
{
    internal class LoggingInfoStream<T> : InfoStream
    {
        public LoggingInfoStream(ILogger<T> logger)
        {
            Logger = logger;
        }

        public ILogger<T> Logger { get; }

        public override bool IsEnabled(string component) => Logger.IsEnabled(LogLevel.Debug);
        public override void Message(string component, string message)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("{Component} - {Message}", component, message);
            }
        }
    }
}
