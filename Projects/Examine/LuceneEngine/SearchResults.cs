using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Web.Script.Serialization;
using Examine;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.SearchCriteria;
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

        [ScriptIgnore]
        public ICriteriaContext CriteriaContext
        {
            
            get;
            
            private set;
        }

        [ScriptIgnore]
        public FacetCounts FacetCounts { get; private set; }

        /// <summary>
        /// Exposes the internal Lucene searcher
        /// </summary>
        public Searcher Searcher { get { return CriteriaContext.Searcher; } }

        /// <summary>
        /// Exposes the internal lucene query to run the search
        /// </summary>
        public Query LuceneQuery
        {
            
            get;
            
            private set;
        }


        //private AllHitsCollector _collector;
        private TopDocs _topDocs;

        private SearchOptions _options;


        public IDictionary<string, List<Func<SearchResult,string>>> Highlighters { get; private set; }

            //private ScoreD

        
        internal SearchResults(Query query, IEnumerable<SortField> sortField, ICriteriaContext searcherContext, SearchOptions options)
        {
            LuceneQuery = query;

            CriteriaContext = searcherContext;
            DoSearch(query, sortField, options);

            Highlighters =
                new Dictionary<string, List<Func<SearchResult, string>>>(StringComparer.InvariantCultureIgnoreCase);
        }

        
        private void DoSearch(Query query, IEnumerable<SortField> sortField, SearchOptions options)
        {
            _options = options;

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


            var count = Math.Min(options.MaxCount, Searcher.MaxDoc());

            var topDocsCollector =
                sortField.Any() ?
                (TopDocsCollector)TopFieldCollector.create(new Sort(sortField.ToArray()), count, false, false, false, false)
                : TopScoreDocCollector.create(count, true);

            if (options.CountFacets && CriteriaContext.FacetMap != null)
            {
                var collector = new FacetCountCollector(CriteriaContext, topDocsCollector);

                Searcher.Search(query, collector);
                FacetCounts = collector.Counts;
            }
            else
            {
                Searcher.Search(query, topDocsCollector);
            }



            _topDocs = topDocsCollector.TopDocs();


            //if (sortField.Count() == 0)
            //{
            //    var topDocs = LuceneSearcher.Search(query, null, LuceneSearcher.MaxDoc(), new Sort());                
            //    _collector = new AllHitsCollector(topDocs.scoreDocs);
            //    topDocs = null;
            //}
            //else
            //{
            //    var topDocs = LuceneSearcher.Search(query, null, LuceneSearcher.MaxDoc(), new Sort(sortField.ToArray()));
            //    _collector = new AllHitsCollector(topDocs.scoreDocs);
            //    topDocs = null;
            //}
            TotalItemCount = _topDocs.TotalHits;
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


        static FacetLevel[] _noFacets = new FacetLevel[0];

        /// <summary>
        /// Creates the search result from a <see cref="Lucene.Net.Documents.Document"/>
        /// </summary>
        /// <param name="doc">The doc to convert.</param>
        /// <param name="score">The score.</param>
        /// <returns>A populated search result object</returns>
        
        protected SearchResult CreateSearchResult(int docId, Document doc, float score)
        {
            string id = doc.Get("id");
            if (string.IsNullOrEmpty(id))
            {
                id = doc.Get(LuceneIndexer.IndexNodeIdFieldName);
            }
            var readerData = CriteriaContext.GetDocumentData(docId);
            var sr = new SearchResult(this)
            {
                LongId = long.Parse(id),
                DocId = docId,
                Score = score,
                Facets = readerData != null && readerData.Data.FacetLevels != null
                                            ? readerData.Data.FacetLevels[readerData.SubDocument] : _noFacets,
                Document = doc
            };

            if ( _options.CountFacetReferences)
            {

                if (_options.CountFacets || _options.FacetReferenceCountBasis != null)
                {
                    var counts = _options.FacetReferenceCountBasis ?? FacetCounts;
                    FacetReferenceInfo[] info;
                    if (counts.FacetMap.TryGetReferenceInfo(sr.LongId, out info))
                    {
                        sr.FacetCounts =
                            info.Select(i => new FacetReferenceCount(i.FieldName, counts.Counts[i.Id])).ToArray();
                    }
                    else
                    {
                        sr.FacetCounts = new FacetReferenceCount[0];
                    }
                }
            }


            //we can use lucene to find out the fields which have been stored for this particular document
            //I'm not sure if it'll return fields that have null values though
            var fields = doc.GetFields();

            //ignore our internal fields though
            foreach (var fieldGroup in fields.Cast<Field>().GroupBy(f => f.Name()))
            {

                sr.Fields.Add(fieldGroup.Key, string.Join(",", fieldGroup.Select(f => f.StringValue())));
                sr.FieldValues.Add(fieldGroup.Key, fieldGroup.Select(f => f.StringValue()).ToArray());
            }

            return sr;
        }

        //NOTE: If we moved this logic inside of the 'Skip' method like it used to be then we get the Code Analysis barking
        // at us because of Linq requirements and 'MoveNext()'. This method is to work around this behavior.
        
        private SearchResult CreateFromDocumentItem(int i)
        {
            var docId = _topDocs.ScoreDocs[i].doc;
            var doc = Searcher.Doc(docId);
            var score = _topDocs.ScoreDocs[i].score;
            var result = CreateSearchResult(docId, doc, score);
            return result;
        }

        /// <summary>
        /// Skips to a particular point in the search results.
        /// </summary>
        /// <remarks>
        /// This allows for lazy loading of the results paging. We don't go into Lucene until we have to.
        /// </remarks>
        /// <param name="skip">The number of items in the results to skip.</param>
        /// <returns>A collection of the search results</returns>
        
        public IEnumerable<SearchResult> Skip(int skip)
        {
            for (int i = skip, n = _topDocs.ScoreDocs.Length; i < n; i++)
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

        #region IEnumerable<SearchResult> Members

        /// <summary>
        /// Gets the enumerator starting at position 0
        /// </summary>
        /// <returns>A collection of the search results</returns>
        public IEnumerator<SearchResult> GetEnumerator()
        {
            //if we're going to Enumerate from this itself we're not going to be skipping
            //so unless we made it IQueryable we can't do anything better than start from 0
            return this.Skip(0).GetEnumerator();
        }

        #endregion

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

        #endregion
    }
}
