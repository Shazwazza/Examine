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
		
		public SimpleDataIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer, IValueSetDataService dataService, IEnumerable<string> indexTypes, bool async)
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
		
		public SimpleDataIndexer(IIndexCriteria indexerData, Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer, IValueSetDataService dataService, IEnumerable<string> indexTypes, bool async)
			: base(indexerData, luceneDirectory, analyzer, async)
		{
			DataService = dataService;
			IndexTypes = indexTypes;
		}

        /// <summary>
        /// The data service used to retrieve all of the data for an index type
        /// </summary>
        public IValueSetDataService DataService { get; set; }

        /// <summary>
        /// A list of index types defined for this indexer
        /// </summary>
        public IEnumerable<string> IndexTypes { get; set; }

        /// <summary>
        /// Gets the data for the index type from the data service and indexes it.
        /// </summary>
        /// <param name="category"></param>
        protected override void PerformIndexAll(string category)
        {
            IndexItems(DataService.GetAllData(category));
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
                var serviceType = TypeHelper.FindType(config["dataService"]);
                DataService = (IValueSetDataService)Activator.CreateInstance(serviceType);
            }
            else
            {
                throw new ArgumentNullException("The dataService property must be specified for the SimpleDataIndexer provider");
            }
        }

    }
}
