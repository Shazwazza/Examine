/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Examine.SearchCriteria
{
    [Obsolete("", true)]
    public class SearchCriteria : ISearchCriteria
    {
        public SearchCriteria()
        {

        }
        /// <summary>
        /// New SearchCriteria defaulting to searching Content
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="parentNodeId"></param>
        /// <param name="maxResults"></param>
        public SearchCriteria(string searchText, bool useWildcards, int? parentNodeId, int maxResults)
            : this(searchText, useWildcards, parentNodeId, maxResults, IndexType.Content) { }

        public SearchCriteria(string searchText, bool useWildcards, int? parentNodeId, int maxResults, IndexType type)
            : this(searchText, new string[] { }, new string[] { }, useWildcards, parentNodeId, maxResults, type, false) { }

        /// <summary>
        /// New Search Criteria defaulting to Content searching
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="searchFields">If an empty array is passed in, the searcher will search all fields</param>
        /// <param name="nodeTypes">If an empty array is passed in, the searcher will search all fields</param>
        /// <param name="useWildcards"></param>
        /// <param name="parentNodeId"></param>
        /// <param name="maxResults"></param>
        public SearchCriteria(string searchText, IEnumerable<string> searchFields, IEnumerable<string> nodeTypes, bool useWildcards, int? parentNodeId, int maxResults)
            : this(searchText, searchFields, nodeTypes, useWildcards, parentNodeId, maxResults, IndexType.Content, false) { }

        public SearchCriteria(string searchText, IEnumerable<string> searchFields, IEnumerable<string> nodeTypes, bool useWildcards, int? parentNodeId, int maxResults, IndexType type, bool matchAllWords)
        {
            Text = searchText;
            SearchFields = searchFields;
            NodeTypeAliases = nodeTypes;
            UseWildcards = useWildcards;
            ParentNodeId = parentNodeId;
            MaxResults = maxResults;
            SearchIndexType = type;
            MatchAllWords = matchAllWords;
        }

        public string Text { get; private set; }
        public int MaxResults { get; private set; }
        public bool UseWildcards { get; private set; }
        public int? ParentNodeId { get; private set; }
        public IEnumerable<string> SearchFields { get; private set; }
        public IEnumerable<string> NodeTypeAliases { get; private set; }
        public IndexType SearchIndexType { get; private set; }
        public bool MatchAllWords { get; private set; }
    }
}
*/