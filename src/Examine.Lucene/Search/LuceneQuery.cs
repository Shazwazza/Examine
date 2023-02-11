using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
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

        public IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
            => RangeQuery<T>(new[] { fieldName }, fieldValue, fieldValue);

        public IBooleanOperation Field(string fieldName, string fieldValue)
            => _search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
            => _search.FieldInternal(fieldName, fieldValue, _occurrence);

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
            => _search.GroupedAndInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), _occurrence);

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedAndInternal(fields.ToArray(), query, _occurrence);

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
            => _search.GroupedOrInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), _occurrence);

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedOrInternal(fields.ToArray(), query, _occurrence);

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
            => _search.GroupedNotInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray());

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedNotInternal(fields.ToArray(), query);

        public IOrdering All() => _search.All();

        public IBooleanOperation ManagedQuery(string query, string[] fields = null) 
            => _search.ManagedQueryInternal(query, fields, _occurrence);

        public IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct
            => _search.RangeQueryInternal(fields, min, max, minInclusive: minInclusive, maxInclusive: maxInclusive, _occurrence);

        public string Category => _search.Category;

        public IBooleanOperation NativeQuery(string query) => _search.NativeQuery(query);

        /// <inheritdoc />
        public IBooleanOperation Group(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = new LuceneBooleanOperation(_search);
            bo.Op(inner, _occurrence.ToBooleanOperation(), defaultOp);
            return bo;
        }

        public IBooleanOperation Id(string id) => _search.IdInternal(id, _occurrence);

        INestedBooleanOperation INestedQuery.Field<T>(string fieldName, T fieldValue)
            => _search.RangeQueryInternal<T>(new[] { fieldName }, fieldValue, fieldValue, true, true, _occurrence);

        INestedBooleanOperation INestedQuery.Field(string fieldName, string fieldValue)
            => _search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);

        INestedBooleanOperation INestedQuery.Field(string fieldName, IExamineValue fieldValue)
            => _search.FieldInternal(fieldName, fieldValue, _occurrence);

        INestedBooleanOperation INestedQuery.GroupedAnd(IEnumerable<string> fields, params string[] query)
            => _search.GroupedAndInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), _occurrence);

        INestedBooleanOperation INestedQuery.GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedAndInternal(fields.ToArray(), query, _occurrence);

        INestedBooleanOperation INestedQuery.GroupedOr(IEnumerable<string> fields, params string[] query)
            => _search.GroupedOrInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray(), _occurrence);

        INestedBooleanOperation INestedQuery.GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedOrInternal(fields.ToArray(), query, _occurrence);

        INestedBooleanOperation INestedQuery.GroupedNot(IEnumerable<string> fields, params string[] query)
            => _search.GroupedNotInternal(fields.ToArray(), query.Select(f => new ExamineValue(Examineness.Explicit, f)).Cast<IExamineValue>().ToArray());

        INestedBooleanOperation INestedQuery.GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
            => _search.GroupedNotInternal(fields.ToArray(), query);

        INestedBooleanOperation INestedQuery.ManagedQuery(string query, string[] fields) 
            => _search.ManagedQueryInternal(query, fields, _occurrence);

        INestedBooleanOperation INestedQuery.RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive, bool maxInclusive)
            => _search.RangeQueryInternal(fields, min, max, minInclusive: minInclusive, maxInclusive: maxInclusive, _occurrence);

        public IBooleanOperation SpatialOperationQuery(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape)
            => _search.SpatialOperationQuery(field, spatialOperation, shape);
        public IBooleanOperation SpatialOperationQuery(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IEnumerable<IExamineSpatialShape>> shapes)
            => _search.SpatialOperationQuery(field, spatialOperation,shapes);
    }
}
