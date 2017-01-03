using System;
using System.Collections.Generic;
using System.Linq;

namespace Examine
{
    public class SearchResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SearchResult()
        {
            Fields = new Dictionary<string, string>();
            MultiValueFields = new Dictionary<string, List<string>>();
        }

        public int Id { get; set; }
        public float Score { get; set; }
        public IDictionary<string, string> Fields { get; protected set; }

        internal IDictionary<string, List<string>> MultiValueFields { get; private set; }

        /// <summary>
        /// If a single field was indexed with multiple values this will return those values, otherwise it will just return the single 
        /// value stored for that field. If the field is not found it returns an empty collection.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetValues(string key)
        {
            return MultiValueFields.ContainsKey(key)
                ? MultiValueFields[key]
                : Fields.ContainsKey(key)
                    ? new[] {Fields[key]}
                    : Enumerable.Empty<string>();
        } 

        /// <summary>
        /// Returns the key value pair for the index specified
        /// </summary>
        /// <param name="resultIndex"></param>
        /// <returns></returns>
        public KeyValuePair<string, string> this[int resultIndex] 
        {
            get { return Fields.ElementAt(resultIndex); }
        }

        /// <summary>
        /// Returns the value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] 
        {
            get
            {
                return Fields[key];
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
