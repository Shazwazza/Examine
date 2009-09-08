using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmbracoExamine.Core
{
    public interface ISearcher
    {

        List<SearchResult> Search(string text, bool includeWildcards);
        List<SearchResult> Search(string text, string nodeTypeAlias, bool includeWildcards, int? startNodeId);
        List<SearchResult> Search(string text, string nodeTypeAlias, bool includeWildcards, int? startNodeId, string[] searchFields, int maxResults);


    }
}
