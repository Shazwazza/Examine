using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Documents;

namespace Examine
{
    public interface ISearchResult
    {
        [Obsolete("Use LongId instead")]
        int Id { get; }

        long LongId { get; set; }
        float Score { get; set; }
        IDictionary<string, string> Fields { get; }
        IDictionary<string, string[]> FieldValues { get; }

        /// <summary>
        /// Returns the key value pair for the index specified
        /// </summary>
        /// <param name="resultIndex"></param>
        /// <returns></returns>
        KeyValuePair<string, string> this[int resultIndex] { get; }

        /// <summary>
        /// Returns the value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string this[string key] { get; }
    }

    public class SearchResult : ISearchResult
    {

        public SearchResult()
        {
            Fields = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            FieldValues = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);            
        }

        public long LongId { get; set; }

        [Obsolete("Use LongId instead")]
        public int Id { get { return (int) LongId; } }
        
        public float Score { get; set; }
        public IDictionary<string, string> Fields { get; protected set; }

        public IDictionary<string, string[]> FieldValues { get; protected set; }

        
        /// <summary>
        /// Returns the key value pair for the index specified
        /// </summary>
        /// <param name="resultIndex"></param>
        /// <returns></returns>
        public KeyValuePair<string, string> this[int resultIndex] 
        {
            get
            {
                return Fields.ToArray()[resultIndex];
            }
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

            return LongId.Equals(result.LongId);
        }

        /// <summary>
        /// Override this method so that the Distinct() operator works
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return LongId.GetHashCode();
        }

    }
}
