using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using UmbracoExamine.Configuration;
using System.Configuration.Provider;

namespace UmbracoExamine.Providers
{
    public class Manager
    {

        private Manager()
		{
			LoadProviders();
		}

		/// <summary>
		/// Singleton
		/// </summary>
        public static Manager Instance
		{
			get
			{					
				return m_Manager;
			}
		}

        private static readonly Manager m_Manager = new Manager();

        private object m_Lock = new object();
        private UEBaseSearchProvider m_SearchProvider = null;
        private UESearchProviderCollection m_SearchProviders = null;
        private UEIndexProviderCollection m_IndexProviders = null;

        private const string SectionName = "UmbracoExamineProviders";

        private void LoadProviders()
        {
            if (m_IndexProviders == null)
            {
                lock (m_Lock)
                {
                    // Do this again to make sure _provider is still null
                    if (m_IndexProviders == null)
                    {
                        m_IndexProviders = new UEIndexProviderCollection();
                        
                        // Load registered providers and point _provider to the default provider	
                        ProvidersHelper.InstantiateProviders(IndexProvidersSection.Instance.Providers, m_IndexProviders, typeof(UEBaseIndexProvider));

                        m_SearchProviders = new UESearchProviderCollection();
                        ProvidersHelper.InstantiateProviders(SearchProvidersSection.Instance.Providers, m_SearchProviders, typeof(UEBaseSearchProvider));
                        //set the default
                        m_SearchProvider = m_SearchProviders[SearchProvidersSection.Instance.DefaultProvider];
                        if (m_SearchProvider == null)
                            throw new ProviderException("Unable to load default search provider");

                    }
                }
            }
        }
    }
}
