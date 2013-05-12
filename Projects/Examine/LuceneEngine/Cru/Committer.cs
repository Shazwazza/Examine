using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using Lucene.Net.Index;

namespace LuceneManager.Infrastructure
{
    public class Committer : IDisposable
    {
        private readonly IndexWriter _writer;
        private readonly TimeSpan _commitInterval;
        private readonly TimeSpan _optimizeInterval;
        private ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);
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