using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Examine
{
    public class SearchResult
    {
        private OrderedDictionary<string, string> _fields;
        private readonly OrderedDictionary<string, string[]> _fieldValues;

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchResult(string id, int docId, float score, IDictionary<string, string[]> fieldVals)
        {
            Id = id;
            DocId = docId;
            Score = score;
            _fieldValues = new OrderedDictionary<string, string[]>();
            var asWritable = (IDictionary<string, string[]>)_fieldValues;
            foreach (var fieldValue in fieldVals)
            {
                asWritable[fieldValue.Key] = fieldValue.Value;
            }
        }

        public int DocId { get; }
        public string Id { get;  }
        public float Score { get; }

        public IReadOnlyDictionary<string, string> Fields
        {
            get
            {
                if (_fields != null) return _fields;

                //initialize from the multi fields
                _fields = new OrderedDictionary<string, string>();
                var asWritable = (IDictionary<string, string>) _fields;
                foreach (var fieldValue in _fieldValues)
                {
                    if (fieldValue.Value.Length > 0)
                        asWritable[fieldValue.Key] = fieldValue.Value[0];
                }
                return _fields;
            }
        }

        public IReadOnlyDictionary<string, string[]> FieldValues => _fieldValues;


        /// <summary>
        /// If a single field was indexed with multiple values this will return those values, otherwise it will just return the single 
        /// value stored for that field. If the field is not found it returns an empty collection.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetValues(string key)
        {
            if (FieldValues.TryGetValue(key, out var found))
            {
                return found;
            }

            return Fields.TryGetValue(key, out var single) ? new[] { single } : Enumerable.Empty<string>();
        } 

        /// <summary>
        /// Returns the key value pair for the index specified
        /// </summary>
        /// <param name="resultIndex"></param>
        /// <returns></returns>
        public KeyValuePair<string, string> this[int resultIndex] => ((OrderedDictionary<string, string>)Fields)[resultIndex];

        /// <summary>
        /// Returns the value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] => Fields.TryGetValue(key, out var single) ? single : null;

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
