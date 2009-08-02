using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using System.IO;

namespace UmbracoExamine.Providers
{
    public abstract class UEBaseIndexProvider : ProviderBase, IIndexer
    {

        /// <summary>
        /// Used for thread locking.
        /// </summary>
        public object m_Lock = new object();

        public abstract void ReIndexNode(int nodeId);

        public abstract bool DeleteFromIndex(int nodeId);

        public abstract void IndexAll();
    
    }
}
