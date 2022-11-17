using System;

namespace Examine
{
    /// <inheritdoc/>
    public class FieldDefinitionCollection : ReadOnlyFieldDefinitionCollection
    {
        /// <inheritdoc/>
        public FieldDefinitionCollection(params FieldDefinition[] definitions) : base(definitions)
        {
        }

        /// <inheritdoc/>
        public FieldDefinitionCollection()
        {
        }

        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary`2
        /// by using the specified function if the key does not already exist, or returns
        /// the existing value if the key exists.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">fieldName or add is null</exception>
        /// <exception cref="OverflowException">The dictionary already contains the maximum number of elements (<see cref="int.MaxValue"/>)</exception>
        public FieldDefinition GetOrAdd(string fieldName, Func<string, FieldDefinition> add) => Definitions.GetOrAdd(fieldName, add);

        /// <summary>
        /// Replace any definition with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="definition"></param>
        public void AddOrUpdate(FieldDefinition definition) => Definitions.AddOrUpdate(definition.Name, definition, (s, factory) => definition);

        /// <summary>
        /// Attempts to add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary`2.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns>
        /// True if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary`2
        /// successfully; false if the key already exists.
        /// </returns>
        /// <exception cref="ArgumentNullException">definition.Name is null</exception>
        /// <exception cref="OverflowException">The dictionary already contains the maximum number of elements (<see cref="int.MaxValue"/>)</exception>
        public bool TryAdd(FieldDefinition definition) => Definitions.TryAdd(definition.Name, definition);
    }
}
