using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;
using Examine.Config;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Examine.SearchCriteria;
using System.Web;
using Examine.LuceneEngine;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine
{
    ///<summary>
    /// Exposes searchers and indexers
    ///</summary>
    public class ExamineManager : ISearcher, IIndexer, IRegisteredObject
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
            if (!_defaultRegisteration) return;
            _defaultRegisteration = false;

            var instance = Instance;
            if (instance is ExamineManager e) HostingEnvironment.UnregisterObject(e);
        }

        private ExamineManager()
        {
            if (!_defaultRegisteration) return;
            HostingEnvironment.RegisterObject(this);
        }

        /// <summary>
        /// Singleton
        /// </summary>
        public static ExamineManager Instance
        {
            get { return Manager; }
        }

        private static readonly ExamineManager Manager = new ExamineManager();

        private readonly object _lock = new object();

        ///<summary>
        /// Returns the default search provider
        ///</summary>
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
        public SearchProviderCollection SearchProviderCollection
        {
            get
            {
                EnsureProviders();
                return _searchProviderCollection;
            }            
        }

        /// <summary>
        /// Return the colleciton of indexers
        /// </summary>
        public IndexProviderCollection IndexProviderCollection
        {
            get
            {
                EnsureProviders();
                return _indexProviderCollection;
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
                            _defaultSearchProvider = _searchProviderCollection[ExamineSettings.Instance.SearchProviders.DefaultProvider];

                        if (_defaultSearchProvider == null)
                            throw new ProviderException("Unable to load default search provider");

                        _providersInit = true;


                        //check if we need to rebuild on startup
                        if (ExamineSettings.Instance.RebuildOnAppStart)
                        {
                            foreach (IIndexer index in _indexProviderCollection)
                            {
                                var rebuild = false;
                                var healthy = true;
                                Exception unhealthyEx = null;
                                var exists = index.IndexExists();
                                if (!exists)
                                    rebuild = true;
                                if (!rebuild)
                                {
                                    healthy = IsIndexHealthy(index, out unhealthyEx);
                                    if (!healthy)
                                        rebuild = true;
                                }
                                //if it doesn't exist ... or if it's unhealthy/corrupt
                                
                                if (rebuild)
                                {
                                    var args = new BuildingEmptyIndexOnStartupEventArgs(index, healthy, unhealthyEx);
                                    OnBuildingEmptyIndexOnStartup(args);
                                    if (!args.Cancel)
                                    {
                                        index.RebuildIndex();    
                                    }
                                }
                            }    
                        }

                    }
                }
            }
        }

        private bool IsIndexHealthy(IIndexer index, out Exception e)
        {
            var luceneIndex = index as LuceneIndexer;
            if (luceneIndex == null)
            {
                e = null;
                return true;
            }
            Exception ex;
            var readable = luceneIndex.IsReadable(out ex);
            e = ex;
            return readable;
        }


        #region ISearcher Members

        /// <summary>
        /// Uses the default provider specified to search
        /// </summary>
        /// <param name="searchParameters"></param>
        /// <returns></returns>
        /// <remarks>This is just a wrapper for the default provider</remarks>
        public ISearchResults Search(ISearchCriteria searchParameters)
        {
            return DefaultSearchProvider.Search(searchParameters);
        }

        /// <summary>
        /// Uses the default provider specified to search
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="maxResults"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public ISearchResults Search(string searchText, bool useWildcards)
        {
            return DefaultSearchProvider.Search(searchText, useWildcards);
        }


        #endregion

        /// <summary>
        /// Reindex nodes for the providers specified
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        /// <param name="providers"></param>
        public void ReIndexNode(XElement node, string type, IEnumerable<BaseIndexProvider> providers)
        {
            _ReIndexNode(node, type, providers);
        }

        /// <summary>
        /// Deletes index for node for the specified providers
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="providers"></param>
        public void DeleteFromIndex(string nodeId, IEnumerable<BaseIndexProvider> providers)
        {
            _DeleteFromIndex(nodeId, providers);
        }

        #region IIndexer Members

        /// <summary>
        /// Reindex nodes for all providers
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        public void ReIndexNode(XElement node, string type)
        {
            _ReIndexNode(node, type, IndexProviderCollection);
        }
        private void _ReIndexNode(XElement node, string type, IEnumerable<BaseIndexProvider> providers)
        {
            foreach (var provider in providers)
            {
                provider.ReIndexNode(node, type);
            }
        }

        /// <summary>
        /// Deletes index for node for all providers
        /// </summary>
        /// <param name="nodeId"></param>
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

        public void IndexAll(string type)
        {
            _IndexAll(type);
        }
        private void _IndexAll(string type)
        {
            foreach (BaseIndexProvider provider in IndexProviderCollection)
            {
                provider.IndexAll(type);
            }
        }

        public void RebuildIndex()
        {
            _RebuildIndex();
        }
        private void _RebuildIndex()
        {
            foreach (BaseIndexProvider provider in IndexProviderCollection)
            {
                provider.RebuildIndex();
            }
        }

        public IIndexCriteria IndexerData
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IndexExists()
        {
            throw new NotImplementedException();
        }

        #endregion


        #region ISearcher Members

        /// <summary>
        /// Creates search criteria that defaults to IndexType.Any and BooleanOperation.And
        /// </summary>
        /// <returns></returns>
        public ISearchCriteria CreateSearchCriteria()
        {
            return this.CreateSearchCriteria(string.Empty, BooleanOperation.And);
        }

        public ISearchCriteria CreateSearchCriteria(string type)
        {
            return this.CreateSearchCriteria(type, BooleanOperation.And);
        }

        public ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation)
        {
            return this.CreateSearchCriteria(string.Empty, defaultOperation);
        }

        public ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        {
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
                    foreach (var indexer in IndexProviderCollection.OfType<IDisposable>())
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
