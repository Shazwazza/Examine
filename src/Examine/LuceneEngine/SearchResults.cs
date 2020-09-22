using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Examine.LuceneEngine.Providers;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// An implementation of the search results returned from Lucene.Net
    /// </summary>
    public class SearchResults : ISearchResults
    {
        ///<summary>
        /// Returns an empty search result
        ///</summary>
        ///<returns></returns>
        public static ISearchResults Empty()
        {
            return new EmptySearchResults();
        }

        /// <summary>
        /// Exposes the internal Lucene searcher
        /// </summary>
        public Searcher LuceneSearcher
        {
            [SecuritySafeCritical]
            get;
            [SecuritySafeCritical]
            private set;
        }

        /// <summary>
        /// Exposes the internal lucene query to run the search
        /// </summary>
        public Query LuceneQuery
        {
            [SecuritySafeCritical]
            get;
            [SecuritySafeCritical]
            private set;
        }

        public TopDocs TopDocs
        {
            [SecuritySafeCritical]
            get;
            [SecuritySafeCritical]
            private set;
        }

        [SecuritySafeCritical]
        internal SearchResults(Query query, IEnumerable<SortField> sortField, Searcher searcher, bool extractTermsNotSupported = false)
        {
            LuceneQuery = query;

            LuceneSearcher = searcher;
            DoSearch(query, sortField, 0,extractTermsNotSupported);
        }

        [SecuritySafeCritical]
        internal SearchResults(Query query, IEnumerable<SortField> sortField, Searcher searcher, int maxResults, bool extractTermsNotSupported = false)
        {
            LuceneQuery = query;

            LuceneSearcher = searcher;
            DoSearch(query, sortField, maxResults,extractTermsNotSupported);
        }
        [SecuritySafeCritical]
        internal SearchResults(Query query, IEnumerable<SortField> sortField, Searcher searcher, int maxResults, FieldSelector fieldSelector, bool extractTermsNotSupported = false)
        {
            LuceneQuery = query;

            LuceneSearcher = searcher;
            _fieldSelector = fieldSelector;
            DoSearch(query, sortField, maxResults, extractTermsNotSupported);
        }

        [SecuritySafeCritical]
        private void DoSearch(Query query, IEnumerable<SortField> sortField, int maxResults,bool extractTermsNotSupported)
        {

            //Avoid throwing NotSupportedException when we know calling ExtractTerms will throw
            if (!extractTermsNotSupported)
            {
                //This try catch is because analyzers strip out stop words and sometimes leave the query
                //with null values. This simply tries to extract terms, if it fails with a null
                //reference then its an invalid null query, NotSupporteException occurs when the query is
                //valid but the type of query can't extract terms.
                //This IS a work-around, theoretically Lucene itself should check for null query parameters
                //before throwing exceptions.
                try
                {
                    var set = new Hashtable();
                    query.ExtractTerms(set);
                }
                catch (NullReferenceException)
                {
                    //this means that an analyzer has stipped out stop words and now there are
                    //no words left to search on
                    TotalItemCount = 0;
                    return;
                }
                catch (NotSupportedException)
                {
                    //swallow this exception, we should continue if this occurs.
                }
            }
            

            maxResults = maxResults >= 1 ? maxResults : LuceneSearcher.MaxDoc();

            Collector topDocsCollector;
            if (sortField.Any())
            {
                topDocsCollector = TopFieldCollector.create(
                    new Sort(sortField.ToArray()), maxResults, false, false, false, false);
            }
            else
            {
                topDocsCollector = TopScoreDocCollector.create(maxResults, true);
            }

            LuceneSearcher.Search(query, topDocsCollector);

            TopDocs = sortField.Any()
                ? ((TopFieldCollector)topDocsCollector).TopDocs()
                : ((TopScoreDocCollector)topDocsCollector).TopDocs();

            TotalItemCount = TopDocs.TotalHits;
        }

        /// <summary>
        /// Gets the total number of results for the search
        /// </summary>
        /// <value>The total items from the search.</value>
        public int TotalItemCount { get; private set; }

        /// <summary>
        /// Internal cache of search results
        /// </summary>
        protected Dictionary<int, SearchResult> Docs = new Dictionary<int, SearchResult>();
        private readonly FieldSelector _fieldSelector;

        // <summary>
        /// Creates the search result from a <see cref="Lucene.Net.Documents.Document"/>
        /// </summary>
        /// <param name="docId">The doc id of the lucene document.</param>
        /// <param name="doc">The doc to convert.</param>
        /// <param name="score">The score.</param>
        /// <returns>A populated search result object</returns>
        [SecuritySafeCritical]
        protected SearchResult CreateSearchResult(int docId, Document doc, float score)
        {
            string id = doc.Get("id");

            if (string.IsNullOrEmpty(id))
            {
                id = doc.Get(LuceneIndexer.IndexNodeIdFieldName);
            }

            var sr = new SearchResult
            {
                DocId = docId,
                Id = int.Parse(id),
                Score = score
            };

            var searchResult = PrepareSearchResult(sr, doc);
            return searchResult;
        }

        /// <summary>
        /// Creates the search result from a <see cref="Lucene.Net.Documents.Document"/>
        /// </summary>
        /// <param name="doc">The doc to convert.</param>
        /// <param name="score">The score.</param>
        /// <returns>A populated search result object</returns>
        [SecuritySafeCritical]
        protected SearchResult CreateSearchResult(Document doc, float score)
        {
            string id = doc.Get("id");

            if (string.IsNullOrEmpty(id))
            {
                id = doc.Get(LuceneIndexer.IndexNodeIdFieldName);
            }

            var sr = new SearchResult
            {
                Id = int.Parse(id),
                Score = score
            };

            var searchResult = PrepareSearchResult(sr, doc);
            return searchResult;
        }

        [SecuritySafeCritical]
        private SearchResult PrepareSearchResult(SearchResult sr, Document doc)
        {
            //we can use lucene to find out the fields which have been stored for this particular document
            //I'm not sure if it'll return fields that have null values though
            var fields = doc.GetFields();

            //ignore our internal fields though
            foreach (var field in fields.Cast<Field>())
            {
                var fieldName = field.Name();
                var values = doc.GetValues(fieldName);

                if (values.Length > 1)
                {
                    sr.MultiValueFields[fieldName] = values.ToList();
                    //ensure the first value is added to the normal fields
                    sr.Fields[fieldName] = values[0];
                }
                else if (values.Length > 0)
                {
                    sr.Fields[fieldName] = values[0];
                }
            }

            return sr;
        }

        //NOTE: If we moved this logic inside of the 'Skip' method like it used to be then we get the Code Analysis barking
        // at us because of Linq requirements and 'MoveNext()'. This method is to work around this behavior.
        [SecuritySafeCritical]
        private SearchResult CreateFromDocumentItem(int i)
        {
            // I have seen IndexOutOfRangeException here which is strange as this is only called in one place
            // and from that one place "i" is always less than the size of this collection. 
            // but we'll error check here anyways
            if (TopDocs?.ScoreDocs.Length < i)
                return null;

            var scoreDoc = TopDocs.ScoreDocs[i];

            var docId = scoreDoc.doc;
            Document doc;
            if (_fieldSelector != null)
            {
                doc = LuceneSearcher.Doc(docId, _fieldSelector);
            }
            else
            {
                doc = LuceneSearcher.Doc(docId);
            }
            var score = scoreDoc.score;
            var result = CreateSearchResult(docId, doc, score);
            return result;
        }

        //NOTE: This is totally retarded but it is required for medium trust as I cannot put this code inside the Skip method... wtf
        [SecuritySafeCritical]
        private int GetScoreDocsLength()
        {
            if (TopDocs?.ScoreDocs == null)
                return 0;

            var length = TopDocs.ScoreDocs.Length;
            return length;
        }

        /// <summary>
        /// Skips to a particular point in the search results.
        /// </summary>
        /// <remarks>
        /// This allows for lazy loading of the results paging. We don't go into Lucene until we have to.
        /// </remarks>
        /// <param name="skip">The number of items in the results to skip.</param>
        /// <returns>A collection of the search results</returns>
		[SecuritySafeCritical]
        public IEnumerable<SearchResult> Skip(int skip)
        {
            for (int i = skip, n = GetScoreDocsLength(); i < n; i++)
            {
                //first check our own cache to make sure it's not there
                if (!Docs.ContainsKey(i))
                {
                    var r = CreateFromDocumentItem(i);
                    if (r == null)
                        continue;

                    Docs.Add(i, r);
                }
                //using yield return means if the user breaks out we wont keep going
                //only load what we need to load!
                //and we'll get it from our cache, this means you can go
                //forward/ backwards without degrading performance
                var result = Docs[i];
                yield return result;
            }
        }

        /// <summary>
        /// Used to Increment/Decrement the index reader so that when the app is shutdown, a reader doesn't actually
        /// get closed if one is open still and it will self close at the end of it's process.
        /// </summary>
        private struct DecrementReaderResult : IEnumerator<SearchResult>
        {
            private readonly IEnumerator<SearchResult> _baseEnumerator;
            private readonly IndexSearcher _searcher;

            [SecuritySafeCritical]
            public DecrementReaderResult(IEnumerator<SearchResult> baseEnumerator, Searcher searcher)
            {
                _baseEnumerator = baseEnumerator;
                _searcher = searcher as IndexSearcher;

                if (_searcher != null)
                {
                    _searcher.GetIndexReader().IncRef();
                }
            }

            [SecuritySafeCritical]
            public void Dispose()
            {
                _baseEnumerator.Dispose();

                if (_searcher != null)
                {
                    _searcher.GetIndexReader().DecRef();
                }
            }

            public bool MoveNext()
            {
                return _baseEnumerator.MoveNext();
            }

            public void Reset()
            {
                _baseEnumerator.Reset();
            }

            public SearchResult Current
            {
                get { return _baseEnumerator.Current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        /// <summary>
        /// Gets the enumerator starting at position 0
        /// </summary>
        /// <returns>A collection of the search results</returns>
        [SecuritySafeCritical]
        public IEnumerator<SearchResult> GetEnumerator()
        {
            return new DecrementReaderResult(
                Skip(0).GetEnumerator(),
                LuceneSearcher);
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}