using System;
using Lucene.Net.Replicator;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene
{
    /// <summary>
    /// Custom replication client that logs
    /// </summary>
    public class LoggingReplicationClient : ReplicationClient
    {
        private readonly ILogger<LoggingReplicationClient> _logger;

        /// <inheritdoc/>
        public LoggingReplicationClient(
            ILogger<LoggingReplicationClient> logger,
            IReplicator replicator,
            IReplicationHandler handler,
            ISourceDirectoryFactory factory)
            : base(replicator, handler, factory)
        {
            _logger = logger;
            InfoStream = new CustomLoggingInfoStream(logger);
        }

        /// <inheritdoc/>
        protected override void HandleUpdateException(Exception exception)
            => _logger.LogError(exception, "Index replication error occurred");

        private class CustomLoggingInfoStream : LoggingInfoStream<LoggingReplicationClient>
        {
            public CustomLoggingInfoStream(ILogger<LoggingReplicationClient> logger) : base(logger)
            {
            }

            public override void Message(string component, string message)
            {
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    // don't log this, it means there is no session
                    if (!message.EndsWith("="))
                    {
                        base.Message(component, message);
                    }
                }
            }
        }
    }
}
