using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using Examine;
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

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// This class is used to query against Lucene.Net. Not thread safe.
    /// </summary>
    [DebuggerDisplay("SearchIndexType: {SearchIndexType}, LuceneQuery: {Query}")]
    public class LuceneSearchCriteria : ISearchCriteria<LuceneBooleanOperation, LuceneSearchCriteria>
    {
        internal static Regex SortMatchExpression = new Regex(@"(\[Type=(?<type>\w+?)\])", RegexOptions.Compiled);
        internal MultiFieldQueryParser QueryParser;

        internal Stack<BooleanQuery> Queries = new Stack<BooleanQuery>();
        internal BooleanQuery Query { get { return Queries.Peek(); } }
        internal List<SortField> SortFields = new List<SortField>();

        internal ICriteriaContext CriteriaContext;

        /// <summary>
        /// Used to inject the lucene searcher for facet filters etc. Set the value by SearcherContext.
        /// </summary>
        internal Func<ICriteriaContext> LateBoundSearcherContext;

        private readonly BooleanClause.Occur _occurance;
        private readonly Lucene.Net.Util.Version _luceneVersion = Lucene.Net.Util.Version.LUCENE_29;
        private int _maxResults = int.MaxValue;

        public BaseLuceneSearcher Searcher { get; set; }
        
        internal LuceneSearchCriteria(BaseLuceneSearcher searcher, string type, Analyzer analyzer, string[] fields, bool allowLeadingWildcards, BooleanOperation occurance)
        {
            Enforcer.ArgumentNotNull(fields, "fields");

            //This is how the lucene searcher is injected into filters.
            LateBoundSearcherContext = () => CriteriaContext;


            SearchOptions = SearchOptions.Default;

            SearchIndexType = type;
            Queries.Push(new BooleanQuery());
            this.BooleanOperation = occurance;
            Searcher = searcher;
            this.QueryParser = new MultiFieldQueryParser(_luceneVersion, fields, analyzer);
            this.QueryParser.SetAllowLeadingWildcard(allowLeadingWildcards);
            this._occurance = occurance.ToLuceneOccurance();
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
            get;
            protected set;
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

        private static void ValidateIExamineValue(IExamineValue v)
        {
            var ev = v as ExamineValue;
            if (ev == null)
            {
                throw new ArgumentException("IExamineValue was not created from this provider. Ensure that it is created from the ISearchCriteria this provider exposes");
            }
        }

        internal SearchOptions SearchOptions { get; set; }

        #region ISearchCriteria Members

        public int MaxResults
        {
            get { return _maxResults; }
        }

        public string SearchIndexType
        {
            get;
            protected set;
        }

        //public bool IncludeHitCount
        //{
        //    get;
        //    set;
        //}

        //public int TotalHits
        //{
        //    get;
        //    internal protected set;
        //}

        #endregion

        #region ISearch Members

        /// <summary>
        /// Query on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.Id(int id)
        {
            return IdInternal(id, _occurance);
        }

        public LuceneBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation Field(string fieldName, string fieldValue)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation OrderBy(params string[] fieldNames)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation OrderByDescending(params string[] fieldNames)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation All()
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation ManagedQuery(string query, string[] fields = null, IManagedQueryParameters parameters = null)
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation ManagedRangeQuery<T>(T? min, T? max, string[] fields, bool minInclusive = true, bool maxInclusive = true, IManagedQueryParameters parameters = null) where T : struct
        {
            throw new NotImplementedException();
        }

        public LuceneBooleanOperation Id(int id)
        {
            throw new NotImplementedException();
        }

        IBooleanOperation IQuery.Field<T>(string fieldName, T fieldValue)
        {
            return ManagedRangeQuery<T>(fieldValue, fieldValue, new[] {fieldName});
        }


        internal protected LuceneBooleanOperation IdInternal(int id, BooleanClause.Occur occurance)
        {
            //use a query parser (which uses the analyzer) to build up the field query which we want
            Query.Add(this.QueryParser.GetFieldQuery(LuceneIndexer.IndexNodeIdFieldName, id.ToString(CultureInfo.InvariantCulture)), occurance);

            return new LuceneBooleanOperation(this);
        }
        
        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.Field(string fieldName, string fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurance);
        }

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.Field(string fieldName, IExamineValue fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, fieldValue, _occurance);
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
            foreach (var val in txt.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries))
            {
                phraseQuery.Add(new Term(field, val));
            }
            return phraseQuery;
        }

        internal protected LuceneBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, BooleanClause.Occur occurance)
        {
            return FieldInternal(fieldName, fieldValue, occurance, true);
        }
        
        internal protected LuceneBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, BooleanClause.Occur occurance, bool useQueryParser)
        {
            Query queryToAdd = GetFieldInternalQuery(fieldName, fieldValue, useQueryParser);

            if (queryToAdd != null)
                Query.Add(queryToAdd, occurance);

            return new LuceneBooleanOperation(this);
        }
        
        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end)
        {
            return this.Range(fieldName, start, end, true, true);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <returns></returns>
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper)
        {
            return this.Range(fieldName, start, end, true, true, DateResolution.Millisecond);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        /// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        /// <param name="resolution">The resolution.</param>
        /// <returns></returns>
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper, DateResolution resolution)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return ManagedRangeQuery<DateTime>(start, end, new[] { fieldName }, includeLower, includeUpper);
        }

        public IBooleanOperation Range(string fieldName, int start, int end)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return ManagedRangeQuery<int>(start, end, new[] { fieldName });
        }

        
        public IBooleanOperation Range(string fieldName, int start, int end, bool includeLower, bool includeUpper)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return ManagedRangeQuery<int>(start, end, new[] { fieldName }, includeLower, includeUpper);
        }

        [Obsolete("This is no longer used, use ManagedRangeQuery instead")]
        protected internal IBooleanOperation RangeInternal(string fieldName, int start, int end, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(NumericRangeQuery.NewIntRange(fieldName, start, end, includeLower, includeUpper), occurance);
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, double lower, double upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        
        public IBooleanOperation Range(string fieldName, double lower, double upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurance);
        }

        
        protected internal IBooleanOperation RangeInternal(string fieldName, double lower, double upper, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(NumericRangeQuery.NewDoubleRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, float lower, float upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        
        public IBooleanOperation Range(string fieldName, float lower, float upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurance);
        }

        
        protected internal IBooleanOperation RangeInternal(string fieldName, float lower, float upper, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(NumericRangeQuery.NewFloatRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, long lower, long upper)
        {
            return this.Range(fieldName, lower, upper, true, true);
        }

        
        public IBooleanOperation Range(string fieldName, long lower, long upper, bool includeLower, bool includeUpper)
        {
            return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurance);
        }

        
        protected internal IBooleanOperation RangeInternal(string fieldName, long lower, long upper, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(NumericRangeQuery.NewLongRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Range(string fieldName, string start, string end)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(start, "start");
            Enforcer.ArgumentNotNull(end, "end");
            return this.Range(fieldName, start, end, true, true);
        }

        
        public IBooleanOperation Range(string fieldName, string start, string end, bool includeLower, bool includeUpper)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(start, "start");
            Enforcer.ArgumentNotNull(end, "end");
            return this.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurance);
        }

        
        protected internal IBooleanOperation RangeInternal(string fieldName, string start, string end, bool includeLower, bool includeUpper, BooleanClause.Occur occurance)
        {
            Query.Add(new TermRangeQuery(fieldName, start, end, includeLower, includeUpper), occurance);

            return new LuceneBooleanOperation(this);
        }

        IBooleanOperation IQuery.GroupedAnd(IEnumerable<string> fields, params string[] query)
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


        IBooleanOperation IQuery.GroupedAnd(IEnumerable<string> fields, IExamineValue[] fieldVals)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "fieldVals");

            return this.GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), _occurance);
        }


        protected internal LuceneBooleanOperation GroupedAndInternal(string[] fields, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {

            //if there's only 1 query text we want to build up a string like this:
            //(+field1:query +field2:query +field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, BooleanClause.Occur.MUST), occurance);

            return new LuceneBooleanOperation(this);
        }

        IBooleanOperation IQuery.GroupedOr(IEnumerable<string> fields, params string[] query)
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


        IBooleanOperation IQuery.GroupedOr(IEnumerable<string> fields, params IExamineValue[] fieldVals)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "query");

            return this.GroupedOrInternal(fields.ToArray(), fieldVals, _occurance);
        }


        protected internal LuceneBooleanOperation GroupedOrInternal(string[] fields, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, BooleanClause.Occur.SHOULD, true), occurance);

            return new LuceneBooleanOperation(this);
        }

        IBooleanOperation IQuery.GroupedNot(IEnumerable<string> fields, params string[] query)
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


        IBooleanOperation IQuery.GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            return this.GroupedNotInternal(fields.ToArray(), query, _occurance);
        }


        protected internal LuceneBooleanOperation GroupedNotInternal(string[] fields, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(!field1:query !field2:query !field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, BooleanClause.Occur.MUST_NOT), occurance);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Creates our own style 'multi field query' used internal for the grouped operations
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="fieldVals"></param>
        /// <param name="occurance"></param>
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
            BooleanClause.Occur occurance,
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
                            qry.Add(q, occurance);
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
                        qry.Add(q, occurance);
                    }
                }
            }

            return qry;
        }

        IBooleanOperation IQuery.GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
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


        IBooleanOperation IQuery.GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] fieldVals)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "query");
            Enforcer.ArgumentNotNull(operations, "operations");

            return this.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), fieldVals, _occurance);
        }


        protected internal LuceneBooleanOperation GroupedFlexibleInternal(string[] fields, BooleanOperation[] operations, IExamineValue[] fieldVals, BooleanClause.Occur occurance)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            var flags = new BooleanClause.Occur[operations.Count()];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = operations.ElementAt(i).ToLuceneOccurance();

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
            this.Query.Add(this.QueryParser.Parse(query), this._occurance);
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
                            bq.Add(q, BooleanClause.Occur.SHOULD);
                        }

                    }
                    return bq;
                }), _occurance);


            return new LuceneBooleanOperation(this);
        }

        IBooleanOperation IQuery.ManagedRangeQuery<T>(
            T? min, T? max, 
            string[] fields, 
            bool minInclusive = true, 
            bool maxInclusive = true, 
            IManagedQueryParameters parameters = null)
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
                            bq.Add(q, BooleanClause.Occur.SHOULD);
                        }
                    }
                }
                return bq;
            }), _occurance);


            return new LuceneBooleanOperation(this);
        }
        
        /// <summary>
        /// Orders the results by the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.OrderBy(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(false, fieldNames);
        }

        /// <summary>
        /// Orders the results by the specified fields in a descending order
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.OrderByDescending(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(true, fieldNames);
        }

        IBooleanOperation IQuery.All()
        {
            Query.Add(new MatchAllDocsQuery(), BooleanOperation.ToLuceneOccurance());

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
                        case "AUTO":
                            defaultSort = SortField.AUTO;
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



        #endregion

        public LuceneSearchCriteria WrapScoreQuery(Func<Query, ReaderDataScoreQuery> scoreQuery)
        {
            var newQuery = new BooleanQuery();
            newQuery.Add(scoreQuery(Queries.Pop()), BooleanClause.Occur.MUST);
            Queries.Push(newQuery);

            return this;
        }
    }
}
