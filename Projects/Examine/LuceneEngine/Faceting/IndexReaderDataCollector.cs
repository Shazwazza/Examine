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

        protected IndexReaderDataCollector(ICriteriaContext criteriaContext, Collector inner)
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

        /// <summary>
        /// Return <c>true</c> if this collector does not
        ///             require the matching docIDs to be delivered in int sort
        ///             order (smallest to largest) to <see cref="M:Lucene.Net.Search.Collector.Collect(System.Int32)"/>.
        ///             <p/> Most Lucene Query implementations will visit
        ///             matching docIDs in order.  However, some queries
        ///             (currently limited to certain cases of <see cref="T:Lucene.Net.Search.BooleanQuery"/>)
        ///             can achieve faster searching if the
        ///             <c>Collector</c> allows them to deliver the
        ///             docIDs out of order.
        ///             <p/> Many collectors don't mind getting docIDs out of
        ///             order, so it's important to return <c>true</c>
        ///             here. 
        /// </summary>
        /// <value/>
        public override bool AcceptsDocsOutOfOrder
        {
            get { return Inner == null || Inner.AcceptsDocsOutOfOrder; }
        }
    }
}