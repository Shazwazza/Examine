using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Index;

namespace Examine.Lucene.Providers
{
    /// <summary>
    /// This queues up a commit for the index so that a commit doesn't happen on every individual write since that is quite expensive
    /// </summary>
    internal class IndexCommitter : DisposableObjectSlim, IIndexCommitter
    {
        private DateTime _timestamp;
        private Timer? _timer;
        private readonly object _locker = new object();
        private readonly LuceneIndex _index;
        private readonly IndexWriter _taxonomyIndexWriter;
        private readonly CancellationToken _cancellationToken;
        private const int WaitMilliseconds = 1000;

        public IndexCommitter(
            LuceneIndex index,
            IndexWriter taxonomyIndexWriter,
            CancellationToken cancellationToken)
        {
            _index = index;
            _taxonomyIndexWriter = taxonomyIndexWriter;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// The maximum time period that will elapse until we must commit (5 mins)
        /// </summary>
        private const int MaxWaitMilliseconds = 300000;

        public event EventHandler<IndexingErrorEventArgs>? CommitError;
        public event EventHandler? Committed;

        /// <inheritdoc/>
        public void CommitNow()
        {
            _taxonomyIndexWriter.Commit();
            _index.IndexWriter.IndexWriter.Commit();
            Committed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void ScheduleCommit()
        {
            lock (_locker)
            {
                if (_timer == null)
                {
                    //if we've been cancelled then be sure to commit now
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        // perform the commit
                        CommitNow();
                    }
                    else
                    {
                        //It's the initial call to this at the beginning or after successful commit
                        _timestamp = DateTime.Now;
                        _timer = new Timer(_ => TimerRelease());
                        _timer.Change(WaitMilliseconds, 0);
                    }
                }
                else
                {
                    //if we've been cancelled then be sure to cancel the timer and commit now
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        //Stop the timer
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer.Dispose();
                        _timer = null;

                        //perform the commit
                        CommitNow();
                    }
                    else if (
                        // must be less than the max
                        DateTime.Now - _timestamp < TimeSpan.FromMilliseconds(MaxWaitMilliseconds) &&
                        // and less than the delay
                        DateTime.Now - _timestamp < TimeSpan.FromMilliseconds(WaitMilliseconds))
                    {
                        //Delay  
                        _timer.Change(WaitMilliseconds, 0);
                    }
                    else
                    {
                        //Cannot delay! the callback will execute on the pending timeout
                    }
                }
            }
        }

        private void TimerRelease()
        {
            lock (_locker)
            {
                //if the timer is not null then a commit has been scheduled
                if (_timer != null)
                {
                    //Stop the timer
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _timer.Dispose();
                    _timer = null;

                    try
                    {
                        //perform the commit
                        CommitNow();

                        // after the commit, refresh the searcher
                        _index.WaitForChanges();
                    }
                    catch (Exception e)
                    {
                        // It is unclear how/why this happens but probably indicates index corruption
                        // see https://github.com/Shazwazza/Examine/issues/164
                        CommitError?.Invoke(this, new IndexingErrorEventArgs(
                            _index,
                            "An error occurred during the index commit operation, if this error is persistent then index rebuilding is necessary",
                            "-1",
                            e));
                    }
                }
            }
        }

        protected override void DisposeResources()
        {
            TimerRelease();
            Committed = null;
            CommitError = null;
        }
    }
}
