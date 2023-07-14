using System;

namespace Examine
{
    public class RelevanceScorerDefinitionCollection : ReadOnlyRelevanceScorerDefinitionCollection
    {
        public RelevanceScorerDefinitionCollection(params RelevanceScorerDefinition[] definitions) : base(definitions)
        {
        }

        public RelevanceScorerDefinitionCollection()
        {
        }

        public RelevanceScorerDefinition GetOrAdd(string fieldName, Func<string, RelevanceScorerDefinition> add) => Definitions.GetOrAdd(fieldName, add);

        /// <summary>
        /// Replace any definition with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="definition"></param>
        public void AddOrUpdate(RelevanceScorerDefinition definition) => Definitions.AddOrUpdate(definition.Name, definition, (s, factory) => definition);

        public bool TryAdd(RelevanceScorerDefinition definition) => Definitions.TryAdd(definition.Name, definition);
    }
}
