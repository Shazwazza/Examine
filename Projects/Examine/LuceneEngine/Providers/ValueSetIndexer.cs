using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
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
        /// Constructor to create an indexer at runtime
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        /// <param name="facetConfiguration"></param>
        /// <param name="dataService"></param>
        /// <param name="indexTypes"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="defaultAnalyzer">Specifies the default analyzer to use per field</param>
        /// <param name="indexValueTypes">
        /// Specifies the index value types to use for this indexer, if this is not specified then the result of LuceneIndexer.GetDefaultIndexValueTypes() will be used.
        /// This is generally used to initialize any custom value types for your indexer since the value type collection cannot be modified at runtime.
        /// </param>
        public ValueSetIndexer(IEnumerable<FieldDefinition> fieldDefinitions,
            IValueSetService dataService,
            IEnumerable<string> indexTypes,
            Directory luceneDirectory,
            Analyzer defaultAnalyzer,
            FacetConfiguration facetConfiguration = null,
            IDictionary<string, Func<string, IIndexValueType>> indexValueTypes = null)
            : base(fieldDefinitions, luceneDirectory, defaultAnalyzer, facetConfiguration, indexValueTypes)
        {
            DataService = dataService;
            IndexTypes = indexTypes;
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

    }
}
