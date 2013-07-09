using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine.Providers
{
    public class ValueSetIndexer : LuceneIndexer
    {
        public ValueSetIndexer()
        {
        }

        public ValueSetIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer,
            IEnumerable<string> indexTypes, IValueSetService valueSetService, bool async) : base(indexerData, workingFolder, analyzer, async)
        {
            IndexTypes = indexTypes;
            ValueSetService = valueSetService;
        }

        public ValueSetIndexer(IIndexCriteria indexerData, Directory luceneDirectory, Analyzer analyzer, 
            IEnumerable<string> indexTypes, IValueSetService valueSetService, bool async) : base(indexerData, luceneDirectory, analyzer, async)
        {
            IndexTypes = indexTypes;
            ValueSetService = valueSetService;
        }

        /// <summary>
        /// The data service used to retrieve all of the data for an index type
        /// </summary>
        public IValueSetService DataService { get; set; }

        /// <summary>
        /// A list of index types defined for this indexer
        /// </summary>
        public IEnumerable<string> IndexTypes { get; set; }

        public IValueSetService ValueSetService { get; set; }

        protected override void PerformIndexAll(string type)
        {
            AddNodesToIndex(DataService.GetAllData(type));            
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
