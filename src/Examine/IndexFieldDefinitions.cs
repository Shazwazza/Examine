using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Config;

namespace Examine
{
    /// <summary>
    /// Defines the mappings between a field name and it's index type
    /// </summary>
    public class IndexFieldDefinitions : ConcurrentDictionary<string, IIndexField>
    {
        public IndexFieldDefinitions(IEnumerable<IIndexField> allFields)
        {
            foreach (var f in allFields.GroupBy(x => x.Name))
            {
                var indexField = f.FirstOrDefault() ?? new IndexField
                {
                    Name = f.Key,
                    Type = FieldDefinitionTypes.FullText //default
                };
                if (string.IsNullOrWhiteSpace(indexField.Type))
                {
                    indexField = new IndexField
                    {
                        Name = indexField.Name,
                        Type = FieldDefinitionTypes.FullText, //default
                        EnableSorting = indexField.EnableSorting
                    };
                }
                TryAdd(f.Key, indexField);
            }
        }
        
    }
}