using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Default Similarity for Examine Lucene.
    /// </summary>
    /// <remarks>In Examine V3, this is <see cref="DefaultSimilarity"/>. In Examine V4 this will change to <see cref="BM25Similarity"/></remarks>
    public class ExamineLuceneDefaultSimilarityDefinition : LuceneSimilarityDefinitionBase
    {
        private Lazy<DefaultSimilarity> _similarityLazy = new Lazy<DefaultSimilarity>(() => new DefaultSimilarity());
        public ExamineLuceneDefaultSimilarityDefinition() : base(ExamineLuceneSimilarityNames.ExamineDefault)
        {
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarityLazy.Value;
    }
}
