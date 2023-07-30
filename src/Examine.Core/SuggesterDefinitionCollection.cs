using System;

namespace Examine
{
    /// <summary>
    /// Collection of Suggester Definitions on an Index
    /// </summary>
    public class SuggesterDefinitionCollection : ReadOnlySuggesterDefinitionCollection
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definitions">Suggester Definitions</param>
        public SuggesterDefinitionCollection(params SuggesterDefinition[] definitions) : base(definitions)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SuggesterDefinitionCollection()
        {
        }

        /// <summary>
        /// Get or Add a Suggester Definition
        /// </summary>
        /// <param name="suggesterName">Name of Suggester</param>
        /// <param name="add">Function to add Suggester</param>
        /// <returns></returns>
        public SuggesterDefinition GetOrAdd(string suggesterName, Func<string, SuggesterDefinition> add) => Definitions.GetOrAdd(suggesterName, add);

        /// <summary>
        /// Replace any definition with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="definition"></param>
        public void AddOrUpdate(SuggesterDefinition definition) => Definitions.AddOrUpdate(definition.Name, definition, (s, factory) => definition);

        /// <summary>
        /// Try Add a Suggester Definition
        /// </summary>
        /// <param name="definition">Suggester Defintion</param>
        /// <returns>Whether the Suggester was added</returns>
        public bool TryAdd(SuggesterDefinition definition) => Definitions.TryAdd(definition.Name, definition);
    }
}
