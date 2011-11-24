using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Examine;
using Examine.Azure;
using Examine.LuceneEngine.Config;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.Azure
{
    public class UmbracoAzureDataService : UmbracoDataService
    {
        public UmbracoAzureDataService()
        {
            //overwrite the log service
            LogService = new UmbracoAzureLogService();
        }
    }

    public class UmbracoAzureLogService : ILogService
    {
        public string ProviderName { get; set; }

        public void AddErrorLog(int nodeId, string msg)
        {
            AzureExtensions.LogExceptionFile(ProviderName, new IndexingErrorEventArgs(msg, nodeId, null));
        }

        public void AddInfoLog(int nodeId, string msg)
        {
            AzureExtensions.LogMessageFile("[UmbracoExamine] (" + ProviderName + ")" + msg + ". " + nodeId);
        }

        public void AddVerboseLog(int nodeId, string msg)
        {
            if (LogLevel == LoggingLevel.Verbose)
                AzureExtensions.LogMessageFile("[UmbracoExamine] (" + ProviderName + ")" + msg + ". " + nodeId);
        }

        public LoggingLevel LogLevel { get; set; }
    }

    public class AzureContentIndexer : UmbracoContentIndexer, IAzureCatalogue
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static AzureContentIndexer()
        {
            AzureExtensions.EnsureAzureConfig();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AzureContentIndexer()
            : base() { }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        public AzureContentIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {
            
        }

        public string Catalogue { get; private set; }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config["dataService"] != null && !string.IsNullOrEmpty(config["dataService"]))
            {
                //this should be a fully qualified type
                var serviceType = Type.GetType(config["dataService"]);
                DataService = (IDataService)Activator.CreateInstance(serviceType);
            }
            else if (DataService == null)
            {
                //By default, we will be using the UmbracoAzureDataService
                DataService = new UmbracoAzureDataService();
            }

            base.Initialize(name, config);

            this.SetOptimizationThresholdOnInit(config);
            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;

            AzureExtensions.LogMessageFile(string.Format("Azure Indexer {0} initialized with cataloge {1}", name, Catalogue));
        }

        private Lucene.Net.Store.Directory _directory;
        public override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            return _directory ?? (_directory = this.GetAzureDirectory());
        }

        public override Lucene.Net.Index.IndexWriter GetIndexWriter()
        {
            return this.GetAzureIndexWriter();
        }

        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            AzureExtensions.LogExceptionFile(Name, e);
            base.OnIndexingError(e);
        }

    }
}
