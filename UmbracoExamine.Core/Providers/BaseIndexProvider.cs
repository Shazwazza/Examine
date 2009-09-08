using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using System.IO;
using UmbracoExamine.Core;

namespace UmbracoExamine.Providers
{
    public abstract class BaseIndexProvider : ProviderBase, IIndexer
    {

        public abstract void ReIndexNode(int nodeId);

        public abstract bool DeleteFromIndex(int nodeId);

        public abstract void IndexAll();
    
    }
}
