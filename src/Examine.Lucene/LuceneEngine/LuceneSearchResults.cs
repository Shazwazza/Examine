using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// An implementation of the search results returned from Lucene.Net
    /// </summary>
    public class LuceneSearchResults : ISearchResults
    {
        ///<summary>
        /// Returns an empty search result
        ///</summary>
        ///<returns></returns>
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

        public TopDocs TopDocs { get; private set; }


        internal LuceneSearchResults(Query query, IEnumerable<SortField> sortField, Searcher searcher, int maxResults)
        {
            LuceneQuery = query;

            LuceneSearcher = searcher;
            DoSearch(query, sortField, maxResults);
        }

        
        private void DoSearch(Query query, IEnumerable<SortField> sortField, int maxResults)
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

        /// <summary>
        /// Gets the total number of results for the search
        /// </summary>
        /// <value>The total items from the search.</value>
        public long TotalItemCount { get; private set; }

        /// <summary>
        /// Internal cache of search results
        /// </summary>
        protected Dictionary<int, SearchResult> Docs = new Dictionary<int, SearchResult>();

        /// <summary>
        /// Creates the search result from a <see cref="Lucene.Net.Documents.Document"/>
        /// </summary>
        /// <param name="doc">The doc to convert.</param>
        /// <param name="score">The score.</param>
        /// <returns>A populated search result object</returns>
        protected SearchResult CreateSearchResult(Document doc, float score)
        {
            var searchResult = PrepareSearchResult(score, doc);
            return searchResult;
        }

        private SearchResult PrepareSearchResult(float score, Document doc)
        {
            var id = doc.Get("id");
            if (string.IsNullOrEmpty(id))
            {
                id = doc.Get(ExamineFieldNames.ItemIdFieldName);
            }

            var sr = new SearchResult(id, score, () =>
            {
                //we can use lucene to find out the fields which have been stored for this particular document
                var fields = doc.GetFields();

                var resultVals = new Dictionary<string, List<string>>();

                foreach (var field in fields.Cast<Field>())
                {
                    var fieldName = field.Name;
                    var values = doc.GetValues(fieldName);

                    if (resultVals.TryGetValue(fieldName, out var resultFieldVals))
                    {
                        foreach (var value in values)
                        {
                            if (!resultFieldVals.Contains(value))
                            {
                                resultFieldVals.Add(value);
                            }
                        }
                    }
                    else
                    {
                        resultVals[fieldName] = values.ToList();
                    }
                }

                return resultVals;
            });
            
            return sr;
        }

        //NOTE: If we moved this logic inside of the 'Skip' method like it used to be then we get the Code Analysis barking
        // at us because of Linq requirements and 'MoveNext()'. This method is to work around this behavior.
        
        private SearchResult CreateFromDocumentItem(int i)
        {
            var docId = TopDocs.ScoreDocs[i].Doc;
            var doc = LuceneSearcher.Doc(docId);
            var score = TopDocs.ScoreDocs[i].Score;
            var result = CreateSearchResult(doc, score);
            return result;
        }

        //NOTE: This is totally retarded but it is required for medium trust as I cannot put this code inside the Skip method... wtf
        
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
		
        public IEnumerable<ISearchResult> Skip(int skip)
        {
            for (int i = skip, n = GetScoreDocsLength(); i < n; i++)
            {
                //first check our own cache to make sure it's not there
                if (!Docs.ContainsKey(i))
                {
                    var r = CreateFromDocumentItem(i);
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

        /// <summary>
        /// Gets the enumerator starting at position 0
        /// </summary>
        /// <returns>A collection of the search results</returns>
        
        public IEnumerator<ISearchResult> GetEnumerator()
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