using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Examine.Lucene.Indexing;
using Examine.Lucene.Search;
using Microsoft.Extensions.Logging;

namespace Examine.Lucene
{
    /// <summary>
    /// Manages the collection of <see cref="ISimilarityTypeFactory"/>
    /// </summary>
    public class SimilarityFactoryCollection : IEnumerable<KeyValuePair<string, ISimilarityTypeFactory>>
    {
        private readonly ConcurrentDictionary<string, ISimilarityTypeFactory> _similarityFactories;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueTypeFactories"></param>
        public SimilarityFactoryCollection(IReadOnlyDictionary<string, ISimilarityTypeFactory> valueTypeFactories)
        {
            _similarityFactories = new ConcurrentDictionary<string, ISimilarityTypeFactory>(
                        valueTypeFactories,
                        StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Try get for the factory
        /// </summary>
        /// <param name="valueTypeName"></param>
        /// <param name="fieldValueTypeFactory"></param>
        /// <returns></returns>
        public bool TryGetFactory(string valueTypeName,
#if !NETSTANDARD2_0
            [MaybeNullWhen(false)]
#endif
            out ISimilarityTypeFactory fieldValueTypeFactory)
            => _similarityFactories.TryGetValue(valueTypeName, out fieldValueTypeFactory);

        /// <summary>
        /// Returns the <see cref="ISimilarityTypeFactory"/> by name, if it's not found an exception is thrown
        /// </summary>
        /// <param name="valueTypeName"></param>
        /// <returns></returns>
        public ISimilarityTypeFactory GetRequiredFactory(string valueTypeName)
        {
            if (!TryGetFactory(valueTypeName, out var fieldValueTypeFactory))
            {
                throw new InvalidOperationException($"The required {typeof(ISimilarityTypeFactory).Name} was not found with name {valueTypeName}");
            }

            return fieldValueTypeFactory;
        }

        /// <summary>
        /// The ammount of key/value pairs in the collection
        /// </summary>
        public int Count => _similarityFactories.Count;

        /// <summary>
        /// Returns the default index similarity types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, ISimilarityTypeFactory> GetDefaultSimilarities()
            => GetDefaults().ToDictionary(x => x.Key, x => (ISimilarityTypeFactory)new DelegateSimilarityTypeFactory(x.Value));

        private static IReadOnlyDictionary<string, Func<IIndexSimilarityType>> GetDefaults() =>
            new Dictionary<string, Func<IIndexSimilarityType>>(StringComparer.InvariantCultureIgnoreCase) //case insensitive
            {
                {ExamineLuceneSimilarityNames.ExamineDefault, () => new LuceneClassicSimilarityType()},
                {ExamineLuceneSimilarityNames.BM25, () => new LuceneBM25imilarityType()},
                {ExamineLuceneSimilarityNames.Classic, () => new LuceneClassicSimilarityType()},
                {ExamineLuceneSimilarityNames.LMDirichlet, () => new LuceneLMDirichletSimilarityType()},
                {ExamineLuceneSimilarityNames.LMJelinekMercerLongText, () => new LuceneLMJelinekMercerLongTextSimilarityType()},
                {ExamineLuceneSimilarityNames.LMJelinekMercerTitle, () => new LuceneLMJelinekMercerTitleSimilarityType()}
                };


        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, ISimilarityTypeFactory>> GetEnumerator()
            => _similarityFactories.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
