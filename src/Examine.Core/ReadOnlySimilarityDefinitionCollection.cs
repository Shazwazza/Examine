using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Manages the mappings between a field name and it's index type
    /// </summary>
    public class ReadOnlySimilarityDefinitionCollection : IEnumerable<SimilarityDefinition>
    {
        public ReadOnlySimilarityDefinitionCollection()
            : this(Enumerable.Empty<SimilarityDefinition>())
        {
        }

        public ReadOnlySimilarityDefinitionCollection(params SimilarityDefinition[] definitions)
            : this((IEnumerable<SimilarityDefinition>)definitions)
        {

        }

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

        public int Count => Definitions.Count;

        protected ConcurrentDictionary<string, SimilarityDefinition> Definitions { get; } = new ConcurrentDictionary<string, SimilarityDefinition>(StringComparer.InvariantCultureIgnoreCase);

        public IEnumerator<SimilarityDefinition> GetEnumerator() => Definitions.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
