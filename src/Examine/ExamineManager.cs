using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;
using Examine.Config;
using Examine.LuceneEngine;
using Examine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;

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
        /// Singleton
        /// </summary>
        public static ExamineManager Instance
        {
            get
            {
                InstanceInitialized = true;
                return Manager;
            }
        }

        private static readonly ExamineManager Manager = new ExamineManager();

        private object _lock = new object();
        private readonly ConcurrentDictionary<string, IIndexer> _indexers = new ConcurrentDictionary<string, IIndexer>();
        private readonly ConcurrentDictionary<string, ISearcher> _searchers = new ConcurrentDictionary<string, ISearcher>();

        /// <summary>
        /// Returns the collection of searchers
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Searchers should be resolved using the GetIndexSearcher method, this only returns the configuration based searchers")]
        public SearchProviderCollection SearchProviderCollection => ConfigBasedSearchProviders;

        private SearchProviderCollection ConfigBasedSearchProviders
        {
            get
            {
                EnsureProviders();
                return _providerCollections.Item1;
            }
        }

        /// <summary>
        /// Return the colleciton of indexer providers (only the ones registered in config)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the IndexProviders property instead, this only returns the configuration based indexers")]
        public IndexProviderCollection IndexProviderCollection => ConfigBasedIndexProviders;

        private IndexProviderCollection ConfigBasedIndexProviders
        {
            get
            {
                EnsureProviders();
                return _providerCollections.Item2;
            }
        }

        /// <summary>
        /// Returns the searcher for a given index
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        public ISearcher GetSearcher(string indexerName)
        {
            if (IndexProviders.ContainsKey(indexerName))
            {
                if (IndexProviders.TryGetValue(indexerName, out var indexer))
                {
                    return indexer.GetSearcher();
                }
            }
            throw new KeyNotFoundException("No indexer defined by name " + indexerName);
        }
        
        /// <summary>
        /// Returns an indexer by name
        /// </summary>
        /// <param name="indexerName"></param>
        /// <returns></returns>
        public IIndexer GetIndexer(string indexerName)
        {
            return IndexProviders[indexerName];
        }

        /// <summary>
        /// Gets a list of all index providers
        /// </summary>
        /// <remarks>
        /// This returns all config based indexes and indexers registered in code
        /// </remarks>
        public IReadOnlyDictionary<string, IIndexer> IndexProviders
        {
            get
            {
                var providerDictionary = ConfigBasedIndexProviders.ToDictionary(x => x.Name, x => (IIndexer)x);
                foreach (var i in _indexers)
                {
                    providerDictionary[i.Key] = i.Value;
                }
                return new Dictionary<string, IIndexer>(providerDictionary, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Adds an indexer to the manager
        /// </summary>
        /// <param name="name"></param>
        /// <param name="indexer"></param>
        public void AddIndexer(string name, IIndexer indexer)
        {
            //make sure this name doesn't exist in

            if (ConfigBasedIndexProviders[name] != null)
            {
                throw new InvalidOperationException("The indexer with name " + name + " already exists");
            }
            if (!_indexers.TryAdd(name, indexer))
            {
                throw new InvalidOperationException("The indexer with name " + name + " already exists");
            }
        }

        /// <summary>
        /// Adds an index searcher to the manager - generally this would be a multi index searcher since most searchers are created from an existing index
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searcher"></param>
        public void AddSearcher(string name, ISearcher searcher)
        {
            //make sure this name doesn't exist in

            if (ConfigBasedSearchProviders[name] != null)
            {
                throw new InvalidOperationException("The searcher with name " + name + " already exists");
            }
            if (!_searchers.TryAdd(name, searcher))
            {
                throw new InvalidOperationException("The searcher with name " + name + " already exists");
            }
        }


        private bool _providersInit = false;
        
        private Tuple<SearchProviderCollection, IndexProviderCollection> _providerCollections;

        /// <summary>
        /// Before any of the index/search collections are accessed, the providers need to be loaded
        /// </summary>
        private void EnsureProviders()
        {
            LazyInitializer.EnsureInitialized(ref _providerCollections, ref _providersInit, ref _lock, () =>
            {
                // Load registered providers and point _provider to the default provider	`

                var indexProviderCollection = new IndexProviderCollection();
                ProvidersHelper.InstantiateProviders(ExamineSettings.Instance.IndexProviders.Providers, indexProviderCollection, typeof(BaseIndexProvider));

                var searchProviderCollection = new SearchProviderCollection();
                ProvidersHelper.InstantiateProviders(ExamineSettings.Instance.SearchProviders.Providers, searchProviderCollection, typeof(BaseSearchProvider));

                return new Tuple<SearchProviderCollection, IndexProviderCollection>(searchProviderCollection, indexProviderCollection);
            });
        }

        /// <summary>
        /// Re-indexes items for the providers specified
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="providers"></param>
        public void IndexItems(ValueSet[] nodes, IEnumerable<IIndexer> providers)
        {
            foreach (var provider in providers)
            {
                provider.IndexItems(nodes);
            }
        }

        /// <summary>
        /// Reindex nodes for all providers
        /// </summary>
        /// <param name="nodes"></param>
        public void IndexItems(ValueSet[] nodes)
        {
            IndexItems(nodes, IndexProviders.Values);
        }

        /// <summary>
        /// Deletes index for node for the specified providers
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="providers"></param>
        public void DeleteFromIndexes(string nodeId, IEnumerable<IIndexer> providers)
        {
            foreach (var provider in providers)
            {
                provider.DeleteFromIndex(nodeId);
            }
        }

        /// <summary>
        /// Deletes index for node for all providers
        /// </summary>
        /// <param name="nodeId"></param>
        public void DeleteFromIndexes(string nodeId)
        {
            DeleteFromIndexes(nodeId, IndexProviders.Values);
        }

        /// <summary>
        /// Indexes all items for the index category for all providers
        /// </summary>
        /// <param name="indexCategory"></param>
        public void IndexAll(string indexCategory)
        {
            foreach (var provider in IndexProviders.Values)
            {
                provider.IndexAll(indexCategory);
            }
        }

        /// <summary>
        /// Rebuilds indexes for all providers
        /// </summary>
        public void RebuildIndexes()
        {
            foreach (var provider in IndexProviders.Values)
            {
                provider.RebuildIndex();
            }
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

                    foreach (var searcher in SearchProviderCollection.OfType<IDisposable>())
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
                    foreach (var indexer in IndexProviders.OfType<IDisposable>())
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