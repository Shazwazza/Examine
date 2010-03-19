using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Examine.Config
{
    /// <summary>
    /// Config section for the Examine Index Providers
    /// </summary>
    public class IndexProvidersSection : ConfigurationElement
    {

        /// <summary>
        /// Gets the indexing providers.
        /// </summary>
        /// <value>The providers.</value>
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get { return (ProviderSettingsCollection)base["providers"]; }
        }

        /// <summary>
        /// If true, the IndexingActionHandler will be run to keep the default index up to date.
        /// </summary>
        [ConfigurationProperty("enableDefaultEventHandler", IsRequired = true)]
        public bool EnableDefaultEventHandler
        {
            get
            {
                return (bool)this["enableDefaultEventHandler"];
            }
        }


    }
}
