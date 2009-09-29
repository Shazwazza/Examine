using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UmbracoExamine.Core.Config
{
    public class IndexProvidersSection : ConfigurationElement
    {
        
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get { return (ProviderSettingsCollection)base["providers"]; }
        }

        /// <summary>
        /// If true, the IndexingActionHandler will be run to keep the default index up to date.
        /// </summary>
        [ConfigurationProperty("enabledDefaultEventHandler", IsRequired = true)]
        public bool EnabledDefaultEventHandler
        {
            get
            {
                return (bool)this["enabledDefaultEventHandler"];
            }
        }
    }
}
