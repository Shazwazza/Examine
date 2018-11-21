using System;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// An index provider that can be used to index simple data structures such as those from a database, dictionary or array.
    /// </summary>
    public class ValueSetIndexer : LuceneIndexer
    {
        public ValueSetIndexer(string name, IValueSetDataService dataService, IEnumerable<string> indexCategories, IEnumerable<FieldDefinition> fieldDefinitions, Directory luceneDirectory, Analyzer analyzer, IValueSetValidator validator = null, IReadOnlyDictionary<string, Func<string, IIndexValueType>> indexValueTypesFactory = null) 
            : base(name, fieldDefinitions, luceneDirectory, analyzer, validator, indexValueTypesFactory)
        {
            DataService = dataService;
            IndexCategories = indexCategories;
        }

        //TODO: Not sure if we want to expose this one yet
        internal ValueSetIndexer(string name, IValueSetDataService dataService, IEnumerable<string> indexCategories, IEnumerable<FieldDefinition> fieldDefinitions, IndexWriter writer, IValueSetValidator validator = null, IReadOnlyDictionary<string, Func<string, IIndexValueType>> indexValueTypesFactory = null) 
            : base(name, fieldDefinitions, writer, validator, indexValueTypesFactory)
        {
            DataService = dataService;
            IndexCategories = indexCategories;
        }

        /// <summary>
        /// The data service used to retrieve all of the data for an index type
        /// </summary>
        public IValueSetDataService DataService { get; }

        /// <summary>
        /// A list of index types defined for this indexer
        /// </summary>
        public IEnumerable<string> IndexCategories { get; }

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
            foreach (var t in IndexCategories)
            {
                IndexAll(t);
            }
        }

    }
}
