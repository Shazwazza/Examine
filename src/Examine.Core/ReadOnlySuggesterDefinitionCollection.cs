using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Manages the mappings between a suggester name and it's index type
    /// </summary>
    public class ReadOnlySuggesterDefinitionCollection : IEnumerable<SuggesterDefinition>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ReadOnlySuggesterDefinitionCollection()
            : this(Enumerable.Empty<SuggesterDefinition>())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definitions">Suggester Definitions</param>
        public ReadOnlySuggesterDefinitionCollection(params SuggesterDefinition[] definitions)
            : this((IEnumerable<SuggesterDefinition>)definitions)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definitions">Suggester Definitions</param>
        public ReadOnlySuggesterDefinitionCollection(IEnumerable<SuggesterDefinition> definitions)
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
        /// Tries to get a <see cref="SuggesterDefinition"/> by suggester name
        /// </summary>
        /// <param name="suggesterName"></param>
        /// <param name="suggesterDefinition"></param>
        /// <returns>
        /// returns true if one was found otherwise false
        /// </returns>
        /// <remarks>
        /// Marked as virtual so developers can inherit this class and override this method in case
        /// suggester definitions are dynamic.
        /// </remarks>
        public virtual bool TryGetValue(string suggesterName, out SuggesterDefinition suggesterDefinition) => Definitions.TryGetValue(suggesterName, out suggesterDefinition);

        /// <summary>
        /// Count of Suggesters
        /// </summary>
        public int Count => Definitions.Count;

        /// <summary>
        /// Suggester Definitions
        /// </summary>
        protected ConcurrentDictionary<string, SuggesterDefinition> Definitions { get; } = new ConcurrentDictionary<string, SuggesterDefinition>(StringComparer.InvariantCultureIgnoreCase);

        /// <inheritdoc/>
        public IEnumerator<SuggesterDefinition> GetEnumerator() => Definitions.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
