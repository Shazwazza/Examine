using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// An index provider that can be used to index any data structures such as those from a database, dictionary or array.
    /// </summary>
    public class ValueSetIndexer : LuceneIndexer
    {
        /// <summary>
        /// Constructor used for provider instantiation
        /// </summary>
        public ValueSetIndexer()
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        /// <param name="indexTypes"></param>
        /// <param name="valueSetService"></param>
        public ValueSetIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer,
            IEnumerable<string> indexTypes, IValueSetService valueSetService) : base(indexerData, workingFolder, analyzer)
        {
            IndexTypes = indexTypes;
            DataService = valueSetService;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="analyzer"></param>
        /// <param name="indexTypes"></param>
        /// <param name="valueSetService"></param>
        public ValueSetIndexer(IIndexCriteria indexerData, Directory luceneDirectory, Analyzer analyzer, 
            IEnumerable<string> indexTypes, IValueSetService valueSetService) : base(indexerData, luceneDirectory, analyzer)
        {
            IndexTypes = indexTypes;
            DataService = valueSetService;
        }

        /// <summary>
        /// The data service used to retrieve all of the data for an index type
        /// </summary>
        public IValueSetService DataService { get; private set; }

        /// <summary>
        /// A list of index types defined for this indexer
        /// </summary>
        public IEnumerable<string> IndexTypes { get; private set; }

        protected override void PerformIndexAll(string category)
        {
            IndexItems(DataService.GetAllData(category).ToArray());
        }

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
                var serviceType = Type.GetType(config["dataService"]);
                DataService = (IValueSetService)Activator.CreateInstance(serviceType);
            }
            else
            {
                throw new ArgumentNullException("The dataService property must be specified for the SimpleDataIndexer provider");
            }
        }
    }
}
