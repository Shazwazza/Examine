using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UmbracoExamine.Core
{
    public class SearchCriteria : ISearchCriteria
    {

        public SearchCriteria(string searchText, string[] searchFields, string[] nodeTypes, bool useWildcards, int? parentNodeId, int maxResults)
        {
            Text = searchText;
            SearchFields = searchFields == null ? new List<string>() : searchFields.ToList();
            NodeTypeAliases = nodeTypes == null ? new List<string>() : nodeTypes.ToList();
            UseWildcards = useWildcards;
            ParentNodeId = parentNodeId;
            MaxResults = maxResults;
        }

        public string Text { get; private set; }
        public int MaxResults { get; private set; }
        public bool UseWildcards { get; private set; }
        public int? ParentNodeId { get; private set; }
        public IEnumerable<string> SearchFields { get; private set; }
        public IEnumerable<string> NodeTypeAliases { get; private set; }

    }
}
