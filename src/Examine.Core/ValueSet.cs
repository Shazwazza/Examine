using System.Collections.Generic;
using System.Linq;

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
        public IReadOnlyDictionary<string, IReadOnlyList<object>> Values { get; }

        /// <summary>
        /// Constructor that only specifies an ID
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>normally used for deletions</remarks>
        public ValueSet(string id) => Id = id;

        public static ValueSet FromObject(string id, string category, string itemType, object values)
            => new ValueSet(id, category, itemType, ObjectExtensions.ConvertObjectToDictionary(values));

        public static ValueSet FromObject(string id, string category, object values)
            => new ValueSet(id, category, ObjectExtensions.ConvertObjectToDictionary(values));

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
            : this(id, category, itemType, (IReadOnlyDictionary<string, IReadOnlyList<object>>)BuildSingleValueDict(values))
        {
        }

        // Replaces values.ToDictionary(x => x.Key, x => (IReadOnlyList<object>)new[] { x.Value })
        // — eliminates one LINQ state-machine allocation per ValueSet construction, and pre-sizes
        // the output dictionary to avoid internal resizes.
        private static Dictionary<string, IReadOnlyList<object>> BuildSingleValueDict(IDictionary<string, object> values)
        {
            var dict = new Dictionary<string, IReadOnlyList<object>>(values.Count);
            foreach (var kvp in values)
                dict[kvp.Key] = new[] { kvp.Value };
            return dict;
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
            : this(id, category, itemType, (IReadOnlyDictionary<string, IReadOnlyList<object>>)BuildMultiValueDict(values))
        {
        }

        // Replaces values.ToDictionary(x => x.Key, x => (IReadOnlyList<object>)x.Value.ToList())
        // — eliminates the LINQ state-machine allocation and pre-sizes the output dictionary.
        // Fast-paths when the value is already an IReadOnlyList<object> to skip ToList() allocation.
        private static Dictionary<string, IReadOnlyList<object>> BuildMultiValueDict(IDictionary<string, IEnumerable<object>> values)
        {
            var dict = new Dictionary<string, IReadOnlyList<object>>(values.Count);
            foreach (var kvp in values)
                dict[kvp.Key] = kvp.Value is IReadOnlyList<object> rl ? rl : (IReadOnlyList<object>)kvp.Value.ToList();
            return dict;
        }

        private ValueSet(string id, string category, string itemType, IReadOnlyDictionary<string, IReadOnlyList<object>> values)
        {
            Id = id;
            Category = category;
            ItemType = itemType;
            Values = values;
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
            return !Values.TryGetValue(key, out var values) ? null : values.Count > 0 ? values[0] : null;
        }

        public ValueSet Clone() => new ValueSet(Id, Category, ItemType, Values);
    }
}
