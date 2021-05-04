using System;

namespace Examine
{
    public sealed class FieldDefinitionCollection : ReadOnlyFieldDefinitionCollection
    {
        public FieldDefinitionCollection(params FieldDefinition[] definitions) : base(definitions)
        {
        }

        public FieldDefinitionCollection()
        {
        }

        public FieldDefinition GetOrAdd(string fieldName, Func<string, FieldDefinition> add) => Definitions.GetOrAdd(fieldName, add);

        /// <summary>
        /// Replace any definition with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="definition"></param>
        public void AddOrUpdate(FieldDefinition definition) => Definitions.AddOrUpdate(definition.Name, definition, (s, factory) => definition);

        public bool TryAdd(FieldDefinition definition) => Definitions.TryAdd(definition.Name, definition);
    }
}
