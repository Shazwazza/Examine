using System;
using System.Collections;
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
        public string? Category { get; }

        /// <summary>
        /// The item's node type (in umbraco terms this would be the doc type alias)
        /// </summary>
        public string? ItemType { get; }

        /// <summary>
        /// The values to be indexed
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<object>>? Values { get; }

        /// <summary>
        /// Constructor that only specifies an ID
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>normally used for deletions</remarks>
        public ValueSet(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Creates a value set from an object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        /// <param name="itemType"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static ValueSet FromObject(string id, string category, string itemType, object values)
            => new ValueSet(id, category, itemType, ObjectExtensions.ConvertObjectToDictionary(values));

       /// <summary>
       /// Creates a value set from an object
       /// </summary>
       /// <param name="id"></param>
       /// <param name="category"></param>
       /// <param name="values"></param>
       /// <returns></returns>
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
        public ValueSet(string id, string? category, string? itemType, IDictionary<string, IEnumerable<object>> values)
            : this(id, category, itemType, values.ToDictionary(x => x.Key, x => (IReadOnlyList<object>)x.Value.ToList()))
        {
        }

        private ValueSet(string id, string? category, string? itemType, IReadOnlyDictionary<string, IReadOnlyList<object>>? values)
        {
            Id = id;
            Category = category;
            ItemType = itemType;
            Values = values?.ToDictionary(x => x.Key, x => (IReadOnlyList<object>)x.Value.ToList()) ?? default;
        }

        /// <summary>
        /// Gets the values for the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<object> GetValues(string key)
        {
#pragma warning disable IDE0022 // Use expression body for method
            return Values != null && Values.TryGetValue(key, out var values) ? values : Enumerable.Empty<object>();
#pragma warning restore IDE0022 // Use expression body for method
        }

        /// <summary>
        /// Gets a single value for the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        /// If there are multiple values, this will return the first
        /// </returns>
        public object? GetValue(string key)
        {
#pragma warning disable IDE0022 // Use expression body for method
            return Values != null && Values.TryGetValue(key, out var values) ? values.Count > 0 ? values[0] : null : null;
#pragma warning restore IDE0022 // Use expression body for method
        }

        /// <summary>
        /// Helper method to return IEnumerable from a single
        /// If object passed in is also enumerable
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static IEnumerable<object> Yield(object i)
        {
            if (i is string)
            {
                yield return i;
            }
            else if (i is IEnumerable enumerable)
            {
                foreach (var element in enumerable)
                {
                    yield return element;
                }
            }
            else
            {
                yield return i;
            }
        }

        /// <summary>
        /// Clones the value set
        /// </summary>
        /// <returns></returns>
        public ValueSet Clone() => new ValueSet(Id, Category, ItemType, Values);
    }
}
