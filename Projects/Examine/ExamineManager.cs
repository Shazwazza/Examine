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
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Examine
{
    ///<summary>
    /// Exposes searchers and indexers
    ///</summary>
    public class ExamineManager : ISearcher, IIndexer, IRegisteredObject
    {
        private ExamineManager()
        {
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
                            foreach (var index in _indexProviderCollection.Cast<IIndexer>())
                            {
                                if (!index.IndexExists())
                                {
                                    var args = new BuildingEmptyIndexOnStartupEventArgs(index);
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
                HostingEnvironment.UnregisterObject(this);
            }
            else
            {
                foreach (var searcher in SearchProviderCollection.OfType<IDisposable>())
                {
                    searcher.Dispose();
                }
                foreach (var indexer in IndexProviderCollection.OfType<IDisposable>())
                {
                    indexer.Dispose();
                }    
            }
        }
    }
}
