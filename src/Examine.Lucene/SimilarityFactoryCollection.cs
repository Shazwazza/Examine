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
    /// Manages the collection of <see cref="ISimilarityFactory"/>
    /// </summary>
    public class SimilarityFactoryCollection : IEnumerable<KeyValuePair<string, ISimilarityFactory>>
    {
        private readonly ConcurrentDictionary<string, ISimilarityFactory> _similarityFactories;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="valueTypeFactories"></param>
        public SimilarityFactoryCollection(IReadOnlyDictionary<string, ISimilarityFactory> valueTypeFactories)
        {
            _similarityFactories = new ConcurrentDictionary<string, ISimilarityFactory>(
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
            out ISimilarityFactory fieldValueTypeFactory)
            => _similarityFactories.TryGetValue(valueTypeName, out fieldValueTypeFactory);

        /// <summary>
        /// Returns the <see cref="ISimilarityFactory"/> by name, if it's not found an exception is thrown
        /// </summary>
        /// <param name="valueTypeName"></param>
        /// <returns></returns>
        public ISimilarityFactory GetRequiredFactory(string valueTypeName)
        {
            if (!TryGetFactory(valueTypeName, out var fieldValueTypeFactory))
            {
                throw new InvalidOperationException($"The required {typeof(ISimilarityFactory).Name} was not found with name {valueTypeName}");
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
        public static IReadOnlyDictionary<string, ISimilarityFactory> GetDefaultSimilarities(ILoggerFactory loggerFactory)
            => GetDefaults(loggerFactory).ToDictionary(x => x.Key, x => (ISimilarityFactory)new DelegateSimilarityFactory(x.Value));

        private static IReadOnlyDictionary<string, Func<IIndexSimilarity>> GetDefaults(ILoggerFactory loggerFactory) =>
            new Dictionary<string, Func<IIndexSimilarity>>(StringComparer.InvariantCultureIgnoreCase) //case insensitive
            {
                {ExamineLuceneSimilarityNames.ExamineDefault, () => new LuceneClassicSimilarity()},
                {ExamineLuceneSimilarityNames.BM25, () => new LuceneBM25imilarity()},
                {ExamineLuceneSimilarityNames.Classic, () => new LuceneClassicSimilarity()},
                {ExamineLuceneSimilarityNames.LMDirichlet, () => new LuceneLMDirichletSimilarity()},
                {ExamineLuceneSimilarityNames.LMJelinekMercerLongText, () => new LuceneLMJelinekMercerLongTextSimilarity()},
                {ExamineLuceneSimilarityNames.LMJelinekMercerTitle, () => new LuceneLMJelinekMercerTitleSimilarity()}
                };


        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, ISimilarityFactory>> GetEnumerator()
            => _similarityFactories.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
