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
    public class FieldDefinitionCollection : IEnumerable<FieldDefinition>
    {
        private readonly ConcurrentDictionary<string, FieldDefinition> _definitions = new ConcurrentDictionary<string, FieldDefinition>();

        public FieldDefinitionCollection()
            : this(Enumerable.Empty<FieldDefinition>())
        {   
        }

        public FieldDefinitionCollection(params FieldDefinition[] definitions)
            : this((IEnumerable<FieldDefinition>)definitions)
        {
            
        }

        public FieldDefinitionCollection(IEnumerable<FieldDefinition> definitions)
        {
            if (definitions == null) return;

            foreach (var f in definitions.GroupBy(x => x.Name))
            {
                var indexField = f.FirstOrDefault();
                if (indexField != default(FieldDefinition))
                    _definitions.TryAdd(f.Key, indexField);
            }
        }

        public FieldDefinition GetOrAdd(string fieldName, Func<string, FieldDefinition> add)
        {
            return _definitions.GetOrAdd(fieldName, add);
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
        public virtual bool TryGetValue(string fieldName, out FieldDefinition fieldDefinition)
        {
            return _definitions.TryGetValue(fieldName, out fieldDefinition);
        }

        public bool TryAdd(string fieldName, FieldDefinition definition)
        {
            return _definitions.TryAdd(fieldName, definition);
        }

        public int Count => _definitions.Count;

        public IEnumerator<FieldDefinition> GetEnumerator()
        {
            return _definitions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }    
    }
}