using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using UmbracoExamine.DataServices;
using Examine;
using System.IO;
using System.Xml.Linq;

namespace UmbracoExamine
{

    /// <summary>
    /// An abstract provider containing the basic functionality to be able to query against
    /// Umbraco data.
    /// </summary>
    public abstract class BaseUmbracoIndexer : LuceneIndexer
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        protected BaseUmbracoIndexer()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        protected BaseUmbracoIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, analyzer, async)
        {
            DataService = dataService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The data service used for retreiving and submitting data to the cms
        /// </summary>
        public IDataService DataService { get; protected internal set; }

        /// <summary>
        /// the supported indexable types
        /// </summary>
        protected abstract IEnumerable<string> SupportedTypes { get; }

        #endregion

        #region Initialize


        /// <summary>
        /// Setup the properties for the indexer from the provider settings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
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

            DataService.LogService.LogLevel = LoggingLevel.Normal;

            if (config["logLevel"] != null && !string.IsNullOrEmpty(config["logLevel"]))
            {
                try
                {
                    var logLevel = (LoggingLevel)Enum.Parse(typeof(LoggingLevel), config["logLevel"]);
                    DataService.LogService.LogLevel = logLevel;
                }
                catch (ArgumentException)
                {                    
                    //FAILED
                    DataService.LogService.LogLevel = LoggingLevel.Normal;
                }
            }

            DataService.LogService.AddVerboseLog(-1, string.Format("{0} indexer initializing", name));

            base.Initialize(name, config);
        }

        #endregion

        #region Protected

        /// <summary>
        /// Ensures that the node being indexed is of a correct type and is a descendent of the parent id specified.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override bool ValidateDocument(XElement node)
        {
            //check if this document is a descendent of the parent
            if (IndexerData.ParentNodeId.HasValue && IndexerData.ParentNodeId.Value > 0)
                if (!((string)node.Attribute("path")).Contains("," + IndexerData.ParentNodeId.Value.ToString() + ","))
                    return false;

            return base.ValidateDocument(node);
        }

        /// <summary>
        /// Reindexes all supported types
        /// </summary>
        protected override void PerformIndexRebuild()
        {
            foreach (var t in SupportedTypes)
            {
                IndexAll(t);
            }
        }

        public override void ReIndexNode(XElement node, string type)
        {
            if (!SupportedTypes.Contains(type))
                return;

            base.ReIndexNode(node, type);
        }

        /// <summary>
        /// Builds an xpath statement to query against Umbraco data for the index type specified, then
        /// initiates the re-indexing of the data matched.
        /// </summary>
        /// <param name="type"></param>
        protected override void PerformIndexAll(string type)
        {
            if (!SupportedTypes.Contains(type))
                return;

            var xPath = "//*[(number(@id) > 0 and (@isDoc or @nodeTypeAlias)){0}]"; //we'll add more filters to this below if needed

            var sb = new StringBuilder();

            //create the xpath statement to match node type aliases if specified
            if (IndexerData.IncludeNodeTypes.Count() > 0)
            {
                sb.Append("(");
                foreach (var field in IndexerData.IncludeNodeTypes)
                {
                    //this can be used across both schemas
                    const string nodeTypeAlias = "(@nodeTypeAlias='{0}' or (count(@nodeTypeAlias)=0 and name()='{0}'))";

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
                sb.Append("contains(@path, '," + IndexerData.ParentNodeId.Value + ",')"); //if the path contains comma - id - comma then the nodes must be a child
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

            DataService.LogService.AddVerboseLog(-1, string.Format("({0}) PerformIndexAll with XPATH: {1}", this.Name, xPath));

            AddNodesToIndex(xPath, type);
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

                AddNodesToIndex(children, type);
            }

        }
        #endregion
    }
}
