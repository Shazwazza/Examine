using System;
using System.Collections.Generic;
using System.Text;
using Examine.Lucene.Scoring;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;

namespace Examine.Lucene
{

    public class LuceneIndexOptions : IndexOptions
    {

        public IndexDeletionPolicy IndexDeletionPolicy { get; set; }

        public Analyzer Analyzer { get; set; }

        /// <summary>
        /// Specifies the index value types to use for this indexer, if this is not specified then the result of <see cref="ValueTypeFactoryCollection.GetDefaultValueTypes"/> will be used.
        /// This is generally used to initialize any custom value types for your indexer since the value type collection cannot be modified at runtime.
        /// </summary>
        public IReadOnlyDictionary<string, IFieldValueTypeFactory> IndexValueTypesFactory { get; set; }
    }
}
