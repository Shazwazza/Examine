using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    ///<summary>
    /// Exposes searchers and indexers
    ///</summary>
    public class ExamineManager : IDisposable, IExamineManager
    {
        /// <inheritdoc/>
        public ExamineManager(IEnumerable<IIndex> indexes, IEnumerable<ISearcher> searchers)
        {
            foreach(IIndex i in indexes)
            {
                AddIndex(i);
            }

            foreach(ISearcher s in searchers)
            {
                AddSearcher(s);
            }
        }

        private readonly ConcurrentDictionary<string, IIndex> _indexers = new ConcurrentDictionary<string, IIndex>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<string, ISearcher> _searchers = new ConcurrentDictionary<string, ISearcher>(StringComparer.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        public bool TryGetSearcher(string searcherName, out ISearcher searcher) => 
            (searcher = _searchers.TryGetValue(searcherName, out var s) ? s : null) != null;

        /// <inheritdoc />
        public bool TryGetIndex(string indexName, out IIndex index) => 
            (index = _indexers.TryGetValue(indexName, out var i) ? i : null) != null;

        /// <inheritdoc />
        public IEnumerable<ISearcher> RegisteredSearchers => _searchers.Values;

        /// <inheritdoc />
        public IEnumerable<IIndex> Indexes => _indexers.Values;
       
        private IIndex AddIndex(IIndex index)
        {
            //make sure this name doesn't exist in
            if (!_indexers.TryAdd(index.Name, index))
            {
                throw new InvalidOperationException("The indexer with name " + index.Name + " already exists");
            }

            return index;
        }

        private ISearcher AddSearcher(ISearcher searcher)
        {
            //make sure this name doesn't exist in
            if (!_searchers.TryAdd(searcher.Name, searcher))
            {
                throw new InvalidOperationException("The searcher with name " + searcher.Name + " already exists");
            }

            return searcher;
        }

        /// <summary>
        /// Call this in Application_End.
        /// </summary>
        public void Dispose() => Dispose(true);

        private bool _disposed = false;

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop(false);
                    Stop(true);
                }

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }

        /// <summary>
        /// Used to dispose the manager, can be overridden in web projects to control the unwinding of an appdomain
        /// </summary>
        /// <param name="immediate">true to indicate the registered object should unregister from the hosting environment before returning; otherwise, false.</param>
        public virtual void Stop(bool immediate)
        {
            if (immediate)
            {
                try
                {
                    //This is sort of a hack at the moment. We are disposing the searchers at the last possible point in time because there might
                    // still be pages executing when 'immediate' == false. In which case, when we close the readers, exceptions will occur
                    // if the search results are still being enumerated.
                    // I've tried using DecRef and IncRef to keep track of searchers using readers, however there is no guarantee that DecRef can
                    // be called when a search is finished and since search results are lazy, we don't know when they end unless people dispose them
                    // or always use a foreach loop which can't really be forced. The only alternative to using DecRef and IncRef would be to make the results
                    // not lazy which isn't good.

                    foreach (var searcher in RegisteredSearchers.OfType<IDisposable>())
                    {
                        searcher.Dispose();
                    }
                }
                catch
                {
                    // we don't want to kill the app or anything, even though it is terminating, best to just ensure that 
                    // no strange lucene background thread stuff causes issues here.
                }                
            }
            else
            {
                try
                {
                    foreach (var indexer in Indexes.OfType<IDisposable>())
                    {
                        indexer.Dispose();
                    }
                }
                catch (Exception)
                {
                    //an exception shouldn't occur but if so we need to terminate
                    Stop(true);
                }
            }
        }
    }
}
