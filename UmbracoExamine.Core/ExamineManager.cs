using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Configuration.Provider;
using UmbracoExamine.Providers;
using UmbracoExamine.Core.Config;

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
        public BaseIndexProvider DefaultIndexProvider { get; private set; }

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
                        ProvidersHelper.InstantiateProviders(IndexProvidersSection.Instance.Providers, IndexProviderCollection, typeof(BaseIndexProvider));

                        SearchProviderCollection = new SearchProviderCollection();
                        ProvidersHelper.InstantiateProviders(SearchProvidersSection.Instance.Providers, SearchProviderCollection, typeof(BaseSearchProvider));

                        //set the default
                        DefaultSearchProvider = SearchProviderCollection[SearchProvidersSection.Instance.DefaultProvider];
                        if (DefaultSearchProvider == null)
                            throw new ProviderException("Unable to load default search provider");
                        DefaultIndexProvider = IndexProviderCollection[IndexProvidersSection.Instance.DefaultProvider];
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
        /// <param name="criteria"></param>
        /// <returns></returns>
        /// <remarks>This is just a wrapper for the default provider</remarks>
        public IEnumerable<SearchResult> Search(ISearchCriteria criteria)
        {
            return DefaultSearchProvider.Search(criteria);
        }

        #endregion

        #region IIndexer Members

        /// <summary>
        /// Uses the default provider specified to index
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        /// <remarks>This is just a wrapper for the default provider</remarks>
        public void ReIndexNode(int nodeId)
        {
            DefaultIndexProvider.ReIndexNode(nodeId);
        }

        /// <summary>
        /// Uses the default provider specified to delete
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        /// <remarks>This is just a wrapper for the default provider</remarks>
        public bool DeleteFromIndex(int nodeId)
        {
            return DefaultIndexProvider.DeleteFromIndex(nodeId);
        }

        /// <summary>
        /// Uses the default provider specified to index
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        /// <remarks>This is just a wrapper for the default provider</remarks>
        public void IndexAll()
        {
            DefaultIndexProvider.IndexAll();
        }

        #endregion
    }
}
