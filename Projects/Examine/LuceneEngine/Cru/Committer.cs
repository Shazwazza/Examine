using System;
using System.Diagnostics;
using System.Threading;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Cru
{
    internal class Committer : IDisposable
    {
        private readonly IndexWriter _writer;
        private readonly TimeSpan _commitInterval;
        private readonly TimeSpan _optimizeInterval;
        private readonly ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);
        private bool _finish;

        public Committer(IndexWriter writer, TimeSpan commitInterval, TimeSpan optimizeInterval)
        {
            _writer = writer;
            _commitInterval = commitInterval;
            _optimizeInterval = optimizeInterval;
        }

        private bool _optimizeNow = false;
        public void OptimizeNow()
        {
            _optimizeNow = true;
            _waitHandle.Set();
        }

        public void CommitNow()
        {
            _waitHandle.Set();
        }

        public void Start()
        {
            var sw = new Stopwatch();
            var lastOptimize = 0L;
            sw.Start();
            while (!_finish)
            {
                _waitHandle.Reset();


                //TODO: I think we really need a try/catch here, if this fails for whatever reason this loop 
                // somehow still continues and keeps calling this... Need to check with Niels K.
                _writer.Commit();

                if (_optimizeNow || sw.ElapsedTicks - lastOptimize > _optimizeInterval.Ticks)
                {
                    _optimizeNow = false;
                    _writer.Optimize(2, false);
                    lastOptimize = sw.ElapsedTicks;
                }

                _waitHandle.Wait(_commitInterval);
            }
            sw.Stop();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _finish = true;
            _waitHandle.Set();
        }
    }
}