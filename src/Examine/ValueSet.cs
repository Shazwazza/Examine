using System;
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
        /// The id of the object to be indexed
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The index category
        /// </summary>
        /// <remarks>
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </remarks>
        public string Category { get; }

        /// <summary>
        /// The item's node type (in umbraco terms this would be the doc type alias)
        /// </summary>
        public string ItemType { get; }

        /// <summary>
        /// The values to be indexed
        /// </summary>
        public Dictionary<string, List<object>> Values { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values">
        /// An anonymous object converted to a dictionary
        /// </param>
        public ValueSet(string id, string category, string itemType, object values)
            : this(id, category, itemType, ObjectExtensions.ConvertObjectToDictionary(values)) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values">
        /// An anonymous object converted to a dictionary
        /// </param>
        public ValueSet(string id, string category, object values)
            : this(id, category, string.Empty, ObjectExtensions.ConvertObjectToDictionary(values)) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, string itemType, IEnumerable<KeyValuePair<string, object>> values)
            : this(id, category, itemType, values.GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.Select(vv => vv.Value).ToList())) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, IEnumerable<KeyValuePair<string, object>> values)
            : this(id, category, string.Empty, values.GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.Select(vv => vv.Value).ToList())) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, string itemType, Dictionary<string, List<object>> values = null)
        {
            Id = id;
            Category = category;
            ItemType = itemType;
            Values = values ?? new Dictionary<string, List<object>>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>        
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, Dictionary<string, List<object>> values = null)
            : this(id, category, string.Empty, values) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, string itemType, IEnumerable<KeyValuePair<string, object[]>> values)
            : this(id, category, itemType, values.Where(kv => kv.Value != null).SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, object>(kv.Key, v))))
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, IEnumerable<KeyValuePair<string, object[]>> values)
            : this(id, category, string.Empty, values.Where(kv => kv.Value != null).SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, object>(kv.Key, v))))
        {
        }

        /// <summary>
        /// Gets the values for the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<object> GetValues(string key)
        {
            return !Values.TryGetValue(key, out var values) ? Enumerable.Empty<object>() : values;
        }

        /// <summary>
        /// Gets a single value for the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        /// If there are multiple values, this will return the first
        /// </returns>
        public object GetValue(string key)
        {
            return !Values.TryGetValue(key, out var values) ? null : values.FirstOrDefault();
        }

        /// <summary>
        /// Adds a value to the keyed item, if it doesn't exist the key will be created
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>
        /// The number of items stored for the key
        /// </returns>
        public int Add(string key, object value)
        {
            if (!Values.TryGetValue(key, out var values))
            {
                Values.Add(key, values = new List<object>());
            }
            values.Add(value);
            return values.Count;
        }

        /// <summary>
        /// sets a value to the keyed item, if it doesn't exist the key will be created
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, object value)
        {
            //replace
            Values[key] = new List<object> { value };
        }

        /// <summary>
        /// Adds a value to the keyed item if it doesn't exist
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool TryAdd(string key, object value)
        {
            if (Values.ContainsKey(key)) return false;
            Values.Add(key, new List<object> {value});
            return true;

        }
    }
}
