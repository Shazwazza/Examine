using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Examine.Config
{
    /// <summary>
    /// Config section for the Examine search providers
    /// </summary>
    public class SearchProvidersSection : ConfigurationElement
    {
        /// <summary>
        /// Gets the search providers.
        /// </summary>
        /// <value>The providers.</value>
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers => (ProviderSettingsCollection)base["providers"];
    }
}
