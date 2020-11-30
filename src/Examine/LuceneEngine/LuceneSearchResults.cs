using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// An implementation of the search results returned from Lucene.Net
    /// </summary>
    public class LuceneSearchResults : LuceneSearchResultsBase
    {
        ///<summary>
        /// Returns an empty search result
        ///</summary>
        [Obsolete("Use the Empty property on SearchResultsBase instead")]
        public static ISearchResults Empty()
        {
            return EmptySearchResults.Instance;
        }

        /// <summary>
        /// Exposes the internal Lucene searcher
        /// </summary>
        public Searcher LuceneSearcher { get; }

        /// <summary>
        /// Exposes the internal lucene query to run the search
        /// </summary>
        public Query LuceneQuery { get; }

        /// <summary>
        /// Exposes the Lucene docs returned by the search
        /// </summary>
        public TopDocs TopDocs { get; private set; }

        public FieldSelector FieldSelector { get; }

        internal LuceneSearchResults(Query query, IEnumerable<SortField> sortField, Searcher searcher, int maxResults, FieldSelector fieldSelector)
        {
            LuceneQuery = query;
            FieldSelector = fieldSelector;
            LuceneSearcher = searcher;

            DoSearch(query, sortField, maxResults);
        }

        private void DoSearch(Query query, IEnumerable<SortField> sortField, int maxResults)
        {
            var extractTermsSupported = CheckQueryForExtractTerms(query);

            if (extractTermsSupported)
            {
                //This try catch is because analyzers strip out stop words and sometimes leave the query
                //with null values. This simply tries to extract terms, if it fails with a null
                //reference then its an invalid null query, NotSupporteException occurs when the query is
                //valid but the type of query can't extract terms.
                //This IS a work-around, theoretically Lucene itself should check for null query parameters
                //before throwing exceptions.
                try
                {
                    var set = new HashSet<Term>();
                    query.ExtractTerms(set);
                }
                catch (NullReferenceException)
                {
                    //this means that an analyzer has stipped out stop words and now there are
                    //no words left to search on

                    //it could also mean that potentially a IIndexFieldValueType is throwing a null ref
                    TotalItemCount = 0;
                    return;
                }
                catch (NotSupportedException)
                {
                    //swallow this exception, we should continue if this occurs.
                }
            }   

            maxResults = maxResults >= 1 ? Math.Min(maxResults, LuceneSearcher.MaxDoc) : LuceneSearcher.MaxDoc;

            Collector topDocsCollector;
            var sortFields = sortField as SortField[] ?? sortField.ToArray();
            if (sortFields.Length > 0)
            {
                topDocsCollector = TopFieldCollector.Create(
                    new Sort(sortFields), maxResults, false, false, false, false);
            }
            else
            {
                topDocsCollector = TopScoreDocCollector.Create(maxResults, true);
            }

            LuceneSearcher.Search(query, topDocsCollector);

            TopDocs = sortFields.Length > 0
                ? ((TopFieldCollector)topDocsCollector).TopDocs()
                : ((TopScoreDocCollector)topDocsCollector).TopDocs();

            TotalItemCount = TopDocs.TotalHits;
        }

        //NOTE: If we moved this logic inside of the 'Skip' method like it used to be then we get the Code Analysis barking
        // at us because of Linq requirements and 'MoveNext()'. This method is to work around this behavior.
        protected override ISearchResult GetSearchResult(int index)
        {
            // I have seen IndexOutOfRangeException here which is strange as this is only called in one place
            // and from that one place "i" is always less than the size of this collection. 
            // but we'll error check here anyways
            if (TopDocs?.ScoreDocs.Length < index)
                return null;

            var scoreDoc = TopDocs.ScoreDocs[index];

            var docId = scoreDoc.Doc;
            Document doc;
            if(FieldSelector != null)
            {
                doc = LuceneSearcher.Doc(docId, FieldSelector);
            }
            else
            {
                doc = LuceneSearcher.Doc(docId);
            }
            var score = scoreDoc.Score;
            var result = CreateSearchResult(doc, score);

            return result;
        }

        ///<inheritdoc/>
        protected override int GetTotalDocs()
        {
            if (TopDocs?.ScoreDocs == null)
                return 0;

            var length = TopDocs.ScoreDocs.Length;
            return length;
        }

        private bool CheckQueryForExtractTerms(Query query)
        {
            if (query is TermRangeQuery || query is WildcardQuery || query is FuzzyQuery)
            {
                return false; //ExtractTerms() not supported by TermRangeQuery, WildcardQuery,FuzzyQuery and will throw NotSupportedException 
            }

            if (query is BooleanQuery bq)
            {
                foreach (BooleanClause clause in bq.Clauses)
                {
                    //recurse
                    var check = CheckQueryForExtractTerms(clause.Query);
                    if (!check)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Used to Increment/Decrement the index reader so that when the app is shutdown, a reader doesn't actually
        /// get closed if one is open still and it will self close at the end of it's process.
        /// </summary>
        private struct DecrementReaderResult : IEnumerator<ISearchResult>
        {
            private readonly IEnumerator<ISearchResult> _baseEnumerator;
            private readonly IndexSearcher _searcher;


            public DecrementReaderResult(IEnumerator<ISearchResult> baseEnumerator, Searcher searcher)
            {
                _baseEnumerator = baseEnumerator;
                _searcher = searcher as IndexSearcher;

                _searcher?.IndexReader.IncRef();
            }


            public void Dispose()
            {
                _baseEnumerator.Dispose();

                _searcher?.IndexReader.DecRef();
            }

            public bool MoveNext()
            {
                return _baseEnumerator.MoveNext();
            }

            public void Reset()
            {
                _baseEnumerator.Reset();
            }

            public ISearchResult Current => _baseEnumerator.Current;

            object IEnumerator.Current => Current;
        }

        ///<inheritdoc/>
        public override IEnumerator<ISearchResult> GetEnumerator()
        {
            return new DecrementReaderResult(base.GetEnumerator(), LuceneSearcher);
        }
    }
}