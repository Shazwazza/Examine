using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Scoring;
using Examine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Examine.LuceneEngine.Providers;
using Version = Lucene.Net.Util.Version;

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// This class is used to query against Lucene.Net. Not thread safe.
    /// </summary>
    [DebuggerDisplay("SearchIndexType: {SearchIndexType}, LuceneQuery: {Query}")]
    public class LuceneSearchCriteria : ISearchCriteria<LuceneBooleanOperation, LuceneSearchCriteria, LuceneSearchCriteria>
    {
        internal static Regex SortMatchExpression = new Regex(@"(\[Type=(?<type>\w+?)\])", RegexOptions.Compiled);
        private CustomMultiFieldQueryParser QueryParser;

        internal Stack<BooleanQuery> Queries = new Stack<BooleanQuery>();
        internal BooleanQuery Query { get { return Queries.Peek(); } }
        internal List<SortField> SortFields = new List<SortField>();

        internal ICriteriaContext CriteriaContext;

        /// <summary>
        /// Used to inject the lucene searcher for facet filters etc. Set the value by SearcherContext.
        /// </summary>
        internal Func<ICriteriaContext> LateBoundSearcherContext;

        private Occur _occurrence;
        private BooleanOperation _boolOp;

        private readonly Lucene.Net.Util.Version _luceneVersion = Lucene.Net.Util.Version.LUCENE_29;
        private int _maxResults = int.MaxValue;

        /// <summary>
        /// Gets or sets the searcher.
        /// </summary>
        /// <value>
        /// The searcher.
        /// </value>
        public BaseLuceneSearcher Searcher { get; set; }
        
        internal LuceneSearchCriteria(BaseLuceneSearcher searcher, string type, Analyzer analyzer, string[] fields, bool allowLeadingWildcards, BooleanOperation occurrence)
        {
            Enforcer.ArgumentNotNull(fields, "fields");

            //This is how the lucene searcher is injected into filters.
            LateBoundSearcherContext = () => CriteriaContext;

            SearchOptions = new SearchOptions();

            SearchIndexType = type;
            Queries.Push(new BooleanQuery());
            this.BooleanOperation = occurrence;
            Searcher = searcher;
            this.QueryParser = new CustomMultiFieldQueryParser(_luceneVersion, fields, analyzer);
            this.QueryParser.AllowLeadingWildcard = allowLeadingWildcards;
        }

        ///// <summary>
        ///// Makes a copy of the LuceneSearchCriteria for a nested query in IBooleanOperation
        ///// </summary>
        ///// <returns></returns>
        //internal LuceneSearchCriteria CloneForInnerQuery()
        //{
        //    var clone = (LuceneSearchCriteria)MemberwiseClone();
        //    clone.Query = new BooleanQuery();
        //    clone.LateBoundSearcherContext = LateBoundSearcherContext;
        //    return clone;
        //}

        /// <summary>
        /// Gets the boolean operation which this query method will be added as
        /// </summary>
        /// <value>The boolean operation.</value>
        public BooleanOperation BooleanOperation
        {
            get { return _boolOp; }
            internal protected set
            {
                _boolOp = value;
                _occurrence = _boolOp.ToLuceneOccurrence();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>        
        public override string ToString()
        {
            return string.Format("{{ SearchIndexType: {0}, LuceneQuery: {1} }}", this.SearchIndexType, this.Query.ToString());
        }

        //private static void ValidateIExamineValue(IExamineValue v)
        //{
        //    var ev = v as ExamineValue;
        //    if (ev == null)
        //    {
        //        throw new ArgumentException("IExamineValue was not created from this provider. Ensure that it is created from the ISearchCriteria this provider exposes");
        //    }
        //}

        internal SearchOptions SearchOptions { get; set; }

        #region ISearchCriteria Members

        /// <summary>
        /// Get the maximum number of results to be easy
        /// </summary>
        public int MaxResults
        {
            get { return _maxResults; }
        }

        /// <summary>
        /// Get the search index type set for the search
        /// </summary>
        public string SearchIndexType
        {
            get;
            protected set;
        }
        
        #endregion

        #region ISearch Members

        /// <summary>
        /// Creates an inner group query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp">The default operation is OR, generally a grouped query would have complex inner queries with an OR against another complex group query</param>
        /// <returns></returns>
        public LuceneBooleanOperation Group(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = new LuceneBooleanOperation(this);
            bo.Op(inner, defaultOp);
            return bo;
        }

        /// <summary>
        /// Query on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.Id(int id)
        {
            return IdInternal(id, _occurrence);
        }

        public LuceneBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
        {
            return ManagedRangeQuery<T>(fieldValue, fieldValue, new[] { fieldName });
        }

        public LuceneBooleanOperation Field(string fieldName, string fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);
        }

        public LuceneBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, fieldValue, _occurrence);
        }

        public LuceneBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this.GroupedAnd(fields.ToArray(), fieldVals.ToArray());
        }

        public LuceneBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] fieldVals)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "fieldVals");

            return this.GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), _occurrence);
        }

        public LuceneBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return this.GroupedOr(fields.ToArray(), fieldVals.ToArray());
        }

        public LuceneBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "query");

            return this.GroupedOrInternal(fields.ToArray(), query, _occurrence);
        }

        public LuceneBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return this.GroupedNot(fields.ToArray(), fieldVals.ToArray());
        }

        public LuceneBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            return this.GroupedNotInternal(fields.ToArray(), query, _occurrence);
        }

        public LuceneBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");
            Enforcer.ArgumentNotNull(operations, "operations");

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return this.GroupedFlexible(fields.ToArray(), operations.ToArray(), fieldVals.ToArray());
        }

        public LuceneBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "query");
            Enforcer.ArgumentNotNull(operations, "operations");

            return this.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, _occurrence);

        }

        public LuceneBooleanOperation OrderBy(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(false, fieldNames);
        }

        public LuceneBooleanOperation OrderByDescending(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(true, fieldNames);
        }

        public LuceneBooleanOperation All()
        {
            Query.Add(new MatchAllDocsQuery(), BooleanOperation.ToLuceneOccurrence());

            return new LuceneBooleanOperation(this);
        }

        public LuceneBooleanOperation ManagedQuery(string query, string[] fields = null, IManagedQueryParameters parameters = null)
        {
            Query.Add(new LateBoundQuery(() =>
            {
                var types = fields != null
                                ? fields.Select(f => CriteriaContext.GetValueType(f)).Where(t => t != null)
                                : CriteriaContext.ValueTypes;

                var bq = new BooleanQuery();
                foreach (var type in types)
                {
                    var q = type.GetQuery(query, CriteriaContext.Searcher, CriteriaContext.FacetsLoader, parameters);
                    if (q != null)
                    {
                        CriteriaContext.ManagedQueries.Add(new KeyValuePair<IIndexValueType, Query>(type, q));
                        bq.Add(q, Occur.SHOULD);
                    }

                }
                return bq;
            }), _occurrence);


            return new LuceneBooleanOperation(this);
        }

        public LuceneBooleanOperation ManagedRangeQuery<T>(T? min, T? max, string[] fields, bool minInclusive = true, bool maxInclusive = true, IManagedQueryParameters parameters = null) where T : struct
        {
            Query.Add(new LateBoundQuery(() =>
            {
                var bq = new BooleanQuery();
                foreach (var f in fields)
                {
                    var type = CriteriaContext.GetValueType(f) as IIndexRangeValueType<T>;
                    if (type != null)
                    {
                        var q = type.GetQuery(min, max, minInclusive, maxInclusive, parameters: parameters);
                        if (q != null)
                        {
                            //CriteriaContext.FieldQueries.Add(new KeyValuePair<IIndexValueType, Query>(type, q));
                            bq.Add(q, Occur.SHOULD);
                        }
                    }
                }
                return bq;
            }), _occurrence);


            return new LuceneBooleanOperation(this);
        }

        public LuceneBooleanOperation Id(int id)
        {
            throw new NotImplementedException();
        }

        IBooleanOperation IQuery.Field<T>(string fieldName, T fieldValue)
        {
            return Field(fieldName, fieldValue);
        }

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.Field(string fieldName, string fieldValue)
        {
            return Field(fieldName, fieldValue);
        }

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.Field(string fieldName, IExamineValue fieldValue)
        {
            return Field(fieldName, fieldValue);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end)
        {
            return this.Range(fieldName, start, end, true, true);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper)
        {
            return this.Range(fieldName, start, end, true, true, DateResolution.Millisecond);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper, DateResolution resolution)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return ManagedRangeQuery<DateTime>(start, end, new[] { fieldName }, includeLower, includeUpper);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, int start, int end)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return ManagedRangeQuery<int>(start, end, new[] { fieldName });
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, int start, int end, bool includeLower, bool includeUpper)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return ManagedRangeQuery<int>(start, end, new[] { fieldName }, includeLower, includeUpper);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        protected internal IBooleanOperation RangeInternal(string fieldName, int start, int end, bool includeLower, bool includeUpper, Occur occurrence)
        {
            Query.Add(NumericRangeQuery.NewIntRange(fieldName, start, end, includeLower, includeUpper), occurrence);
            return new LuceneBooleanOperation(this);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, double lower, double upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, double lower, double upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurrence);
        }
        
        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, float lower, float upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, float lower, float upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurrence);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, long lower, long upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, long lower, long upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurrence);
        }
      
        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, string start, string end)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(start, "start");
            Enforcer.ArgumentNotNull(end, "end");
            return this.Range(fieldName, start, end, true, true);
        }

        [Obsolete("Use ManagedRangeQuery instead")]
        public IBooleanOperation Range(string fieldName, string start, string end, bool includeLower, bool includeUpper)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(start, "start");
            Enforcer.ArgumentNotNull(end, "end");
            return this.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurrence);
        }

        IBooleanOperation IQuery.GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            return GroupedAnd(fields, query);
        }

        IBooleanOperation IQuery.GroupedAnd(IEnumerable<string> fields, IExamineValue[] fieldVals)
        {
            return GroupedAnd(fields, fieldVals);
        }

        IBooleanOperation IQuery.GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            return GroupedOr(fields, query);
        }

        IBooleanOperation IQuery.GroupedOr(IEnumerable<string> fields, params IExamineValue[] fieldVals)
        {
            return GroupedOr(fields, fieldVals);
        }

        IBooleanOperation IQuery.GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            return GroupedNot(fields, query);
        }

        IBooleanOperation IQuery.GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return GroupedNot(fields, query);
        }

        IBooleanOperation IQuery.GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            return GroupedFlexible(fields, operations, query);
        }
        
        IBooleanOperation IQuery.GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] fieldVals)
        {
            return GroupedFlexible(fields, operations, fieldVals);
        }               

        /// <summary>
        /// Passes a raw search query to the provider to handle
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        ISearchCriteria ISearchCriteria.RawQuery(string query)
        {
            return RawQuery(query);
        }

        /// <summary>
        /// Passes a raw search query to the provider to handle
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public LuceneSearchCriteria RawQuery(string query)
        {
            this.Query.Add(this.QueryParser.Parse(query), this._occurrence);
            return this;
        }

        /// <summary>
        /// Set the Max number of items to return
        /// </summary>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public LuceneSearchCriteria MaxCount(int maxCount)
        {
            _maxResults = maxCount;
            return this;
        }

        ISearchCriteria ISearchCriteria.MaxCount(int maxCount)
        {
            return MaxCount(maxCount);
        }

        IBooleanOperation IQuery.ManagedQuery(string query, string[] fields = null, IManagedQueryParameters parameters = null)
        {
            return ManagedQuery(query, fields, parameters);
        }

        IBooleanOperation IQuery.ManagedRangeQuery<T>(
            T? min, T? max, 
            string[] fields, 
            bool minInclusive = true, 
            bool maxInclusive = true, 
            IManagedQueryParameters parameters = null)
        {
            return ManagedRangeQuery(min, max, fields, minInclusive, maxInclusive, parameters);
        }
        
        /// <summary>
        /// Orders the results by the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.OrderBy(params string[] fieldNames)
        {
            return OrderBy(fieldNames);
        }

        /// <summary>
        /// Orders the results by the specified fields in a descending order
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.OrderByDescending(params string[] fieldNames)
        {
            return OrderByDescending(fieldNames);
        }

        IBooleanOperation IQuery.All()
        {
            return All();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a true Lucene Query 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public LuceneBooleanOperation LuceneQuery(Query query, BooleanOperation? op = null)
        {
            this.Query.Add(query, (op ?? this.BooleanOperation).ToLuceneOccurrence());

            return new LuceneBooleanOperation(this);
        }

        public LuceneBooleanOperation Facets(params FacetKey[] keys)
        {
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    this.Query.Add(new ConstantScoreQuery(new FacetFilter(this.LateBoundSearcherContext, key)), this.BooleanOperation.ToLuceneOccurrence());
                }
            }
            return new LuceneBooleanOperation(this);
        }

        public LuceneSearchCriteria WrapRelevanceScore(ScoreOperation op, params IFacetLevel[] levels)
        {
            this.WrapScoreQuery(q => new FacetLevelScoreQuery(q, this.LateBoundSearcherContext, op, levels));

            return this;
        }

        public LuceneSearchCriteria WrapExternalDataScore<TData>(ScoreOperation op, Func<TData, float> scorer)
            where TData : class
        {
            this.WrapScoreQuery(q => new ExternalDataScoreQuery<TData>(q, this.LateBoundSearcherContext, op, scorer));

            return this;
        }

        /// <summary>
        /// Toggles facet counting
        /// </summary>
        /// <param name="toggle"></param>
        /// <returns></returns>        
        public LuceneSearchCriteria CountFacets(bool toggle)
        {
            this.SearchOptions.CountFacets = toggle;
            return this;
        }

        /// <summary>
        /// If a search result is used as a reference facet, toggle whether the count for different field names in the result will be included
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="basis"></param>
        /// <returns></returns>        
        public LuceneSearchCriteria CountFacetReferences(bool toggle, FacetCounts basis = null)
        {
            this.SearchOptions.CountFacetReferences = toggle;
            this.SearchOptions.FacetReferenceCountBasis = basis;
            return this;
        }

        public LuceneSearchCriteria CountFacetReferences(FacetCounts basis)
        {
            return this.CountFacetReferences(true, basis);
        }

        public LuceneSearchCriteria WrapScoreQuery(Func<Query, ReaderDataScoreQuery> scoreQuery)
        {
            var newQuery = new BooleanQuery();
            newQuery.Add(scoreQuery(Queries.Pop()), Occur.MUST);
            Queries.Push(newQuery);

            return this;
        } 

        #endregion

        #region Internal query methods

        protected internal LuceneBooleanOperation GroupedFlexibleInternal(string[] fields, BooleanOperation[] operations, IExamineValue[] fieldVals, Occur occurance)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            var flags = new Occur[operations.Count()];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = operations.ElementAt(i).ToLuceneOccurrence();

            var queryVals = new IExamineValue[fields.Length];
            if (fieldVals.Length == 1)
            {
                for (int i = 0; i < queryVals.Length; i++)
                    queryVals[i] = fieldVals[0];
            }
            else
            {
                queryVals = fieldVals;
            }

            var qry = new BooleanQuery();
            for (int i = 0; i < fields.Length; i++)
            {
                var q = GetFieldInternalQuery(fields[i], queryVals[i], true);
                if (q != null)
                {
                    qry.Add(q, flags[i]);
                }
            }

            this.Query.Add(qry, occurance);

            return new LuceneBooleanOperation(this);
        }
        
        /// <summary>
        /// Internal operation for adding the ordered results
        /// </summary>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        /// <remarks>
        /// 
        /// In order to support sorting based on a real lucene type we have to 'hack' some new syntax in for the field names. 
        /// Ideally we'd have a new method but changing the interface will break things. So instead we're going to detect if each field
        /// name contains a Sort definition. The syntax will be:
        /// 
        /// myFieldName[Type=INT]
        /// 
        /// We then detect if the field name contains [Type=xxx] using regex, if it does then we'll parse out the type and see if it
        /// matches a real lucene type.
        /// 
        /// </remarks>        
        protected internal LuceneBooleanOperation OrderByInternal(bool descending, params string[] fieldNames)
        {
            foreach (var f in fieldNames)
            {
                var fieldName = f;
                var defaultSort = SortField.STRING;
                var match = SortMatchExpression.Match(fieldName);
                if (match.Success && match.Groups["type"] != null)
                {
                    switch (match.Groups["type"].Value.ToUpper())
                    {
                        case "SCORE":
                            defaultSort = SortField.SCORE;
                            break;
                        case "DOC":
                            defaultSort = SortField.DOC;
                            break;
                        case "STRING":
                            defaultSort = SortField.STRING;
                            break;
                        case "INT":
                            defaultSort = SortField.INT;
                            break;
                        case "FLOAT":
                            defaultSort = SortField.FLOAT;
                            break;
                        case "LONG":
                            defaultSort = SortField.LONG;
                            break;
                        case "DOUBLE":
                            defaultSort = SortField.DOUBLE;
                            break;
                        case "SHORT":
                            defaultSort = SortField.SHORT;
                            break;
                        case "CUSTOM":
                            defaultSort = SortField.CUSTOM;
                            break;
                        case "BYTE":
                            defaultSort = SortField.BYTE;
                            break;
                        case "STRING_VAL":
                            defaultSort = SortField.STRING_VAL;
                            break;
                    }
                    //now strip the type from the string
                    fieldName = fieldName.Substring(0, match.Index);
                }

                this.SortFields.Add(new SortField(LuceneIndexer.SortedFieldNamePrefix + fieldName, defaultSort, descending));
            }

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Creates our own style 'multi field query' used internal for the grouped operations
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="fieldVals"></param>
        /// <param name="occurrence"></param>
        /// <param name="matchAllCombinations">If true will match all combinations, if not will only match the values corresponding with fields</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        /// <remarks>
        /// 
        /// if matchAllCombinations == false then...
        /// this will create a query that matches the field index to the value index
        /// For example if we have these fields:
        /// bodyText, pageTitle
        /// and these values:
        /// "hello", "world"
        /// 
        /// then the query output will be:
        /// 
        /// bodyText: "hello" pageTitle: "world"
        /// 
        /// if matchAllCombinations == true then...
        /// This will create a query for all combinations of fields and values. 
        /// For example if we have these fields:
        /// bodyText, pageTitle
        /// and these values:
        /// "hello", "world"
        /// 
        /// then the query output will be:
        /// 
        /// bodyText: "hello" bodyText: "world" pageTitle: "hello" pageTitle: "world"
        /// 
        /// </remarks>        
        protected internal BooleanQuery GetMultiFieldQuery(
            string[] fields,
            IExamineValue[] fieldVals,
            Occur occurrence,
            bool matchAllCombinations = false)
        {

            var qry = new BooleanQuery();
            if (matchAllCombinations)
            {
                foreach (var f in fields)
                {
                    foreach (var val in fieldVals)
                    {
                        var q = GetFieldInternalQuery(f, val, true);
                        if (q != null)
                        {
                            qry.Add(q, occurrence);
                        }
                    }
                }
            }
            else
            {
                var queryVals = new IExamineValue[fields.Length];
                if (fieldVals.Length == 1)
                {
                    for (int i = 0; i < queryVals.Length; i++)
                        queryVals[i] = fieldVals[0];
                }
                else
                {
                    queryVals = fieldVals;
                }

                for (int i = 0; i < fields.Length; i++)
                {
                    var q = GetFieldInternalQuery(fields[i], queryVals[i], true);
                    if (q != null)
                    {
                        qry.Add(q, occurrence);
                    }
                }
            }

            return qry;
        }

        protected internal LuceneBooleanOperation GroupedAndInternal(string[] fields, IExamineValue[] fieldVals, Occur occurrence)
        {

            //if there's only 1 query text we want to build up a string like this:
            //(+field1:query +field2:query +field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.MUST), occurrence);

            return new LuceneBooleanOperation(this);
        }

        protected internal LuceneBooleanOperation GroupedNotInternal(string[] fields, IExamineValue[] fieldVals, Occur occurrence)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(!field1:query !field2:query !field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.MUST_NOT), occurrence);

            return new LuceneBooleanOperation(this);
        }


        protected internal LuceneBooleanOperation GroupedOrInternal(string[] fields, IExamineValue[] fieldVals, Occur occurrence)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.SHOULD, true), occurrence);

            return new LuceneBooleanOperation(this);
        }


        protected internal IBooleanOperation RangeInternal(string fieldName, float lower, float upper, bool includeLower, bool includeUpper, Occur occurrence)
        {
            Query.Add(NumericRangeQuery.NewFloatRange(fieldName, lower, upper, includeLower, includeUpper), occurrence);
            return new LuceneBooleanOperation(this);
        }


        protected internal IBooleanOperation RangeInternal(string fieldName, string start, string end, bool includeLower, bool includeUpper, Occur occurrence)
        {
            Query.Add(new TermRangeQuery(fieldName, start, end, includeLower, includeUpper), occurrence);

            return new LuceneBooleanOperation(this);
        }


        protected internal IBooleanOperation RangeInternal(string fieldName, long lower, long upper, bool includeLower, bool includeUpper, Occur occurrence)
        {
            Query.Add(NumericRangeQuery.NewLongRange(fieldName, lower, upper, includeLower, includeUpper), occurrence);
            return new LuceneBooleanOperation(this);
        }


        protected internal IBooleanOperation RangeInternal(string fieldName, double lower, double upper, bool includeLower, bool includeUpper, Occur occurrence)
        {
            Query.Add(NumericRangeQuery.NewDoubleRange(fieldName, lower, upper, includeLower, includeUpper), occurrence);
            return new LuceneBooleanOperation(this);
        }

        internal protected LuceneBooleanOperation IdInternal(int id, Occur occurrence)
        {
            //use a query parser (which uses the analyzer) to build up the field query which we want
            Query.Add(this.QueryParser.GetFieldQuery(LuceneIndexer.IndexNodeIdFieldName, id.ToString(CultureInfo.InvariantCulture)), occurrence);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Returns the Lucene query object for a field given an IExamineValue
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="useQueryParser">True to use the query parser to parse the search text, otherwise, manually create the queries</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        internal protected Query GetFieldInternalQuery(string fieldName, IExamineValue fieldValue, bool useQueryParser)
        {
            Query queryToAdd;

            switch (fieldValue.Examineness)
            {
                case Examineness.Fuzzy:
                    if (useQueryParser)
                    {
                        queryToAdd = this.QueryParser.GetFuzzyQuery(fieldName, fieldValue.Value, fieldValue.Level);
                    }
                    else
                    {
                        //REFERENCE: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html#Fuzzy%20Searches
                        var proxQuery = fieldName + ":" + fieldValue.Value + "~" + Convert.ToInt32(fieldValue.Level).ToString();
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
                case Examineness.SimpleWildcard:
                case Examineness.ComplexWildcard:
                    if (useQueryParser)
                    {
                        queryToAdd = this.QueryParser.GetWildcardQuery(fieldName, fieldValue.Value);
                    }
                    else
                    {
                        //this will already have a * or a . suffixed based on the extension methods
                        //REFERENCE: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html#Wildcard%20Searches
                        var proxQuery = fieldName + ":" + fieldValue.Value;
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
                case Examineness.Boosted:
                    if (useQueryParser)
                    {
                        queryToAdd = this.QueryParser.GetFieldQuery(fieldName, fieldValue.Value);
                        queryToAdd.SetBoost(fieldValue.Level);
                    }
                    else
                    {
                        //REFERENCE: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html#Boosting%20a%20Term
                        var proxQuery = fieldName + ":\"" + fieldValue.Value + "\"^" + Convert.ToInt32(fieldValue.Level).ToString();
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
                case Examineness.Proximity:

                    //This is how you are supposed to do this based on this doc here:
                    //http://lucene.apache.org/java/2_4_1/api/org/apache/lucene/search/spans/package-summary.html#package_description
                    //but i think that lucene.net has an issue with it's internal parser since it parses to a very strange query
                    //we'll just manually make it instead below

                    //var spans = new List<SpanQuery>();
                    //foreach (var s in fieldValue.Value.Split(' '))
                    //{
                    //    spans.Add(new SpanTermQuery(new Term(fieldName, s)));
                    //}
                    //queryToAdd = new SpanNearQuery(spans.ToArray(), Convert.ToInt32(fieldValue.Level), true);

                    var qry = fieldName + ":\"" + fieldValue.Value + "\"~" + Convert.ToInt32(fieldValue.Level);
                    if (useQueryParser)
                    {
                        queryToAdd = QueryParser.Parse(qry);
                    }
                    else
                    {
                        queryToAdd = ParseRawQuery(qry);
                    }
                    break;
                case Examineness.Escaped:

                    //This uses the KeywordAnalyzer to parse the 'phrase'
                    var stdQuery = fieldName + ":" + fieldValue.Value;

                    //NOTE: We used to just use this but it's more accurate/exact with the below usage of phrase query
                    //queryToAdd = ParseRawQuery(stdQuery);

                    //This uses the PhraseQuery to parse the phrase, the results seem identical
                    queryToAdd = ParseRawQuery(fieldName, fieldValue.Value);

                    break;
                case Examineness.Explicit:
                default:
                    if (useQueryParser)
                    {
                        queryToAdd = this.QueryParser.GetFieldQuery(fieldName, fieldValue.Value);
                    }
                    else
                    {
                        //standard query 
                        var proxQuery = fieldName + ":" + fieldValue.Value;
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
            }
            return queryToAdd;
        }

        /// <summary>
        /// This parses a raw query into a non-tokenized query.
        /// not analyzing/tokenizing the search string
        /// </summary>
        /// <remarks>
        /// Currently this is done by just using the keyword analyzer which doesn't parse special chars, whitespace, etc..
        /// however there may be a better way to acheive this, or could manually parse into a boolean query
        /// using TermQueries.
        /// </remarks>
        /// <param name="rawQuery"></param>
        /// <returns></returns>
        internal protected Query ParseRawQuery(string rawQuery)
        {
            var parser = new QueryParser(_luceneVersion, "", new KeywordAnalyzer());
            return parser.Parse(rawQuery);
        }

        /// <summary>
        /// Uses a PhraseQuery to build a 'raw/exact' match
        /// </summary>
        /// <param name="field"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        /// <remarks>
        /// The result of this seems to be better than the above since it does not include results that contain part of the phrase.
        /// For example, 'codegarden 090' would be matched against the search term 'codegarden 09' with the above, whereas when using the 
        /// PhraseQuery this is not the case
        /// </remarks>
        private Query ParseRawQuery(string field, string txt)
        {
            var phraseQuery = new PhraseQuery();
            phraseQuery.SetSlop(0);
            foreach (var val in txt.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                phraseQuery.Add(new Term(field, val));
            }
            return phraseQuery;
        }

        internal protected LuceneBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, Occur occurrence)
        {
            return FieldInternal(fieldName, fieldValue, occurrence, true);
        }

        internal protected LuceneBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, Occur occurrence, bool useQueryParser)
        {
            Query queryToAdd = GetFieldInternalQuery(fieldName, fieldValue, useQueryParser);

            if (queryToAdd != null)
                Query.Add(queryToAdd, occurrence);

            return new LuceneBooleanOperation(this);
        } 
        #endregion

         //TODO: We need to fix up all the queries now :( Will take some thought since the underlying methods are different.

        /// <summary>
        /// We use this to get at the protected methods directly since the new version makes them not public
        /// </summary>
        private class CustomMultiFieldQueryParser : MultiFieldQueryParser
        {
           
            public CustomMultiFieldQueryParser(Version matchVersion, string[] fields, Analyzer analyzer, IDictionary<string, float> boosts) : base(matchVersion, fields, analyzer, boosts)
            {
            }
            public CustomMultiFieldQueryParser(Version matchVersion, string[] fields, Analyzer analyzer) : base(matchVersion, fields, analyzer)
            {
            }
        }
    }
}
