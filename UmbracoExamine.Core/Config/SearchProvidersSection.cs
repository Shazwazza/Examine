using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UmbracoExamine.Core.Config
{
    public class SearchProvidersSection : ConfigurationSection
    {
        #region Singleton definition

        private static readonly SearchProvidersSection m_Providers;
        private SearchProvidersSection() { }
        static SearchProvidersSection()
        {
            m_Providers = ConfigurationManager.GetSection(SectionName) as SearchProvidersSection;

        }
        public static SearchProvidersSection Instance
        {
            get { return m_Providers; }
        }

        #endregion

        private const string SectionName = "ExamineSearchProviders";

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get { return (ProviderSettingsCollection)base["providers"]; }
        }

        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("defaultProvider", DefaultValue = "InternalSearch")]
        public string DefaultProvider
        {
            get { return (string)base["defaultProvider"]; }
            set { base["defaultProvider"] = value; }
        }
    }
}
