using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.SearchCriteria
{
    public interface ICriteriaContext
    {
        Searcher Searcher { get; }

        FacetsLoader FacetsLoader { get; }

        FacetMap FacetMap { get; }

        ReaderData GetReaderData(IndexReader reader);

        IIndexValueType GetValueType(string fieldName);

        DocumentData GetDocumentData(int doc);

        List<KeyValuePair<IIndexValueType, Query>> FieldQueries { get; }
    }
}
