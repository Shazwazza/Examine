using System;
namespace UmbracoExamine.Core
{
    public interface ISearchCriteria
    {
        int MaxResults { get; }
        System.Collections.Generic.IEnumerable<string> NodeTypeAliases { get; }
        int? ParentNodeId { get; }
        System.Collections.Generic.IEnumerable<string> SearchFields { get; }
        string Text { get; }
        bool UseWildcards { get; }
    }
}
