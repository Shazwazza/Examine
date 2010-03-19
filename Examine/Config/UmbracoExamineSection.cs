using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UmbracoExamine.Core.Config
{
    public class UmbracoExamineSettings : ConfigurationSection
    {
        private const string SectionName = "UmbracoExamine";

        #region Singleton definition

        private static readonly UmbracoExamineSettings m_Examine;
        private UmbracoExamineSettings() { }
        static UmbracoExamineSettings()
        {
            m_Examine = ConfigurationManager.GetSection(SectionName) as UmbracoExamineSettings;

        }
        public static UmbracoExamineSettings Instance
        {
            get { return m_Examine; }
        }

        #endregion

        [ConfigurationProperty("ExamineSearchProviders")]
        public SearchProvidersSection SearchProviders
        {
            get { return (SearchProvidersSection)base["ExamineSearchProviders"]; }
        }

        [ConfigurationProperty("ExamineIndexProviders")]
        public IndexProvidersSection IndexProviders
        {
            get { return (IndexProvidersSection)base["ExamineIndexProviders"]; }
        }

    }
}
