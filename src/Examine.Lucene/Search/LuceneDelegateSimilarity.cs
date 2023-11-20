using System;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Lucene.NET Similarity created by delegate
    /// </summary>
    public class LuceneDelegateSimilarity : LuceneSimilarityBase
    {
        private readonly Func<Similarity> _similarityFunc;

        public LuceneDelegateSimilarity(string name, Func<Similarity> similarityFunc) : base(name)
        {
            _similarityFunc = similarityFunc;
        }

        public override Similarity GetSimilarity() => _similarityFunc();
    }
}
