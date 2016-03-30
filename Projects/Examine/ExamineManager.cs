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
using Examine.Config;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Examine.SearchCriteria;
using Examine.Session;
using Lucene.Net.Analysis;

namespace Examine
{
    ///<summary>
    /// Exposes searchers and indexers
    ///</summary>
    public class ExamineManager : IDisposable, IRegisteredObject
    {

        private ExamineManager()
        {
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
        private readonly ConcurrentDictionary<string, IExamineIndexer> _indexers = new ConcurrentDictionary<string, IExamineIndexer>();

        ///<summary>
        /// Returns the default search provider
        ///</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method shouldn't be used, searchers should be resolved using the GetSearcher method and if no searchers are defined in the legacy config this will return null")]
        public BaseSearchProvider DefaultSearchProvider
        {
            get
            {
                EnsureProviders();
                if (_defaultSearchProvider == null)
                    throw new ProviderException("Unable to load default search provider");
                return _defaultSearchProvider;
            }
        }

        /// <summary>
        /// Returns the collection of searchers
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Searchers should be resolved using the GetSearcher method")]
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
        [Obsolete("Use the IndexProviders property instead")]
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
        /// Returns a search based on the indexer name
        /// </summary>
        /// <param name="indexerName"></param>        
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>       
        public ISearcher<TResults, TResult, TSearchCriteria> GetSearcher<TResults, TResult, TSearchCriteria>(string indexerName) 
            where TSearchCriteria : ISearchCriteria
            where TResults : ISearchResults<TResult>
            where TResult : ISearchResult
        {
            if (IndexProviders.ContainsKey(indexerName))
            {
                var indexer = IndexProviders[indexerName] as ISearchableExamineIndex<TResults, TResult, TSearchCriteria>;
                if (indexer == null)
                {
                    throw new InvalidOperationException("The indexer is not of type " + typeof(ISearchableExamineIndex<TResults, TResult, TSearchCriteria>));
                }
                return indexer.GetSearcher();
            }
            throw new KeyNotFoundException("No indexer defined by name " + indexerName);
        }

        /// <summary>
        /// Returns a lucene search based on the lucene indexer name
        /// </summary>
        /// <param name="indexerName"></param>
        /// <param name="searchAnalyzer">
        /// A custom analyzer to use for searching, if not specified then the analyzer defined on the indexer will be used.
        /// </param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>       
        public ILuceneSearcher GetSearcher(string indexerName, Analyzer searchAnalyzer = null)
        {
            if (IndexProviders.ContainsKey(indexerName))
            {
                var indexer = IndexProviders[indexerName] as ISearchableLuceneExamineIndex;
                if (indexer == null)
                {
                    throw new InvalidOperationException("The indexer is not of type " + typeof(ISearchableLuceneExamineIndex));
                }
                return indexer.GetSearcher(searchAnalyzer);
            }
            throw new KeyNotFoundException("No indexer defined by name " + indexerName);
        }

        /// <summary>
        /// Gets a list of all index providers
        /// </summary>
        public IReadOnlyDictionary<string, IExamineIndexer> IndexProviders
        {
            get
            {
                var providerDictionary = ConfigBasedIndexProviders.ToDictionary(x => x.Name, x => (IExamineIndexer)x);
                foreach (var i in _indexers)
                {
                    providerDictionary[i.Key] = i.Value;
                }
                return new Dictionary<string, IExamineIndexer>(providerDictionary, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Adds an index provider to the manager
        /// </summary>
        /// <param name="name"></param>
        /// <param name="indexer"></param>
        public void AddIndexProvider(string name, IExamineIndexer indexer)
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

        private bool _providersInit = false;
        private BaseSearchProvider _defaultSearchProvider;

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

                //set the default
                if (!string.IsNullOrEmpty(ExamineSettings.Instance.SearchProviders.DefaultProvider))
                    _defaultSearchProvider =
                        searchProviderCollection[ExamineSettings.Instance.SearchProviders.DefaultProvider] ??
                        searchProviderCollection.Cast<BaseSearchProvider>().FirstOrDefault();
                
                if (ExamineSettings.Instance.ConfigurationAction != null)
                {
                    ExamineSettings.Instance.ConfigurationAction(this);
                }

                return new Tuple<SearchProviderCollection, IndexProviderCollection>(searchProviderCollection, indexProviderCollection);
            });
        }

        /// <summary>
        /// Re-indexes items for the providers specified
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="providers"></param>
        public void IndexItems(ValueSet[] nodes, IEnumerable<IExamineIndexer> providers)
        {
            foreach (var provider in providers)
            {
                provider.IndexItems(nodes);
            }
        }
        
        /// <summary>
        /// Deletes index for node for the specified providers
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="providers"></param>
        public void DeleteFromIndex(long nodeId, IEnumerable<IExamineIndexer> providers)
        {
            foreach (var provider in providers)
            {
                provider.DeleteFromIndex(nodeId);
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
        /// Deletes index for node for all providers
        /// </summary>
        /// <param name="nodeId"></param>
        public void DeleteFromIndex(long nodeId)
        {
            DeleteFromIndex(nodeId, IndexProviders.Values);
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
        public void RebuildIndex()
        {
            foreach (var provider in IndexProviders.Values)
            {
                provider.RebuildIndex();
            }
        }

        #region ISearcher Members

        /// <summary>
        /// Creates search criteria that defaults to IndexType.Any and BooleanOperation.And
        /// </summary>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method shouldn't be used, searchers should be resolved using the GetSearcher method and if no searchers are defined in the legacy config this will throw an exception")]
        public ISearchCriteria CreateSearchCriteria()
        {
            return this.CreateSearchCriteria(string.Empty, BooleanOperation.And);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method shouldn't be used, searchers should be resolved using the GetSearcher method and if no searchers are defined in the legacy config this will throw an exception")]
        public ISearchCriteria CreateSearchCriteria(string type)
        {
            return this.CreateSearchCriteria(type, BooleanOperation.And);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method shouldn't be used, searchers should be resolved using the GetSearcher method and if no searchers are defined in the legacy config this will throw an exception")]
        public ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation)
        {
            return this.CreateSearchCriteria(string.Empty, defaultOperation);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method shouldn't be used, searchers should be resolved using the GetSearcher method and if no searchers are defined in the legacy config this will throw an exception")]
        public ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        {
            if (DefaultSearchProvider == null)
            {
                throw new InvalidOperationException("ExamineManager.CreateSearchCriteria should not be used, get a searcher using the GetSearcher method");
            }
            return this.DefaultSearchProvider.CreateSearchCriteria(type, defaultOperation);
        }

        #endregion
        
        /// <summary>
        /// Call this as last thing of the thread or request using Examine.
        /// In web context, this MUST be called add Application_EndRequest. Otherwise horrible memory leaking may occur
        /// </summary>
        public void EndRequest()
        {
            if (ExamineSession.RequireImmediateConsistency)
            {
                ExamineSession.WaitForChanges();
            }

            DisposableCollector.Clean();
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
                    Trace.WriteLine("ExamineManager.Dispose");
                    SearcherContextCollection.Instance.Dispose();
                }

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }

        /// <summary>
        /// Occurs when a web app shuts down, we need to ensure that any indexing processes are completed, then dispose ourselves
        /// </summary>
        /// <param name="immediate"></param>
        public void Stop(bool immediate)
        {
            if (!immediate)
            {
                ExamineSession.WaitForChanges();
                Dispose();
            }
            HostingEnvironment.UnregisterObject(this);
        }
    }
}