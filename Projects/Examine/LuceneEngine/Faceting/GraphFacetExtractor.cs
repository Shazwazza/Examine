using System;
using System.Collections.Generic;

namespace Examine.LuceneEngine.Faceting
{
    //TODO: Figure out what this does? and when we want to use it since its not in use

    /// <summary>
    /// A hierarchical facet extractor based on external information about facet relations.
    /// </summary>
    internal class GraphFacetExtractor : TermFacetExtractor
    {
        private readonly IFacetGraph _graph;

        public Func<float, int, float> DistanceScorer { get; private set; }

        public GraphFacetExtractor(string fieldName, IFacetGraph graph, Func<float, int, float> distanceScorer = null)
            : base(fieldName)
        {
            DistanceScorer = distanceScorer ?? ((level, distance) => level / distance);
            _graph = graph;
        }


        protected override IEnumerable<DocumentFacet> ExpandTerm(int docId, string fieldName, string termValue, float level)
        {
            foreach( var df in base.ExpandTerm(docId, fieldName, termValue, level) )
            {                
                yield return df;
            }

            foreach( var df in ExpandKey(docId, new FacetKey(fieldName, termValue), level, 1, new HashSet<FacetKey>()))
            {
                yield return df;
            }
        }

        protected IEnumerable<DocumentFacet> ExpandKey(int docId, FacetKey key, float level, int distance, HashSet<FacetKey> seen)
        {
            foreach( var parent in _graph.GetParents(key))
            {
                if( !seen.Contains(parent)) //The hashset is used to avoid infinte recursion
                {
                    yield return new DocumentFacet(docId, false, parent, DistanceScorer(level, distance + 1));

                    foreach( var df in ExpandKey(docId, parent, level, distance + 1, seen))
                    {
                        yield return df;
                    }
                }
                seen.Add(parent);
            }
        } 
    }
}
