using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Examine
{
    public class SearchResult
    {
        private readonly OrderedDictionary<string, string> _fields;

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchResult()
        {
            _fields = new OrderedDictionary<string, string>();
            MultiValueFields = new Dictionary<string, List<string>>();
        }

        public int Id { get; set; }
        public float Score { get; set; }

        //TODO: This should have been IReadOnlyDictionary
        public IDictionary<string, string> Fields
        {
            get => _fields;
            //TODO: This was a mistake and should have never allowed setting
            protected set => throw new NotSupportedException("Setting the Fields property is not supported");
        }

        internal IDictionary<string, List<string>> MultiValueFields { get; }

        /// <summary>
        /// If a single field was indexed with multiple values this will return those values, otherwise it will just return the single 
        /// value stored for that field. If the field is not found it returns an empty collection.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetValues(string key)
        {
            if (MultiValueFields.TryGetValue(key, out List<string> found))
            {
                return found;
            }

            if (Fields.TryGetValue(key, out string single))
            {
                return new[] { single };
            }

            return Enumerable.Empty<string>();
        } 

        /// <summary>
        /// Returns the key value pair for the index specified
        /// </summary>
        /// <param name="resultIndex"></param>
        /// <returns></returns>
        public KeyValuePair<string, string> this[int resultIndex] => _fields[resultIndex];

        /// <summary>
        /// Returns the value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] 
        {
            get
            {
                if (Fields.TryGetValue(key, out string single))
                {
                    return single;
                }
                return null;
            }
        }
        
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
