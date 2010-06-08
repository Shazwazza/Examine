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

    }
}
