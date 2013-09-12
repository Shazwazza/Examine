using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Examine.LuceneEngine;
using Examine;
using Examine.LuceneEngine.Config;
using System.Xml.Linq;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Analysis;

namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// An index provider that can be used to index simple data structures such as those from a database, dictionary or array.
    /// </summary>
    public class SimpleDataIndexer : LuceneIndexer
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public SimpleDataIndexer()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        /// <param name="dataService"></param>
        /// <param name="indexTypes"></param>
        /// <param name="async"></param>
		
		public SimpleDataIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer, ISimpleDataService dataService, IEnumerable<string> indexTypes, bool async)
            : base(indexerData, workingFolder, analyzer, async)
        {
            DataService = dataService;
            IndexTypes = indexTypes;
        }

		/// <summary>
		/// Constructor to allow for creating an indexer at runtime
		/// </summary>
		/// <param name="indexerData"></param>
		/// <param name="luceneDirectory"></param>
		/// <param name="analyzer"></param>
		/// <param name="dataService"></param>
		/// <param name="indexTypes"></param>
		/// <param name="async"></param>
		
		public SimpleDataIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer, ISimpleDataService dataService, IEnumerable<string> indexTypes, bool async)
			: base(indexerData, luceneDirectory, analyzer, async)
		{
			DataService = dataService;
			IndexTypes = indexTypes;
		}

        /// <summary>
        /// The data service used to retrieve all of the data for an index type
        /// </summary>
        public ISimpleDataService DataService { get; set; }

        /// <summary>
        /// A list of index types defined for this indexer
        /// </summary>
        public IEnumerable<string> IndexTypes { get; set; }

        /// <summary>
        /// Gets the data for the index type from the data service and indexes it.
        /// </summary>
        /// <param name="type"></param>
        protected override void PerformIndexAll(string type)
        {
            //get the data for the index type
            var data = DataService.GetAllData(type);

            //loop through the data and add it to the index
            var nodes = new List<ValueSet>();
            foreach (var d in data)
            {
                nodes.Add(ValueSet.FromLegacyFields(d.NodeDefinition.NodeId, type ?? d.NodeDefinition.Type, d.RowData));                   
            }
            
            //now that we have XElement nodes of all of the data, process it as per normal
            AddNodesToIndex(nodes);
        }              

        /// <summary>
        /// Indexes each index type defined in IndexTypes property
        /// </summary>
        protected override void PerformIndexRebuild()
        {            
            foreach (var t in IndexTypes)
            {
                IndexAll(t);
            }
        }
        
        /// <summary>
        /// Initializes the provider from the config
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        [SecuritySafeCritical]
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config["indexTypes"] == null || string.IsNullOrEmpty(config["indexTypes"]))
            {
                throw new ArgumentNullException("The indexTypes property must be specified for the SimpleDataIndexer provider");
            }
            IndexTypes = config["indexTypes"].Split(',');

            if (config["dataService"] != null && !string.IsNullOrEmpty(config["dataService"]))
            {
                //this should be a fully qualified type
                var serviceType = Type.GetType(config["dataService"]);
                DataService = (ISimpleDataService)Activator.CreateInstance(serviceType);
            }
            else
            {
                throw new ArgumentNullException("The dataService property must be specified for the SimpleDataIndexer provider");
            }
        }

    }
}
