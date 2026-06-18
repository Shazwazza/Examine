using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Examine.Lucene.Search
{

    /// <summary>
    /// An implementation of the search results returned from Lucene.Net
    /// </summary>
    public class LuceneSearchExecutor
    {
        private readonly QueryOptions _options;
        private readonly LuceneQueryOptions _luceneQueryOptions;
        private readonly IEnumerable<SortField> _sortField;
        private readonly ISearchContext _searchContext;
        private readonly Query _luceneQuery;
        private readonly ISet<string> _fieldsToLoad;
        private int? _maxDoc;

        internal LuceneSearchExecutor(QueryOptions options, Query query, IEnumerable<SortField> sortField, ISearchContext searchContext, ISet<string> fieldsToLoad)
        {
            _options = options ?? QueryOptions.Default;
            _luceneQueryOptions = _options as LuceneQueryOptions;
            _luceneQuery = query ?? throw new ArgumentNullException(nameof(query));
            _fieldsToLoad = fieldsToLoad;
            _sortField = sortField ?? throw new ArgumentNullException(nameof(sortField));
            _searchContext = searchContext ?? throw new ArgumentNullException(nameof(searchContext));
        }

        public ISearchResults Execute()
        {
            var extractTermsSupported = CheckQueryForExtractTerms(_luceneQuery);

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
                    _luceneQuery.ExtractTerms(set);
                }
                catch (NullReferenceException)
                {
                    //this means that an analyzer has stipped out stop words and now there are
                    //no words left to search on

                    //it could also mean that potentially a IIndexFieldValueType is throwing a null ref
                    return LuceneSearchResults.Empty;
                }
                catch (NotSupportedException)
                {
                    //swallow this exception, we should continue if this occurs.
                }
            }

            var sortFields = _sortField as SortField[] ?? _sortField.ToArray();
            Sort sort = null;
            FieldDoc scoreDocAfter = null;
            Filter filter = null;

            using (var searcher = _searchContext.GetSearcher())
            {
                var maxSkipTakeDataSetSize = _luceneQueryOptions?.AutoCalculateSkipTakeMaxResults ?? false
                    ? GetMaxDoc()
                    : _luceneQueryOptions?.SkipTakeMaxResults ?? QueryOptions.AbsoluteMaxResults;

                var maxResults = Math.Min((_options.Skip + 1) * _options.Take, maxSkipTakeDataSetSize);
                maxResults = maxResults >= 1 ? maxResults : QueryOptions.DefaultMaxResults;
                int numHits = maxResults;

                if (sortFields.Length > 0)
                {
                    sort = new Sort(sortFields);
                    sort.Rewrite(searcher.IndexSearcher);
                }
                if (_luceneQueryOptions != null && _luceneQueryOptions.SearchAfter != null)
                {
                    //The document to find results after.
                    scoreDocAfter = GetScoreDocAfter(_luceneQueryOptions);

                    // We want to only collect only the actual number of hits we want to take after the last document. We don't need to collect all previous/next docs.
                    numHits = _options.Take >= 1 ? _options.Take : QueryOptions.DefaultMaxResults;
                }

                TopDocs topDocs;
                ICollector topDocsCollector;
                bool trackMaxScore = _luceneQueryOptions == null ? false : _luceneQueryOptions.TrackDocumentMaxScore;
                bool trackDocScores = _luceneQueryOptions == null ? false : _luceneQueryOptions.TrackDocumentScores;

                if (sortFields.Length > 0)
                {
                    bool fillFields = true;
                    topDocsCollector = TopFieldCollector.Create(sort, numHits, scoreDocAfter, fillFields, trackDocScores, trackMaxScore, false);
                }
                else
                {
                    topDocsCollector = TopScoreDocCollector.Create(numHits, scoreDocAfter, true);
                }

                if (scoreDocAfter != null && sort != null)
                {
                    topDocs = searcher.IndexSearcher.SearchAfter(scoreDocAfter, _luceneQuery, filter, _options.Take, sort, trackDocScores, trackMaxScore);
                }
                else if (scoreDocAfter != null && sort == null)
                {
                    topDocs = searcher.IndexSearcher.SearchAfter(scoreDocAfter, _luceneQuery, _options.Take);
                }
                else
                {
                    searcher.IndexSearcher.Search(_luceneQuery, topDocsCollector);
                    if (sortFields.Length > 0)
                    {
                        topDocs = ((TopFieldCollector)topDocsCollector).GetTopDocs(_options.Skip, _options.Take);
                    }
                    else
                    {
                        topDocs = ((TopScoreDocCollector)topDocsCollector).GetTopDocs(_options.Skip, _options.Take);
                    }
                }

                var totalItemCount = topDocs.TotalHits;

                var results = new List<LuceneSearchResult>(topDocs.ScoreDocs.Length);

                // TODO: Order by Doc Id for improved perf??
                // Our benchmarks show this is isn't a significant performance improvement,
                // but they could be wrong. Sorting by DocId here could only be done if there
                // are no sort options.
                // See https://cwiki.apache.org/confluence/display/lucene/ImproveSearchingSpeed
                foreach (var scoreDoc in topDocs.ScoreDocs)
                {
                    var result = GetSearchResult(scoreDoc, searcher.IndexSearcher);
                    results.Add(result);
                }

                var searchAfterOptions = GetSearchAfterOptions(topDocs);
                float maxScore = topDocs.MaxScore;

                return new LuceneSearchResults(results, totalItemCount, maxScore, searchAfterOptions);
            }
        }

        /// <summary>
        /// Used to calculate the total number of documents in the index.
        /// </summary>
        private int GetMaxDoc()
        {
            if (_maxDoc == null)
            {
                using var searcher = _searchContext.GetSearcher();
                _maxDoc = searcher.IndexSearcher.IndexReader.MaxDoc;
            }

            return _maxDoc.Value;
        }

        private static FieldDoc GetScoreDocAfter(LuceneQueryOptions luceneQueryOptions)
        {
            FieldDoc scoreDocAfter;
            var searchAfter = luceneQueryOptions.SearchAfter;

            object[] searchAfterSortFields = Array.Empty<object>();
            if (luceneQueryOptions.SearchAfter.Fields != null && luceneQueryOptions.SearchAfter.Fields.Length > 0)
            {
                searchAfterSortFields = luceneQueryOptions.SearchAfter.Fields;
            }
            if (searchAfter.ShardIndex != null)
            {
                scoreDocAfter = new FieldDoc(searchAfter.DocumentId, searchAfter.DocumentScore, searchAfterSortFields, searchAfter.ShardIndex.Value);
            }
            else
            {
                scoreDocAfter = new FieldDoc(searchAfter.DocumentId, searchAfter.DocumentScore, searchAfterSortFields);
            }

            return scoreDocAfter;
        }

        internal static SearchAfterOptions GetSearchAfterOptions(TopDocs topDocs)
        {
            var scoreDocs = topDocs.ScoreDocs;
            if (scoreDocs is { Length: > 0 })
            {
                var lastDoc = scoreDocs[scoreDocs.Length - 1];
                if (lastDoc is FieldDoc lastFieldDoc)
                {
                    return new SearchAfterOptions(
                        lastFieldDoc.Doc,
                        lastFieldDoc.Score,
                        lastFieldDoc.Fields is { Length: > 0 } fields ? (object[])fields.Clone() : Array.Empty<object>(),
                        lastFieldDoc.ShardIndex);
                }
                if (lastDoc is ScoreDoc scoreDoc)
                {
                    return new SearchAfterOptions(scoreDoc.Doc, scoreDoc.Score, Array.Empty<object>(), scoreDoc.ShardIndex);
                }
            }

            return null;
        }

        private LuceneSearchResult GetSearchResult(ScoreDoc scoreDoc, IndexSearcher luceneSearcher)
        {
            var docId = scoreDoc.Doc;
            Document doc;
            if (_fieldsToLoad != null)
            {
                doc = luceneSearcher.Doc(docId, _fieldsToLoad);
            }
            else
            {
                doc = luceneSearcher.Doc(docId);
            }

            var score = scoreDoc.Score;
            var shardIndex = scoreDoc.ShardIndex;
            var result = CreateSearchResult(doc, score, shardIndex);
            return result;
        }

        /// <summary>
        /// Creates the search result from a <see cref="Lucene.Net.Documents.Document"/>
        /// </summary>
        /// <param name="doc">The doc to convert.</param>
        /// <param name="score">The score.</param>
        /// <returns>A populated search result object</returns>
        internal static LuceneSearchResult CreateSearchResult(Document doc, float score, int shardIndex)
        {
            var id = doc.Get("id");

            if (string.IsNullOrEmpty(id) == true)
            {
                id = doc.Get(ExamineFieldNames.ItemIdFieldName);
            }

            var searchResult = new LuceneSearchResult(id, score, () =>
            {
                //we can use Lucene to find out the fields which have been stored for this particular document
                var fields = doc.Fields;

                // Pre-size to fields.Count — may over-estimate when field names repeat, but avoids
                // resizes. doc.Fields may list the same field name multiple times (once per stored
                // value); use ContainsKey to skip duplicates instead of a separate HashSet.
                var resultVals = new Dictionary<string, List<string>>(fields.Count);

                foreach (var field in fields)
                {
                    var fieldName = field.Name;
                    if (!resultVals.ContainsKey(fieldName))
                    {
                        resultVals[fieldName] = doc.GetValues(fieldName).ToList();
                    }
                }

                return resultVals;
            }, shardIndex);

            return searchResult;
        }

        private bool CheckQueryForExtractTerms(Query query)
        {
            if (query is BooleanQuery bq)
            {
                foreach (var clause in bq.Clauses)
                {
                    //recurse
                    var check = CheckQueryForExtractTerms(clause.Query);
                    if (!check)
                    {
                        return false;
                    }
                }
            }

            if (query is LateBoundQuery lbq)
            {
                return CheckQueryForExtractTerms(lbq.Wrapped);
            }

            // Use pattern matching (isinst IL instruction) instead of reflection for the common query types
            if (query is TermRangeQuery or WildcardQuery or FuzzyQuery)
            {
                return false; //ExtractTerms() not supported by TermRangeQuery, WildcardQuery,FuzzyQuery and will throw NotSupportedException
            }

            var queryType = query.GetType();
            if (queryType.IsGenericType && queryType.GetGenericTypeDefinition() == typeof(NumericRangeQuery<>))
            {
                return false; //ExtractTerms() not supported by NumericRangeQuery and will throw NotSupportedException
            }

            return true;
        }
    }
}
