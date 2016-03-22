using System;
using System.Collections.Generic;
using System.Threading;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Cru
{
    /// <summary>
    /// Near real time manager
    /// </summary>
    internal class NrtManager : IDisposable
    {
        private const long MaxSearcherGen = long.MaxValue;

        private readonly IndexWriter _writer;

        private long _indexingGen = 1;

        private readonly List<IWaitingListener> _waitingListeners = new List<IWaitingListener>();

        private readonly object _reopenLock = new object();

        //The Java condition _newGeneration is not used in this port. Monitor.Wait and PulseAll suffice.

        private readonly SearcherManagerRef _withDeletes;
        private readonly SearcherManagerRef _withoutDeletes;

        /// <summary>
        /// This function is called when an operation is performed on the indexwriter.
        /// </summary>
        public Action<NrtManager, long> Tracker { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warmer"></param>
        public NrtManager(IndexWriter writer, ISearcherWarmer warmer = null)
        {
            _writer = writer;
            _withDeletes = _withoutDeletes = new SearcherManagerRef(true, 0, new SearcherManager(writer, true, warmer));
        }

        public interface IWaitingListener
        {
            void Waiting(bool needsDeletes, long targetGen);
        }

        public void AddWaitingListener(IWaitingListener listener)
        {
            lock (_waitingListeners)
            {
                _waitingListeners.Add(listener);
            }
        }

        public void RemoveWaitingListener(IWaitingListener listener)
        {
            lock (_waitingListeners)
            {
                _waitingListeners.Remove(listener);
            }
        }

        private long Track(long generation)
        {
            if (Tracker != null)
            {
                Tracker(this, generation);
            }
            return generation;
        }

        public long UpdateDocument(Term term, Document doc)
        {
            _writer.UpdateDocument(term, doc);

            return Track(_indexingGen);
        }

        public long UpdateDocument(Term term, Document doc, Analyzer a)
        {
            _writer.UpdateDocument(term, doc, a);

            return Track(_indexingGen);
        }


        public long DeleteDocuments(params Query[] queries)
        {
            _writer.DeleteDocuments(queries);

            return Track(_indexingGen);
        }

        public long DeleteDocuments(params Term[] terms)
        {
            _writer.DeleteDocuments(terms);

            return Track(_indexingGen);
        }

        public long DeleteAll()
        {
            _writer.DeleteAll();

            return Track(_indexingGen);
        }

        public long AddDocument(Document doc)
        {
            _writer.AddDocument(doc);

            return Track(_indexingGen);
        }

        public long AddDocument(Document doc, Analyzer a)
        {
            _writer.AddDocument(doc, a);

            return Track(_indexingGen);
        }

        public long AddIndexesNoOptimize(params Directory[] directories)
        {
            _writer.AddIndexesNoOptimize(directories);

            return Track(_indexingGen);
        }

        public long AddIndexes(params IndexReader[] readers)
        {
            _writer.AddIndexes(readers);

            return Track(_indexingGen);
        }

        public SearcherManager WaitForGeneration(long targetGen, bool requireDeletes = true)
        {
            return WaitForGeneration(targetGen, requireDeletes, TimeSpan.Zero);
        }

        public SearcherManager WaitForGeneration(long targetGen, bool requireDeletes, TimeSpan time)
        {
            var curGen = _indexingGen;
            if (targetGen > curGen)
            {
                throw new ArgumentException("targetGen=" + targetGen +
                                            " was never returned by this NRTManager instance (current gen=" + curGen +
                                            ")");
            }

            lock (_reopenLock)
            {
                if (targetGen > GetCurrentSearchingGen(requireDeletes))
                {
                    foreach (var listener in _waitingListeners)
                    {
                        listener.Waiting(requireDeletes, targetGen);
                    }
                    while (targetGen > GetCurrentSearchingGen(requireDeletes))
                    {
                        if (!WaitOnGenCondition(time))
                        {
                            return GetSearcherManager(requireDeletes);
                        }
                    }
                }
            }

            return GetSearcherManager(requireDeletes);
        }

        private bool WaitOnGenCondition(TimeSpan time)
        {

            if (time == TimeSpan.Zero)
            {
                Monitor.Wait(_reopenLock);
                return true;
            }

            return Monitor.Wait(_reopenLock, time);
        }

        public long GetCurrentSearchingGen(bool applyAllDeletes)
        {
            if (applyAllDeletes)
            {
                return _withDeletes.Generation;
            }
            else
            {
                return Math.Max(_withoutDeletes.Generation, _withDeletes.Generation);
            }
        }

        public bool MaybeReopen(bool applyAllDeletes)
        {
            if (Monitor.TryEnter(_reopenLock))
            {
                try
                {
                    var reference = applyAllDeletes ? _withDeletes : _withoutDeletes;
                    // Mark gen as of when reopen started:                
                    var newSearcherGen = _indexingGen;
                    Interlocked.Increment(ref _indexingGen);

                    bool setSearchGen;
                    if (reference.Generation == MaxSearcherGen)
                    {
                        Monitor.PulseAll(_reopenLock); // wake up threads if we have a new generation
                        return false;
                    }
                    if (!(setSearchGen = reference.Manager.IsSearcherCurrent))
                    {
                        setSearchGen = reference.Manager.MaybeReopen();
                    }
                    if (setSearchGen)
                    {
                        reference.Generation = newSearcherGen; // update searcher gen
                        Monitor.PulseAll(_reopenLock); // wake up threads if we have a new generation
                    }
                    return setSearchGen;
                }
                finally
                {
                    Monitor.Exit(_reopenLock);
                }
            }
            return false;
        }

        public SearcherManager GetSearcherManager(bool applyAllDeletes = true)
        {
            if (applyAllDeletes)
            {
                return _withDeletes.Manager;
            }
            else
            {
                if (_withDeletes.Generation > _withoutDeletes.Generation)
                {
                    return _withDeletes.Manager;
                }
                else
                {
                    return _withoutDeletes.Manager;
                }
            }
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_reopenLock)
            {
                try
                {
                    var disposeActions = new List<Action>
                        {
                            _withDeletes.Dispose
                        };
                    
                    if (_withoutDeletes != _withDeletes)
                    {
                        disposeActions.Add(_withoutDeletes.Dispose);
                    }
                    
                    DisposeUtil.PostponeExceptions(disposeActions.ToArray());
                }
                finally
                {
                    // make sure we signal even if close throws an exception
                    Monitor.PulseAll(_reopenLock);
                }
            }
        }

        private class SearcherManagerRef : IDisposable
        {
            private bool ApplyDeletes { get; set; }
            public long Generation { get; set; }
            public SearcherManager Manager { get; private set; }

            public SearcherManagerRef(bool applyDeletes, long generation, SearcherManager manager)
            {
                ApplyDeletes = applyDeletes;
                Generation = generation;
                Manager = manager;
            }

            public void Dispose()
            {
                Generation = MaxSearcherGen; // max it out to make sure nobody can wait on another gen
                Manager.Dispose();
            }
        }

    }
}