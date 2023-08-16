using System;

namespace Examine
{
    /// <summary>
    /// Manages the mappings between a similarity name and it's similarity type
    /// </summary>
    public class SimilarityDefinitionCollection : ReadOnlySimilarityDefinitionCollection
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definitions"></param>
        public SimilarityDefinitionCollection(params SimilarityDefinition[] definitions) : base(definitions)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SimilarityDefinitionCollection()
        {
        }

        /// <inheritdoc/>
        public SimilarityDefinition GetOrAdd(string fieldName, Func<string, SimilarityDefinition> add) => Definitions.GetOrAdd(fieldName, add);

        /// <summary>
        /// Replace any definition with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="definition"></param>
        public void AddOrUpdate(SimilarityDefinition definition) => Definitions.AddOrUpdate(definition.Name, definition, (s, factory) => definition);

        /// <inheritdoc/>
        public bool TryAdd(SimilarityDefinition definition) => Definitions.TryAdd(definition.Name, definition);
    }
}
