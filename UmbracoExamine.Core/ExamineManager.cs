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
    public class ExamineManager
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
        
        private SearchProviderCollection m_SearchProviders = null;
        private IndexProviderCollection m_IndexProviders = null;

        
        //private const string SectionName = "UmbracoExamineProviders";

        private void LoadProviders()
        {
            if (m_IndexProviders == null)
            {
                lock (m_Lock)
                {
                    // Do this again to make sure _provider is still null
                    if (m_IndexProviders == null)
                    {
                        m_IndexProviders = new IndexProviderCollection();
                        
                        // Load registered providers and point _provider to the default provider	
                        ProvidersHelper.InstantiateProviders(IndexProvidersSection.Instance.Providers, m_IndexProviders, typeof(BaseIndexProvider));

                        m_SearchProviders = new SearchProviderCollection();
                        ProvidersHelper.InstantiateProviders(SearchProvidersSection.Instance.Providers, m_SearchProviders, typeof(BaseSearchProvider));
                        //set the default
                        DefaultSearchProvider = m_SearchProviders[SearchProvidersSection.Instance.DefaultProvider];
                        if (DefaultSearchProvider == null)
                            throw new ProviderException("Unable to load default search provider");

                    }
                }
            }
        }
    }
}
