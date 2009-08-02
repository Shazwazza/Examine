using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using System.Xml;
using System.Collections;
using umbraco.cms;
using System.Xml.XPath;
using System.Xml.Linq;
using UmbracoExamine;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using UmbracoExamine.Configuration;
using UmbracoExamine.Core;
using System.Runtime.CompilerServices;
using umbraco.BusinessLogic;


namespace UmbracoExamine.Lucene2
{

    /// <summary>
    /// Used to index umbraco data. The UmbracoIndexer is configured by application settings.
    /// </summary>
    public class UmbracoIndexer : IIndexer
    {

        /// <summary>
        /// Initializes a new indexer using the default index set.
        /// </summary>
        public UmbracoIndexer() : this(IndexSets.Instance.DefaultIndexSet) { }

        /// <summary>
        /// Initializes a new indexer using the specified index set.
        /// </summary>
        /// <param name="setName"></param>
        public UmbracoIndexer(string setName)
        {
            //get the index set
            IndexSet indexSet = IndexSets.Instance.Sets[setName];

            IndexerData = indexSet.ToIndexerData();

        }

        /// <summary>
        /// Manually create an Umbraco indexer
        /// </summary>
        /// <param name="indexerData"></param>
        public UmbracoIndexer(IndexerData indexerData)
        {
            IndexerData = indexerData;
        }

        /// <summary>
        /// Manually creates an Umbraco indexer
        /// </summary>
        /// <param name="umbracoFields">The umbraco fields to index/search</param>
        /// <param name="userFields">The umbraco fields to index/search</param>
        /// <param name="indexPath">The path to the index files</param>        
        /// <param name="parentNodeId">The parent node id of the children that are to be indexed, set to null for all</param>
        public UmbracoIndexer(string[] umbracoFields, string[] userFields, string indexPath, string[] includeNodeTypes, string[] excludeNodeTypes, int? parentNodeId, int maxResults)
        {
            IndexerData = new IndexerData(umbracoFields,
                                userFields,
                                indexPath,
                                includeNodeTypes,
                                excludeNodeTypes,
                                parentNodeId, 
                                maxResults);
        }

        protected IndexerData IndexerData { get; private set; }

        /// <summary>
        /// Returns a string array of all fields that are indexed including Umbraco fields
        /// </summary>
        public string[] AllIndexedFields
        {
            get
            {
                return IndexerData.UserFields.Concat(IndexerData.UmbracoFields).ToArray();
            }
        }

        private void AddLog(int nodeId, string msg, LogTypes type)
        {
            Log.Add(type, nodeId, "[UmbracoExamine] " + msg);
        }

        #region Events

        public event IndexingNodesEventHandler IndexingNodesBegin;
        public event IndexingNodesEventHandler IndexingNodesComplete;
        public event IndexingNodeEventHandler DocumentCreated;
        public event IndexingNodeDataEventHandler GatheringNodeData;
        public event IndexingNodeEventHandler NodeIndexDeleted;
        public event IndexingFieldDataEventHandler GatheringFieldData;
        public event IndexingErrorEventHandler IndexingError;
        public event IndexingNodeDataEventHandler IgnoringNode;

        #region Delegates
        public delegate void IndexingNodeEventHandler(object sender, IndexingNodeEventArgs e);
        public delegate void IndexingErrorEventHandler(object sender, IndexingErrorEventArgs e);

        /// <summary>
        /// Returns the value of what will be indexed. Event subscribers should return e.FieldValue if they wish not to modify it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate string IndexingFieldDataEventHandler(object sender, IndexingFieldDataEventArgs e);

        /// <summary>
        /// Returns the full dictionary of what will be indexed. Event subscribers should return e.Value if they wish not to modify it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate Dictionary<string, string> IndexingNodeDataEventHandler(object sender, IndexingNodeDataEventArgs e);

        /// <summary>
        /// Returns the xpath statement to select the umbraco nodes that will be indexed. Event subscribers should return e.XPath if they wish not to modify it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public delegate string IndexingNodesEventHandler(object sender, IndexingNodesEventArgs e);
        #endregion

        /// <summary>
        /// Called when a node is ignored by the ValidateDocument method.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnIgnoringNode(IndexingNodeDataEventArgs e)
        {
            if (IgnoringNode != null)
                IgnoringNode(this, e);
        }

        protected virtual void OnIndexingError(IndexingErrorEventArgs e)
        {
            AddLog(e.NodeId, e.Message + ". INNER EXCEPTION: " + e.InnerException.Message, LogTypes.Error);

            if (IndexingError != null)
                IndexingError(this, e);
        }

        protected virtual void OnDocumentCreated(IndexingNodeEventArgs e)
        {
            AddLog(e.NodeId, string.Format("Index created for node. ({0})", IndexerData.IndexDirectory.FullName), LogTypes.System);

            if (DocumentCreated != null)
                DocumentCreated(this, e);
        }

        protected virtual void OnNodeIndexDeleted(IndexingNodeEventArgs e)
        {
            AddLog(e.NodeId, string.Format("Index deleted for node ({0})", IndexerData.IndexDirectory.FullName), LogTypes.System);

            if (NodeIndexDeleted != null)
                NodeIndexDeleted(this, e);
        }

        protected virtual Dictionary<string, string> OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            if (GatheringNodeData != null)
                return GatheringNodeData(this, e);

            return e.Values;
        }

        protected virtual string OnGatheringFieldData(IndexingFieldDataEventArgs e)
        {
            if (GatheringFieldData != null)
                return GatheringFieldData(this, e);

            return e.FieldValue;
        }

        protected virtual string OnIndexingNodesBegin(IndexingNodesEventArgs e)
        {
            if (IndexingNodesBegin != null)
                IndexingNodesBegin(this, e);

            return e.XPath;
        }

        /// <summary>
        /// Though this returns an xpath statement, the return value does nothing.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual string OnIndexingNodesComplete(IndexingNodesEventArgs e)
        {

            if (IndexingNodesComplete != null)
                IndexingNodesComplete(this, e);

            return e.XPath;
        }

        #endregion

        #region Indexing

        #region Public        

        /// <summary>
        /// Re-indexes everything defined for the set name.
        /// Locking is applied to this method
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void IndexAll()
        {
            IndexWriter writer = null;
            try
            {
                //ensure the folder exists
                VerifyFolder(IndexerData.IndexDirectory);

                //check if the index exists and it's locked
                if (IndexExists() && !IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot index node, the index is currently locked", -1, new Exception()));
                    return;
                }

                //create the writer (this will overwrite old index files)
                writer = new IndexWriter(IndexerData.IndexDirectory.FullName, new StandardAnalyzer(), true);

                string xPath = "//node[{0}]";
                StringBuilder sb = new StringBuilder();

                //create the xpath statement to match node type aliases if specified
                if (IndexerData.IncludeNodeTypes.Length > 0)
                {
                    sb.Append("(");
                    foreach (string field in IndexerData.IncludeNodeTypes)
                    {
                        string nodeTypeAlias = "@nodeTypeAlias='{0}'";
                        sb.Append(string.Format(nodeTypeAlias, field));
                        sb.Append(" or ");
                    }
                    sb.Remove(sb.Length - 4, 4); //remove last " or "
                    sb.Append(")");
                }

                //create the xpath statement to match all children of the current node.
                if (IndexerData.ParentNodeId.HasValue)
                {
                    if (sb.Length > 0)
                        sb.Append(" and ");
                    sb.Append("(");
                    //contains(@path, ',1234,')
                    sb.Append("contains(@path, '," + IndexerData.ParentNodeId.Value.ToString() + ",')"); //if the path contains comma - id - comma then the nodes must be a child
                    sb.Append(")");
                }

                //create the full xpath statement to match the appropriate nodes
                xPath = string.Format(xPath, sb.ToString());

                //in case there are no filters:
                xPath = xPath.Replace("[]", "");

                //raise the event and set the xpath statement to the value returned
                xPath = OnIndexingNodesBegin(new IndexingNodesEventArgs(IndexerData, xPath));

                AddNodesToIndex(xPath, writer);

                //raise the completed event, the data returned is irrelevant.
                OnIndexingNodesComplete(new IndexingNodesEventArgs(IndexerData, xPath));

                writer.Optimize();
            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("An error occured recreating the index set", -1, ex));
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

        }


        /// <summary>
        /// Removes a node from the Lucene index.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool DeleteFromIndex(int nodeId)
        {
            IndexReader ir = null;
            try
            {
                VerifyFolder(IndexerData.IndexDirectory);

                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                    return true;

                ir = IndexReader.Open(IndexerData.IndexDirectory.FullName);
                ir.DeleteDocuments(new Term("id", nodeId.ToString()));
                               
                OnNodeIndexDeleted(new IndexingNodeEventArgs(nodeId));
                return true;
            }
            catch (Exception ee)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error deleting Lucene index", nodeId, ee));
                return false;
            }
            finally
            {
                if (ir != null)
                    ir.Close();
            }
        }


        /// <summary>
        /// Deletes and re-creates the node
        /// </summary>
        /// <param name="nodeId"></param>
        public void ReIndexNode(int nodeId)
        {
            if (DeleteFromIndex(nodeId))
                AddSingleNodeToIndex(nodeId);

        }

        /// <summary>
        /// Adds single node to index. If the node already exists, a duplicate will probably be created. To re-index, use the ReIndex method.
        /// </summary>
        /// <param name="nodeID"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddSingleNodeToIndex(int nodeID)
        {
            if (nodeID <= 0)
                return;
            IndexWriter writer = null;
            try
            {
                VerifyFolder(IndexerData.IndexDirectory);

                //check if the index doesn't exist, and if so, create it and reindex everything
                if (!IndexExists())
                    IndexAll();

                //check if the index is ready to be written to.
                if (!IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot index node, the index is currently locked", nodeID, new Exception()));
                    return;
                }

                XPathNodeIterator umbXml = umbraco.library.GetXmlNodeById(nodeID.ToString());
                XDocument xDoc = umbXml.UmbToXDocument();
                var rootNode = xDoc.Elements().First();
                if (!ValidateDocument(rootNode))
                {
                    OnIgnoringNode(new IndexingNodeDataEventArgs(rootNode, null, nodeID));
                    return;
                }

                //check if we need to create a new index...
                bool createNew = false;
                if (!IndexReader.IndexExists(IndexerData.IndexDirectory.FullName))
                    createNew = true;
                writer = new IndexWriter(IndexerData.IndexDirectory.FullName, new StandardAnalyzer(), createNew);
                AddDocument(GetDataToIndex(rootNode), writer, nodeID);

                writer.Optimize();

            }
            catch (Exception ex)
            {
                OnIndexingError(new IndexingErrorEventArgs("Error deleting Lucene index", nodeID, ex));
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
        #endregion

        #region Protected
        /// <summary>
        /// Ensures that the node being indexed is of a correct type and is a descendent of the parent id specified.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual bool ValidateDocument(XElement node)
        {
            //check if this document is of a correct type of node type alias
            if (IndexerData.IncludeNodeTypes.Length > 0)
                if (!IndexerData.IncludeNodeTypes.Contains((string)node.Attribute("nodeTypeAlias")))
                    return false;

            //if this node type is part of our exclusion list, do not validate
            if (IndexerData.ExcludeNodeTypes.Length > 0)
                if (IndexerData.ExcludeNodeTypes.Contains((string)node.Attribute("nodeTypeAlias")))
                    return false;

            //check if this document is a descendent of the parent
            if (IndexerData.ParentNodeId.HasValue)
                if (!((string)node.Attribute("path")).Contains("," + IndexerData.ParentNodeId.Value.ToString() + ","))
                    return false;

            return true;
        }

        /// <summary>
        /// Collects all of the data that neesd to be indexed as defined in the index set.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> GetDataToIndex(XElement node)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            int nodeId = int.Parse(node.Attribute("id").Value);

            // Test for access
            if (umbraco.library.IsProtected(nodeId, node.Attribute("path").Value))
                return values;

            // Get all user data that we want to index and store into a dictionary 
            foreach (string fieldName in IndexerData.UserFields)
            {
                // Get the value of the data                
                string value = node.UmbSelectDataValue(fieldName);
                //raise the event and assign the value to the returned data from the event
                value = OnGatheringFieldData(new IndexingFieldDataEventArgs(node, fieldName, value, false, nodeId));
                if (!string.IsNullOrEmpty(value))
                    values.Add(fieldName, umbraco.library.StripHtml(value));
            }

            // Add umbraco node properties 
            foreach (string fieldName in IndexerData.UmbracoFields)
            {
                string val = (string)node.Attribute(fieldName);
                val = OnGatheringFieldData(new IndexingFieldDataEventArgs(node, fieldName, val, true, nodeId));
                values.Add(fieldName, val);
            }

            //raise the event and assign the value to the returned data from the event
            values = OnGatheringNodeData(new IndexingNodeDataEventArgs(node, values, nodeId));

            return values;
        }


        /// <summary>
        /// Collects the data for the fields and adds the document.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="writer"></param>
        /// <param name="nodeId"></param>
        protected virtual void AddDocument(Dictionary<string, string> fields, IndexWriter writer, int nodeId)
        {
            Document d = new Document();
            //add all of our fields to the document index individally            
            fields.ToList().ForEach(x => d.Add(new Field(x.Key, x.Value, Field.Store.YES, Field.Index.TOKENIZED, Field.TermVector.YES)));

            writer.AddDocument(d);

            OnDocumentCreated(new IndexingNodeEventArgs(nodeId));
        }
        #endregion

        #region Private
        /// <summary>
        /// Adds all nodes with the given xPath root.
        /// </summary>
        /// <param name="xPath"></param>
        /// <param name="writer"></param>
        private void AddNodesToIndex(string xPath, IndexWriter writer)
        {
            // Get all the nodes of nodeTypeAlias == nodeTypeAlias
            XPathNodeIterator umbXml = umbraco.library.GetXmlNodeByXPath(xPath);
            XDocument xDoc = umbXml.UmbToXDocument();
            XElement rootNode = xDoc.Elements().First();

            IEnumerable<XElement> children = rootNode.Elements();

            foreach (XElement node in children)
            {
                if (ValidateDocument(node))
                    AddDocument(GetDataToIndex(node), writer, int.Parse(node.Attribute("id").Value));
            }

        }

        /// <summary>
        /// Creates the folder if it does not exist.
        /// </summary>
        /// <param name="folder"></param>
        private void VerifyFolder(DirectoryInfo folder)
        {
            if (!folder.Exists)
                folder.Create();
        }

        /// <summary>
        /// Checks if the index is ready to open/write to.
        /// </summary>
        /// <returns></returns>
        private bool IndexReady()
        {
            return (!IndexReader.IsLocked(IndexerData.IndexDirectory.FullName));
        }

        /// <summary>
        /// If the index doesn't exist, then create it AND re=index everything.
        /// </summary>
        /// <returns></returns>
        private bool IndexExists()
        {
            return (IndexReader.IndexExists(IndexerData.IndexDirectory.FullName));
        }

        #endregion

        #endregion Indexing

        #region Searching

        #region Public
        /// <summary>
        /// Search all fields specified in the index set.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="includeWildcards"></param>
        /// <returns></returns>
        public List<SearchResult> Search(string text, bool includeWildcards)
        {
            return Search(text, "", includeWildcards, null, AllIndexedFields, IndexerData.MaxResults);
        }

        /// <summary>
        /// Custom search. Will search in all fields specified in the index set.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="nodeTypeAlias"></param>
        /// <param name="includeWildcards"></param>
        /// <param name="startNodeId"></param>
        /// <returns></returns>
        public List<SearchResult> Search(string text, string nodeTypeAlias, bool includeWildcards, int? startNodeId)
        {
            return Search(text, nodeTypeAlias, includeWildcards, startNodeId, AllIndexedFields, IndexerData.MaxResults);
        }

        /// <summary>
        /// Custom search
        /// </summary>
        /// <param name="text">The text to search for</param>
        /// <param name="nodeTypeAlias">The node type aliases to search in</param>
        /// <param name="includeWildcards">whether or not to use wildcard matching</param>
        /// <param name="startNodeId">If specified will only match children of this node id</param>
        /// <param name="searchFields">What fields to search on</param>
        /// <returns></returns>
        public List<SearchResult> Search(string text, string nodeTypeAlias, bool includeWildcards, int? startNodeId, string[] searchFields, int maxResults)
        {
            IndexSearcher searcher = null;
            try
            {
                text = text.ToLower();
                nodeTypeAlias = nodeTypeAlias.ToLower();

                List<SearchResult> results = new List<SearchResult>();
                if (string.IsNullOrEmpty(text))
                    return results;

                // Remove all entries that are 2 letters or less, remove other invalid search chars. Replace all " " with AND 
                string queryText = PrepareSearchText(text, true, true);

                searcher = new IndexSearcher(IndexerData.IndexDirectory.FullName);

                //create the full query
                BooleanQuery fullQry = new BooleanQuery();

                //add the nodeTypeAlias query if specified
                //TODO : Allow for multiple node type aliases
                if (!string.IsNullOrEmpty(nodeTypeAlias))
                    fullQry.Add(GetNodeTypeLookupQuery(nodeTypeAlias), BooleanClause.Occur.MUST);

                //add the path query if specified
                if (startNodeId.HasValue)
                {
                    Query qryParent = GetParentDocQuery(startNodeId.Value);
                    if (qryParent == null)
                        return results;
                    fullQry.Add(qryParent, BooleanClause.Occur.MUST);
                }

                //create an inner query to query our fields using both an exact match and wildcard match.
                BooleanQuery fieldQry = new BooleanQuery();
                Query standardFieldQry = GetStandardFieldQuery(queryText, searchFields);
                fieldQry.Add(standardFieldQry, BooleanClause.Occur.SHOULD);
                if (includeWildcards)
                {
                    //get the wildcard query
                    Query wildcardFieldQry = GetWildcardFieldQuery(queryText, searchFields);
                    //change the weighting of the queries so exact match have a higher priority
                    standardFieldQry.SetBoost(2);
                    wildcardFieldQry.SetBoost((float)0.5);
                    fieldQry.Add(wildcardFieldQry, BooleanClause.Occur.SHOULD);
                }

                fullQry.Add(fieldQry, BooleanClause.Occur.MUST);

                TopDocs tDocs = searcher.Search(fullQry, (Filter)null, maxResults);

                results = PrepareResults(tDocs, searchFields, searcher);

                return results.ToList();
            }
            finally
            {
                if (searcher != null)
                    searcher.Close();
            }
        }
        #endregion

        #region Private
        /// <summary>
        /// This will create a query with wildcards to match.
        /// TODO: this doesn't support prefixed wildcards. this must be enabled in lucene and produces very slow queries.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Query GetWildcardFieldQuery(string text, string[] searchFields)
        {
            string queryText = PrepareSearchText(text, false, false);
            List<string> words = queryText.Split(" ".ToCharArray(), StringSplitOptions.None).ToList();
            List<string> fixedWords = new List<string>();
            words.ForEach(x => fixedWords.Add(x + "*"));
            string wildcardQuery = PrepareSearchText(string.Join(" ", fixedWords.ToArray()), false, true);
            //now that we have a wildcard match for each word, we'll make a query with it

            BooleanClause.Occur[] bc = new BooleanClause.Occur[searchFields.Length];
            for (int i = 0; i < bc.Length; i++)
            {
                bc[i] = BooleanClause.Occur.SHOULD;
            }

            return MultiFieldQueryParser.Parse(wildcardQuery, searchFields, bc, new StandardAnalyzer());
        }

        /// <summary>
        /// Return a standard query to query all of our fields
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns></returns>
        private Query GetStandardFieldQuery(string queryText, string[] searchFields)
        {
            BooleanClause.Occur[] bc = new BooleanClause.Occur[searchFields.Length];
            for (int i = 0; i < bc.Length; i++)
            {
                bc[i] = BooleanClause.Occur.SHOULD;
            }

            return MultiFieldQueryParser.Parse(queryText, searchFields, bc, new StandardAnalyzer());
        }

        /// <summary>
        /// Return a query to query for a node type Alias
        /// </summary>
        /// <param name="nodeTypeAlias"></param>
        /// <returns></returns>
        private Query GetNodeTypeLookupQuery(string nodeTypeAlias)
        {
            PhraseQuery phraseQuery = new PhraseQuery();
            string[] terms = nodeTypeAlias.Split(' ');
            foreach (string term in terms)
                phraseQuery.Add(new Term("nodeTypeAlias", term.ToLower()));
            return phraseQuery;
        }

        /// <summary>
        /// Returns a query to ensure only the children of the document type specified are returned
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private Query GetParentDocQuery(int nodeId)
        {
            umbraco.cms.businesslogic.web.Document doc = new umbraco.cms.businesslogic.web.Document(nodeId);
            if (doc == null)
                return null;

            List<string> path = doc.Path.Split(',').ToList();
            List<string> searchPath = new List<string>();
            int idIndex = path.IndexOf(nodeId.ToString());
            for (int i = 0; i <= idIndex; i++)
                searchPath.Add(path[i]);

            //need to remove the leading "-" as Lucene will not search on this for whatever reason.
            string pathQuery = (string.Join(",", searchPath.ToArray()) + ",*").Replace("-", "");
            return new WildcardQuery(new Term("path", pathQuery));
        }

        /// <summary>
        /// Removes spaces, small strings, and invalid characters
        /// </summary>
        /// <param name="text">the text to prepare for search</param>
        /// <param name="removeWildcards">whether or not to remove wildcard chars</param>
        /// <param name="addBooleans">whether or not to add boolean "AND" logic between words. If false, words are returned with spaces.</param>
        /// <returns></returns>
        private string PrepareSearchText(string text, bool removeWildcards, bool addBooleans)
        {
            if (text.Length < 3)
                return "";

            string charsToRemove = "~!@#$%^&()_+`-={}|[]\\:\";'<>,./";

            if (removeWildcards)
                charsToRemove = charsToRemove.Replace("*", "").Replace("?", "");

            List<string> words = new List<string>();

            // Remove all spaces and strings <= 2 chars
            words = text.Trim()
                        .Split(' ')
                        .Select(x => x.ToString())
                        .Where(x => x.Length > 2).ToList();

            // Remove all other invalid chars
            for (int i = 0; i < words.Count(); i++)
                foreach (char c in charsToRemove)
                    words[i] = words[i].Replace(c.ToString(), "");

            if (addBooleans)
            {
                // Create new text
                string queryText = "";
                words.ForEach(x => queryText += " AND " + x.ToString());

                return queryText.Remove(0, 5); // remove first " AND "
            }
            else
            {
                return string.Join(" ", words.ToArray());
            }

        }

        /// <summary>
        /// Creates a list of dictionary's from the hits object and returns a list of SearchResult.
        /// This also removes duplicates.
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="searchFields"></param>
        /// <returns></returns>
        private List<SearchResult> PrepareResults(TopDocs tDocs, string[] searchFields, IndexSearcher searcher)
        {
            List<SearchResult> results = new List<SearchResult>();

            for (int i = 0; i < tDocs.scoreDocs.Length; i++)
            {
                Document doc = searcher.Doc(tDocs.scoreDocs[i].doc);
                Dictionary<string, string> fields = new Dictionary<string, string>();

                foreach (Field f in doc.Fields())
                {
                    if (searchFields.Contains(f.Name()))
                        fields.Add(f.Name(), f.StringValue());
                }

                results.Add(new SearchResult()
                {
                    Score = tDocs.scoreDocs[i].score,
                    Id = int.Parse(fields["id"]), //if the id field isn't indexed in the config, an error will occur!
                    Fields = fields
                });
            }

            //return the distinct results ordered by the highest score descending.
            return (from r in results.Distinct().ToList()
                    orderby r.Score descending
                    select r).ToList();
        }
        #endregion

        #endregion Searching
    }
}
