using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UmbracoExamine.Configuration
{
    public class IndexProvidersSection : ConfigurationSection
    {
        #region Singleton definition

        private static readonly IndexProvidersSection m_Providers;
        private IndexProvidersSection() { }
        static IndexProvidersSection()
        {
            m_Providers = ConfigurationManager.GetSection(SectionName) as IndexProvidersSection;     
  
        }
        public static IndexProvidersSection Instance
        {
            get { return m_Providers; }
        }

        #endregion

        private const string SectionName = "UEIndexProviders";

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get { return (ProviderSettingsCollection)base["providers"]; }
        }

        //[StringValidator(MinLength = 1)]
        //[ConfigurationProperty("defaultProvider", DefaultValue = "PageHeaderProvider")]
        //public string DefaultProvider
        //{
        //    get { return (string)base["defaultProvider"]; }
        //    set { base["defaultProvider"] = value; }
        //}
    }
}
