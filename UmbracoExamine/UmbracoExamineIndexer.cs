using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Examine;
using Examine.Providers;
using umbraco.cms.businesslogic;
using UmbracoExamine.DataServices;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using UmbracoExamine.Config;


namespace UmbracoExamine
{
    /// <summary>
    /// 
    /// </summary>
	public class UmbracoExamineIndexer : Examine.LuceneEngine.LuceneExamineIndexer, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoExamineIndexer()
            : base() { }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        public UmbracoExamineIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath)
            : base(indexerData, indexPath) { }

        #endregion

        #region Constants & Fields
        
        /// <summary>
        /// Used to store the path of a content object
        /// </summary>
        public const string IndexPathFieldName = "__Path";

        /// <summary>
        /// A type that defines the type of index for each Umbraco field (non user defined fields)
        /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
        /// for retreival after searching.
        /// </summary>
        internal static readonly Dictionary<string, FieldIndexTypes> IndexFieldPolicies
            = new Dictionary<string, FieldIndexTypes>()
            {
                { "id", FieldIndexTypes.NOT_ANALYZED},
                { "version", FieldIndexTypes.NO},
                { "parentID", FieldIndexTypes.NO},
                { "level", FieldIndexTypes.NO},
                { "writerID", FieldIndexTypes.NO},
                { "creatorID", FieldIndexTypes.NO},
                { "nodeType", FieldIndexTypes.NOT_ANALYZED},
                { "template", FieldIndexTypes.NOT_ANALYZED},
                { "sortOrder", FieldIndexTypes.NO},
                { "createDate", FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { "updateDate", FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { "nodeName", FieldIndexTypes.ANALYZED},
                { "urlName", FieldIndexTypes.NOT_ANALYZED},
                { "writerName", FieldIndexTypes.NOT_ANALYZED},
                { "creatorName", FieldIndexTypes.NOT_ANALYZED},
                { "nodeTypeAlias", FieldIndexTypes.NOT_ANALYZED},
                { "path", FieldIndexTypes.NOT_ANALYZED}
            };

        #endregion

        #region Initialize

        /// <summary>
        /// Set up all properties for the indexer based on configuration information specified. This will ensure that
        /// all of the folders required by the indexer are created and exist. This will also create an instruction
        /// file declaring the computer name that is part taking in the indexing. This file will then be used to
        /// determine the master indexer machine in a load balanced environment (if one exists).
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The name of the provider is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The name of the provider has a length of zero.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        /// </exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {            

            if (config["dataService"] != null && !string.IsNullOrEmpty(config["dataService"]))
            {
                //this should be a fully qualified type
                var serviceType = Type.GetType(config["dataService"]);
                DataService = (IDataService)Activator.CreateInstance(serviceType);
            }
            else
            {
                //By default, we will be using the UmbracoDataService
                //generally this would only need to be set differently for unit testing
                DataService = new UmbracoDataService();
            }

            //check if there's a flag specifying to support unpublished content,
            //if not, set to false;
            bool supportUnpublished;
            if (config["supportUnpublished"] != null && bool.TryParse(config["supportUnpublished"], out supportUnpublished))
                SupportUnpublishedContent = supportUnpublished;
            else
                SupportUnpublishedContent = false;


            //check if there's a flag specifying to support protected content,
            //if not, set to false;
            bool supportProtected;
            if (config["supportProtected"] != null && bool.TryParse(config["supportProtected"], out supportProtected))
                SupportProtectedContent = supportProtected;
            else
                SupportProtectedContent = false;

            



            base.Initialize(name, config);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The data service used for retreiving and submitting data to the cms
        /// </summary>
        public IDataService DataService { get; protected internal set; }

        /// <summary>
        /// By default this is false, if set to true then the indexer will include indexing content that is flagged as publicly protected.
        /// This property is ignored if SupportUnpublishedContent is set to true.
        /// </summary>
        public bool SupportProtectedContent { get; protected internal set; }


        #endregion

        #region Event handlers

        protected override void OnIndexerExecutiveAssigned(IndexerExecutiveAssignedEventArgs e)
        {
            DataService.LogService.AddInfoLog(-1, string.Format("{0} machine is the Executive Indexer with {1} servers in the cluster",
                        e.MachineName,
                        e.ClusterNodeCount));

            base.OnIndexerExecutiveAssigned(e);
        }

        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            DataService.LogService.AddErrorLog(e.NodeId, string.Format("{0},{1}", e.Message, e.InnerException != null ? e.InnerException.Message : ""));
            base.OnIndexingError(e);
        }

        protected override void OnDocumentWriting(DocumentWritingEventArgs docArgs)
        {
            DataService.LogService.AddInfoLog(docArgs.NodeId, string.Format("DocumentWriting event for node ({0})", LuceneIndexFolder.FullName));
            base.OnDocumentWriting(docArgs);
        }

        protected override void OnNodeIndexed(IndexedNodeEventArgs e)
        {
            DataService.LogService.AddInfoLog(e.NodeId, string.Format("Index created for node. ({0})", LuceneIndexFolder.FullName));
            base.OnNodeIndexed(e);
        }

        protected override void OnIndexDeleted(DeleteIndexEventArgs e)
        {
            DataService.LogService.AddInfoLog(-1, string.Format("Index deleted for term: {0} with value {1}", e.DeletedTerm.Key, e.DeletedTerm.Value));
            base.OnIndexDeleted(e);
        }

        protected virtual void OnIndexOptimizing(EventArgs e)
        {
            DataService.LogService.AddInfoLog(-1, "Index is being optimized");
            base.OnIndexOptimizing(e);
        }

        #endregion

        /// <summary>
        /// Deletes a node from the index.                
        /// </summary>
        /// <remarks>
        /// When a content node is deleted, we also need to delete it's children from the index so we need to perform a 
        /// custom Lucene search to find all decendents and create Delete item queues for them too.
        /// </remarks>
        /// <param name="nodeId">ID of the node to delete</param>
        public override void DeleteFromIndex(string nodeId)
        {
            //find all descendants based on path
            var descendantPath = string.Format(@"\-1\,*{0}\,*", nodeId);
            var rawQuery = string.Format("{0}:{1}", IndexPathFieldName, descendantPath);
            var c = InternalSearcher.CreateSearchCriteria();
            var filtered = c.RawQuery(rawQuery);
            var results = InternalSearcher.Search(filtered);

            //need to create a delete queue item for each one found
            foreach (var r in results)
            {
                SaveDeleteIndexQueueItem(new KeyValuePair<string, string>(IndexNodeIdFieldName, r.Id.ToString()));
            }

            base.DeleteFromIndex(nodeId);
        }

        #region Protected

        /// <summary>
        /// Override this method to strip all html from all user fields before raising the event, then after the event 
        /// ensure our special Path field is added to the collection
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            //strip html of all users fields
            // Get all user data that we want to index and store into a dictionary 
            foreach (string fieldName in IndexerData.UserFields)
            {
                if (e.Fields.ContainsKey(fieldName))
                {
                    e.Fields[fieldName] = DataService.ContentService.StripHtml(e.Fields[fieldName]);
                }
            }

            base.OnGatheringNodeData(e);

            //ensure the special path field is added to the dictionary to be saved to file
            var path = e.Node.Attribute("path").Value;
            if (!e.Fields.ContainsKey(IndexPathFieldName)) e.Fields.Add(IndexPathFieldName, path.ToLower());
        }

        /// <summary>
        /// Called when a duplicate field is detected in the dictionary that is getting indexed.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="indexSetName"></param>
        /// <param name="fieldName"></param>
        protected override void OnDuplicateFieldWarning(int nodeId, string indexSetName, string fieldName)
        {
            base.OnDuplicateFieldWarning(nodeId, indexSetName, fieldName);

            this.DataService.LogService.AddInfoLog(nodeId, "Field \"" + fieldName + "\" is listed multiple times in the index set \"" + indexSetName + "\". Please ensure all names are unique");
        }

        /// <summary>
        /// Overridden to add the path property to the special fields to index
        /// </summary>
        /// <param name="allValuesForIndexing"></param>
        /// <returns></returns>
        protected override Dictionary<string, string> GetSpecialFieldsToIndex(Dictionary<string, string> allValuesForIndexing)
        {
            var fields = base.GetSpecialFieldsToIndex(allValuesForIndexing);

            //adds the special path property to the index
            fields.Add(IndexPathFieldName, allValuesForIndexing[IndexPathFieldName].ToLower());
            return fields;

        }

        /// <summary>
        /// Called to do the actual indexing of the data for the index type
        /// </summary>
        /// <param name="type"></param>
        protected override void PerformIndexAll(string type)
        {
            string xPath = "//*[(number(@id) > 0){0}]"; //we'll add more filters to this below if needed

            StringBuilder sb = new StringBuilder();

            //create the xpath statement to match node type aliases if specified
            if (IndexerData.IncludeNodeTypes.Count() > 0)
            {
                sb.Append("(");
                foreach (string field in IndexerData.IncludeNodeTypes)
                {
                    //this can be used across both schemas
                    string nodeTypeAlias = "(@nodeTypeAlias='{0}' or (count(@nodeTypeAlias)=0 and name()='{0}'))";

                    sb.Append(string.Format(nodeTypeAlias, field));
                    sb.Append(" or ");
                }
                sb.Remove(sb.Length - 4, 4); //remove last " or "
                sb.Append(")");
            }

            //create the xpath statement to match all children of the current node.
            if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.Append("(");
                sb.Append("contains(@path, '," + IndexerData.ParentNodeId.Value.ToString() + ",')"); //if the path contains comma - id - comma then the nodes must be a child
                sb.Append(")");
            }

            //create the full xpath statement to match the appropriate nodes. If there is a filter
            //then apply it, otherwise just select all nodes.
            var filter = sb.ToString();
            xPath = string.Format(xPath, filter.Length > 0 ? " and " + filter : "");

            //raise the event and set the xpath statement to the value returned
            var args = new IndexingNodesEventArgs(IndexerData, xPath, type);
            OnNodesIndexing(args);
            if (args.Cancel)
            {
                return;
            }

            xPath = args.XPath;

            AddNodesToIndex(xPath, type);
        }

        /// <summary>
        /// Creates an IIndexCriteria object based on the indexSet passed in and our DataService
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            return indexSet.ToIndexCriteria(DataService);
        }

        /// <summary>
        /// Must override this to do the actual re-indexing operation
        /// </summary>
        protected override void PerformIndexRebuild()
        {
            IndexAll(IndexTypes.Content);
            IndexAll(IndexTypes.Media);
        }

        /// <summary>
        /// Returns an XDocument for the entire tree stored for the IndexType specified.
        /// </summary>
        /// <param name="xPath">The xpath to the node.</param>
        /// <param name="type">The type of data to request from the data service.</param>
        /// <returns>Either the Content or Media xml. If the type is not of those specified null is returned</returns>
        protected virtual XDocument GetXDocument(string xPath, string type)
        {
            if (type == IndexTypes.Content)
            {
                if (this.SupportUnpublishedContent)
                {
                    return DataService.ContentService.GetLatestContentByXPath(xPath);
                }
                else
                {
                    return DataService.ContentService.GetPublishedContentByXPath(xPath);
                }
            }
            else if (type == IndexTypes.Media)
            {
                return DataService.MediaService.GetLatestMediaByXpath(xPath);
            }
            return null;
        }

        /// <summary>
        /// return the index policy for the field name passed in, if not found, return normal
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected override FieldIndexTypes GetPolicy(string fieldName)
        {
            var def = IndexFieldPolicies.Where(x => x.Key == fieldName);
            return (def.Count() == 0 ? FieldIndexTypes.ANALYZED : def.Single().Value);
        }

        /// <summary>
        /// Ensures that the node being indexed is of a correct type and is a descendent of the parent id specified.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override bool ValidateDocument(XElement node)
        {
            //check if this document is of a correct type of node type alias
            if (IndexerData.IncludeNodeTypes.Count() > 0)
                if (!IndexerData.IncludeNodeTypes.Contains(node.ExamineNodeTypeAlias()))
                    return false;

            //if this node type is part of our exclusion list, do not validate
            if (IndexerData.ExcludeNodeTypes.Count() > 0)
                if (IndexerData.ExcludeNodeTypes.Contains(node.ExamineNodeTypeAlias()))
                    return false;

            return base.ValidateDocument(node);
        }

        /// <summary>
        /// Collects all of the data that needs to be indexed as defined in the index set.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>A dictionary representing the data which will be indexed</returns>
        /// <param name="type"></param>
        protected override Dictionary<string, string> GetDataToIndex(XElement node, string type)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            int nodeId = int.Parse(node.Attribute("id").Value);

            // Test for access if we're only indexing published content
            // return nothing if we're not supporting protected content and it is protected, and we're not supporting unpublished content
            if (!SupportUnpublishedContent && (!SupportProtectedContent && DataService.ContentService.IsProtected(nodeId, node.Attribute("path").Value)))
                return values;

            return base.GetDataToIndex(node, type);

            return values;
        } 
        #endregion

        #region Private
        /// <summary>
        /// Adds all nodes with the given xPath root.
        /// </summary>
        /// <param name="xPath">The x path.</param>
        /// <param name="type">The type.</param>
        private void AddNodesToIndex(string xPath, string type)
        {
            // Get all the nodes of nodeTypeAlias == nodeTypeAlias
            XDocument xDoc = GetXDocument(xPath, type);
            if (xDoc != null)
            {
                XElement rootNode = xDoc.Root;

                IEnumerable<XElement> children = rootNode.Elements();

                foreach (XElement node in children)
                {
                    if (ValidateDocument(node))
                    {
                        //save the index item to a queue file
                        var fields = GetDataToIndex(node, type);

                        //make sure that 

                        SaveAddIndexQueueItem(fields, int.Parse(node.Attribute("id").Value), type);
                    }

                }
            }

            //run the indexer on all queued files
            SafelyProcessQueueItems();
        } 
        #endregion
    }
}
