using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// 
    /// </summary>
    public class LuceneFacetSelectionOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public IList<IFacetField> FacetFields { get; set; } = new List<IFacetField>();


        /// <summary>
        /// 
        /// </summary>
        public bool FacetAllFieldsWithHits { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int FacetAllFieldsWithHitsMaxCount { get; set; } = 10;
    }
}
