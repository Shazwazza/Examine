using Lucene.Net.Search;

namespace Examine.LuceneEngine.Facets
{
    public abstract class IndexReaderDataCollector : Collector
    {
        private readonly FacetsLoader _facetsLoader;
        protected Collector Inner { get; set; }

        protected Scorer Scorer { get; set; }
        protected ReaderData Data { get; set; }

        public IndexReaderDataCollector(FacetsLoader facetsLoader, Collector inner)
        {
            _facetsLoader = facetsLoader;
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
            Data = _facetsLoader.GetReaderData(reader);

            if (Inner != null)
                Inner.SetNextReader(reader, docBase);
        }

        public override bool AcceptsDocsOutOfOrder()
        {
            return Inner == null || Inner.AcceptsDocsOutOfOrder();
        }
    }
}