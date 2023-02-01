using System;
using System.Collections.Generic;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Use a similarity per field, falling back to a default similarity.
    /// </summary>
    public class DictionaryPerFieldSimilarityWrapper : PerFieldSimilarityWrapper
    {
        private readonly Similarity _defaultSimilarity;
        private readonly IReadOnlyDictionary<string, Similarity> _fieldSimilarities;

        /// <summary>
        /// Creates a new instance of <see cref="DictionaryPerFieldSimilarityWrapper"/>.
        /// </summary>
        /// <param name="fieldSimilarities">Mapping from field name to Similarity</param>
        /// <param name="defaultSimilarity">Default Similarity to use for non mapped fields</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DictionaryPerFieldSimilarityWrapper(IReadOnlyDictionary<string, Similarity> fieldSimilarities, Similarity defaultSimilarity)
        {
            _defaultSimilarity = defaultSimilarity ?? throw new ArgumentNullException(nameof(defaultSimilarity));
            _fieldSimilarities = fieldSimilarities ?? throw new ArgumentNullException(nameof(fieldSimilarities));
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
