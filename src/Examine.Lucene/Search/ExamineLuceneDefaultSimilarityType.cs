using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Default Similarity for Examine Lucene.
    /// </summary>
    /// <remarks>In Examine V3 and V4, this is <see cref="DefaultSimilarity"/>. In Examine V5 this will change to <see cref="BM25Similarity"/></remarks>
    public class ExamineLuceneDefaultSimilarityType : LuceneSimilarityTypeBase
    {
        private readonly Lazy<DefaultSimilarity> _similarityLazy = new Lazy<DefaultSimilarity>(() => new DefaultSimilarity());

        /// <summary>
        /// Constructor
        /// </summary>
        public ExamineLuceneDefaultSimilarityType() : base(ExamineLuceneSimilarityNames.ExamineDefault)
        {
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
