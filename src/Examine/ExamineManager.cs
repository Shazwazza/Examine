using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Hosting;
using Examine.LuceneEngine;

namespace Examine
{
    ///<summary>
    /// Exposes searchers and indexers
    ///</summary>
    public class ExamineManager : IDisposable, IRegisteredObject, IExamineManager
    {
        //tracks if the ExamineManager should register itself with the HostingEnvironment
        private static volatile bool _defaultRegisteration = true;

        /// <summary>
        /// By default the <see cref="ExamineManager"/> will use itself to to register against the HostingEnvironment for tracking
        /// app domain shutdown. In some cases a library may wish to manage this shutdown themselves in which case this can be called
        /// on startup to disable the default registration.
        /// </summary>        
        /// <returns></returns>
        public static void DisableDefaultHostingEnvironmentRegistration()
        {
            _defaultRegisteration = false;
        }

        private ExamineManager()
        {
            if (!_defaultRegisteration) return;
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => Dispose();
            HostingEnvironment.RegisterObject(this);
        }

        /// <summary>
        /// Returns true if this singleton has been initialized
        /// </summary>
        public static bool InstanceInitialized { get; private set; }

        /// <summary>
        /// Singleton instance - but it's much more preferable to use IExamineManager
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IExamineManager Instance
        {
            get
            {
                InstanceInitialized = true;
                return Manager;
            }
        }

        private static readonly ExamineManager Manager = new ExamineManager();

        private readonly ConcurrentDictionary<string, IIndex> _indexers = new ConcurrentDictionary<string, IIndex>();
        private readonly ConcurrentDictionary<string, ISearcher> _searchers = new ConcurrentDictionary<string, ISearcher>();

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
       
        /// <inheritdoc />
        public IIndex AddIndex(IIndex index)
        {
            //make sure this name doesn't exist in
            if (!_indexers.TryAdd(index.Name, index))
            {
                throw new InvalidOperationException("The indexer with name " + index.Name + " already exists");
            }

            return index;
        }

        /// <inheritdoc />
        public ISearcher AddSearcher(ISearcher searcher)
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
        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass 
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }
        private bool _disposed = false;
        private void Dispose(bool disposing)
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
        /// Requests a registered object to unregister on app domain unload in a web project
        /// </summary>
        /// <param name="immediate">true to indicate the registered object should unregister from the hosting environment before returning; otherwise, false.</param>
        public void Stop(bool immediate)
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

                    OpenReaderTracker.Current.CloseAllReaders();
                }
                finally
                {
                    //unregister if the default registration was used
                    if (_defaultRegisteration)
                        HostingEnvironment.UnregisterObject(this);
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