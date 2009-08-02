using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UmbracoExamine
{
    /// <summary>
    /// a data structure for storing indexing instructions
    /// </summary>
    public class IndexerData
    {

        public IndexerData(string[] umbracoFields, string[] userFields, string indexPath, string[] includeNodeTypes, string[] excludeNodeTypes, int? parentNodeId, int maxResults)
        {
            IndexPath = indexPath;
            UserFields = userFields;
            UmbracoFields = umbracoFields;
            IncludeNodeTypes = includeNodeTypes;
            ExcludeNodeTypes = excludeNodeTypes;
            ParentNodeId = parentNodeId;
            IndexDirectory = new DirectoryInfo(IndexPath);
            MaxResults = maxResults;
        }

        public string IndexPath { get; private set; }
        public string[] UmbracoFields { get; private set; }
        public string[] UserFields { get; private set; }
        public string[] IncludeNodeTypes { get; private set; }
        public string[] ExcludeNodeTypes { get; private set; }
        public int? ParentNodeId { get; private set; }
        public DirectoryInfo IndexDirectory { get; private set; }
        public int MaxResults { get; private set; }
    }
}
