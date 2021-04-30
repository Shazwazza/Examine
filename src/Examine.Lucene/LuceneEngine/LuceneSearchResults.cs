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
        private readonly IEnumerable<SortField> _sortField;
        private readonly int _maxResults;
        private ResultType? _searchResultType;

        // used for testing
        // TODO: Hopefully we can remove the need for this with https://github.com/Shazwazza/Examine/issues/222
        public int ExecutionCount { get; private set; }

        private enum ResultType
        {
            Default,
            SkipTake
        }

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
        public IndexSearcher LuceneSearcher { get; }

        /// <summary>
        /// Exposes the internal lucene query to run the search
        /// </summary>
        public Query LuceneQuery { get; }

        /// <summary>
        /// Exposes the Lucene docs returned by the search
        /// </summary>
        public TopDocs TopDocs { get; private set; }

        public ISet<string> FieldsToLoad { get; }

        internal LuceneSearchResults(Query query, IEnumerable<SortField> sortField, IndexSearcher searcher, int maxResults, ISet<string> fieldsToLoad)
        {
            LuceneQuery = query;

            FieldsToLoad = fieldsToLoad;
            LuceneSearcher = searcher;
            _maxResults = maxResults;
            _sortField = sortField;
        }

        private void DoSearch(Query query, IEnumerable<SortField> sortField, int maxResults, int? skip = null, int? take = null)
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

            maxResults = maxResults >= 1 ? Math.Min(maxResults, LuceneSearcher.IndexReader.MaxDoc > 0 ? LuceneSearcher.IndexReader.MaxDoc : maxResults) : LuceneSearcher.IndexReader.MaxDoc > 0 ? LuceneSearcher.IndexReader.MaxDoc : 500;

            ICollector topDocsCollector;
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

            if (!skip.HasValue)
            {
                TopDocs = sortFields.Length > 0
                ? ((TopFieldCollector)topDocsCollector).GetTopDocs()
                : ((TopScoreDocCollector)topDocsCollector).GetTopDocs();
            }
            else
            {
                if (sortFields.Length > 0 && take != null && take.Value >= 0)
                {
                    TopDocs = ((TopFieldCollector)topDocsCollector).GetTopDocs(skip.Value, take.Value);
                }
                else if (sortFields.Length > 0 && (take == null || take.Value < 0))
                {
                    TopDocs = ((TopFieldCollector)topDocsCollector).GetTopDocs(skip.Value);
                }
                else if (take != null && take.Value >= 0)
                {
                    TopDocs = ((TopScoreDocCollector)topDocsCollector).GetTopDocs(skip.Value, take.Value);
                }
                else
                {
                    TopDocs = ((TopScoreDocCollector)topDocsCollector).GetTopDocs(skip.Value);
                }
            }

            TotalItemCount = TopDocs.TotalHits;

            ExecutionCount++;
        }

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
            if (FieldsToLoad != null)
            {
                doc = LuceneSearcher.Doc(docId, FieldsToLoad);
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
            // execute the search if it's not already been done or if the previous result was
            // from the SkipTake method
            if (TopDocs == null || _searchResultType == ResultType.SkipTake)
            {
                DoSearch(LuceneQuery, _sortField, _maxResults);
                _searchResultType = ResultType.Default;
            }

            if (TopDocs?.ScoreDocs == null)
                return 0;

            var length = TopDocs.ScoreDocs.Length;
            return length;
        }

        ///<inheritdoc/>
        protected override int GetTotalDocs(int skip, int? take = null)
        {
            int maxResults = take != null ? take.Value + skip : int.MaxValue;

            // execute the search if it's not already been done or if the previous result was
            // from the Default (non SkipTake) method
            if (TopDocs == null || _searchResultType == ResultType.Default)
            {
                DoSearch(LuceneQuery, _sortField, maxResults, skip, take);
                _searchResultType = ResultType.SkipTake;
            }

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
        /// Used as a custom enumerable to return a custom enumerator <see cref="DecrementReaderResult"/> in order to track open readers
        /// </summary>
        private struct SkipEnumerable : IEnumerable<ISearchResult>
        {
            // DO NOT use readonly for a wrapped enumerator, see https://www.red-gate.com/simple-talk/blogs/why-enumerator-structs-are-a-really-bad-idea/
            // "…if the field is readonly and the reference occurs outside an instance constructor of the class in which the field is declared, then the result is a value, namely the value of the field I in the object referenced by E."
#pragma warning disable IDE0044 // Add readonly modifier
            private IEnumerator<ISearchResult> _enumerator;
#pragma warning restore IDE0044 // Add readonly modifier
            private readonly IndexSearcher _searcher;

            public SkipEnumerable(IEnumerator<ISearchResult> enumerator, IndexSearcher searcher)
            {
                _enumerator = enumerator;
                _searcher = searcher;
            }

            public IEnumerator<ISearchResult> GetEnumerator()
            {
                return new DecrementReaderResult(_enumerator, _searcher);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Used to Increment/Decrement the index reader so that when the app is shutdown, a reader doesn't actually
        /// get closed if one is open still and it will self close at the end of it's process.
        /// </summary>
        private struct DecrementReaderResult : IEnumerator<ISearchResult>
        {
            // DO NOT use readonly for a wrapped enumerator, see https://www.red-gate.com/simple-talk/blogs/why-enumerator-structs-are-a-really-bad-idea/
            // "…if the field is readonly and the reference occurs outside an instance constructor of the class in which the field is declared, then the result is a value, namely the value of the field I in the object referenced by E."
#pragma warning disable IDE0044 // Add readonly modifier
            private IEnumerator<ISearchResult> _baseEnumerator;
#pragma warning restore IDE0044 // Add readonly modifier
            private readonly IndexSearcher _searcher;

            public DecrementReaderResult(IEnumerator<ISearchResult> baseEnumerator, IndexSearcher searcher)
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

        /// <summary>
        /// Override skip to use a custom enumerator to track index reader usage
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        public override IEnumerable<ISearchResult> Skip(int skip)
        {
            return new SkipEnumerable(base.Skip(skip).GetEnumerator(), LuceneSearcher);
        }

    }
}
