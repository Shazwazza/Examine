using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Defines the mappings between a field name and it's index type
    /// </summary>
    public class FieldDefinitionCollection : ConcurrentDictionary<string, FieldDefinition>
    {
        public FieldDefinitionCollection(IEnumerable<FieldDefinition> allFields)
        {
            foreach (var f in allFields.GroupBy(x => x.Name))
            {
                var indexField = f.FirstOrDefault();
                if (indexField != null)
                {
                    TryAdd(f.Key, f.FirstOrDefault());
                }
            }
        }
        
    }
}