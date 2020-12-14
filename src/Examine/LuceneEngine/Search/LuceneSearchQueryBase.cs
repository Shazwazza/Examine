using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Providers;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Version = Lucene.Net.Util.Version;

namespace Examine.LuceneEngine.Search
{
    public abstract class LuceneSearchQueryBase : IQuery, INestedQuery
    {
        private readonly CustomMultiFieldQueryParser _queryParser;
        public QueryParser QueryParser => _queryParser;

        internal readonly Stack<BooleanQuery> Queries = new Stack<BooleanQuery>();
        public BooleanQuery Query => Queries.Peek();

        public IList<SortField> SortFields { get; } = new List<SortField>();

        protected Occur Occurrence;
        private BooleanOperation _boolOp;

        public const Version LuceneVersion = Version.LUCENE_30;

        protected internal FieldSelector Selector = null;
        private static readonly ISet<string> EmptySet = new HashSet<string>();

        protected LuceneSearchQueryBase(CustomMultiFieldQueryParser queryParser,
            string category, string[] fields, LuceneSearchOptions searchOptions, BooleanOperation occurance)
        {
            Category = category;
            AllFields = fields ?? throw new ArgumentNullException(nameof(fields));
            SearchOptions = searchOptions;
            Queries.Push(new BooleanQuery());
            BooleanOperation = occurance;
            _queryParser = queryParser;
        }

        [Obsolete("Use the ctor specifying a query parser instead")]
        protected LuceneSearchQueryBase(
            string category, Analyzer analyzer, string[] fields, LuceneSearchOptions searchOptions, BooleanOperation occurance)
        {
            Category = category;
            AllFields = fields ?? throw new ArgumentNullException(nameof(fields));
            SearchOptions = searchOptions;
            Queries.Push(new BooleanQuery());
            BooleanOperation = occurance;
            _queryParser = new CustomMultiFieldQueryParser(LuceneVersion, fields, analyzer);
            _queryParser.AllowLeadingWildcard = searchOptions.AllowLeadingWildcard;
        }

        protected abstract LuceneBooleanOperationBase CreateOp();

        public BooleanOperation BooleanOperation
        {
            get => _boolOp;
            set
            {
                _boolOp = value;
                Occurrence = _boolOp.ToLuceneOccurrence();
            }
        }

        public string Category { get; }

        public string[] AllFields { get; }
        public LuceneSearchOptions SearchOptions { get; }

        /// <inheritdoc />
        public IBooleanOperation Group(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = CreateOp();
            bo.Op(inner, BooleanOperation, defaultOp);
            return bo;
        }

        public IOrdering All()
        {
            Query.Add(new MatchAllDocsQuery(), BooleanOperation.ToLuceneOccurrence());
            return CreateOp();
        }

        /// <inheritdoc />
        public IBooleanOperation NativeQuery(string query)
        {
            Query.Add(_queryParser.Parse(query), Occurrence);
            return CreateOp();
        }

        /// <summary>
        /// Adds a true Lucene Query 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public LuceneBooleanOperationBase LuceneQuery(Query query, BooleanOperation? op = null)
        {
            Query.Add(query, (op ?? BooleanOperation).ToLuceneOccurrence());
            return CreateOp();
        }

        public IBooleanOperation Id(string id)
        {
            return IdInternal(id, Occurrence);
        }

        public abstract IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct;
        public abstract IBooleanOperation ManagedQuery(string query, string[] fields = null);
        public abstract IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct;

        public IBooleanOperation Field(string fieldName, string fieldValue)
            => FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), Occurrence);

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
            => FieldInternal(fieldName, fieldValue, Occurrence);

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
            => GroupedAnd(fields, query?.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray());

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] fieldVals)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fieldVals == null) throw new ArgumentNullException(nameof(fieldVals));

            return GroupedAndInternal(fields.ToArray(), fieldVals, Occurrence);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
            => GroupedOr(fields, query?.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray());

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (query == null) throw new ArgumentNullException(nameof(query));

            return GroupedOrInternal(fields.ToArray(), query, Occurrence);
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
            => GroupedNot(fields, query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray());

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (query == null) throw new ArgumentNullException(nameof(query));

            return GroupedNotInternal(fields.ToArray(), query);
        }

        #region INested

        private static readonly string[] EmptyStringArray = new string[0];

        protected abstract INestedBooleanOperation FieldNested<T>(string fieldName, T fieldValue) where T : struct;
        protected abstract INestedBooleanOperation ManagedQueryNested(string query, string[] fields = null);
        protected abstract INestedBooleanOperation RangeQueryNested<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct;

        INestedBooleanOperation INestedQuery.Field(string fieldName, string fieldValue)
            => FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), Occurrence);

        INestedBooleanOperation INestedQuery.Field(string fieldName, IExamineValue fieldValue)
            => FieldInternal(fieldName, fieldValue, Occurrence);

        INestedBooleanOperation INestedQuery.GroupedAnd(IEnumerable<string> fields, params string[] query)
            => GroupedAndInternal(fields == null ? EmptyStringArray : fields.ToArray(), query?.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), Occurrence);

        INestedBooleanOperation INestedQuery.GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
            => GroupedAndInternal(fields == null ? EmptyStringArray : fields.ToArray(), query, Occurrence);

        INestedBooleanOperation INestedQuery.GroupedOr(IEnumerable<string> fields, params string[] query)
            => GroupedOrInternal(fields == null ? EmptyStringArray : fields.ToArray(), query?.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), Occurrence);

        INestedBooleanOperation INestedQuery.GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
            => GroupedOrInternal(fields == null ? EmptyStringArray : fields.ToArray(), query, Occurrence);

        INestedBooleanOperation INestedQuery.GroupedNot(IEnumerable<string> fields, params string[] query)
            => GroupedNotInternal(fields == null ? EmptyStringArray : fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray());

        INestedBooleanOperation INestedQuery.GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
            => GroupedNotInternal(fields == null ? EmptyStringArray : fields.ToArray(), query);

        INestedBooleanOperation INestedQuery.ManagedQuery(string query, string[] fields) => ManagedQueryNested(query, fields);

        INestedBooleanOperation INestedQuery.RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive, bool maxInclusive)
            => RangeQueryNested(fields, min, max, minInclusive, maxInclusive);

        INestedBooleanOperation INestedQuery.Field<T>(string fieldName, T fieldValue) => FieldNested(fieldName, fieldValue);

        #endregion

        #region Internal

        protected internal LuceneBooleanOperationBase FieldInternal(string fieldName, IExamineValue fieldValue, Occur occurrence)
        {
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));
            return FieldInternal(fieldName, fieldValue, occurrence, true);
        }

        private LuceneBooleanOperationBase FieldInternal(string fieldName, IExamineValue fieldValue, Occur occurrence, bool useQueryParser)
        {
            Query queryToAdd = GetFieldInternalQuery(fieldName, fieldValue, useQueryParser);

            if (queryToAdd != null)
                Query.Add(queryToAdd, occurrence);

            return CreateOp();
        }

        protected internal LuceneBooleanOperationBase GroupedAndInternal(string[] fields, IExamineValue[] fieldVals, Occur occurrence)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fieldVals == null) throw new ArgumentNullException(nameof(fieldVals));

            //if there's only 1 query text we want to build up a string like this:
            //(+field1:query +field2:query +field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.MUST), occurrence);

            return CreateOp();
        }

        protected internal LuceneBooleanOperationBase GroupedNotInternal(string[] fields, IExamineValue[] fieldVals)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fieldVals == null) throw new ArgumentNullException(nameof(fieldVals));

            // if there's only one field and one value then deal with this like a normal And().Not()
            if (fields.Length == 1 && fieldVals.Length == 1)
            {
                FieldInternal(fields[0], fieldVals[0], Occur.MUST_NOT);
                return CreateOp();
            }

            //if there's only 1 query text we want to build up a string like this:
            //(!field1:query !field2:query !field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            // NOTE: This is important because we cannot prefix a + to a group of NOT's, that doesn't work. 
            // for example, it cannot be:  +(-id:1 -id:2 -id:3) 
            // and it cannot be:            (-id:1 -id:2 -id:3) - this will be an optional list of must not's so really nothing is filtered
            // It needs to be:              -id:1 -id:2 -id:3

            // So we get all clauses 
            var subQueries = GetMultiFieldQuery(fields, fieldVals, Occur.MUST_NOT, true);

            // then add each individual one directly to the query
            foreach (var c in subQueries.Clauses)
            {
                Query.Add(c);
            }

            return CreateOp();
        }

        protected internal LuceneBooleanOperationBase GroupedOrInternal(string[] fields, IExamineValue[] fieldVals, Occur occurrence)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fieldVals == null) throw new ArgumentNullException(nameof(fieldVals));

            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.SHOULD, true), occurrence);

            return CreateOp();
        }

        protected internal LuceneBooleanOperationBase IdInternal(string id, Occur occurrence)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            //use a query parser (which uses the analyzer) to build up the field query which we want
            Query.Add(_queryParser.GetFieldQueryInternal(LuceneIndex.ItemIdFieldName, id), occurrence);

            return CreateOp();
        }

        #endregion

        /// <summary>
        /// Returns the Lucene query object for a field given an IExamineValue
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="useQueryParser">True to use the query parser to parse the search text, otherwise, manually create the queries</param>
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
        protected virtual Query GetFieldInternalQuery(string fieldName, IExamineValue fieldValue, bool useQueryParser)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty", nameof(fieldName));
            if (fieldValue is null) throw new ArgumentNullException(nameof(fieldValue));
            if (string.IsNullOrEmpty(fieldValue.Value)) throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty", nameof(fieldName));

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

                    var searchValue = QueryParser.Escape(fieldValue.Value) + (fieldValue.Examineness == Examineness.ComplexWildcard ? "*" : "?");

                    if (useQueryParser)
                    {
                        queryToAdd = _queryParser.GetWildcardQueryInternal(fieldName, searchValue);
                    }
                    else
                    {
                        //REFERENCE: http://lucene.apache.org/java/2_4_0/queryparsersyntax.html#Wildcard%20Searches
                        var proxQuery = fieldName + ":" + searchValue;
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
                    //var stdQuery = fieldName + ":" + fieldValue.Value;

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
            var phraseQuery = new PhraseQuery { Slop = 0 };
            foreach (var val in txt.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                phraseQuery.Add(new Term(field, val));
            }
            return phraseQuery;
        }

        /// <summary>
        /// Creates our own style 'multi field query' used internal for the grouped operations
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="fieldVals"></param>
        /// <param name="occurance"></param>
        /// <param name="matchAllCombinations">If true will match all combinations, if not will only match the values corresponding with fields</param>
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
        /// <remarks>
        ///
        /// docs about this are here: https://github.com/Shazwazza/Examine/wiki/Grouped-Operations
        /// 
        /// if matchAllCombinations == false then...
        /// this will create a query that matches the field index to the value index if the value length is >= to the field length
        /// otherwise we will have to match all combinations.
        /// 
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
            IReadOnlyList<string> fields,
            IReadOnlyList<IExamineValue> fieldVals,
            Occur occurance,
            bool matchAllCombinations = false)
        {

            var qry = new BooleanQuery();

            //if there's only one field defined then we will match all combinations
            //if matchAllCombinations is explicitly specified, or there's no way that the key/value pairs can be aligned,
            //we will have to match all combinations
            if (fields.Count == 1 || matchAllCombinations || fieldVals.Count < fields.Count)
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
                return qry;
            }

            //This will align the key value pairs:            
            for (int i = 0; i < fields.Count; i++)
            {
                var queryVal = fieldVals[i];
                var q = GetFieldInternalQuery(fields[i], queryVal, true);
                if (q != null)
                {
                    qry.Add(q, occurance);
                }
            }

            return qry;
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


    }
}