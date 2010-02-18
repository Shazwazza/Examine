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
