using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UmbracoExamine.Core
{
    /// <summary>
    /// a data structure for storing indexing/searching instructions
    /// </summary>
    public class IndexCriteria : IIndexCriteria
    {

        public IndexCriteria(string[] umbracoFields, string[] userFields, string[] includeNodeTypes, string[] excludeNodeTypes, int? parentNodeId)
        {
            UserFields = userFields.ToList();
            UmbracoFields = umbracoFields.ToList();
            IncludeNodeTypes = includeNodeTypes;
            ExcludeNodeTypes = excludeNodeTypes;
            ParentNodeId = parentNodeId;
        }
        
        public IEnumerable<string> UmbracoFields { get; private set; }
        public IEnumerable<string> UserFields { get; private set; }

        public string[] IncludeNodeTypes { get; private set; }
        public string[] ExcludeNodeTypes { get; private set; }
        public int? ParentNodeId { get; private set; }
    }

    
}
