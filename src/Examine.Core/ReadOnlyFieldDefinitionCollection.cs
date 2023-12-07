using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Manages the mappings between a field name and it's index type
    /// </summary>
    public class ReadOnlyFieldDefinitionCollection : IEnumerable<FieldDefinition>
    {
        /// <inheritdoc/>
        public ReadOnlyFieldDefinitionCollection()
            : this(Enumerable.Empty<FieldDefinition>())
        {   
        }

        /// <inheritdoc/>
        public ReadOnlyFieldDefinitionCollection(params FieldDefinition[] definitions)
            : this((IEnumerable<FieldDefinition>)definitions)
        {
            
        }

        /// <inheritdoc/>
        public ReadOnlyFieldDefinitionCollection(IEnumerable<FieldDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            foreach (var f in definitions.GroupBy(x => x.Name))
            {
                var indexField = f.FirstOrDefault();
                if (indexField != default)
                {
                    Definitions.TryAdd(f.Key, indexField);
                }
            }
        }

        /// <summary>
        /// Tries to get a <see cref="FieldDefinition"/> by field name
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldDefinition"></param>
        /// <returns>
        /// returns true if one was found otherwise false
        /// </returns>
        /// <remarks>
        /// Marked as virtual so developers can inherit this class and override this method in case
        /// field definitions are dynamic.
        /// </remarks>
        public virtual bool TryGetValue(string fieldName, out FieldDefinition fieldDefinition) => Definitions.TryGetValue(fieldName, out fieldDefinition);

        /// <summary>
        /// Gets the ammount of key/value paris in the <see cref="Definitions"/> collection
        /// </summary>
        public int Count => Definitions.Count;

        /// <summary>
        /// A collection of field definitions
        /// </summary>
        protected ConcurrentDictionary<string, FieldDefinition> Definitions { get; } = new ConcurrentDictionary<string, FieldDefinition>(StringComparer.InvariantCultureIgnoreCase);

        /// <inheritdoc/>
        public IEnumerator<FieldDefinition> GetEnumerator() => Definitions.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
