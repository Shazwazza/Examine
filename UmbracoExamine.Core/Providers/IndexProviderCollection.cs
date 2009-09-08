using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace UmbracoExamine.Providers
{
    public class IndexProviderCollection : ProviderCollection
    {
        public new BaseIndexProvider this[string name]
        {
            get { return (BaseIndexProvider)base[name]; }
        }

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is BaseIndexProvider))
                throw new ArgumentException("Invalid provider type", "provider");

            base.Add(provider);
        }

    }
}
