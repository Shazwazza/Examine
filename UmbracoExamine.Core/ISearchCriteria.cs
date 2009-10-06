using System;
using System.Collections.Generic;
namespace UmbracoExamine.Core
{
    public interface ISearchCriteria
    {
        int MaxResults { get; }
        IEnumerable<string> NodeTypeAliases { get; }
        int? ParentNodeId { get; }
        IEnumerable<string> SearchFields { get; }
        string Text { get; }
        bool UseWildcards { get; }
        IndexType SearchIndexType { get; }
    }
}
