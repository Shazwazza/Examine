using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Examine.LuceneEngine.Indexing;
using Examine.SearchCriteria;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// Defines the chainable query methods for the fluent search API for Lucene
    /// </summary>
    public class LuceneQuery : IQuery<LuceneBooleanOperation>
    {
        private readonly LuceneSearchCriteria _search;

        private readonly BooleanClause.Occur _occurrence;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneQuery"/> class.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <param name="occurrence">The occurrence.</param>		
        internal LuceneQuery(LuceneSearchCriteria search, BooleanClause.Occur occurrence)
        {
            this._search = search;
            this._occurrence = occurrence;
        }
        
        /// <summary>
        /// Gets the boolean operation which this query method will be added as
        /// </summary>
        /// <value>The boolean operation.</value>
        public BooleanOperation BooleanOperation
        {
			
            get { return _occurrence.ToBooleanOperation(); }
        }


        #region IQuery Members

         public LuceneBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
        {
            return ManagedRangeQuery<T>(fieldValue, fieldValue, new[] { "fieldName" });
        }

        public LuceneBooleanOperation Field(string fieldName, string fieldValue)
        {
            return this._search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);
        }

        public LuceneBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            return this._search.FieldInternal(fieldName, fieldValue, _occurrence);
        }

        public LuceneBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this._search.GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), this._occurrence);
        }

        public LuceneBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this._search.GroupedAndInternal(fields.ToArray(), query, this._occurrence);
        }

        public LuceneBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this._search.GroupedOrInternal(fields.ToArray(), fieldVals.ToArray(), this._occurrence);
        }

        public LuceneBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this._search.GroupedOrInternal(fields.ToArray(), query, this._occurrence);
        }

        public LuceneBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this._search.GroupedNotInternal(fields.ToArray(), fieldVals.ToArray(), this._occurrence);
        }

        public LuceneBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this._search.GroupedNotInternal(fields.ToArray(), query, this._occurrence);
        }

        public LuceneBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this._search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), fieldVals.ToArray(), _occurrence);
        }

        public LuceneBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            return this._search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, _occurrence);
        }

        public LuceneBooleanOperation OrderBy(params string[] fieldNames)
        {
            return this._search.OrderBy(fieldNames);
        }

        public LuceneBooleanOperation OrderByDescending(params string[] fieldNames)
        {
            return this._search.OrderByDescending(fieldNames);
        }

        public LuceneBooleanOperation All()
        {
            return _search.All();
        }

        public LuceneBooleanOperation ManagedQuery(string query, string[] fields = null, IManagedQueryParameters parameters = null)
        {
            return _search.ManagedQuery(query, fields, parameters);
        }

        public LuceneBooleanOperation ManagedRangeQuery<T>(T? min, T? max, string[] fields, bool minInclusive = true, bool maxInclusive = true, IManagedQueryParameters parameters = null) where T : struct
        {
            return _search.ManagedRangeQuery(min, max, fields, minInclusive: minInclusive, maxInclusive: maxInclusive, parameters: parameters);
        }

        /// <summary>
        /// Creates an inner group query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp">The default operation is OR, generally a grouped query would have complex inner queries with an OR against another complex group query</param>
        /// <returns></returns>
        public LuceneBooleanOperation Group(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = new LuceneBooleanOperation(_search);
            bo.Op(inner, defaultOp);
            return bo;
        }

        public LuceneBooleanOperation Id(int id)
        {
            return this._search.IdInternal(id, this._occurrence);
        }

        /// <summary>
        /// Query on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		
        IBooleanOperation IQuery.Id(int id)
        {
            return Id(id);
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
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
         IBooleanOperation IQuery.Range(string fieldName, DateTime start, DateTime end)
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
        /// <returns>
        /// A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended
        /// </returns>
        public IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool includeLower, bool includeUpper)
        {
            return this.Range(fieldName, start, end, includeLower, includeUpper, DateResolution.Millisecond);
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
            return this._search.Range(fieldName, start, end, includeLower, includeUpper);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation Range(string fieldName, int start, int end)
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
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		public IBooleanOperation Range(string fieldName, int start, int end, bool includeLower, bool includeUpper)
        {
            Enforcer.ArgumentNotNull(fieldName, "fieldName");
            return ManagedRangeQuery<int>(start, end, new[] { fieldName }, includeLower, includeUpper);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation Range(string fieldName, double start, double end)
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
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		
		public IBooleanOperation Range(string fieldName, double start, double end, bool includeLower, bool includeUpper)
        {
            return this._search.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurrence);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation Range(string fieldName, float start, float end)
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
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		
		public IBooleanOperation Range(string fieldName, float start, float end, bool includeLower, bool includeUpper)
        {            
            return this._search.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurrence);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation Range(string fieldName, long start, long end)
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
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		
		public IBooleanOperation Range(string fieldName, long start, long end, bool includeLower, bool includeUpper)
        {
            return this._search.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurrence);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation Range(string fieldName, string start, string end)
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
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		public IBooleanOperation Range(string fieldName, string start, string end, bool includeLower, bool includeUpper)
        {
            return this._search.RangeInternal(fieldName, start, end, includeLower, includeUpper, _occurrence);
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

        /// <summary>
        /// Queries multiple fields with each being an And boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            return GroupedAnd(fields, query);
        }

        /// <summary>
        /// Queries multiple fields with each being an And boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return GroupedAnd(fields, query);
        }

        /// <summary>
        /// Queries multiple fields with each being an Or boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            return GroupedOr(fields, query);
        }

        /// <summary>
        /// Queries multiple fields with each being an Or boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return GroupedOr(fields, query);
        }

        /// <summary>
        /// Queries multiple fields with each being an Not boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            return GroupedNot(fields, query);
        }

        /// <summary>
        /// Queries multiple fields with each being an Not boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return GroupedNot(fields, query);
        }

        /// <summary>
        /// Queries on multiple fields with their inclusions customly defined
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="operations">The operations.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            return GroupedFlexible(fields, operations, query);
        }

        /// <summary>
        /// Queries on multiple fields with their inclusions customly defined
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="operations">The operations.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        IBooleanOperation IQuery.GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            return GroupedFlexible(fields, operations, query);
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

        IBooleanOperation IQuery.ManagedQuery(string query, string[] fields = null, IManagedQueryParameters parameters = null)
        {
            return ManagedQuery(query, fields, parameters);
        }

        IBooleanOperation IQuery.ManagedRangeQuery<T>(T? min, T? max, string[] fields, bool minInclusive = true, bool maxInclusive = true, IManagedQueryParameters parameters = null)
        {
            return ManagedRangeQuery(min, max, fields, minInclusive, maxInclusive, parameters);
        }
        
        #endregion

    }
}
