using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Examine
{
    public class SearchResult : ISearchResult
    {
        private OrderedDictionary<string, string> _fields;

        // Storing the factory directly avoids allocating a Lazy<T> object and an inner closure
        // per SearchResult — two heap objects (~80 bytes total) that would otherwise be created
        // unconditionally on every search result even if field values are never accessed.
        // The first access to AllValues (or Values) calls the factory, builds and caches the result.
        // This matches the same non-thread-safe lazy pattern already used by the Values getter.
        private readonly Func<IDictionary<string, List<string>>> _lazyFieldVals;
        private OrderedDictionary<string, IReadOnlyList<string>> _allValues;

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchResult(string id, float score, Func<IDictionary<string, List<string>>> lazyFieldVals)
        {
            Id = id;
            Score = score;
            _lazyFieldVals = lazyFieldVals;
        }

        public string Id { get;  }

        public float Score { get; }

        private OrderedDictionary<string, IReadOnlyList<string>> BuildAllValues()
        {
            if (_allValues != null) return _allValues;

            var result = new OrderedDictionary<string, IReadOnlyList<string>>();
            var asWritable = (IDictionary<string, IReadOnlyList<string>>)result;

            var fieldVals = _lazyFieldVals(); //defer execution of collection to here

            foreach (var fieldValue in fieldVals)
            {
                asWritable[fieldValue.Key] = fieldValue.Value;
            }

            return _allValues = result;
        }

        /// <summary>
        /// Returns the values in the result
        /// </summary>
        public IReadOnlyDictionary<string, string> Values
        {
            get
            {
                if (_fields != null) return _fields;

                //initialize from the multi fields
                _fields = new OrderedDictionary<string, string>();
                var asWritable = (IDictionary<string, string>) _fields;
                foreach (var fieldValue in BuildAllValues())
                {
                    if (fieldValue.Value.Count > 0)
                        asWritable[fieldValue.Key] = fieldValue.Value[0];
                }
                return _fields;
            }
        }

        /// <summary>
        /// Returns the values in the result
        /// </summary>
        /// <remarks>
        /// This is used to retrieve multiple values per field if there are any
        /// </remarks>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> AllValues => BuildAllValues();


        /// <summary>
        /// If a single field was indexed with multiple values this will return those values, otherwise it will just return the single 
        /// value stored for that field. If the field is not found it returns an empty collection.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetValues(string key)
        {
            if (AllValues.TryGetValue(key, out var found))
            {
                return found;
            }

            return Values.TryGetValue(key, out var single) ? new[] { single } : Enumerable.Empty<string>();
        } 

        /// <summary>
        /// Returns the key value pair for the index specified
        /// </summary>
        /// <param name="resultIndex"></param>
        /// <returns></returns>
        public KeyValuePair<string, string> this[int resultIndex] => ((OrderedDictionary<string, string>)Values)[resultIndex];

        /// <summary>
        /// Returns the value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] => Values.TryGetValue(key, out var single) ? single : null;

        /// <summary>
        /// Override this method so that the Distinct() operator works
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var result = (SearchResult)obj;

            return Id.Equals(result.Id);
        }

        /// <summary>
        /// Override this method so that the Distinct() operator works
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
