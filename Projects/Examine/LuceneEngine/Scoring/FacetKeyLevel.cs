using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;

namespace Examine.LuceneEngine.Scoring
{
    public class FacetKeyLevel : IFacetLevel
    {
        public FacetKey Key { get; set; }
        public float Level { get; set; }

        public FacetKeyLevel(FacetKey key, float level)
        {
            Key = key;
            Level = level;
        }

        public FacetKeyLevel(string fieldName, string value, float level)
            : this(new FacetKey(fieldName, value), level)
        {
            
        }

        public FacetLevel ToFacetLevel(ISearcherContext context)
        {
            return new FacetLevel {FacetId = context.Searcher.FacetConfiguration.FacetMap.GetIndex(Key), Level = Level};
        }
    }
}
