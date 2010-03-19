using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Configuration.Provider;
using UmbracoExamine.Providers;
using UmbracoExamine.Core.Config;
using System.Threading;
using umbraco.cms.businesslogic;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Runtime.CompilerServices;
using UmbracoExamine.Core.SearchCriteria;

namespace UmbracoExamine.Core
{
    public class ExamineManager : ISearcher, IIndexer
    {

        private ExamineManager()
        {
            LoadProviders();
        }

        /// <summary>
        /// Singleton
        /// </summary>
        public static ExamineManager Instance
        {
            get
            {
                return m_Manager;
            }
        }

        private static readonly ExamineManager m_Manager = new ExamineManager();

        private object m_Lock = new object();

        public BaseSearchProvider DefaultSearchProvider { get; private set; }

        public SearchProviderCollection SearchProviderCollection { get; private set; }
        public IndexProviderCollection IndexProviderCollection { get; private set; }

        private void LoadProviders()
        {
            if (IndexProviderCollection == null)
            {
                lock (m_Lock)
                {
                    // Do this again to make sure _provider is still null
                    if (IndexProviderCollection == null)
                    {

                        // Load registered providers and point _provider to the default provider	

                        IndexProviderCollection = new IndexProviderCollection();
                        ProvidersHelper.InstantiateProviders(UmbracoExamineSettings.Instance.IndexProviders.Providers, IndexProviderCollection, typeof(BaseIndexProvider));

                        SearchProviderCollection = new SearchProviderCollection();
                        ProvidersHelper.InstantiateProviders(UmbracoExamineSettings.Instance.SearchProviders.Providers, SearchProviderCollection, typeof(BaseSearchProvider));

                        //set the default
                        DefaultSearchProvider = SearchProviderCollection[UmbracoExamineSettings.Instance.SearchProviders.DefaultProvider];
                        if (DefaultSearchProvider == null)
                            throw new ProviderException("Unable to load default search provider");
                        if (DefaultSearchProvider == null)
                            throw new ProviderException("Unable to load default index provider");

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
        public IEnumerable<SearchResult> Search(ISearchCriteria searchParameters)
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
        public IEnumerable<SearchResult> Search(string searchText, int maxResults, bool useWildcards)
        {
            return DefaultSearchProvider.Search(searchText, maxResults, useWildcards);
        }


        #endregion

        /// <summary>
        /// Reindex nodes for the providers specified
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        /// <param name="providers"></param>
        public void ReIndexNode(XElement node, IndexType type, IEnumerable<BaseIndexProvider> providers)
        {
            _ReIndexNode(node, type, providers);
        }

        /// <summary>
        /// Deletes index for node for the specified providers
        /// </summary>
        /// <param name="node"></param>
        /// <param name="providers"></param>
        public void DeleteFromIndex(XElement node, IEnumerable<BaseIndexProvider> providers)
        {
            _DeleteFromIndex(node, providers);
        }

        #region IIndexer Members

        /// <summary>
        /// Reindex nodes for all providers
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        public void ReIndexNode(XElement node, IndexType type)
        {
            _ReIndexNode(node, type, IndexProviderCollection);
        }       
        private void _ReIndexNode(XElement node, IndexType type, IEnumerable<BaseIndexProvider> providers)
        {
            foreach (var provider in providers)
            {
                provider.ReIndexNode(node, type);
            }
        }

        /// <summary>
        /// Deletes index for node for all providers
        /// </summary>
        /// <param name="node"></param>
        public void DeleteFromIndex(XElement node)
        {
            _DeleteFromIndex(node, IndexProviderCollection);
        }    
        private void _DeleteFromIndex(XElement node, IEnumerable<BaseIndexProvider> providers)
        {
            foreach (var provider in providers)
            {
                provider.DeleteFromIndex(node);
            }
        }

        public void IndexAll(IndexType type)
        {
            _IndexAll(type);
        }
        private void _IndexAll(IndexType type)
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

        #endregion


        #region ISearcher Members


        public ISearchCriteria CreateSearchCriteria(int maxResults, IndexType type)
        {
            return this.DefaultSearchProvider.CreateSearchCriteria(maxResults, type);
        }

        #endregion
    }
}
