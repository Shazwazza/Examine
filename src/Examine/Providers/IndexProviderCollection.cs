using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using System.Collections;

namespace Examine.Providers
{
    /// <summary>
    /// A collection of Examine Index Providers
    /// </summary>
    public class IndexProviderCollection : ProviderCollection, IEnumerable<BaseIndexProvider>
    {
        /// <summary>
        /// Gets the <see cref="Examine.Providers.BaseIndexProvider"/> with the specified name.
        /// </summary>
        /// <value></value>
        public new BaseIndexProvider this[string name]
        {
            get { return (BaseIndexProvider)base[name]; }
        }

        /// <summary>
        /// Gets the <see cref="Examine.Providers.BaseIndexProvider"/> at the specified index.
        /// </summary>
        /// <value></value>
        public BaseIndexProvider this[int index]
        {
            get
            {
                return this.Cast<BaseIndexProvider>().ToArray()[index];
            }
        }

        /// <summary>
        /// Adds a provider to the collection.
        /// </summary>
        /// <param name="provider">The provider to be added.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The collection is read-only.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="provider"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The <see cref="P:System.Configuration.Provider.ProviderBase.Name"/> of <paramref name="provider"/> is null.
        /// - or -
        /// The length of the <see cref="P:System.Configuration.Provider.ProviderBase.Name"/> of <paramref name="provider"/> is less than 1.
        /// </exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
        /// </PermissionSet>
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is BaseIndexProvider))
                throw new ArgumentException("Invalid provider type", "provider");

            base.Add(provider);
        }

        private List<BaseIndexProvider> m_List = null;

        #region IEnumerable<BaseIndexProvider> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<BaseIndexProvider> IEnumerable<BaseIndexProvider>.GetEnumerator()
        {
            if (m_List == null)
            {
                m_List = new List<BaseIndexProvider>();
                foreach (var x in this)
                {
                    m_List.Add((BaseIndexProvider)x);
                }
            }
            return m_List.GetEnumerator();
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
