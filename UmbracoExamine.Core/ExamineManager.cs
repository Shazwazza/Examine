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

        #region IIndexer Members

        public void ReIndexNode(XElement node, IndexType type)
        {
            if (UmbracoExamineSettings.Instance.IndexProviders.EnableAsync)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        _ReIndexNode(node, type);
                    });
            }
            else
            {
                _ReIndexNode(node, type);
            }
        }
        private void _ReIndexNode(XElement node, IndexType type)
        {
            foreach (BaseIndexProvider provider in IndexProviderCollection)
            {
                provider.ReIndexNode(node, type);                
            }
        }

        public void DeleteFromIndex(XElement node)
        {
            if (UmbracoExamineSettings.Instance.IndexProviders.EnableAsync)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        _DeleteFromIndex(node);
                    });
            }
            else
            {
                _DeleteFromIndex(node);
            }
        }
        private void _DeleteFromIndex(XElement node)
        {
            foreach (BaseIndexProvider provider in IndexProviderCollection)
            {
                provider.DeleteFromIndex(node);
            }
        }

        public void IndexAll(IndexType type)
        {
            if (UmbracoExamineSettings.Instance.IndexProviders.EnableAsync)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        _IndexAll(type);
                    });
            }
            else
            {
                _IndexAll(type);
            }
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
            if (UmbracoExamineSettings.Instance.IndexProviders.EnableAsync)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        _RebuildIndex();
                    });
            }
            else
            {
                _RebuildIndex();
            }
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

    }
}
