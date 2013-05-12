using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.SearchCriteria
{
    public class MutliCriteriaContext : ICriteriaContext
    {
        private readonly ICriteriaContext[] _inner;

        public MutliCriteriaContext(ICriteriaContext[] inner)
        {
            _inner = inner;
            //We can do this because all facet configurations share a single static facetmap.
            FacetMap = inner.Select(cc => cc.FacetMap).FirstOrDefault(fm => fm != null);
        }

        public FacetMap FacetMap { get; private set; }
        public ReaderData GetReaderData(IndexReader reader)
        {
            for (int i = 0, n = _inner.Length; i < n; i++)
            {
                var data = _inner[i].GetReaderData(reader);
                if (data != null)
                {
                    return data;
                }
            }

            return null;
        }
    }
}
