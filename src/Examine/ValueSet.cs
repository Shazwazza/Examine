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
        public IDictionary<string, List<object>> Values { get; }

        /// <summary>
        /// Constructor that only specifies an ID
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>normally used for deletions</remarks>
        public ValueSet(string id)
        {
            Id = id;
        }

        public static ValueSet FromObject(string id, string category, string itemType, object values)
        {
            return new ValueSet(id, category, itemType, ObjectExtensions.ConvertObjectToDictionary(values));
        }

        public static ValueSet FromObject(string id, string category, object values)
        {
            return new ValueSet(id, category, ObjectExtensions.ConvertObjectToDictionary(values));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, IDictionary<string, object> values)
            : this(id, category, string.Empty, values)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="itemType"></param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, string itemType, IDictionary<string, object> values)
            : this(id, category, itemType, values.ToDictionary(x => x.Key, x => Yield(x.Value)))
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
        public ValueSet(string id, string category, IDictionary<string, IEnumerable<object>> values)
            : this(id, category, string.Empty, values)
        {
        }

        /// <summary>
        /// Primary constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public ValueSet(string id, string category, string itemType, IDictionary<string, IEnumerable<object>> values)
        {
            Id = id;
            Category = category;
            ItemType = itemType;
            Values = values.ToDictionary(x => x.Key, x => x.Value.ToList());
        }

        ///// <summary>
        ///// Constructor
        ///// </summary>
        ///// <param name="id"></param>        
        ///// <param name="category">
        ///// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        ///// </param>
        ///// <param name="values"></param>
        //public ValueSet(string id, string category, Dictionary<string, List<object>> values = null)
        //    : this(id, category, string.Empty, values) { }

        ///// <summary>
        ///// Constructor
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="itemType">
        ///// The item's node type (in umbraco terms this would be the doc type alias)</param>
        ///// <param name="category">
        ///// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        ///// </param>
        ///// <param name="values"></param>
        //public ValueSet(string id, string category, string itemType, IEnumerable<KeyValuePair<string, object>> values)
        //    : this(id, category, itemType, values.GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.Select(vv => vv.Value))) { }

        ///// <summary>
        ///// Constructor
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="category">
        ///// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        ///// </param>
        ///// <param name="values"></param>
        //public ValueSet(string id, string category, IEnumerable<KeyValuePair<string, object>> values)
        //    : this(id, category, string.Empty, values) { }

        ///// <summary>
        ///// Constructor
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="category">
        ///// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        ///// </param>
        ///// <param name="values"></param>
        //public ValueSet(string id, string category, IEnumerable<KeyValuePair<string, IEnumerable<object>>> values)
        //    : this(id, category, string.Empty, values)
        //{
        //}

        ///// <summary>
        ///// Primary Constructor
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="itemType">
        ///// The item's node type (in umbraco terms this would be the doc type alias)</param>
        ///// <param name="category">
        ///// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        ///// </param>
        ///// <param name="values"></param>
        //public ValueSet(string id, string category, string itemType, IEnumerable<KeyValuePair<string, IEnumerable<object>>> values)
        //{
        //    Id = id;
        //    Category = category;
        //    ItemType = itemType;
        //    var v = new Dictionary<string, List<object>>();
        //    if (values != null)
        //    {
        //        foreach (var val in values)
        //            v[val.Key] = val.Value.ToList();
        //    }
        //    Values = v;
        //}

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

        /// <summary>
        /// Helper method to return IEnumerable from a single
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static IEnumerable<object> Yield(object i)
        {
            yield return i;
        }
    }
}
