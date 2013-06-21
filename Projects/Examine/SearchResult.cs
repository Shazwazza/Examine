using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Documents;

namespace Examine
{
    public class SearchResult
    {
        [ScriptIgnore]
        public ISearchResults Results { get; set; }

        [Obsolete("You need to specify the owning ISearchResults to enable highlighting")]
        public SearchResult()
        {
            Fields = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            FieldValues = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);            
        }

        public SearchResult(ISearchResults results) : this()
        {
            Results = results;
        }

        internal Document Document { get; set; }

        internal int DocId { get; set; }

        public long LongId { get; set; }

        public int Id { get { return (int) LongId; } }
        
        public float Score { get; set; }
        public IDictionary<string, string> Fields { get; protected set; }

        public IDictionary<string, string[]> FieldValues { get; protected set; }

        public FacetLevel[] Facets { get; set; }


        /// <summary>
        /// How many times this document is used as a facet in the search results or facet count basis (SearchOptions.FacetReferenceCountBasis).
        /// </summary>
        public FacetReferenceCount[] FacetCounts { get; set; }


        
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

        public string GetHighlight(string fieldName)
        {
            if (Results != null && Results.Highlighters != null)
            {
                List<Func<SearchResult,string>> hls;
                if (Results.Highlighters.TryGetValue(fieldName, out hls))
                {
                    return hls.Select(hl => hl(this)).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r));
                }
            }
            

            return null;
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
