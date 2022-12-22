using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Facet.Taxonomy;

namespace Examine.Lucene.Search
{
    public class LuceneFacetLabel : Examine.Search.IFacetLabel
    {
        private readonly FacetLabel _facetLabel;

        public LuceneFacetLabel(FacetLabel facetLabel)
        {
            _facetLabel = facetLabel;
        }

        public string[] Components => _facetLabel.Components;

        public int Length => _facetLabel.Length;

        public int CompareTo(Examine.Search.IFacetLabel other) => _facetLabel.CompareTo(new FacetLabel(other.Components));
        public Examine.Search.IFacetLabel Subpath(int length) => new LuceneFacetLabel(_facetLabel.Subpath(length));
    }
}
