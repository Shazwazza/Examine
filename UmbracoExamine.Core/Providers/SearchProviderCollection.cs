using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace UmbracoExamine.Providers
{
    public class SearchProviderCollection : ProviderCollection
    {
        public new BaseSearchProvider this[string name]
        {
            get { return (BaseSearchProvider)base[name]; }
        }

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is BaseSearchProvider))
                throw new ArgumentException("Invalid provider type", "provider");

            base.Add(provider);
        }

    }
}
