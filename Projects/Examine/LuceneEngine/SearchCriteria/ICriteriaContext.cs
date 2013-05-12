using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.SearchCriteria
{
    public interface ICriteriaContext
    {
        FacetMap FacetMap { get; }
       
        ReaderData GetReaderData(IndexReader reader);
    }
}
