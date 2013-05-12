using System.Collections.Generic;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Facets
{
    public interface IFacetExtractor
    {
        IEnumerable<DocumentFacet> GetDocumentFacets(IndexReader reader, FacetConfiguration data);
    }
}