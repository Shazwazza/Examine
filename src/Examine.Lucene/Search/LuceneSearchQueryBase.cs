using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Examine.Search;
using Lucene.Net.Facet.Range;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a base for <see cref="LuceneSearchQuery"/>
    /// </summary>
    public abstract class LuceneSearchQueryBase : IQuery, INestedQuery
    {
        private readonly CustomMultiFieldQueryParser _queryParser;

        /// <summary>
        /// The query parser of the query
        /// </summary>
        public QueryParser QueryParser => _queryParser;

        internal Stack<BooleanQuery> Queries { get; } = new Stack<BooleanQuery>();

        /// <summary>
        /// The <see cref="BooleanQuery"/>
        /// </summary>
        public BooleanQuery Query => Queries.Peek();

        /// <summary>
        /// The sort fields of the query
        /// </summary>
        public IList<SortField> SortFields { get; } = new List<SortField>();

        /// <summary>
        /// Specifies how clauses are to occur in matching documents
        /// </summary>
        protected Occur Occurrence { get; set; }

        /// <inheritdoc/>
        protected LuceneSearchQueryBase(CustomMultiFieldQueryParser queryParser,
            string? category, LuceneSearchOptions searchOptions, BooleanOperation occurance)
        {
            Category = category;
            SearchOptions = searchOptions;
            Queries.Push(new BooleanQuery());
            BooleanOperation = occurance;
            _queryParser = queryParser;
        }

        /// <summary>
        /// Creates a <see cref="LuceneBooleanOperationBase"/>
        /// </summary>
        /// <returns></returns>
        protected abstract LuceneBooleanOperationBase CreateOp();

        /// <summary>
        /// The type of boolean operation
        /// </summary>
        public BooleanOperation BooleanOperation
        {
            get;
            set
            {
                field = value;
                Occurrence = field.ToLuceneOccurrence();
            }
        }

        /// <summary>
        /// The category of the query
        /// </summary>
        public string? Category { get; }

        /// <summary>
        /// All the searchable fields of the query
        /// </summary>
        public string[] AllFields => _queryParser.SearchableFields;

        /// <summary>
        /// The query search options
        /// </summary>
        public LuceneSearchOptions SearchOptions { get; }

        /// <inheritdoc />
        public IBooleanOperation Group(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = CreateOp();
            bo.Op(inner, BooleanOperation, defaultOp);
            return bo;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IBooleanOperation Id(string id) => IdInternal(id, Occurrence);

        /// <inheritdoc/>
        public abstract IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct;

        /// <inheritdoc/>
        public abstract IBooleanOperation ManagedQuery(string query, string[]? fields = null);

        /// <inheritdoc/>
        public abstract IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct;

        /// <inheritdoc/>
        public IBooleanOperation Field(string fieldName, string fieldValue)
            => FieldInternal(fieldName, ExamineValue.Create(Examineness.Default, fieldValue), Occurrence);

        /// <inheritdoc/>
        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
            => FieldInternal(fieldName, fieldValue, Occurrence);

        /// <inheritdoc/>
        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
            => GroupedAnd(fields, query.Select(f => ExamineValue.Create(Examineness.Default, f)).Cast<IExamineValue>().ToArray());

        /// <inheritdoc/>
        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[]? fieldVals)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (fieldVals == null)
            {
                throw new ArgumentNullException(nameof(fieldVals));
            }

            return GroupedAndInternal(fields.ToArray(), fieldVals, Occurrence);
        }

        /// <inheritdoc/>
        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
            => GroupedOr(fields, query?.Select(f => ExamineValue.Create(Examineness.Default, f)).Cast<IExamineValue>().ToArray());

        /// <inheritdoc/>
        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[]? query)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return GroupedOrInternal(fields.ToArray(), query, Occurrence);
        }

        /// <inheritdoc/>
        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
            => GroupedNot(fields, query.Select(f => ExamineValue.Create(Examineness.Default, f)).Cast<IExamineValue>().ToArray());

        /// <inheritdoc/>
        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return GroupedNotInternal(fields.ToArray(), query);
        }

        #region INested

        private static readonly string[] EmptyStringArray = new string[0];

        /// <summary>
        /// Query on a specific field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        protected abstract INestedBooleanOperation FieldNested<T>(string fieldName, T fieldValue) where T : struct;

        /// <summary>
        /// The index will determine the most appropiate way to query the fields specified
        /// </summary>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        protected abstract INestedBooleanOperation ManagedQueryNested(string query, string[]? fields = null);

        /// <summary>
        /// Matches items as defined by the IIndexFieldValueType used for the fields specified. 
        /// If a type is not defined for a field name, or the type does not implement IIndexRangeValueType for the types of min and max, nothing will be added
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        protected abstract INestedBooleanOperation RangeQueryNested<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct;

        INestedBooleanOperation INestedQuery.Field(string fieldName, string fieldValue)
            => FieldInternal(fieldName, ExamineValue.Create(Examineness.Default, fieldValue), Occurrence);

        INestedBooleanOperation INestedQuery.Field(string fieldName, IExamineValue fieldValue)
            => FieldInternal(fieldName, fieldValue, Occurrence);

        INestedBooleanOperation INestedQuery.GroupedAnd(IEnumerable<string> fields, params string[] query)
            => GroupedAndInternal(fields == null ? EmptyStringArray : fields.ToArray(), query?.Select(f => ExamineValue.Create(Examineness.Default, f)).Cast<IExamineValue>().ToArray(), Occurrence);

        INestedBooleanOperation INestedQuery.GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
            => GroupedAndInternal(fields == null ? EmptyStringArray : fields.ToArray(), query, Occurrence);

        INestedBooleanOperation INestedQuery.GroupedOr(IEnumerable<string> fields, params string[] query)
            => GroupedOrInternal(fields == null ? EmptyStringArray : fields.ToArray(), query?.Select(f => ExamineValue.Create(Examineness.Default, f)).Cast<IExamineValue>().ToArray(), Occurrence);

        INestedBooleanOperation INestedQuery.GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
            => GroupedOrInternal(fields == null ? EmptyStringArray : fields.ToArray(), query, Occurrence);

        INestedBooleanOperation INestedQuery.GroupedNot(IEnumerable<string> fields, params string[] query)
            => GroupedNotInternal(fields == null ? EmptyStringArray : fields.ToArray(), query.Select(f => ExamineValue.Create(Examineness.Default, f)).Cast<IExamineValue>().ToArray());

        INestedBooleanOperation INestedQuery.GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
            => GroupedNotInternal(fields == null ? EmptyStringArray : fields.ToArray(), query);

        INestedBooleanOperation INestedQuery.ManagedQuery(string query, string[]? fields) => ManagedQueryNested(query, fields);

        INestedBooleanOperation INestedQuery.RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive, bool maxInclusive)
            => RangeQueryNested(fields, min, max, minInclusive, maxInclusive);

        INestedBooleanOperation INestedQuery.Field<T>(string fieldName, T fieldValue) => FieldNested(fieldName, fieldValue);

        #endregion

        #region Internal

        /// <inheritdoc/>
        protected internal LuceneBooleanOperationBase FieldInternal(string fieldName, IExamineValue fieldValue, Occur occurrence)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }

            if (fieldValue == null)
            {
                throw new ArgumentNullException(nameof(fieldValue));
            }

            var queryToAdd = GetFieldInternalQuery(fieldName, fieldValue);

            if (queryToAdd != null)
            {
                Query.Add(queryToAdd, occurrence);
            }

            return CreateOp();
        }

        /// <inheritdoc/>
        protected internal LuceneBooleanOperationBase GroupedAndInternal(string[] fields, IExamineValue[]? fieldVals, Occur occurrence)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (fieldVals == null)
            {
                throw new ArgumentNullException(nameof(fieldVals));
            }

            //if there's only 1 query text we want to build up a string like this:
            //(+field1:query +field2:query +field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.MUST), occurrence);

            return CreateOp();
        }

        /// <inheritdoc/>
        protected internal LuceneBooleanOperationBase GroupedNotInternal(string[] fields, IExamineValue[] fieldVals)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (fieldVals == null)
            {
                throw new ArgumentNullException(nameof(fieldVals));
            }

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

        /// <inheritdoc/>
        protected internal LuceneBooleanOperationBase GroupedOrInternal(string[] fields, IExamineValue[]? fieldVals, Occur occurrence)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (fieldVals == null)
            {
                throw new ArgumentNullException(nameof(fieldVals));
            }

            //if there's only 1 query text we want to build up a string like this:
            //(field1:query field2:query field3:query)
            //but Lucene will bork if you provide an array of length 1 (which is != to the field length)

            Query.Add(GetMultiFieldQuery(fields, fieldVals, Occur.SHOULD, true), occurrence);

            return CreateOp();
        }

        /// <inheritdoc/>
        protected internal LuceneBooleanOperationBase IdInternal(string id, Occur occurrence)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            //use a query parser (which uses the analyzer) to build up the field query which we want
            Query.Add(_queryParser.GetFieldQueryInternal(ExamineFieldNames.ItemIdFieldName, id), occurrence);

            return CreateOp();
        }

        #endregion

        /// <summary>
        /// Returns the Lucene query object for a field given an IExamineValue
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
        protected virtual Query? GetFieldInternalQuery(string fieldName, IExamineValue fieldValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty", nameof(fieldName));
            }

            if (fieldValue is null)
            {
                throw new ArgumentNullException(nameof(fieldValue));
            }

            if (string.IsNullOrEmpty(fieldValue.Value))
            {
                throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty", nameof(fieldName));
            }

            Query? queryToAdd;
            switch (fieldValue.Examineness)
            {
                case Examineness.Fuzzy:
                    queryToAdd = _queryParser.GetFuzzyQueryInternal(fieldName, fieldValue.Value, fieldValue.Level);
                    break;
                case Examineness.SimpleWildcard:
                case Examineness.ComplexWildcard:
                    var searchValue = fieldValue.Value + (fieldValue.Examineness == Examineness.ComplexWildcard ? "*" : "?");
                    queryToAdd = _queryParser.GetWildcardQueryInternal(fieldName, searchValue);
                    break;
                case Examineness.Proximity:
                    int proximity = Convert.ToInt32(fieldValue.Level);
                    queryToAdd = _queryParser.GetProximityQueryInternal(fieldName, fieldValue.Value, proximity);
                    break;
#pragma warning disable CS0618 // Type or member is obsolete
                case Examineness.Escaped:
#pragma warning restore CS0618 // Type or member is obsolete
                case Examineness.Phrase:
                    queryToAdd = _queryParser.GetPhraseQueryInternal(fieldName, fieldValue.Value);
                    break;
#pragma warning disable CS0618 // Type or member is obsolete
                case Examineness.Explicit:
#pragma warning restore CS0618 // Type or member is obsolete
                case Examineness.Default:
                default:
                    queryToAdd = _queryParser.GetFieldQueryInternal(fieldName, fieldValue.Value);
                    break;
            }

            if (fieldValue is IExamineValueBoosted boostedValue)
            {
                queryToAdd.Boost = boostedValue.Boost;
            }

            return queryToAdd;
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
            string[] fields,
            IExamineValue[] fieldVals,
            Occur occurance,
            bool matchAllCombinations = false)
        {

            var qry = new BooleanQuery();

            //if there's only one field defined then we will match all combinations
            //if matchAllCombinations is explicitly specified, or there's no way that the key/value pairs can be aligned,
            //we will have to match all combinations
            if (fields.Length == 1 || matchAllCombinations || fieldVals.Length < fields.Length)
            {
                foreach (var f in fields)
                {
                    foreach (var val in fieldVals)
                    {
                        var q = GetFieldInternalQuery(f, val);
                        if (q != null)
                        {
                            qry.Add(q, occurance);
                        }
                    }
                }
                return qry;
            }

            //This will align the key value pairs:            
            for (int i = 0; i < fields.Length; i++)
            {
                var queryVal = fieldVals[i];
                var q = GetFieldInternalQuery(fields[i], queryVal);
                if (q != null)
                {
                    qry.Add(q, occurance);
                }
            }

            return qry;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString() => $"{{ Category: {Category}, LuceneQuery: {Query} }}";
    }
}
