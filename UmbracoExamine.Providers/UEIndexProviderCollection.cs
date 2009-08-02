using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace UmbracoExamine.Providers
{
    public class UEIndexProviderCollection : ProviderCollection
    {
        public new UEBaseIndexProvider this[string name]
        {
            get { return (UEBaseIndexProvider)base[name]; }
        }

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is UEBaseIndexProvider))
                throw new ArgumentException("Invalid provider type", "provider");

            base.Add(provider);
        }

    }
}
