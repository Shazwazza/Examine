using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    internal abstract class IndexReaderDataCollector : Collector
    {
        private readonly ICriteriaContext _criteriaContext;
        protected Collector Inner { get; set; }

        protected Scorer Scorer { get; set; }
        protected ReaderData Data { get; set; }

        public IndexReaderDataCollector(ICriteriaContext criteriaContext, Collector inner)
        {
            _criteriaContext = criteriaContext;
            Inner = inner;
        }

        public override void SetScorer(Scorer scorer)
        {
            Scorer = scorer;
            if (Inner != null)
            {
                Inner.SetScorer(scorer);
            }
        }

        public override void Collect(int doc)
        {            
            if (Inner != null)
            {
                Inner.Collect(doc);
            }
        }



        public override void SetNextReader(Lucene.Net.Index.IndexReader reader, int docBase)
        {
            Data = _criteriaContext.GetReaderData(reader);

            if (Inner != null)
                Inner.SetNextReader(reader, docBase);
        }

        public override bool AcceptsDocsOutOfOrder()
        {
            return Inner == null || Inner.AcceptsDocsOutOfOrder();
        }
    }
}