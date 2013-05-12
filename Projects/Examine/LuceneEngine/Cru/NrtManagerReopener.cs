using System;
using System.Diagnostics;
using System.Threading;

namespace Lucene.Net.Contrib.Management
{
    public class NrtManagerReopener : NrtManager.IWaitingListener, IDisposable
    {
        private readonly NrtManager _manager;
        private readonly TimeSpan _targetMaxStale;
        private readonly TimeSpan _targetMinStale;
        private bool _finish = false;
        private long _waitingGen;
        private bool _waitingNeedsDeletes;

        private ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);

        public NrtManagerReopener(NrtManager manager, TimeSpan targetMaxStale, TimeSpan targetMinStale)
        {
            if (targetMaxStale < targetMinStale)
            {
                throw new ArgumentException("targetMaxScaleSec (= " + targetMaxStale + ") < targetMinStaleSec (=" + targetMinStale + ")");
            }

            _manager = manager;
            _targetMaxStale = targetMaxStale;
            _targetMinStale = targetMinStale;
            _manager.AddWaitingListener(this);
        }

        public void Waiting(bool needsDeletes, long targetGen)
        {
            _waitingNeedsDeletes |= needsDeletes;
            _waitingGen = Math.Max(_waitingGen, targetGen);

            _waitHandle.Set();
        }


        public void Start()
        {
            var sw = new Stopwatch();
            var lastReopen = 0L;
            sw.Start();

            while (true)
            {

                var hasWaiting = false;

                _waitHandle.Reset();
                // TODO: try to guestimate how long reopen might
                // take based on past data?

                while (!_finish)
                {
                    //System.out.println("reopen: cycle");

                    // True if we have someone waiting for reopen'd searcher:
                    hasWaiting = _waitingGen > _manager.GetCurrentSearchingGen(_waitingNeedsDeletes);
                    var nextReopenStart = lastReopen + (hasWaiting ? _targetMinStale.Ticks : _targetMaxStale.Ticks);

                    var sleep = nextReopenStart - sw.ElapsedTicks;

                    if (sleep > 0)
                    {
                        //System.out.println("reopen: sleep " + (sleepNS/1000000.0) + " ms (hasWaiting=" + hasWaiting + ")");
                        try
                        {
                            _waitHandle.Wait(new TimeSpan(sleep));
                        }
                        catch (ThreadInterruptedException)
                        {
                            Thread.CurrentThread.Interrupt();
                            //System.out.println("NRT: set finish on interrupt");
                            _finish = true;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (_finish)
                {
                    //System.out.println("reopen: finish");
                    return;
                }
                //System.out.println("reopen: start hasWaiting=" + hasWaiting);

                lastReopen = sw.ElapsedTicks;
                _manager.MaybeReopen(_waitingNeedsDeletes);
            }
        }


        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _manager.RemoveWaitingListener(this);
            _finish = true;
            _waitHandle.Set();
        }
    }
}