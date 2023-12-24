using System;
using System.Collections.Generic;
using Examine.Search;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Filtering Operation
    /// </summary>
    public abstract class LuceneSearchFilteringOperationBase : IFilter, INestedFilter
    {
        internal Stack<BooleanFilter> Filters => _luceneSearchQueryBase.Filters;

        /// <summary>
        /// The <see cref="BooleanFilter"/>
        /// </summary>
        internal BooleanFilter Filter => _luceneSearchQueryBase.Filters.Peek();

        private BooleanOperation _boolFilterOp;
        private readonly LuceneSearchQueryBase _luceneSearchQueryBase;

        /// <summary>
        /// Specifies how clauses are to occur in matching documents
        /// </summary>
        protected Occur Occurrence { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="luceneSearchQueryBase"></param>
        public LuceneSearchFilteringOperationBase(LuceneSearchQueryBase luceneSearchQueryBase)
        {
            _boolFilterOp = BooleanOperation.And;
            _luceneSearchQueryBase = luceneSearchQueryBase;
        }

        /// <summary>
        /// The type of boolean operation
        /// </summary>
        public BooleanOperation BooleanFilterOperation
        {
            get => _boolFilterOp;
            set
            {
                _boolFilterOp = value;
                Occurrence = _boolFilterOp.ToLuceneOccurrence();
            }
        }

        /// <summary>
        /// Adds a true Lucene Filter 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public LuceneFilteringBooleanOperationBase LuceneFilter(Filter filter, BooleanOperation? op = null)
        {
            Filter.Add(filter, (op ?? BooleanFilterOperation).ToLuceneOccurrence());
            return CreateBooleanOp();
        }


        /// <summary>
        /// Creates a <see cref="LuceneFilteringBooleanOperationBase"/>
        /// </summary>
        /// <returns></returns>
        protected abstract LuceneFilteringBooleanOperationBase CreateBooleanOp();

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation TermFilter(FilterTerm term);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation TermPrefixFilter(FilterTerm term);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation FieldValueExistsFilter(string field);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation FieldValueNotExistsFilter(string field);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And);

        /// <inheritdoc/>
        protected abstract INestedBooleanFilterOperation NestedTermFilter(FilterTerm term);

        /// <inheritdoc/>
        protected abstract INestedBooleanFilterOperation NestedTermsFilter(IEnumerable<FilterTerm> terms);

        /// <inheritdoc/>
        protected abstract INestedBooleanFilterOperation NestedTermPrefixFilter(FilterTerm term);

        /// <inheritdoc/>
        protected abstract INestedBooleanFilterOperation NestedFieldValueExistsFilter(string field);

        /// <inheritdoc/>
        protected abstract INestedBooleanFilterOperation NestedFieldValueNotExistsFilter(string field);

        /// <inheritdoc/>
        protected abstract INestedBooleanFilterOperation NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp);

        /// <inheritdoc/>
        protected abstract INestedBooleanFilterOperation NestedSpatialOperationFilter(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedTermFilter(FilterTerm term) => NestedTermFilter(term);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedTermsFilter(IEnumerable<FilterTerm> terms) => NestedTermsFilter(terms);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedTermPrefix(FilterTerm term) => NestedTermPrefixFilter(term);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedFieldValueExists(string field) => NestedFieldValueExistsFilter(field);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedFieldValueNotExists(string field) => NestedFieldValueNotExistsFilter(field);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp) => NestedQueryFilter(inner, defaultOp);

        /// <inheritdoc/>
        INestedBooleanFilterOperation INestedFilter.NestedSpatialOperationFilter(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape) => NestedSpatialOperationFilter(field, spatialOperation, shape);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation IntRangeFilter(string field, int? min, int? max, bool minInclusive, bool maxInclusive);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation LongRangeFilter(string field, long? min, long? max, bool minInclusive, bool maxInclusive);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation FloatRangeFilter(string field, float? min, float? max, bool minInclusive, bool maxInclusive);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation DoubleRangeFilter(string field, double? min, double? max, bool minInclusive, bool maxInclusive);

        /// <inheritdoc/>
        public abstract IBooleanFilterOperation SpatialOperationFilter(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape);
    }
}
