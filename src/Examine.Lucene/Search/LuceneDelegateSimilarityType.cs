using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene.NET Similarity created by delegate
    /// </summary>
    public class LuceneDelegateSimilarityType : LuceneSimilarityTypeBase
    {
        private readonly Func<Similarity> _similarityFunc;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Similarity Name</param>
        /// <param name="similarityFunc">Factory</param>
        public LuceneDelegateSimilarityType(string name, Func<Similarity> similarityFunc) : base(name)
        {
            _similarityFunc = similarityFunc;
        }

        /// <inheritdoc/>
        public override Similarity GetSimilarity() => _similarityFunc();
    }
}
