using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine.Indexing
{
    public class ValueSet
    {
        internal XElement OriginalNode { get; set; }

        public long Id { get; set; }

        public string Type { get; set; }

        public Dictionary<string, List<object>> Values { get; set; } 

        
        public ValueSet(long id, string type, IEnumerable<KeyValuePair<string, object>> values)
            : this(id, type, values.GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.Select(vv => vv.Value).ToList())) {}
        

        public ValueSet(long id, string type, Dictionary<string, List<object>> values = null)
        {
            Id = id;
            Type = type;
            Values = values ?? new Dictionary<string, List<object>>();
        }
      
        public ValueSet(long id, string type, IEnumerable<KeyValuePair<string, object[]>> values)
            : this(id, type, values.Where(kv=>kv.Value != null).SelectMany(kv=>kv.Value.Select(v=>new KeyValuePair<string, object>(kv.Key, v))))
        {
            
        }        

        public IEnumerable<object> GetValues(string key)
        {
            List<object> values;
            return !Values.TryGetValue(key, out values) ? (IEnumerable<object>) new object[0] : values;
        }

        
        public void Add(string key, object value)
        {
            List<object> values;
            if (!Values.TryGetValue(key, out values))
            {
                Values.Add(key, values=new List<object>());
            }
            values.Add(value);
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
                foreach( var val in v.Value )
                {
                    if (val != null)
                    {
                        fields.Add(v.Key, "" + val);
                        break;
                    }
                }
            }
            return fields;
        }

        internal  XElement ToExamineXml()
        {
            return OriginalNode ?? ToLegacyFields().ToExamineXml((int) Id, Type);
        }
    }
}
