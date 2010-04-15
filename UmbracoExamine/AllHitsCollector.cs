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
            //this will be called for each document that is matched in the query
            var score = 1.0f;
            if (shouldScore)
            {
                //only get the score if required
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

        /// <summary>
        /// Gets the doc id at a specified index
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int GetDocId(int index)
        {
            return hits.ElementAt(index).DocId;
        }

        /// <summary>
        /// Gets the doc score for a doc at a specified index
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
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
