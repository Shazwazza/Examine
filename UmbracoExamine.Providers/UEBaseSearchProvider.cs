using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace UmbracoExamine.Providers
{
    public abstract class UEBaseSearchProvider : ProviderBase
    {

        public abstract List<SearchResult> Search(string text, bool includeWildcards);
        public abstract List<SearchResult> Search(string text, string nodeTypeAlias, bool includeWildcards, int? startNodeId);
        public abstract List<SearchResult> Search(string text, string nodeTypeAlias, bool includeWildcards, int? startNodeId, string[] searchFields, int maxResults);

    
    }
}
