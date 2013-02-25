using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public abstract class IndexReaderDataCollector : Collector
    {
        protected IndexReaderDataCollection ReaderDataCollection { get; set; }
        protected Collector Inner { get; set; }

        protected Scorer Scorer { get; set; }
        protected IndexReaderData Data { get; set; }

        public IndexReaderDataCollector(IndexReaderDataCollection readerDataCollection, Collector inner)
        {
            ReaderDataCollection = readerDataCollection;
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
            Data = ReaderDataCollection[reader];

            if (Inner != null)
                Inner.SetNextReader(reader, docBase);
        }

        public override bool AcceptsDocsOutOfOrder()
        {
            return Inner == null || Inner.AcceptsDocsOutOfOrder();
        }
    }
}