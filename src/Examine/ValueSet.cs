using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Examine.LuceneEngine;

namespace Examine
{
    /// <summary>
    /// Represents an item to be indexed
    /// </summary>
    public class ValueSet
    {
        /// <summary>
        /// Used for legacy purposes and stores the original xml document that used to be used for indexing
        /// </summary>
        internal XElement OriginalNode { get; set; }

        /// <summary>
        /// The id of the object to be indexed
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The index category
        /// </summary>
        /// <remarks>
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </remarks>
        public string IndexCategory { get; private set; }

        /// <summary>
        /// The item's node type (in umbraco terms this would be the doc type alias)
        /// </summary>
        public string ItemType { get; private set; }

        /// <summary>
        /// The values to be indexed
        /// </summary>
        public Dictionary<string, List<object>> Values { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values">
        /// An anonymous object converted to a dictionary
        /// </param>
        public ValueSet(long id, string indexCategory, string itemType, object values)
            : this(id, indexCategory, itemType, ObjectExtensions.ConvertObjectToDictionary(values)) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values">
        /// An anonymous object converted to a dictionary
        /// </param>
        public ValueSet(long id, string indexCategory, object values)
            : this(id, indexCategory, string.Empty, ObjectExtensions.ConvertObjectToDictionary(values)) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(long id, string indexCategory, string itemType, IEnumerable<KeyValuePair<string, object>> values)
            : this(id, indexCategory, itemType, values.GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.Select(vv => vv.Value).ToList())) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(long id, string indexCategory, IEnumerable<KeyValuePair<string, object>> values)
            : this(id, indexCategory, string.Empty, values.GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.Select(vv => vv.Value).ToList())) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(long id, string indexCategory, string itemType, Dictionary<string, List<object>> values = null)
        {
            Id = id;
            IndexCategory = indexCategory;
            ItemType = itemType;
            Values = values ?? new Dictionary<string, List<object>>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>        
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(long id, string indexCategory, Dictionary<string, List<object>> values = null)
            : this(id, indexCategory, string.Empty, values) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(long id, string indexCategory, string itemType, IEnumerable<KeyValuePair<string, object[]>> values)
            : this(id, indexCategory, itemType, values.Where(kv => kv.Value != null).SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, object>(kv.Key, v))))
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(long id, string indexCategory, IEnumerable<KeyValuePair<string, object[]>> values)
            : this(id, indexCategory, string.Empty, values.Where(kv => kv.Value != null).SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, object>(kv.Key, v))))
        {

        }

        /// <summary>
        /// Gets the values for the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<object> GetValues(string key)
        {
            List<object> values;
            return !Values.TryGetValue(key, out values) ? (IEnumerable<object>)new object[0] : values;
        }

        /// <summary>
        /// Adds a value to the keyed item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, object value)
        {
            List<object> values;
            if (!Values.TryGetValue(key, out values))
            {
                Values.Add(key, values = new List<object>());
            }
            values.Add(value);
        }

        /// <summary>
        /// Convert to a ValueSet from legacy fields
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="indexCategory"></param>
        /// <param name="itemType"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        internal static ValueSet FromLegacyFields(long nodeId, string indexCategory, string itemType, Dictionary<string, string> fields)
        {
            return new ValueSet(nodeId, indexCategory, itemType, fields.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value)));
        }

        /// <summary>
        /// Convert this value set to legacy fields
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, string> ToLegacyFields()
        {
            var fields = new Dictionary<string, string>();
            foreach (var v in Values)
            {
                foreach (var val in v.Value)
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

        /// <summary>
        /// Converts the value set to the legacy XML representation
        /// </summary>
        /// <returns></returns>
        internal XElement ToExamineXml()
        {
            return OriginalNode ?? ToLegacyFields().ToExamineXml((int)Id, IndexCategory);
        }
    }
}
