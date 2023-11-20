using System;
using Examine.Lucene.Indexing;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene.NET Similarity Defintion
    /// </summary>
    public abstract class LuceneSimilarityBase : IIndexSimilarity
    {
        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="name">Similarity Name</param>
        public LuceneSimilarityBase(string name)
        {
            SimilarityName = name;
        }

        /// <inheritdoc/>
        public string SimilarityName { get; }

        /// <inheritdoc/>
        public abstract Similarity GetSimilarity();
    }
}
