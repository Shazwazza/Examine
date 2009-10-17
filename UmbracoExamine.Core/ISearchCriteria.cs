using System;
using System.Collections.Generic;
namespace UmbracoExamine.Core
{
    public interface ISearchCriteria
    {
        int MaxResults { get; }

        /// <summary>
        /// The node types to search
        /// </summary>
        IEnumerable<string> NodeTypeAliases { get; }
        
        /// <summary>
        /// Only search nodes that exist below this Umbraco Id
        /// </summary>
        int? ParentNodeId { get; }

        /// <summary>
        /// Which field names to search against
        /// </summary>
        IEnumerable<string> SearchFields { get; }
        
        /// <summary>
        /// The search term(s) to search for
        /// </summary>
        string Text { get; }
        
        /// <summary>
        /// If true the search provider should add * to the end of each search term.
        /// </summary>
        bool UseWildcards { get; }

        IndexType SearchIndexType { get; }

        /// <summary>
        /// If true the search provider should ensure that the fields being searched match all words in the query string.
        /// </summary>
        /// <remarks>
        /// By default, this should be false
        /// </remarks>
        bool MatchAllWords { get; }
    }
}
