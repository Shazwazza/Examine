using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration.Provider;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;
using Examine.Config;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Examine.SearchCriteria;
using System.Web;
using Examine.Session;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine
{
    ///<summary>
    /// Exposes searchers and indexers
    ///</summary>
    public class ExamineManager : ISearcher, IDisposable, IRegisteredObject
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

        private readonly object _lock = new object();
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
                return _defaultSearchProvider;
            }
        }

        /// <summary>
        /// Returns the collection of searchers
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Searchers should be resolved using the GetSearcher method")]
        public SearchProviderCollection SearchProviderCollection
        {
            get
            {
                EnsureProviders();
                return _searchProviderCollection;
            }
        }

        /// <summary>
        /// Return the colleciton of indexer providers (only the ones registered in config)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the IndexProviders property instead")]
        public IndexProviderCollection IndexProviderCollection
        {
            get
            {
                EnsureProviders();
                return _indexProviderCollection;
            }
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
                var indexer = IndexProviders[indexerName] as LuceneIndexer;
                if (indexer == null)
                {
                    throw new InvalidOperationException("Cannot create an ILuceneSearcher based on a non LuceneIndexer indexer");
                }
                return new LuceneSearcher(indexer.GetLuceneDirectory(), searchAnalyzer ?? indexer.IndexingAnalyzer);
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
                var providerDictionary = IndexProviderCollection.ToDictionary(x => x.Name, x => (IExamineIndexer)x);
                foreach (var i in _indexers)
                {
                    providerDictionary[i.Key] = i.Value;
                }
                return new CaseInsensitiveKeyReadOnlyDictionary<IExamineIndexer>(providerDictionary);
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

            if (IndexProviderCollection[name] != null)
            {
                throw new InvalidOperationException("The indexer with name " + name + " already exists");
            }
            if (!_indexers.TryAdd(name, indexer))
            {
                throw new InvalidOperationException("The indexer with name " + name + " already exists");
            }
        }

        private volatile bool _providersInit = false;
        private BaseSearchProvider _defaultSearchProvider;
        private SearchProviderCollection _searchProviderCollection;
        private IndexProviderCollection _indexProviderCollection;

        /// <summary>
        /// Before any of the index/search collections are accessed, the providers need to be loaded
        /// </summary>
        private void EnsureProviders()
        {
            if (!_providersInit)
            {
                lock (_lock)
                {
                    // Do this again to make sure _provider is still null
                    if (!_providersInit)
                    {
                        // Load registered providers and point _provider to the default provider	

                        _indexProviderCollection = new IndexProviderCollection();
                        ProvidersHelper.InstantiateProviders(ExamineSettings.Instance.IndexProviders.Providers, _indexProviderCollection, typeof(BaseIndexProvider));

                        _searchProviderCollection = new SearchProviderCollection();
                        ProvidersHelper.InstantiateProviders(ExamineSettings.Instance.SearchProviders.Providers, _searchProviderCollection, typeof(BaseSearchProvider));

                        //set the default
                        if (!string.IsNullOrEmpty(ExamineSettings.Instance.SearchProviders.DefaultProvider))
                            _defaultSearchProvider =
                                _searchProviderCollection[ExamineSettings.Instance.SearchProviders.DefaultProvider] ??
                                _searchProviderCollection.Cast<BaseSearchProvider>().FirstOrDefault();

                        if (_defaultSearchProvider == null)
                            throw new ProviderException("Unable to load default search provider");

                        _providersInit = true;


                        if (ExamineSettings.Instance.ConfigurationAction != null)
                        {
                            ExamineSettings.Instance.ConfigurationAction(this);
                        }

                        //check if we need to rebuild on startup
                        if (ExamineSettings.Instance.RebuildOnAppStart)
                        {
                            foreach (var index in _indexProviderCollection)
                            {
                                if (index.IsIndexNew())
                                {
                                    try
                                    {
                                        var args = new BuildingEmptyIndexOnStartupEventArgs(index);
                                        OnBuildingEmptyIndexOnStartup(args);
                                        if (!args.Cancel)
                                        {
                                            index.RebuildIndex();

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        var li = index as LuceneIndexer;
                                        try
                                        {
                                            HttpContext.Current.Response.Write("Rebuilding index" +
                                                                               (li != null ? " " + li.Name : "") +
                                                                               " failed");
                                            HttpContext.Current.Response.Write(ex.ToString());
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                File.WriteAllText(HostingEnvironment.MapPath("~/App_Data/ExamineError.txt"), ex.ToString());
                                            }
                                            catch
                                            {
                                                throw;
                                            }
                                        }
                                    }

                                }
                            }
                        }

                    }
                }
            }
        }


        #region ISearcher Members

        /// <summary>
        /// Uses the default provider specified to search
        /// </summary>
        /// <param name="searchParameters"></param>
        /// <returns></returns>
        /// <remarks>This is just a wrapper for the default provider</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method shouldn't be used, searchers should be resolved using the GetSearcher method and if no searchers are defined in the legacy config this will throw an exception")]
        public ISearchResults Search(ISearchCriteria searchParameters)
        {
            if (DefaultSearchProvider == null)
            {
                throw new InvalidOperationException("ExamineManager.Search should not be used, get a searcher using the GetSearcher method");
            }
            return DefaultSearchProvider.Search(searchParameters);
        }

        /// <summary>
        /// Uses the default provider specified to search
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method shouldn't be used, searchers should be resolved using the GetSearcher method and if no searchers are defined in the legacy config this will throw an exception")]
        public ISearchResults Search(string searchText, bool useWildcards)
        {
            if (DefaultSearchProvider == null)
            {
                throw new InvalidOperationException("ExamineManager.Search should not be used, get a searcher using the GetSearcher method");
            }
            return DefaultSearchProvider.Search(searchText, useWildcards);
        }


        #endregion

        /// <summary>
        /// Reindex nodes for the providers specified
        /// </summary>
        /// <param name="node"></param>
        /// <param name="category"></param>
        /// <param name="providers"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this, use the IndexItems method instead")]
        public void ReIndexNode(XElement node, string category, IEnumerable<BaseIndexProvider> providers)
        {
            _ReIndexNode(node, category, providers);
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
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this, use the other DeleteFromIndex method instead")]
        public void DeleteFromIndex(string nodeId, IEnumerable<BaseIndexProvider> providers)
        {
            _DeleteFromIndex(nodeId, providers);
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
        /// <param name="node"></param>
        /// <param name="category"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this, use the IndexItems method instead")]
        public void ReIndexNode(XElement node, string category)
        {
            _ReIndexNode(node, category, IndexProviderCollection);
        }
        private void _ReIndexNode(XElement node, string type, IEnumerable<BaseIndexProvider> providers)
        {
            foreach (var provider in providers)
            {
                provider.ReIndexNode(node, type);
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
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Do not use this, use the other DeleteFromIndex method instead")]
        public void DeleteFromIndex(string nodeId)
        {
            _DeleteFromIndex(nodeId, IndexProviderCollection);
        }
        private void _DeleteFromIndex(string nodeId, IEnumerable<BaseIndexProvider> providers)
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
        /// Event is raised when an index is being rebuild on app startup when it is empty, this event is cancelable
        /// </summary>
        public event EventHandler<BuildingEmptyIndexOnStartupEventArgs> BuildingEmptyIndexOnStartup;

        /// <summary>
        /// Raises the BuildingEmptyIndexOnStartup event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnBuildingEmptyIndexOnStartup(BuildingEmptyIndexOnStartupEventArgs e)
        {
            var handler = BuildingEmptyIndexOnStartup;
            if (handler != null) handler(this, e);
        }

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
