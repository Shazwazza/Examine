using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a lucene query
    /// </summary>
    public class LuceneQuery : IQuery, INestedQuery
    {
        private readonly LuceneSearchQuery _search;

        private readonly Occur _occurrence;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneQuery"/> class.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <param name="occurrence">The occurance.</param>
        public LuceneQuery(LuceneSearchQuery search, Occur occurrence)
        {
            _search = search;
            _occurrence = occurrence;
        }

        /// <inheritdoc/>
        public IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
            => RangeQuery<T>(new[] { fieldName }, fieldValue, fieldValue);

        /// <inheritdoc/>
        public IBooleanOperation Field(string fieldName, string fieldValue)
            => _search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);

        /// <inheritdoc/>
        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
            => _search.FieldInternal(fieldName, fieldValue, _occurrence);

        /// <inheritdoc/>
        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
            => _search.GroupedAndInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), _occurrence);

        /// <inheritdoc/>
        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedAndInternal(fields.ToArray(), query, _occurrence);

        /// <inheritdoc/>
        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
            => _search.GroupedOrInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), _occurrence);

        /// <inheritdoc/>
        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedOrInternal(fields.ToArray(), query, _occurrence);

        /// <inheritdoc/>
        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
            => _search.GroupedNotInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray());

        /// <inheritdoc/>
        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedNotInternal(fields.ToArray(), query);

        /// <inheritdoc/>
        public IOrdering All() => _search.All();

        /// <inheritdoc/>
        public IBooleanOperation ManagedQuery(string query, string[]? fields = null) 
            => _search.ManagedQueryInternal(query, fields, _occurrence);

        /// <inheritdoc/>
        public IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct
            => _search.RangeQueryInternal(fields, min, max, minInclusive: minInclusive, maxInclusive: maxInclusive, _occurrence);

        /// <summary>
        /// The category of the query
        /// </summary>
        public string? Category => _search.Category;

        /// <inheritdoc/>
        public IBooleanOperation NativeQuery(string query) => _search.NativeQuery(query);

        /// <inheritdoc />
        public IBooleanOperation Group(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = new LuceneBooleanOperation(_search);
            bo.Op(inner, _occurrence.ToBooleanOperation(), defaultOp);
            return bo;
        }

        /// <inheritdoc/>
        public IBooleanOperation Id(string id) => _search.IdInternal(id, _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.Field<T>(string fieldName, T fieldValue)
            => _search.RangeQueryInternal<T>(new[] { fieldName }, fieldValue, fieldValue, true, true, _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.Field(string fieldName, string fieldValue)
            => _search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.Field(string fieldName, IExamineValue fieldValue)
            => _search.FieldInternal(fieldName, fieldValue, _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.GroupedAnd(IEnumerable<string> fields, params string[] query)
            => _search.GroupedAndInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedAndInternal(fields.ToArray(), query, _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.GroupedOr(IEnumerable<string> fields, params string[] query)
            => _search.GroupedOrInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedOrInternal(fields.ToArray(), query, _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.GroupedNot(IEnumerable<string> fields, params string[] query)
            => _search.GroupedNotInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray());

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedNotInternal(fields.ToArray(), query);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.ManagedQuery(string query, string[]? fields) 
            => _search.ManagedQueryInternal(query, fields, _occurrence);

        /// <inheritdoc/>
        INestedBooleanOperation INestedQuery.RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive, bool maxInclusive)
            => _search.RangeQueryInternal(fields, min, max, minInclusive: minInclusive, maxInclusive: maxInclusive, _occurrence);

        /// <inheritdoc/>
        public IQuery WithFilter(Action<IFilter> filter) => _search.WithFilter(filter);

    }
}
