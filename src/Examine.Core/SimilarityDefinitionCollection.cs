using System;

namespace Examine
{
    public class SimilarityDefinitionCollection : ReadOnlySimilarityDefinitionCollection
    {
        public SimilarityDefinitionCollection(params SimilarityDefinition[] definitions) : base(definitions)
        {
        }

        public SimilarityDefinitionCollection()
        {
        }

        public SimilarityDefinition GetOrAdd(string fieldName, Func<string, SimilarityDefinition> add) => Definitions.GetOrAdd(fieldName, add);

        /// <summary>
        /// Replace any definition with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="definition"></param>
        public void AddOrUpdate(SimilarityDefinition definition) => Definitions.AddOrUpdate(definition.Name, definition, (s, factory) => definition);

        public bool TryAdd(SimilarityDefinition definition) => Definitions.TryAdd(definition.Name, definition);
    }
}
