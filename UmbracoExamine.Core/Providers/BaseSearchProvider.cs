using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;
using UmbracoExamine.Core;

namespace UmbracoExamine.Providers
{
    public abstract class BaseSearchProvider : ProviderBase, ISearcher
    {

        public abstract IEnumerable<SearchResult> Search(ISearchCriteria criteria);
    
    }
}
