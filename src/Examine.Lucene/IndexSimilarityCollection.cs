using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Examine.Lucene.Indexing;

namespace Examine.Lucene
{
    /// <summary>
    /// Maintains a collection of field names names and their <see cref="IIndexSimilarityType"/> for an index
    /// </summary>
    public class IndexSimilarityCollection
    {
        private readonly Lazy<ConcurrentDictionary<string, IIndexSimilarityType>> _resolvedIndexSimilarities;

        /// <summary>
        /// Create a <see cref="IndexSimilarityCollection"/>
        /// </summary>
        /// <param name="valueTypeFactories">List of value type factories to initialize the collection with</param>
        /// <param name="defaultSimilarityName">Name of the default Similarity</param>
        /// <param name="similarityDefinitionCollection"></param>
        public IndexSimilarityCollection(
            IReadOnlyDictionary<string, ISimilarityTypeFactory> valueTypeFactories,
            string defaultSimilarityName,
            ReadOnlySimilarityDefinitionCollection similarityDefinitionCollection)
        {
            IndexSimilarityFactories = new SimilarityFactoryCollection(valueTypeFactories);

            //initializes the collection of field aliases to it's correct IIndexSimilarity
            _resolvedIndexSimilarities = new Lazy<ConcurrentDictionary<string, IIndexSimilarityType>>(() =>
            {
                var result = new ConcurrentDictionary<string, IIndexSimilarityType>();

                foreach (var field in similarityDefinitionCollection)
                {
                    if (!string.IsNullOrWhiteSpace(field.Type) && IndexSimilarityFactories.TryGetFactory(field.Type, out var valueTypeFactory))
                    {
                        var valueType = valueTypeFactory.Create();
                        result.TryAdd(field.Name, valueType);
                    }
                }
                return result;
            });
            DefaultSimilarityName = defaultSimilarityName;
        }

        /// <summary>
        /// Defines the field types such as number, fulltext, etc...
        /// </summary>        
        public SimilarityFactoryCollection IndexSimilarityFactories { get; }

        /// <summary>
        /// Returns the <see cref="IIndexSimilarityType"/> for the similarity name specified
        /// </summary>
        /// <param name="similarityName"></param>
        /// <param name="similarityFactory"></param>
        /// <returns></returns>
        /// <remarks>
        /// If it's not found it will create one with the factory supplied and initialize it.
        /// </remarks>
        public IIndexSimilarityType GetIndexSimilarity(string similarityName, ISimilarityTypeFactory similarityFactory)
            => _resolvedIndexSimilarities.Value.GetOrAdd(similarityName, n =>
                {
                    var t = similarityFactory.Create();
                    return t;
                });

        /// <summary>
        /// Returns the value type for the similarity name specified
        /// </summary>
        /// <param name="similarityName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// Throws an exception if a similarity type was not found
        /// </exception>
        public IIndexSimilarityType GetIndexSimilarity(string similarityName)
        {
            if (!_resolvedIndexSimilarities.Value.TryGetValue(similarityName, out var valueType))
            {
                throw new InvalidOperationException($"No {nameof(IIndexSimilarityType)} was found for similarity name {similarityName}");
            }

            return valueType;
        } 

        /// <summary>
        /// Returns the resolved collection of <see cref="IIndexSimilarityType"/> for this index
        /// </summary>
        public IEnumerable<IIndexSimilarityType> Similarities => _resolvedIndexSimilarities.Value.Values;

        /// <summary>
        /// Name of the Similarity to use by default for searches
        /// </summary>
        public string DefaultSimilarityName { get; }
    }
}
