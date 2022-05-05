using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// Represents an item to be indexed.
    /// </summary>
    public class ValueSet
    {
        /// <summary>
        /// The identifier of the item to be indexed.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The index category.
        /// </summary>
        /// <remarks>
        /// Used to categorize the item in the index (in Umbraco terms this would be content, media or member).
        /// </remarks>
        public string Category { get; }

        /// <summary>
        /// The index item type.
        /// </summary>
        /// <remarks>
        /// Used to type the item in the index (in Umbraco terms this would be the document type alias).
        /// </remarks>
        public string ItemType { get; }

        /// <summary>
        /// The values to be indexed.
        /// </summary>
        public IDictionary<string, IList<object>> Values { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet"/> class.
        /// </summary>
        /// <param name="id">The identifier of the object to be indexed.</param>
        /// <remarks>
        /// This is normally used for deletions, as it doesn't contain any values.
        /// </remarks>
        public ValueSet(string id)
            : this(id, string.Empty, string.Empty, Enumerable.Empty<KeyValuePair<string, IEnumerable<object>>>())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet" /> class.
        /// </summary>
        /// <param name="id">The identifier of the item to be indexed.</param>
        /// <param name="category">The index category. Used to categorize the item in the index (in Umbraco terms this would be content, media or member).</param>
        /// <param name="values">The values to be indexed.</param>
        public ValueSet(string id, string category, IDictionary<string, object> values)
            : this(id, category, string.Empty, Yield(values))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet" /> class.
        /// </summary>
        /// <param name="id">The identifier of the item to be indexed.</param>
        /// <param name="category">The index category. Used to categorize the item in the index (in Umbraco terms this would be content, media or member).</param>
        /// <param name="values">The values to be indexed.</param>
        public ValueSet(string id, string category, IDictionary<string, IEnumerable<object>> values)
            : this(id, category, string.Empty, Yield(values))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet" /> class.
        /// </summary>
        /// <param name="id">The identifier of the item to be indexed.</param>
        /// <param name="category">The index category. Used to categorize the item in the index (in Umbraco terms this would be content, media or member).</param>
        /// <param name="itemType">The index item type. Used to type the item in the index (in Umbraco terms this would be the document type alias).</param>
        /// <param name="values">The values to be indexed.</param>
        public ValueSet(string id, string category, string itemType, IDictionary<string, object> values)
            : this(id, category, itemType, Yield(values))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet" /> class.
        /// </summary>
        /// <param name="id">The identifier of the item to be indexed.</param>
        /// <param name="category">The index category. Used to categorize the item in the index (in Umbraco terms this would be content, media or member).</param>
        /// <param name="itemType">The index item type. Used to type the item in the index (in Umbraco terms this would be the document type alias).</param>
        /// <param name="values">The values to be indexed.</param>
        public ValueSet(string id, string category, string itemType, IDictionary<string, IEnumerable<object>> values)
            : this(id, category, itemType, Yield(values))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet" /> class.
        /// </summary>
        /// <param name="valueSet">The value set.</param>
        public ValueSet(ValueSet valueSet)
            : this(valueSet.Id, valueSet.Category, valueSet.ItemType, Yield(valueSet.Values))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet" /> class.
        /// </summary>
        /// <param name="id">The identifier of the item to be indexed.</param>
        /// <param name="category">The index category. Used to categorize the item in the index (in Umbraco terms this would be content, media or member).</param>
        /// <param name="itemType">The index item type. Used to type the item in the index (in Umbraco terms this would be the document type alias).</param>
        /// <param name="values">The values to be indexed.</param>
        /// <remarks>
        /// This constructor is not public, because we want to ensure values is not null and contains unique keys.
        /// </remarks>
        private ValueSet(string id, string category, string itemType, IEnumerable<KeyValuePair<string, IEnumerable<object>>> values)
        {
            Id = id;
            Category = category;
            ItemType = itemType;
            Values = values.ToDictionary<KeyValuePair<string, IEnumerable<object>>, string, IList<object>>(x => x.Key, x => x.Value.ToList());
        }

        /// <summary>
        /// Adds the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The number of values stored for the specified key.
        /// </returns>
        public int AddValue(string key, object value)
        {
            if (!Values.TryGetValue(key, out IList<object> values))
            {
                Values.Add(key, values = new List<object>());
            }

            values.Add(value);

            return values.Count;
        }

        /// <summary>
        /// Adds the values.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// The number of values stored for the specified key.
        /// </returns>
        public int AddValues(string key, IEnumerable<object> values)
        {
            if (!Values.TryGetValue(key, out IList<object> currentValues))
            {
                Values.Add(key, currentValues = new List<object>());
            }

            foreach (var value in values)
            {
                currentValues.Add(value);
            }

            return currentValues.Count;
        }

        /// <summary>
        /// Gets the first value for the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The first value for the specified key.
        /// </returns>
        public object GetValue(string key)
            => !Values.TryGetValue(key, out IList<object> values) || values.Count == 0 ? null : values[0];

        /// <summary>
        /// Gets the values for the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The values for the specified key.
        /// </returns>
        public IEnumerable<object> GetValues(string key)
            => !Values.TryGetValue(key, out IList<object> values) ? Enumerable.Empty<object>() : values;

        /// <summary>
        /// Sets the value for the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetValue(string key, object value)
            => Values[key] = new List<object>
            {
                value
            };

        /// <summary>
        /// Sets the values for the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        public void SetValues(string key, IEnumerable<object> values)
            => Values[key] = new List<object>(values);

        /// <summary>
        /// Adds the value if the key doesn't exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the value was added; otherwise, <c>false</c>.
        /// </returns>
        public bool TryAddValue(string key, object value)
        {
            if (Values.ContainsKey(key))
            {
                return false;
            }

            Values.Add(key, new List<object>
            {
                value
            });

            return true;
        }

        /// <summary>
        /// Adds the values if the key doesn't exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        ///   <c>true</c> if the values were added; otherwise, <c>false</c>.
        /// </returns>
        public bool TryAddValues(string key, IEnumerable<object> values)
        {
            if (Values.ContainsKey(key))
            {
                return false;
            }

            Values.Add(key, new List<object>(values));

            return true;
        }

        /// <summary>
        /// Creates a new <see cref="ValueSet" /> from the specified values.
        /// </summary>
        /// <param name="id">The identifier of the item to be indexed.</param>
        /// <param name="category">The index category. Used to categorize the item in the index (in Umbraco terms this would be content, media or member).</param>
        /// <param name="itemType">The index item type. Used to type the item in the index (in Umbraco terms this would be the document type alias).</param>
        /// <param name="values">The values to be indexed.</param>
        /// <returns>
        /// The value set.
        /// </returns>
        public static ValueSet FromObject(string id, string category, string itemType, object values)
            => new ValueSet(id, category, itemType, ObjectExtensions.ConvertObjectToDictionary(values));

        /// <summary>
        /// Creates a new <see cref="ValueSet" /> from the specified values.
        /// </summary>
        /// <param name="id">The identifier of the item to be indexed.</param>
        /// <param name="category">The index category. Used to categorize the item in the index (in Umbraco terms this would be content, media or member).</param>
        /// <param name="values">The values to be indexed.</param>
        /// <returns>
        /// The value set.
        /// </returns>
        public static ValueSet FromObject(string id, string category, object values)
            => new ValueSet(id, category, ObjectExtensions.ConvertObjectToDictionary(values));

        private static IEnumerable<KeyValuePair<string, IEnumerable<object>>> Yield(IDictionary<string, object> values)
        {
            if (values is null)
            {
                yield break;
            }

            IEnumerable<object> Yield(object value)
            {
                yield return value;
            }

            foreach (KeyValuePair<string, object> value in values)
            {
                yield return new KeyValuePair<string, IEnumerable<object>>(value.Key, Yield(value.Value));
            }
        }

        private static IEnumerable<KeyValuePair<string, IEnumerable<object>>> Yield(IDictionary<string, IEnumerable<object>> values)
        {
            if (values is null)
            {
                yield break;
            }

            foreach (KeyValuePair<string, IEnumerable<object>> value in values)
            {
                yield return value;
            }
        }

        private static IEnumerable<KeyValuePair<string, IEnumerable<object>>> Yield(IDictionary<string, IList<object>> values)
        {
            if (values is null)
            {
                yield break;
            }

            foreach (KeyValuePair<string, IList<object>> value in values)
            {
                yield return new KeyValuePair<string, IEnumerable<object>>(value.Key, value.Value);
            }
        }
    }
}
