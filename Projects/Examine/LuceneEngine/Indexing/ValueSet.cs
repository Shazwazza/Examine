using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Examine.LuceneEngine.Indexing
{
    public class ValueSet
    {
        public long Id { get; set; }

        public string Type { get; set; }

        public List<KeyValuePair<string, object>> Values { get; set; }

        public ValueSet(long id, string type, IEnumerable<KeyValuePair<string, object>> values = null)
        {
            Id = id;
            Type = type;
            Values = values == null
                         ? new List<KeyValuePair<string, object>>()
                         : new List<KeyValuePair<string, object>>(values);
        }
      
        public ValueSet(long id, string type, IEnumerable<KeyValuePair<string, object[]>> values)
            : this(id, type, values.Where(kv=>kv.Value != null).SelectMany(kv=>kv.Value.Select(v=>new KeyValuePair<string, object>(kv.Key, v))))
        {
            
        }        
        

        internal static ValueSet FromLegacyFields(long nodeId, string type, Dictionary<string, string> fields)
        {
            return new ValueSet(nodeId, type, fields.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value)));            
        }

        internal Dictionary<string, string> ToLegacyFields()
        {
            var fields = new Dictionary<string, string>();
            foreach (var v in Values)
            {
                if (v.Value != null && !fields.ContainsKey(v.Key))
                {
                    fields.Add(v.Key, ""+v.Value);
                }
            }
            return fields;
        }

        internal  XElement ToExamineXml()
        {
            return ToLegacyFields().ToExamineXml((int) Id, Type);
        }
    }
}
