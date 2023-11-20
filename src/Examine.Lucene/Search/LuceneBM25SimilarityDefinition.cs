using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// BM25Similarity with default parameters for Lucene. <see cref="BM25Similarity"/>
    /// </summary>
    public class LuceneBM25imilarityType : LuceneSimilarityTypeBase
    {
        private readonly Lazy<BM25Similarity> _similarityLazy = new Lazy<BM25Similarity>(() => new BM25Similarity());

        /// <summary>
        /// Constructor
        /// </summary>
        public LuceneBM25imilarityType() : base(ExamineLuceneSimilarityNames.BM25)
        {
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
