using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Examine.Config
{
    /// <summary>
    /// Config section for Examine
    /// </summary>
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
        /// <summary>
        /// Gets the instance of the Examine settings.
        /// </summary>
        /// <value>The instance.</value>
        public static ExamineSettings Instance
        {
            get { return m_Examine; }
        }

        public Action<ExamineManager> ConfigurationAction { get; set; }

        #endregion

        ///<summary>
        /// Whether or not to rebuild non-existing indexes when the application starts
        ///</summary>
        [ConfigurationProperty("RebuildOnAppStart", DefaultValue = true)]
        public bool RebuildOnAppStart
        {
            get { return (bool)base["RebuildOnAppStart"]; }
        }       

        /// <summary>
        /// Gets the search providers.
        /// </summary>
        /// <value>The search providers.</value>
        [ConfigurationProperty("ExamineSearchProviders")]
        public SearchProvidersSection SearchProviders
        {
            get { return (SearchProvidersSection)base["ExamineSearchProviders"]; }
        }

        /// <summary>
        /// Gets the index providers.
        /// </summary>
        /// <value>The index providers.</value>
        [ConfigurationProperty("ExamineIndexProviders")]
        public IndexProvidersSection IndexProviders
        {
            get { return (IndexProvidersSection)base["ExamineIndexProviders"]; }
        }

    }
}
