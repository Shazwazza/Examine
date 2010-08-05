using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine;
using Examine;
using Examine.LuceneEngine.Config;
using System.Xml.Linq;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// An index provider that can be used to index simple data structures such as those from a database, dictionary or array.
    /// </summary>
    public class SimpleDataIndexer : LuceneExamineIndexer
    {

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
            foreach (var d in data)
            {
                SaveAddIndexQueueItem(d.RowData, d.NodeDefinition.NodeId, d.NodeDefinition.Type);
            }
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
        }

    }
}
