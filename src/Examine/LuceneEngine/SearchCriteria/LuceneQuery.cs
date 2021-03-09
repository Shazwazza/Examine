using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Examine.SearchCriteria;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.SearchCriteria
{
    public class LuceneQuery : IQuery, IFieldSelectableQuery
    {
        private LuceneSearchCriteria search;
        private BooleanClause.Occur occurance;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneQuery"/> class.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <param name="occurance">The occurance.</param>
		[SecuritySafeCritical]
        internal LuceneQuery(LuceneSearchCriteria search, BooleanClause.Occur occurance)
        {
            this.search = search;
            this.occurance = occurance;
        }

        /// <summary>
        /// Gets the boolean operation which this query method will be added as
        /// </summary>
        /// <value>The boolean operation.</value>
        public BooleanOperation BooleanOperation
        {
			[SecuritySafeCritical]
            get { return occurance.ToBooleanOperation(); }
        }


        #region ISearch Members

        /// <summary>
        /// Query on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
        public IBooleanOperation Id(int id)
        {
            return this.search.IdInternal(id, this.occurance);
        }

        /// <summary>
        /// Query on the NodeName
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
        public IBooleanOperation NodeName(string nodeName)
        {
            return this.search.NodeNameInternal(new ExamineValue(Examineness.Explicit, nodeName), occurance);
        }

        /// <summary>
        /// Query on the NodeTypeAlias
        /// </summary>
        /// <param name="nodeTypeAlias">The node type alias.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation NodeTypeAlias(string nodeTypeAlias)
        {
            return this.search.NodeTypeAliasInternal(new ExamineValue(Examineness.Explicit, nodeTypeAlias), occurance);
        }

        /// <summary>
        /// Query on the Parent ID
        /// </summary>
        /// <param name="id">The id of the parent.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation ParentId(int id)
        {
            return this.search.ParentIdInternal(id, occurance);
        }

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            return this.search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), occurance);
        }

        /// <summary>
        /// Ranges the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
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
            return this.search.Range(fieldName, start, end, includeLower, includeUpper, resolution);
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
		[SecuritySafeCritical]
		public IBooleanOperation Range(string fieldName, int start, int end, bool includeLower, bool includeUpper)
        {
            return this.search.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
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
		[SecuritySafeCritical]
		public IBooleanOperation Range(string fieldName, double start, double end, bool includeLower, bool includeUpper)
        {
            return this.search.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
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
		[SecuritySafeCritical]
		public IBooleanOperation Range(string fieldName, float start, float end, bool includeLower, bool includeUpper)
        {
            return this.search.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
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
		[SecuritySafeCritical]
		public IBooleanOperation Range(string fieldName, long start, long end, bool includeLower, bool includeUpper)
        {
            return this.search.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
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
		[SecuritySafeCritical]
		public IBooleanOperation Range(string fieldName, string start, string end, bool includeLower, bool includeUpper)
        {
            return this.search.RangeInternal(fieldName, start, end, includeLower, includeUpper, occurance);
        }

        /// <summary>
        /// Query on the NodeName
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation NodeName(IExamineValue nodeName)
        {
            return this.search.NodeNameInternal(nodeName, occurance);
        }

        /// <summary>
        /// Query on the NodeTypeAlias
        /// </summary>
        /// <param name="nodeTypeAlias">The node type alias.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation NodeTypeAlias(IExamineValue nodeTypeAlias)
        {
            return this.search.NodeTypeAliasInternal(nodeTypeAlias, occurance);
        }

        /// <summary>
        /// Query on the specified field
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            return this.search.FieldInternal(fieldName, fieldValue, occurance);
        }

        /// <summary>
        /// Queries multiple fields with each being an And boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this.search.GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), this.occurance);
        }

        /// <summary>
        /// Queries multiple fields with each being an And boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        [SecuritySafeCritical]
        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this.search.GroupedAndInternal(fields.ToArray(), query, this.occurance);
        }

        /// <summary>
        /// Queries multiple fields with each being an Or boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this.search.GroupedOrInternal(fields.ToArray(), fieldVals.ToArray(), this.occurance);
        }

        /// <summary>
        /// Queries multiple fields with each being an Or boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this.search.GroupedOrInternal(fields.ToArray(), query, this.occurance);
        }

        /// <summary>
        /// Queries multiple fields with each being an Not boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this.search.GroupedNotInternal(fields.ToArray(), fieldVals.ToArray());
        }

        /// <summary>
        /// Queries multiple fields with each being an Not boolean operation
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this.search.GroupedNotInternal(fields.ToArray(), query);
        }

        /// <summary>
        /// Queries on multiple fields with their inclusions customly defined
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="operations">The operations.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this.search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), fieldVals.ToArray(), occurance);
        }

        /// <summary>
        /// Queries on multiple fields with their inclusions customly defined
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="operations">The operations.</param>
        /// <param name="query">The query.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
		[SecuritySafeCritical]
		public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            return this.search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, occurance);
        }

        /// <summary>
        /// Orders the results by the specified fields
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation OrderBy(params string[] fieldNames)
        {
            return this.search.OrderBy(fieldNames);
        }

        /// <summary>
        /// Orders the results by the specified fields in a descending order
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>A new <see cref="Examine.SearchCriteria.IBooleanOperation"/> with the clause appended</returns>
        public IBooleanOperation OrderByDescending(params string[] fieldNames)
        {
            return this.search.OrderByDescending(fieldNames);
        }

        /// <summary>
        /// Return only the specified fields. Use <see cref="SelectFields(Hashtable)"></see> when possible as internally a new Hashtable is created on each call
        /// </summary>
        /// <remarks>The Id field will also be retrieved as it is a required field</remarks>
        /// <param name="fieldNames">The field names for fields to load</param>
        /// <returns></returns>
        public IBooleanOperation SelectFields(params string[] fieldNames)
        {
            return search.SelectFields(fieldNames);
        }

        /// <summary>
        /// Return only the specified field. Use <see cref="SelectFields(Hashtable)"></see> when possible as internally a new Hashtable is created on each call
        /// </summary>
        /// <remarks>The Id field will also be retrieved as it is a required field</remarks>
        /// <param name="fieldNames">The field name of the field to load</param>
        /// <returns></returns>
        public IBooleanOperation SelectField(string fieldName)
        {
            return search.SelectField(fieldName);
        }
        /// <summary>
        /// Return only the first field in the index
        /// </summary>
        /// <remarks>This should be the Id field as it should be first in the index</remarks>
        /// <returns></returns>
        public IBooleanOperation SelectFirstFieldOnly()
        {
            return search.SelectFirstFieldOnly();
        }

        /// <summary>
        /// Return all fields in the index
        /// </summary>
        /// <returns></returns>
        public IBooleanOperation SelectAllFields()
        {
            return search.SelectAllFields();
        }
        /// <summary>
        /// Return only the specified fields
        /// </summary>
        /// <remarks>The Id field will also be retrieved as it is a required field.</remarks>
        /// <param name="fieldNames">The field names for fields to load. Key should be the field name, value should be null</param>
        /// <returns></returns>
        public IBooleanOperation SelectFields(Hashtable fieldNames)
        {
            return search.SelectFields(fieldNames);
        }

        #endregion

    }
}
