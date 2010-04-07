using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Search;
using Lucene.Net.Index;

namespace UmbracoExamine
{
    class AllHitsCollector : Collector
    {
        class AllHit
        {
            public AllHit(int docId, float score)
            {
                this.DocId = docId;
                this.Score = score;
            }

            public int DocId { get; set; }
            public float Score { get; set; }
        }

        private int docBase;
        private bool outOfOrder;
        private bool shouldScore;
        private Scorer scorer;
        private List<AllHit> hits = new List<AllHit>();

        public AllHitsCollector(bool outOfOrder, bool shouldScore)
        {
            this.outOfOrder = outOfOrder;
            this.shouldScore = shouldScore;
        }

        public override bool AcceptsDocsOutOfOrder()
        {
            return this.outOfOrder;
        }

        public override void Collect(int doc)
        {
            var score = 1.0f;
            if (shouldScore)
            {
                score = scorer.Score();
            }
            hits.Add(new AllHit(doc, score));
        }

        public override void SetNextReader(IndexReader reader, int docBase)
        {
            this.docBase = docBase;
        }

        public override void SetScorer(Scorer scorer)
        {
            this.scorer = scorer;
        }

        public int GetDocId(int index)
        {
            return hits.ElementAt(index).DocId;
        }

        public float GetDocScore(int index)
        {
            return hits.ElementAt(index).Score;
        }

        public int Count
        {
            get
            {
                return hits.Count;
            }
        }
    }
}
