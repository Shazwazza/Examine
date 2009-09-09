using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.Core;
using UmbracoExamine.Providers.Config;
using System.IO;
using umbraco.BusinessLogic;
using Lucene.Net.Index;
using System.Xml.Linq;
using System.Xml.XPath;
using Lucene.Net.Documents;
using System.Runtime.CompilerServices;
using Lucene.Net.Analysis.Standard;

namespace UmbracoExamine.Providers
{
    public class LuceneExamineIndexer : BaseIndexProvider
    {
        public LuceneExamineIndexer() : base() { }
        public LuceneExamineIndexer(IIndexCriteria indexerData) : base(indexerData) { }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            //need to check if the index set is specified
            if (config["indexSet"] == null && IndexerData == null)
                throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration and/or the IndexerData property has not been explicitly set");

            if (ExamineLuceneIndexes.Instance.Sets[config["indexSet"]] == null)
                throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

            IndexSetName = config["indexSet"];

            //get the index criteria
            IndexerData = ExamineLuceneIndexes.Instance.Sets[IndexSetName].ToIndexerData();

            //get the folder to index
            LuceneIndexFolder = ExamineLuceneIndexes.Instance.Sets[IndexSetName].IndexDirectory;
        }

        public DirectoryInfo LuceneIndexFolder { get; protected set; }
        
        protected string IndexSetName { get; set; }

        /// <summary>
        /// Adds a log entry to the umbraco log
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        private void AddLog(int nodeId, string msg, LogTypes type)
        {
            Log.Add(type, nodeId, "[UmbracoExamine] " + msg);
        }

        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            AddLog(e.NodeId, e.Message + ". INNER EXCEPTION: " + e.InnerException.Message, LogTypes.Error);
            base.OnIndexingError(e);
        }

        protected override void OnNodeIndexed(IndexingNodeEventArgs e)
        {
            AddLog(e.NodeId, string.Format("Index created for node. ({0})", LuceneIndexFolder.FullName), LogTypes.System);
            base.OnNodeIndexed(e);
        }

        protected override void OnNodeIndexDeleted(IndexingNodeEventArgs e)
        {
            AddLog(e.NodeId, string.Format("Index deleted for node ({0})", LuceneIndexFolder.FullName), LogTypes.System);
            base.OnNodeIndexDeleted(e);
        }
   
        #region Provider implementation

        public override void ReIndexNode(int nodeId)
        {
            if (DeleteFromIndex(nodeId))
                AddSingleNodeToIndex(nodeId);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool DeleteFromIndex(int nodeId)
        {
            IndexReader ir = null;
            try
            {
                VerifyFolder(LuceneIndexFolder);

                //if the index doesn't exist, then no don't attempt to open it.
                if (!IndexExists())
                    return true;

                ir = IndexReader.Open(LuceneIndexFolder.FullName);
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void IndexAll()
        {
            IndexWriter writer = null;
            try
            {
                //ensure the folder exists
                VerifyFolder(LuceneIndexFolder);

                //check if the index exists and it's locked
                if (IndexExists() && !IndexReady())
                {
                    OnIndexingError(new IndexingErrorEventArgs("Cannot index node, the index is currently locked", -1, new Exception()));
                    return;
                }

                //create the writer (this will overwrite old index files)
                writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), true);

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
                xPath = OnNodesIndexing(new IndexingNodesEventArgs(IndexerData, xPath));

                AddNodesToIndex(xPath, writer);

                //raise the completed event, the data returned is irrelevant.
                OnNodesIndexed(new IndexingNodesEventArgs(IndexerData, xPath));

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
                VerifyFolder(LuceneIndexFolder);

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
                if (!IndexReader.IndexExists(LuceneIndexFolder.FullName))
                    createNew = true;
                writer = new IndexWriter(LuceneIndexFolder.FullName, new StandardAnalyzer(), createNew);
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

            OnNodeIndexed(new IndexingNodeEventArgs(nodeId));
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
            return (!IndexReader.IsLocked(LuceneIndexFolder.FullName));
        }

        /// <summary>
        /// If the index doesn't exist, then create it AND re=index everything.
        /// </summary>
        /// <returns></returns>
        private bool IndexExists()
        {
            return (IndexReader.IndexExists(LuceneIndexFolder.FullName));
        }

        #endregion
    }
}
