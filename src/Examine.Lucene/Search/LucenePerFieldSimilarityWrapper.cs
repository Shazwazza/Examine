using System.Collections.Generic;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    public class LucenePerFieldSimilarityWrapper : PerFieldSimilarityWrapper
    {
        private readonly Similarity _defaultSimilarity;
        private readonly IDictionary<string, Similarity> _fieldSimilarities;

        public LucenePerFieldSimilarityWrapper(Similarity defaultSimilarity, IDictionary<string, Similarity> fieldSimilarities)
        {
            _defaultSimilarity = defaultSimilarity;
            _fieldSimilarities = fieldSimilarities;
        }

        /// <inheritdoc/>
        public override Similarity Get(string field)
        {
            if (_fieldSimilarities.TryGetValue(field, out var similarity))
            {
                return similarity;
            }
            return _defaultSimilarity;
        }
    }
}
