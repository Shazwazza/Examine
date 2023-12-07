using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Indexing;
using Examine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.SortedSet;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Index;
using Lucene.Net.Search;
using LuceneFacetResult = Lucene.Net.Facet.FacetResult;

namespace Examine.Lucene.Search
{

    /// <summary>
    /// An implementation of the search results returned from Lucene.Net
    /// </summary>
    public class LuceneSearchExecutor
    {
        private readonly QueryOptions _options;
        private readonly LuceneQueryOptions? _luceneQueryOptions;
        private readonly IEnumerable<SortField> _sortField;
        private readonly ISearchContext _searchContext;
        private readonly Query _luceneQuery;
        private readonly ISet<string>? _fieldsToLoad;
        private readonly IEnumerable<IFacetField>? _facetFields;
        private readonly FacetsConfig? _facetsConfig;
        private readonly Filter? _filter;
        private int? _maxDoc;

        internal LuceneSearchExecutor(QueryOptions? options, Query query, IEnumerable<SortField> sortField, ISearchContext searchContext,
            ISet<string>? fieldsToLoad, IEnumerable<IFacetField>? facetFields, FacetsConfig? facetsConfig)
        {
            _options = options ?? QueryOptions.Default;
            _luceneQueryOptions = _options as LuceneQueryOptions;
            _luceneQuery = query ?? throw new ArgumentNullException(nameof(query));
            _fieldsToLoad = fieldsToLoad;
            _sortField = sortField ?? throw new ArgumentNullException(nameof(sortField));
            _searchContext = searchContext ?? throw new ArgumentNullException(nameof(searchContext));
            _facetFields = facetFields;
            _facetsConfig = facetsConfig;
            _filter = filter;
        }

        private int MaxDoc
        {
            get
            {
                if (_maxDoc == null)
                {
                    using (var searcher = _searchContext.GetSearcher())
                    {
                        _maxDoc = searcher.IndexSearcher.IndexReader.MaxDoc;
                    }
                }
                return _maxDoc.Value;
            }
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <returns></returns>
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

            var maxResults = Math.Min((_options.Skip + 1) * _options.Take, MaxDoc);
            maxResults = maxResults >= 1 ? maxResults : QueryOptions.DefaultMaxResults;
            int numHits = maxResults;

            var sortFields = _sortField as SortField[] ?? _sortField.ToArray();
            Sort? sort = null;
            FieldDoc? scoreDocAfter = null;
            Filter? filter = _filter;

            using (var searcher = _searchContext.GetSearcher())
            {
                if (sortFields.Length > 0)
                {
                    sort = new Sort(sortFields);
                    sort.Rewrite(searcher.IndexSearcher);
                }
                if (_luceneQueryOptions != null && _luceneQueryOptions.SearchAfter != null)
                {
                    //The document to find results after.
                    scoreDocAfter = GetScoreDocAfter(_luceneQueryOptions.SearchAfter);

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
                FacetsCollector? facetsCollector = null;
                if (_facetFields != null && _facetFields.Any() && _luceneQueryOptions != null && _luceneQueryOptions.FacetRandomSampling != null)
                {
                    var facetsCollectors = new RandomSamplingFacetsCollector(_luceneQueryOptions.FacetRandomSampling.SampleSize, _luceneQueryOptions.FacetRandomSampling.Seed);
                }
                else if (_facetFields != null && _facetFields.Any())
                {
                    facetsCollector = new FacetsCollector();
                }

                if (scoreDocAfter != null && sort != null)
                {
                    if (facetsCollector != null)
                    {
                        topDocs = FacetsCollector.SearchAfter(searcher.IndexSearcher, scoreDocAfter, _luceneQuery, filter, _options.Take, sort, MultiCollector.Wrap(topDocsCollector, facetsCollector));
                    }
                    else
                    {
                        topDocs = searcher.IndexSearcher.SearchAfter(scoreDocAfter, _luceneQuery, filter, _options.Take, sort, trackDocScores, trackMaxScore);
                    }
                }
                else if (scoreDocAfter != null && sort == null)
                {
                    if (facetsCollector != null)
                    {
                        topDocs = facetsCollector.SearchAfter(searcher.IndexSearcher, scoreDocAfter, _luceneQuery, _options.Take, MultiCollector.Wrap(topDocsCollector, facetsCollector));
                    }
                    else
                    {
                        topDocs = searcher.IndexSearcher.SearchAfter(scoreDocAfter, _luceneQuery, _options.Take);
                    }
                }
                else
                {
                    if (facetsCollector != null)
                    {
                        searcher.IndexSearcher.Search(_luceneQuery, filter, MultiCollector.Wrap(topDocsCollector, facetsCollector));
                    }
                    else
                    {
                        searcher.IndexSearcher.Search(_luceneQuery, filter, topDocsCollector);
                    }

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

                var results = new List<ISearchResult>(topDocs.ScoreDocs.Length);
                for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
                {
                    var result = GetSearchResult(i, topDocs, searcher.IndexSearcher);
                    if (result != null)
                    {
                        results.Add(result);
                    }
                }
                var searchAfterOptions = GetSearchAfterOptions(topDocs);
                float maxScore = topDocs.MaxScore;
                var facets = ExtractFacets(facetsCollector, searcher);

                return new LuceneSearchResults(results, totalItemCount, facets, maxScore, searchAfterOptions);
            }
        }

        private static FieldDoc GetScoreDocAfter(SearchAfterOptions searchAfterOptions)
        {
            FieldDoc scoreDocAfter;

            object[] searchAfterSortFields = new object[0];
            if (searchAfterOptions.Fields != null && searchAfterOptions.Fields.Length > 0)
            {
                searchAfterSortFields = searchAfterOptions.Fields;
            }
            if (searchAfterOptions.ShardIndex != null)
            {
                scoreDocAfter = new FieldDoc(searchAfterOptions.DocumentId, searchAfterOptions.DocumentScore, searchAfterSortFields, searchAfterOptions.ShardIndex.Value);
            }
            else
            {
                scoreDocAfter = new FieldDoc(searchAfterOptions.DocumentId, searchAfterOptions.DocumentScore, searchAfterSortFields);
            }

            return scoreDocAfter;
        }

        private static SearchAfterOptions? GetSearchAfterOptions(TopDocs topDocs)
        {
            if (topDocs.TotalHits > 0)
            {
                if (topDocs.ScoreDocs.LastOrDefault() is FieldDoc lastFieldDoc && lastFieldDoc != null)
                {
                    return new SearchAfterOptions(lastFieldDoc.Doc, lastFieldDoc.Score, lastFieldDoc.Fields?.ToArray(), lastFieldDoc.ShardIndex);
                }
                if (topDocs.ScoreDocs.LastOrDefault() is ScoreDoc scoreDoc && scoreDoc != null)
                {
                    return new SearchAfterOptions(scoreDoc.Doc, scoreDoc.Score, new object[0], scoreDoc.ShardIndex);
                }
            }
            return null;
        }

        private IReadOnlyDictionary<string, IFacetResult> ExtractFacets(FacetsCollector? facetsCollector, ISearcherReference searcher)
        {
            var facets = new Dictionary<string, IFacetResult>(StringComparer.InvariantCultureIgnoreCase);
            if (facetsCollector == null || _facetFields is null || !_facetFields.Any())
            {
                return facets;
            }

            var facetFields = _facetFields.OrderBy(field => field.FacetField);

            foreach (var field in facetFields)
            {
                var valueType = _searchContext.GetFieldValueType(field.Field);
                if (valueType is IIndexFacetValueType facetValueType)
                {
                    if (_facetsConfig is null)
                    {
                        throw new InvalidOperationException("Facets Config not set. Please use a constructor that passes all parameters");
                    }

                    var facetExtractionContext = new LuceneFacetExtractionContext(facetsCollector, searcher, _facetsConfig);

                    var fieldFacets = facetValueType.ExtractFacets(facetExtractionContext, field);
                    foreach (var fieldFacet in fieldFacets)
                    {
                        // overwrite if necessary (no exceptions thrown in case of collision)
                        facets[fieldFacet.Key] = fieldFacet.Value;
                    }
                }
            }

            return facets;
        }

        private LuceneSearchResult? GetSearchResult(int index, TopDocs topDocs, IndexSearcher luceneSearcher)
        {
            // I have seen IndexOutOfRangeException here which is strange as this is only called in one place
            // and from that one place "i" is always less than the size of this collection. 
            // but we'll error check here anyways
            if (topDocs.ScoreDocs.Length < index)
            {
                return null;
            }

            var scoreDoc = topDocs.ScoreDocs[index];

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
        /// Creates the search result from a <see cref="Document"/>
        /// </summary>
        /// <param name="doc">The doc to convert.</param>
        /// <param name="score">The score.</param>
        /// <param name="shardIndex"></param>
        /// <returns>A populated search result object</returns>
        private LuceneSearchResult CreateSearchResult(Document doc, float score, int shardIndex)
        {
            var id = doc.Get("id");

            if (string.IsNullOrEmpty(id) == true)
            {
                id = doc.Get(ExamineFieldNames.ItemIdFieldName);
            }

            var searchResult = new LuceneSearchResult(id, score, () =>
            {
                //we can use lucene to find out the fields which have been stored for this particular document
                var fields = doc.Fields;

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

            var queryType = query.GetType();

            if (typeof(TermRangeQuery).IsAssignableFrom(queryType)
                || typeof(WildcardQuery).IsAssignableFrom(queryType)
                || typeof(FuzzyQuery).IsAssignableFrom(queryType)
                || (queryType.IsGenericType && queryType.GetGenericTypeDefinition().IsAssignableFrom(typeof(NumericRangeQuery<>))))
            {
                return false; //ExtractTerms() not supported by TermRangeQuery, WildcardQuery,FuzzyQuery and will throw NotSupportedException 
            }

            return true;
        }
    }
}
