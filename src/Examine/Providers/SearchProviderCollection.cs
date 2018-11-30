using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace Examine.Providers
{
    public class SearchProviderCollection : ProviderCollection, IEnumerable<BaseSearchProvider>
    {
        public new BaseSearchProvider this[string name] => (BaseSearchProvider)base[name];

        public BaseSearchProvider this[int index] => this.ToArray()[index];

        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (!(provider is BaseSearchProvider))
                throw new ArgumentException("Invalid provider type", nameof(provider));

            base.Add(provider);
        }

        private List<BaseSearchProvider> _list = null;

        #region IEnumerable<BaseIndexProvider> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<BaseSearchProvider> IEnumerable<BaseSearchProvider>.GetEnumerator()
        {
            if (_list == null)
            {
                _list = new List<BaseSearchProvider>();
                foreach (var x in this)
                {
                    _list.Add((BaseSearchProvider)x);
                }
            }
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an object that implements the <see cref="T:System.Collections.IEnumerator"/> interface to iterate through the collection.
        /// </summary>
        /// <returns>
        /// An object that implements <see cref="T:System.Collections.IEnumerator"/> to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
