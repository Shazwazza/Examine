using System;
using Lucene.Net.Index;


namespace Examine.Lucene.Providers
{
    /// <summary>
    /// Used to prevent the appdomain from crashing when lucene runs into a concurrent merge scheduler failure
    /// </summary>
    internal class ErrorLoggingConcurrentMergeScheduler : ConcurrentMergeScheduler
    {
        private readonly Action<string, Exception> _logger;

        public ErrorLoggingConcurrentMergeScheduler(string indexName, Action<string, Exception> logger)
        {
            IndexName = indexName;
            _logger = logger;
        }
        
        public string IndexName { get; }

        protected override void HandleMergeException(System.Exception exc)
        {
            try
            {
                base.HandleMergeException(exc);
            }
            catch (Exception e)
            {
                _logger($"Concurrent merge failed for index: {IndexName} if this error is persistent then index rebuilding is necessary", e);
            }
        }
    }

    
}

