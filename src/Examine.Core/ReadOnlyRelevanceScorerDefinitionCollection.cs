using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    public class ReadOnlyRelevanceScorerDefinitionCollection : IEnumerable<RelevanceScorerDefinition>
    {
        public ReadOnlyRelevanceScorerDefinitionCollection()
            : this(Enumerable.Empty<RelevanceScorerDefinition>())
        {
        }

        public ReadOnlyRelevanceScorerDefinitionCollection(params RelevanceScorerDefinition[] definitions)
            : this((IEnumerable<RelevanceScorerDefinition>)definitions)
        {

        }

        public ReadOnlyRelevanceScorerDefinitionCollection(IEnumerable<RelevanceScorerDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

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
        /// Tries to get a <see cref="RelevanceScorerDefinition"/> by name
        /// </summary>
        /// <param name="relevanceScorerName"></param>
        /// <param name="relevanceScorerDefinition"></param>
        /// <returns>
        /// returns true if one was found otherwise false
        /// </returns>
        /// <remarks>
        /// Marked as virtual so developers can inherit this class and override this method in case
        /// relevance definitions are dynamic.
        /// </remarks>
        public virtual bool TryGetValue(string relevanceScorerName, out RelevanceScorerDefinition relevanceScorerDefinition) => Definitions.TryGetValue(relevanceScorerName, out relevanceScorerDefinition);

        public int Count => Definitions.Count;

        protected ConcurrentDictionary<string, RelevanceScorerDefinition> Definitions { get; } = new ConcurrentDictionary<string, RelevanceScorerDefinition>(StringComparer.InvariantCultureIgnoreCase);

        public IEnumerator<RelevanceScorerDefinition> GetEnumerator() => Definitions.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
