using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Manages the mappings between a similarity name and it's similarity type
    /// </summary>
    public class ReadOnlySimilarityDefinitionCollection : IEnumerable<SimilarityDefinition>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ReadOnlySimilarityDefinitionCollection()
            : this(Enumerable.Empty<SimilarityDefinition>())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definitions"></param>
        public ReadOnlySimilarityDefinitionCollection(params SimilarityDefinition[] definitions)
            : this((IEnumerable<SimilarityDefinition>)definitions)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definitions"></param>
        public ReadOnlySimilarityDefinitionCollection(IEnumerable<SimilarityDefinition> definitions)
        {
            if (definitions == null)
                return;

            foreach (var s in definitions.GroupBy(x => x.Name))
            {
                var suggester = s.FirstOrDefault();
                if (suggester != default)
                {
                    Definitions.TryAdd(s.Key, suggester);
                }
            }
        }

        /// <summary>
        /// Tries to get a <see cref="SimilarityDefinition"/> by similarity name
        /// </summary>
        /// <param name="similarityName"></param>
        /// <param name="similarityDefinition"></param>
        /// <returns>
        /// returns true if one was found otherwise false
        /// </returns>
        /// <remarks>
        /// Marked as virtual so developers can inherit this class and override this method in case
        /// similarity definitions are dynamic.
        /// </remarks>
        public virtual bool TryGetValue(string similarityName, out SimilarityDefinition similarityDefinition) => Definitions.TryGetValue(similarityName, out similarityDefinition);

        /// <summary>
        /// Count
        /// </summary>
        public int Count => Definitions.Count;

        /// <summary>
        /// Definitions
        /// </summary>
        protected ConcurrentDictionary<string, SimilarityDefinition> Definitions { get; } = new ConcurrentDictionary<string, SimilarityDefinition>(StringComparer.InvariantCultureIgnoreCase);

        /// <inheritdoc/>
        public IEnumerator<SimilarityDefinition> GetEnumerator() => Definitions.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
