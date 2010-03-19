using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Examine.Config
{
    public class ExamineSettings : ConfigurationSection
    {
        private const string SectionName = "Examine";

        #region Singleton definition

        private static readonly ExamineSettings m_Examine;
        private ExamineSettings() { }
        static ExamineSettings()
        {
            m_Examine = ConfigurationManager.GetSection(SectionName) as ExamineSettings;

        }
        public static ExamineSettings Instance
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
