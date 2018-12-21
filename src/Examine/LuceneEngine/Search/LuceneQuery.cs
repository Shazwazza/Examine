using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Search
{
    public class LuceneQuery : IQuery
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

        /// <inheritdoc />
        public BooleanOperation BooleanOperation => _occurrence.ToBooleanOperation();

        public IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
        {
            return RangeQuery<T>(new[] { fieldName }, fieldValue, fieldValue);
        }

        public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            return _search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);
        }

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            return _search.FieldInternal(fieldName, fieldValue, _occurrence);
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var f in query)
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            return _search.GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), _occurrence);
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return _search.GroupedAndInternal(fields.ToArray(), query, _occurrence);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var f in query)
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            return _search.GroupedOrInternal(fields.ToArray(), fieldVals.ToArray(), _occurrence);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return _search.GroupedOrInternal(fields.ToArray(), query, _occurrence);
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var f in query)
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            return _search.GroupedNotInternal(fields.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return _search.GroupedNotInternal(fields.ToArray(), query);
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var f in query)
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            return _search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), fieldVals.ToArray(), _occurrence);
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            return _search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, _occurrence);
        }

        public IBooleanOperation OrderBy(params SortableField[] fields)
        {
            return _search.OrderBy(fields);
        }

        public IBooleanOperation OrderByDescending(params SortableField[] fields)
        {
            return _search.OrderByDescending(fields);
        }

        public IOrdering All()
        {
            return _search.All();
        }

        public IBooleanOperation ManagedQuery(string query, string[] fields = null)
        {
            return _search.ManagedQuery(query, fields);
        }

        public IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct
        {
            return _search.RangeQuery(fields, min, max, minInclusive: minInclusive, maxInclusive: maxInclusive);
        }

        public string Category => _search.Category;

        public IBooleanOperation NativeQuery(string query)
        {
            return _search.NativeQuery(query);
        }

        /// <inheritdoc />
        public IBooleanOperation Group(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = new LuceneBooleanOperation(_search);
            bo.Op(inner, defaultOp);
            return bo;
        }

        public IBooleanOperation Id(string id)
        {
            return _search.IdInternal(id, _occurrence);
        }

    }
}
