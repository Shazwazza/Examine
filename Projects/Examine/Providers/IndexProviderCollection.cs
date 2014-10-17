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
                return this.ToArray()[index];
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

        public new IEnumerator<BaseIndexProvider> GetEnumerator()
        {
            var baseEnum = base.GetEnumerator();
            if (baseEnum != null)
            {
                while (baseEnum.MoveNext())
                {
                    yield return (BaseIndexProvider)baseEnum.Current;
                }    
            }
        }
    }

   
}
