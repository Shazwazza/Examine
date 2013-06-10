using System;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Faceting;
using Lucene.Net.Documents;

namespace Examine
{
    public class SearchResult
    {
        public SearchResult()
        {
            Fields = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            FieldValues = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase);
            Highlights = new Dictionary<string, List<Func<string>>>(StringComparer.InvariantCultureIgnoreCase);
        }

        internal Document Document { get; set; }

        public long Id { get; set; }
        public float Score { get; set; }
        public IDictionary<string, string> Fields { get; protected set; }

        public IDictionary<string, string[]> FieldValues { get; protected set; }

        public FacetLevel[] Facets { get; set; }


        public FacetReferenceCount[] FacetCounts { get; set; }


        public Dictionary<string, List<Func<string>>> Highlights { get; protected set; }

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

            return Id.Equals(result.Id);
        }

        public string GetHighlight(string fieldName)
        {
            List<Func<string>> f;
            if (Highlights.TryGetValue(fieldName, out f))
            {
                var hs = f.Select(hl => hl()).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                if (hs != null)
                {
                    return string.Join("\r\n", hs);
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
            return Id.GetHashCode();
        }

    }
}
