using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine.Indexing;
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
    /// This class is used to query against Lucene.Net
    /// </summary>
    [DebuggerDisplay("Category: {Category}, LuceneQuery: {Query}")]
    public class LuceneSearchCriteria : ISearchCriteria
    {
        private readonly BaseLuceneSearcher _searcher;
        private readonly ICriteriaContext _indexFieldValueTypes;
        internal static Regex SortMatchExpression = new Regex(@"(\[Type=(?<type>\w+?)\])", RegexOptions.Compiled);

        private readonly CustomMultiFieldQueryParser _queryParser;
        public QueryParser QueryParser => _queryParser;

        internal Stack<BooleanQuery> Queries = new Stack<BooleanQuery>();
        internal BooleanQuery Query => Queries.Peek();
        internal List<SortField> SortFields = new List<SortField>();
        
        private Occur _occurrence;
        private BooleanOperation _boolOp;

        private readonly Version _luceneVersion = Version.LUCENE_30;

		
        internal LuceneSearchCriteria(
            BaseLuceneSearcher searcher,
            ICriteriaContext indexFieldValueTypes,
            string type, Analyzer analyzer, string[] fields, LuceneSearchOptions searchOptions, BooleanOperation occurance)
        {   
            Enforcer.ArgumentNotNull(fields, "fields");

            _searcher = searcher;
            _indexFieldValueTypes = indexFieldValueTypes;

            Category = type;
            Queries.Push(new BooleanQuery());
            this.BooleanOperation = occurance;
            this._queryParser = new CustomMultiFieldQueryParser(_luceneVersion, fields, analyzer);
            this._queryParser.AllowLeadingWildcard = searchOptions.AllowLeadingWildcard;
        }

        /// <summary>
        /// Gets the boolean operation which this query method will be added as
        /// </summary>
        /// <value>The boolean operation.</value>
        public BooleanOperation BooleanOperation
        {
            get => _boolOp;
            protected internal set
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
            return $"{{ Category: {this.Category}, LuceneQuery: {Query} }}";
        }

        #region ISearchCriteria Members
        
        /// <summary>
        /// The index category
        /// </summary>
        /// <remarks>
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </remarks>
        public string Category
        {
            get;
            protected set;
        }

        public bool IncludeHitCount
        {
            get;
            set;
        }

        //public int TotalHits
        //{
        //    get;
        //    protected internal set;
        //}

        #endregion

        #region ISearch Members

        /// <summary>
        /// Creates an inner group query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp">The default operation is OR, generally a grouped query would have complex inner queries with an OR against another complex group query</param>
        /// <returns></returns>
        public IBooleanOperation Group(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = new LuceneBooleanOperation(this);
            bo.Op(inner, defaultOp);
            return bo;
        }
        
        public IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
        {
            return ManagedRangeQuery<T>(new[] { fieldName }, fieldValue, fieldValue);
        }

        public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);
        }

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            Enforcer.ArgumentNotNull(fieldValue, "fieldValue");
            return this.FieldInternal(fieldName, fieldValue, _occurrence);
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
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

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] fieldVals)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "fieldVals");

            return this.GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), _occurrence);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
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

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "query");

            return this.GroupedOrInternal(fields.ToArray(), query, _occurrence);
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
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

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(query, "query");

            return this.GroupedNotInternal(fields.ToArray(), query, _occurrence);
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
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

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            Enforcer.ArgumentNotNull(fields, "fields");
            Enforcer.ArgumentNotNull(Query, "query");
            Enforcer.ArgumentNotNull(operations, "operations");

            return this.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, _occurrence);

        }

        public IBooleanOperation OrderBy(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(false, fieldNames);
        }

        public IBooleanOperation OrderByDescending(params string[] fieldNames)
        {
            Enforcer.ArgumentNotNull(fieldNames, "fieldNames");

            return this.OrderByInternal(true, fieldNames);
        }

        public IBooleanOperation All()
        {
            Query.Add(new MatchAllDocsQuery(), BooleanOperation.ToLuceneOccurrence());

            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation ManagedQuery(string query, string[] fields = null)
        {
            Query.Add(new LateBoundQuery(() =>
            {
                var types = fields != null
                                ? fields.Select(f => _indexFieldValueTypes.GetValueType(f)).Where(t => t != null)
                                : _indexFieldValueTypes.ValueTypes;

                var bq = new BooleanQuery();
                foreach (var type in types)
                {
                    var q = type.GetQuery(query, _searcher.GetLuceneSearcher());
                    if (q != null)
                    {
                        //CriteriaContext.ManagedQueries.Add(new KeyValuePair<IIndexValueType, Query>(type, q));
                        bq.Add(q, Occur.SHOULD);
                    }

                }
                return bq;
            }), _occurrence);


            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation ManagedRangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct
        {
            Query.Add(new LateBoundQuery(() =>
            {
                var bq = new BooleanQuery();
                foreach (var f in fields)
                {
                    var type = _indexFieldValueTypes.GetValueType(f) as IIndexRangeValueType<T>;
                    if (type != null)
                    {
                        var q = type.GetQuery(min, max, minInclusive, maxInclusive);
                        if (q != null)
                        {
                            //CriteriaContext.FieldQueries.Add(new KeyValuePair<IIndexValueType, Query>(type, q));
                            bq.Add(q, Occur.SHOULD);
                        }
                    }
                    else
                    {
                        Trace.TraceError("Could not perform a range query on the field {0}, it's value type is {1}", f, _indexFieldValueTypes.GetValueType(f).GetType());
                    }
                }
                return bq;
            }), _occurrence);


            return new LuceneBooleanOperation(this);
        }

        public IBooleanOperation Id(string id)
        {
            return IdInternal(id, _occurrence);
        }

        ///// <summary>
        ///// Ranges the specified field name.
        ///// </summary>
        ///// <param name="fieldName">Name of the field.</param>
        ///// <param name="start">The start.</param>
        ///// <param name="end">The end.</param>
        ///// <returns></returns>
        //public IBooleanOperation Range(string fieldName, DateTime start, DateTime end)
        //{
        //    return this.Range(fieldName, start, end, true, true);
        //}

        ///// <summary>
        ///// Ranges the specified field name.
        ///// </summary>
        ///// <param name="fieldName">Name of the field.</param>
        ///// <param name="start">The start.</param>
        ///// <param name="end">The end.</param>
        ///// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        ///// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        ///// <returns></returns>
        //public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper)
        //{
        //    return this.Range(fieldName, start, end, true, true, DateResolution.Millisecond);
        //}

        ///// <summary>
        ///// Ranges the specified field name.
        ///// </summary>
        ///// <param name="fieldName">Name of the field.</param>
        ///// <param name="start">The start.</param>
        ///// <param name="end">The end.</param>
        ///// <param name="includeLower">if set to <c>true</c> [include lower].</param>
        ///// <param name="includeUpper">if set to <c>true</c> [include upper].</param>
        ///// <param name="resolution">The resolution.</param>
        ///// <returns></returns>

        //public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper, DateResolution resolution)
        //{
        //    //By specifying the resolution we can do more accurate range searching on date fields
        //    DateTools.Resolution luceneResolution;
        //    switch (resolution)
        //    {
        //        case DateResolution.Year:
        //            luceneResolution = DateTools.Resolution.YEAR;
        //            break;
        //        case DateResolution.Month:
        //            luceneResolution = DateTools.Resolution.MONTH;
        //            break;
        //        case DateResolution.Day:
        //            luceneResolution = DateTools.Resolution.DAY;
        //            break;
        //        case DateResolution.Hour:
        //            luceneResolution = DateTools.Resolution.HOUR;
        //            break;
        //        case DateResolution.Minute:
        //            luceneResolution = DateTools.Resolution.MINUTE;
        //            break;
        //        case DateResolution.Second:
        //            luceneResolution = DateTools.Resolution.SECOND;
        //            break;
        //        case DateResolution.Millisecond:
        //        default:
        //            luceneResolution = DateTools.Resolution.MILLISECOND;
        //            break;
        //    }
        //    //since lucene works on string's for all searching we need to flatten the date
        //    return this.RangeInternal(fieldName, DateTools.DateToString(start, luceneResolution), DateTools.DateToString(end, luceneResolution), includeLower, includeUpper, _occurrence);
        //}

        //public IBooleanOperation Range(string fieldName, int start, int end)
        //{
        //    Enforcer.ArgumentNotNull(fieldName, "fieldName");
        //    return this.Range(fieldName, start, end, true, true);
        //}


        //public IBooleanOperation Range(string fieldName, int start, int end, bool includeLower, bool includeUpper)
        //{
        //    return this.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurrence);
        //}

        //public IBooleanOperation Range(string fieldName, float lower, float upper)
        //{
        //    return this.Range(fieldName, lower, upper, true, true);
        //}


        //public IBooleanOperation Range(string fieldName, float lower, float upper, bool includeLower, bool includeUpper)
        //{
        //    return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurrence);
        //}

        //public IBooleanOperation Range(string fieldName, double lower, double upper)
        //{
        //    return this.Range(fieldName, lower, upper, true, true);
        //}


        //public IBooleanOperation Range(string fieldName, double lower, double upper, bool includeLower, bool includeUpper)
        //{
        //    return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurrence);
        //}

        //public IBooleanOperation Range(string fieldName, string start, string end)
        //{
        //    Enforcer.ArgumentNotNull(fieldName, "fieldName");
        //    Enforcer.ArgumentNotNull(start, "start");
        //    Enforcer.ArgumentNotNull(end, "end");
        //    return this.Range(fieldName, start, end, true, true);
        //}


        //public IBooleanOperation Range(string fieldName, string start, string end, bool includeLower, bool includeUpper)
        //{
        //    Enforcer.ArgumentNotNull(fieldName, "fieldName");
        //    Enforcer.ArgumentNotNull(start, "start");
        //    Enforcer.ArgumentNotNull(end, "end");
        //    return this.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurrence);
        //}

        //public IBooleanOperation Range(string fieldName, long lower, long upper)
        //{
        //    return this.Range(fieldName, lower, upper, true, true);
        //}


        //public IBooleanOperation Range(string fieldName, long lower, long upper, bool includeLower, bool includeUpper)
        //{
        //    return this.RangeInternal(fieldName, lower, upper, includeLower, includeUpper, _occurrence);
        //}

        /// <summary>
        /// Passes a raw search query to the provider to handle
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public ISearchCriteria RawQuery(string query)
        {
            this.Query.Add(this._queryParser.Parse(query), this._occurrence);
            return this;
        }
        
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

            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.MUST_NOT, true),
                //NOTE: This is important because we cannot prefix a + to a group of NOT's, that doesn't work. 
                // for example, it cannot be:  +(-id:1 -id:2 -id:3)
                // it just needs to be          (-id:1 -id:2 -id:3)
                Occur.SHOULD);

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


        protected internal IBooleanOperation IdInternal(string id, Occur occurrence)
        {
            //use a query parser (which uses the analyzer) to build up the field query which we want
            Query.Add(this._queryParser.GetFieldQueryInternal(LuceneIndexer.ItemIdFieldName, id), occurrence);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Returns the Lucene query object for a field given an IExamineValue
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="useQueryParser">True to use the query parser to parse the search text, otherwise, manually create the queries</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        protected internal Query GetFieldInternalQuery(string fieldName, IExamineValue fieldValue, bool useQueryParser)
        {
            Query queryToAdd;

            switch (fieldValue.Examineness)
            {
                case Examineness.Fuzzy:
                    if (useQueryParser)
                    {
                        queryToAdd = this._queryParser.GetFuzzyQueryInternal(fieldName, fieldValue.Value, fieldValue.Level);
                    }
                    else
                    {
                        //REFERENCE: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html#Fuzzy%20Searches
                        var proxQuery = fieldName + ":" + fieldValue.Value + "~" + Convert.ToInt32(fieldValue.Level);
                        queryToAdd = ParseRawQuery(proxQuery);
                    }
                    break;
                case Examineness.SimpleWildcard:
                case Examineness.ComplexWildcard:
                    if (useQueryParser)
                    {
                        queryToAdd = this._queryParser.GetWildcardQueryInternal(fieldName, fieldValue.Value);
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
                        queryToAdd = this._queryParser.GetFieldQueryInternal(fieldName, fieldValue.Value);
                        queryToAdd.Boost = fieldValue.Level;
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
                        queryToAdd = _queryParser.Parse(qry);
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
                        queryToAdd = this._queryParser.GetFieldQueryInternal(fieldName, fieldValue.Value);
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
        protected internal Query ParseRawQuery(string rawQuery)
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
            phraseQuery.Slop = 0; ;
            foreach (var val in txt.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                phraseQuery.Add(new Term(field, val));
            }
            return phraseQuery;
        }

        protected internal LuceneBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, Occur occurrence)
        {
            return FieldInternal(fieldName, fieldValue, occurrence, true);
        }

        protected internal LuceneBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, Occur occurrence, bool useQueryParser)
        {
            Query queryToAdd = GetFieldInternalQuery(fieldName, fieldValue, useQueryParser);

            if (queryToAdd != null)
                Query.Add(queryToAdd, occurrence);

            return new LuceneBooleanOperation(this);
        }

        //protected internal IBooleanOperation RangeInternal(string fieldName, int start, int end, bool includeLower, bool includeUpper, Occur occurance)
        //{
        //    Query.Add(NumericRangeQuery.NewIntRange(fieldName, start, end, includeLower, includeUpper), occurance);
        //    return new LuceneBooleanOperation(this);
        //}

		
        //protected internal IBooleanOperation RangeInternal(string fieldName, double lower, double upper, bool includeLower, bool includeUpper, Occur occurance)
        //{
        //    Query.Add(NumericRangeQuery.NewDoubleRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
        //    return new LuceneBooleanOperation(this);
        //}
		
        //protected internal IBooleanOperation RangeInternal(string fieldName, float lower, float upper, bool includeLower, bool includeUpper, Occur occurance)
        //{
        //    Query.Add(NumericRangeQuery.NewFloatRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
        //    return new LuceneBooleanOperation(this);
        //}

		
        //protected internal IBooleanOperation RangeInternal(string fieldName, long lower, long upper, bool includeLower, bool includeUpper, Occur occurance)
        //{
        //    Query.Add(NumericRangeQuery.NewLongRange(fieldName, lower, upper, includeLower, includeUpper), occurance);
        //    return new LuceneBooleanOperation(this);
        //}

        

		
        //protected internal IBooleanOperation RangeInternal(string fieldName, string start, string end, bool includeLower, bool includeUpper, Occur occurance)
        //{
        //    Query.Add(new TermRangeQuery(fieldName, start, end, includeLower, includeUpper), occurance);

        //    return new LuceneBooleanOperation(this);
        //}
        
        
        protected internal IBooleanOperation GroupedNotInternal(string[] fields, IExamineValue[] fieldVals)
        {
            //if there's only 1 query text we want to build up a string like this:
            //(!field1:query !field2:query !field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)
            
            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.MUST_NOT, true), 
                //NOTE: This is important because we cannot prefix a + to a group of NOT's, that doesn't work. 
                // for example, it cannot be:  +(-id:1 -id:2 -id:3)
                // it just needs to be          (-id:1 -id:2 -id:3)
                Occur.SHOULD);

            return new LuceneBooleanOperation(this);
        }


        #endregion

        /// <summary>
        /// We use this to get at the protected methods directly since the new version makes them not public
        /// </summary>
        private class CustomMultiFieldQueryParser : MultiFieldQueryParser
        {

            public CustomMultiFieldQueryParser(Version matchVersion, string[] fields, Analyzer analyzer) : base(matchVersion, fields, analyzer)
            {
            }

            public Query GetFuzzyQueryInternal(string field, string termStr, float minSimilarity)
            {
                return GetFuzzyQuery(field, termStr, minSimilarity);
            }

            public Query GetWildcardQueryInternal(string field, string termStr)
            {
                return GetWildcardQuery(field, termStr);
            }

            public Query GetFieldQueryInternal(string field, string queryText)
            {
                return GetFieldQuery(field, queryText);
            }
        }

    }
}
