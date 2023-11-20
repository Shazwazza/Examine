using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Classic Similarity for Lucene. <see cref="DefaultSimilarity"/>
    /// </summary>
    public class LuceneClassicSimilarity : LuceneSimilarityBase
    {
        private readonly Lazy<DefaultSimilarity> _similarityLazy = new Lazy<DefaultSimilarity>(() => new DefaultSimilarity());

        /// <summary>
        /// Constructor
        /// </summary>
        public LuceneClassicSimilarity() : base(ExamineLuceneSimilarityNames.Classic)
        {
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
