using System;

namespace Examine
{
    public class SuggesterDefinitionCollection : ReadOnlySuggesterDefinitionCollection
    {
        public SuggesterDefinitionCollection(params SuggesterDefinition[] definitions) : base(definitions)
        {
        }

        public SuggesterDefinitionCollection()
        {
        }

        public SuggesterDefinition GetOrAdd(string fieldName, Func<string, SuggesterDefinition> add) => Definitions.GetOrAdd(fieldName, add);

        /// <summary>
        /// Replace any definition with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="definition"></param>
        public void AddOrUpdate(SuggesterDefinition definition) => Definitions.AddOrUpdate(definition.Name, definition, (s, factory) => definition);

        public bool TryAdd(SuggesterDefinition definition) => Definitions.TryAdd(definition.Name, definition);
    }
}
