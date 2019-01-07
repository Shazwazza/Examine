using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Version = Lucene.Net.Util.Version;

namespace Examine.LuceneEngine.Search
{
    /// <summary>
    /// This class is used to query against Lucene.Net
    /// </summary>
    [DebuggerDisplay("Category: {Category}, LuceneQuery: {Query}")]
    public class LuceneSearchQuery : IQuery, IQueryExecutor
    {
        private readonly ISearchContext _searchContext;
        private readonly string[] _fields;

        private readonly CustomMultiFieldQueryParser _queryParser;
        public QueryParser QueryParser => _queryParser;

        internal readonly Stack<BooleanQuery> Queries = new Stack<BooleanQuery>();
        internal BooleanQuery Query => Queries.Peek();
        internal readonly List<SortField> SortFields = new List<SortField>();
        
        private Occur _occurrence;
        private BooleanOperation _boolOp;

        private const Version LuceneVersion = Version.LUCENE_30;

        internal LuceneSearchQuery(
            ISearchContext searchContext,
            string type, Analyzer analyzer, string[] fields, LuceneSearchOptions searchOptions, BooleanOperation occurance)
        {
            _searchContext = searchContext ?? throw new ArgumentNullException(nameof(searchContext));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));

            Category = type;
            Queries.Push(new BooleanQuery());
            BooleanOperation = occurance;
            _queryParser = new CustomMultiFieldQueryParser(LuceneVersion, fields, analyzer);
            _queryParser.AllowLeadingWildcard = searchOptions.AllowLeadingWildcard;
        }

        /// <inheritdoc />
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
            return $"{{ Category: {Category}, LuceneQuery: {Query} }}";
        }

        #region ISearchCriteria Members
        
        /// <inheritdoc />
        public string Category { get; }
        
        #endregion

        #region ISearch Members

        /// <inheritdoc />
        public IBooleanOperation Group(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = new LuceneBooleanOperation(this);
            bo.Op(inner, defaultOp);
            return bo;
        }
        
        public IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
        {
            return RangeQuery<T>(new[] { fieldName }, fieldValue, fieldValue);
        }

        public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));
            return FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);
        }

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));
            return FieldInternal(fieldName, fieldValue, _occurrence);
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (query == null) throw new ArgumentNullException(nameof(query));

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return GroupedAnd(fields.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] fieldVals)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fieldVals == null) throw new ArgumentNullException(nameof(fieldVals));

            return GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), _occurrence);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (query == null) throw new ArgumentNullException(nameof(query));

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return GroupedOr(fields.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (query == null) throw new ArgumentNullException(nameof(query));

            return GroupedOrInternal(fields.ToArray(), query, _occurrence);
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (query == null) throw new ArgumentNullException(nameof(query));

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return GroupedNot(fields.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (query == null) throw new ArgumentNullException(nameof(query));

            return GroupedNotInternal(fields.ToArray(), query);
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (operations == null) throw new ArgumentNullException(nameof(operations));
            if (query == null) throw new ArgumentNullException(nameof(query));

            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }

            return GroupedFlexible(fields.ToArray(), operations.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (operations == null) throw new ArgumentNullException(nameof(operations));
            if (query == null) throw new ArgumentNullException(nameof(query));

            return GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, _occurrence);

        }

        public IBooleanOperation OrderBy(params SortableField[] fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            return OrderByInternal(false, fields);
        }

        public IBooleanOperation OrderByDescending(params SortableField[] fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            return OrderByInternal(true, fields);
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
                //if no fields are specified then use all fields
                fields = fields ?? _fields;

                var types = fields.Select(f => _searchContext.GetValueType(f)).Where(t => t != null);

                var bq = new BooleanQuery();
                foreach (var type in types)
                {
                    var q = type.GetQuery(query, _searchContext.Searcher);
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

        public IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct
        {
            Query.Add(new LateBoundQuery(() =>
            {
                var bq = new BooleanQuery();
                foreach (var f in fields)
                {
                    var valueType = _searchContext.GetValueType(f);
                    if (valueType is IIndexRangeValueType<T> type)
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
                        throw new InvalidOperationException($"Could not perform a range query on the field {f}, it's value type is {valueType?.GetType()}");
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

        /// <inheritdoc />
        public ISearchResults Execute(int maxResults = 500)
        {
            return Search(maxResults);
        }

        /// <inheritdoc />
        public IBooleanOperation NativeQuery(string query)
        {
            Query.Add(_queryParser.Parse(query), _occurrence);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Performs a search with a maximum number of results
        /// </summary>
        private ISearchResults Search(int maxResults = 500)
        {
            var searcher = _searchContext.Searcher;
            if (searcher == null) return EmptySearchResults.Instance;

            var pagesResults = new LuceneSearchResults(Query, SortFields, searcher, maxResults);
            return pagesResults;
        }

        /// <summary>
        /// Adds a true Lucene Query 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public LuceneBooleanOperation LuceneQuery(Query query, BooleanOperation? op = null)
        {
            Query.Add(query, (op ?? BooleanOperation).ToLuceneOccurrence());
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

            Query.Add(qry, occurance);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Internal operation for adding the ordered results
        /// </summary>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        /// <param name="fields">The field names.</param>
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
        private LuceneBooleanOperation OrderByInternal(bool descending, params SortableField[] fields)
        {
            foreach (var f in fields)
            {
                var fieldName = f.FieldName;

                var defaultSort = SortField.STRING;

                switch (f.SortType)
                {
                    case SortType.Score:
                        defaultSort = SortField.SCORE;
                        break;
                    case SortType.DocumentOrder:
                        defaultSort = SortField.DOC;
                        break;
                    case SortType.String:
                        defaultSort = SortField.STRING;
                        break;
                    case SortType.Int:
                        defaultSort = SortField.INT;
                        break;
                    case SortType.Float:
                        defaultSort = SortField.FLOAT;
                        break;
                    case SortType.Long:
                        defaultSort = SortField.LONG;
                        break;
                    case SortType.Double:
                        defaultSort = SortField.DOUBLE;
                        break;
                    case SortType.Short:
                        defaultSort = SortField.SHORT;
                        break;
                    case SortType.Byte:
                        defaultSort = SortField.BYTE;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                //get the sortable field name if this field type has one
                var valType = _searchContext.GetValueType(fieldName);
                if (valType?.SortableFieldName != null)
                    fieldName = valType.SortableFieldName;

                SortFields.Add(new SortField(fieldName, defaultSort, descending));
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
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
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
        private BooleanQuery GetMultiFieldQuery(
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

        protected internal LuceneBooleanOperation GroupedNotInternal(string[] fields, IExamineValue[] fieldVals)
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
            Query.Add(_queryParser.GetFieldQueryInternal(LuceneIndex.ItemIdFieldName, id), occurrence);

            return new LuceneBooleanOperation(this);
        }

        /// <summary>
        /// Returns the Lucene query object for a field given an IExamineValue
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="useQueryParser">True to use the query parser to parse the search text, otherwise, manually create the queries</param>
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
        private Query GetFieldInternalQuery(string fieldName, IExamineValue fieldValue, bool useQueryParser)
        {
            Query queryToAdd;

            switch (fieldValue.Examineness)
            {
                case Examineness.Fuzzy:
                    if (useQueryParser)
                    {
                        queryToAdd = _queryParser.GetFuzzyQueryInternal(fieldName, fieldValue.Value, fieldValue.Level);
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
                        queryToAdd = _queryParser.GetWildcardQueryInternal(fieldName, fieldValue.Value);
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
                        queryToAdd = _queryParser.GetFieldQueryInternal(fieldName, fieldValue.Value);
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
                        queryToAdd = _queryParser.GetFieldQueryInternal(fieldName, fieldValue.Value);
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
        private Query ParseRawQuery(string rawQuery)
        {
            var parser = new QueryParser(LuceneVersion, "", new KeywordAnalyzer());
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
        private static Query ParseRawQuery(string field, string txt)
        {
            var phraseQuery = new PhraseQuery {Slop = 0};
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

        private LuceneBooleanOperation FieldInternal(string fieldName, IExamineValue fieldValue, Occur occurrence, bool useQueryParser)
        {
            Query queryToAdd = GetFieldInternalQuery(fieldName, fieldValue, useQueryParser);

            if (queryToAdd != null)
                Query.Add(queryToAdd, occurrence);

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
