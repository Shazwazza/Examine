using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using System.Collections;

namespace UmbracoExamine.Providers
{
    public class IndexProviderCollection : ProviderCollection, IEnumerable<BaseIndexProvider>
    {
        public new BaseIndexProvider this[string name]
        {
            get { return (BaseIndexProvider)base[name]; }
        }

        public BaseIndexProvider this[int index]
        {
            get
            {
                return this.Cast<BaseIndexProvider>().ToArray()[index];
            }
        }

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is BaseIndexProvider))
                throw new ArgumentException("Invalid provider type", "provider");

            base.Add(provider);
        }


        #region IEnumerable<BaseIndexProvider> Members

        IEnumerator<BaseIndexProvider> IEnumerable<BaseIndexProvider>.GetEnumerator()
        {
            return this.AsEnumerable().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

   
}
