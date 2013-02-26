using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public interface IFacetExtractor
    {
        IEnumerable<DocumentFacet> GetDocumentFacets(IndexReader reader, FacetConfiguration data);
    }
}