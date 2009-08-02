using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace UmbracoExamine.Providers
{
    public class UESearchProviderCollection : ProviderCollection
    {
        public new UEBaseSearchProvider this[string name]
        {
            get { return (UEBaseSearchProvider)base[name]; }
        }

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is UEBaseSearchProvider))
                throw new ArgumentException("Invalid provider type", "provider");

            base.Add(provider);
        }

    }
}
