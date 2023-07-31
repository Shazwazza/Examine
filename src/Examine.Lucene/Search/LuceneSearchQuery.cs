using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Examine.Lucene.Indexing;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// This class is used to query against Lucene.Net
    /// </summary>
    [DebuggerDisplay("Category: {Category}, LuceneQuery: {Query}")]
    public class LuceneSearchQuery : LuceneSearchQueryBase, IQueryExecutor
    {
        private readonly ISearchContext _searchContext;
        private readonly FacetsConfig _facetsConfig;
        private ISet<string>? _fieldsToLoad = null;
        private readonly IList<IFacetField> _facetFields = new List<IFacetField>();

        /// <inheritdoc/>
        public LuceneSearchQuery(
            ISearchContext searchContext,
            string? category, Analyzer analyzer, LuceneSearchOptions searchOptions, BooleanOperation occurance, FacetsConfig facetsConfig)
            : base(CreateQueryParser(searchContext, analyzer, searchOptions), category, searchOptions, occurance)
        {   
            _searchContext = searchContext;
            _facetsConfig = facetsConfig;
        }

        private static CustomMultiFieldQueryParser CreateQueryParser(ISearchContext searchContext, Analyzer analyzer, LuceneSearchOptions searchOptions)
        {
            var parser = new ExamineMultiFieldQueryParser(searchContext, LuceneInfo.CurrentVersion, analyzer);

            if (searchOptions != null)
            {
                if (searchOptions.LowercaseExpandedTerms.HasValue)
                {
                    parser.LowercaseExpandedTerms = searchOptions.LowercaseExpandedTerms.Value;
                }
                if (searchOptions.AllowLeadingWildcard.HasValue)
                {
                    parser.AllowLeadingWildcard = searchOptions.AllowLeadingWildcard.Value;
                }
                if (searchOptions.EnablePositionIncrements.HasValue)
                {
                    parser.EnablePositionIncrements = searchOptions.EnablePositionIncrements.Value;
                }
                if (searchOptions.MultiTermRewriteMethod != null)
                {
                    parser.MultiTermRewriteMethod = searchOptions.MultiTermRewriteMethod;
                }
                if (searchOptions.FuzzyPrefixLength.HasValue)
                {
                    parser.FuzzyPrefixLength = searchOptions.FuzzyPrefixLength.Value;
                }
                if (searchOptions.Locale != null)
                {
                    parser.Locale = searchOptions.Locale;
                }
                if (searchOptions.TimeZone != null)
                {
                    parser.TimeZone = searchOptions.TimeZone;
                }
                if (searchOptions.PhraseSlop.HasValue)
                {
                    parser.PhraseSlop = searchOptions.PhraseSlop.Value;
                }
                if (searchOptions.FuzzyMinSim.HasValue)
                {
                    parser.FuzzyMinSim = searchOptions.FuzzyMinSim.Value;
                }
                if (searchOptions.DateResolution.HasValue)
                {
                    parser.SetDateResolution(searchOptions.DateResolution.Value);
                }
            }

            return parser;
        }

        /// <summary>
        /// Sets the order by of the query
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public virtual IBooleanOperation OrderBy(params SortableField[] fields) => OrderByInternal(false, fields);

        /// <summary>
        /// Sets the order by of the query in a descending manner
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public virtual IBooleanOperation OrderByDescending(params SortableField[] fields) => OrderByInternal(true, fields);

        /// <inheritdoc/>
        public override IBooleanOperation Field<T>(string fieldName, T fieldValue)
            => RangeQueryInternal<T>(new[] { fieldName }, fieldValue, fieldValue, true, true, Occurrence);

        /// <inheritdoc/>
        public override IBooleanOperation ManagedQuery(string query, string[]? fields = null)
            => ManagedQueryInternal(query, fields, Occurrence);

        /// <inheritdoc/>
        public override IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true)
            => RangeQueryInternal(fields, min, max, minInclusive, maxInclusive, Occurrence);

        /// <inheritdoc/>
        protected override INestedBooleanOperation FieldNested<T>(string fieldName, T fieldValue)
            => RangeQueryInternal<T>(new[] { fieldName }, fieldValue, fieldValue, true, true, Occurrence);

        /// <inheritdoc/>
        protected override INestedBooleanOperation ManagedQueryNested(string query, string[]? fields = null)
            => ManagedQueryInternal(query, fields, Occurrence);

        /// <inheritdoc/>
        protected override INestedBooleanOperation RangeQueryNested<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true)
            => RangeQueryInternal(fields, min, max, minInclusive, maxInclusive, Occurrence);

        internal LuceneBooleanOperationBase ManagedQueryInternal(string query, string[]? fields, Occur occurance)
        {
            Query.Add(new LateBoundQuery(() =>
            {
                //if no fields are specified then use all fields
                fields = fields ?? AllFields;

                var types = fields.Select(f => _searchContext.GetFieldValueType(f)).OfType<IIndexFieldValueType>();

                //Strangely we need an inner and outer query. If we don't do this then the lucene syntax returned is incorrect 
                //since it doesn't wrap in parenthesis properly. I'm unsure if this is a lucene issue (assume so) since that is what
                //is producing the resulting lucene string syntax. It might not be needed internally within Lucene since it's an object
                //so it might be the ToString() that is the issue.
                var outer = new BooleanQuery();
                var inner = new BooleanQuery();

                foreach (var type in types)
                {
                    var q = type.GetQuery(query);

                    if (q != null)
                    {
                        //CriteriaContext.ManagedQueries.Add(new KeyValuePair<IIndexFieldValueType, Query>(type, q));
                        inner.Add(q, Occur.SHOULD);
                    }
                }

                outer.Add(inner, Occur.SHOULD);

                return outer;
            }), occurance);

            return CreateOp();
        }

        internal LuceneBooleanOperationBase RangeQueryInternal<T>(string[] fields, T? min, T? max, bool minInclusive, bool maxInclusive, Occur occurance)
            where T : struct
        {
            Query.Add(new LateBoundQuery(() =>
            {
                //Strangely we need an inner and outer query. If we don't do this then the lucene syntax returned is incorrect 
                //since it doesn't wrap in parenthesis properly. I'm unsure if this is a lucene issue (assume so) since that is what
                //is producing the resulting lucene string syntax. It might not be needed internally within Lucene since it's an object
                //so it might be the ToString() that is the issue.
                var outer = new BooleanQuery();
                var inner = new BooleanQuery();

                foreach (var f in fields)
                {
                    var valueType = _searchContext.GetFieldValueType(f);

                    if (valueType is IIndexRangeValueType<T> type)
                    {
                        var q = type.GetQuery(min, max, minInclusive, maxInclusive);

                        if (q != null)
                        {
                            //CriteriaContext.FieldQueries.Add(new KeyValuePair<IIndexFieldValueType, Query>(type, q));
                            inner.Add(q, Occur.SHOULD);
                        }
                    }
#if !NETSTANDARD2_0 && !NETSTANDARD2_1
                    else if(typeof(T) == typeof(DateOnly) && valueType is IIndexRangeValueType<DateTime> dateOnlyType)
                    {
                        TimeOnly minValueTime = minInclusive ? TimeOnly.MinValue : TimeOnly.MaxValue;
                        var minValue = min.HasValue ? (min.Value as DateOnly?)?.ToDateTime(minValueTime) : null;

                        TimeOnly maxValueTime = maxInclusive ? TimeOnly.MaxValue : TimeOnly.MinValue;
                        var maxValue = max.HasValue ? (max.Value as DateOnly?)?.ToDateTime(maxValueTime) : null;

                        var q = dateOnlyType.GetQuery(minValue, maxValue, minInclusive, maxInclusive);

                        if (q != null)
                        {
                            inner.Add(q, Occur.SHOULD);
                        }
                    }
#endif
                    else
                    {
                        throw new InvalidOperationException($"Could not perform a range query on the field {f}, it's value type is {valueType?.GetType()}");
                    }
                }

                outer.Add(inner, Occur.SHOULD);

                return outer;
            }), occurance);

            return CreateOp();
        }

        /// <inheritdoc />
        public ISearchResults Execute(QueryOptions? options = null) => Search(options);

        /// <summary>
        /// Performs a search with a maximum number of results
        /// </summary>
        private ISearchResults Search(QueryOptions? options)
        {
            // capture local
            var query = Query;

            if (!string.IsNullOrEmpty(Category))
            {
                // rebuild the query
                IList<BooleanClause> existingClauses = query.Clauses;

                if (existingClauses.Count == 0)
                {
                    // Nothing to search. This can occur in cases where an analyzer for a field doesn't return
                    // anything since it strips all values.
                    return EmptySearchResults.Instance;
                }

                query = new BooleanQuery
                {
                    // prefix the category field query as a must
                    { GetFieldInternalQuery(ExamineFieldNames.CategoryFieldName, new ExamineValue(Examineness.Explicit, Category), true), Occur.MUST }
                };

                // add the ones that we're already existing
                foreach (var c in existingClauses)
                {
                    query.Add(c);
                }
            }

            var executor = new LuceneSearchExecutor(options, query, SortFields, _searchContext, _fieldsToLoad, _facetFields,_facetsConfig);

            var pagesResults = executor.Execute();

            return pagesResults;
        }        

        /// <summary>
        /// Internal operation for adding the ordered results
        /// </summary>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        /// <param name="fields">The field names.</param>
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
        private LuceneBooleanOperationBase OrderByInternal(bool descending, params SortableField[] fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            foreach (var f in fields)
            {
                var fieldName = f.FieldName;

                var defaultSort =  SortFieldType.STRING;

                switch (f.SortType)
                {
                    case SortType.Score:
                        defaultSort = SortFieldType.SCORE;
                        break;
                    case SortType.DocumentOrder:
                        defaultSort = SortFieldType.DOC;
                        break;
                    case SortType.String:
                        defaultSort = SortFieldType.STRING;
                        break;
                    case SortType.Int:
                        defaultSort = SortFieldType.INT32;
                        break;
                    case SortType.Float:
                        defaultSort = SortFieldType.SINGLE;
                        break;
                    case SortType.Long:
                        defaultSort = SortFieldType.INT64;
                        break;
                    case SortType.Double:
                        defaultSort = SortFieldType.DOUBLE;
                        break;                   
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                //get the sortable field name if this field type has one
                var valType = _searchContext.GetFieldValueType(fieldName);

                if (valType?.SortableFieldName != null)
                    fieldName = valType.SortableFieldName;

                SortFields.Add(new SortField(fieldName, defaultSort, descending));
            }

            return CreateOp();
        }

        internal IBooleanOperation SelectFieldsInternal(ISet<string> loadedFieldNames)
        {
            _fieldsToLoad = loadedFieldNames;
            return CreateOp();
        }

        internal IBooleanOperation SelectFieldInternal(string fieldName)
        {
            _fieldsToLoad = new HashSet<string>(new string[] { fieldName });
            return CreateOp();
        }

        /// <summary>
        /// Selects all fields
        /// </summary>
        /// <returns></returns>
        public IBooleanOperation SelectAllFieldsInternal()
        {
            _fieldsToLoad = null;
            return CreateOp();
        }

        /// <summary>
        /// Creates a new <see cref="LuceneBooleanOperation"/>
        /// </summary>
        /// <returns></returns>
        protected override LuceneBooleanOperationBase CreateOp() => new LuceneBooleanOperation(this);

        internal IFacetOperations FacetInternal(string field, Action<IFacetQueryField>? facetConfiguration, params string[] values)
        {
            if(values == null)
            {
                values = Array.Empty<string>();
            }

            var valueType = _searchContext.GetFieldValueType(field) as IIndexFacetValueType;

            var facet = new FacetFullTextField(field, values, GetFacetField(field), isTaxonomyIndexed: valueType.IsTaxonomyFaceted);

            if(facetConfiguration != null)
            {
                facetConfiguration.Invoke(new FacetQueryField(facet));
            }

            _facetFields.Add(facet);

            return new LuceneFacetOperation(this);
        }

        internal IFacetOperations FacetInternal(string field, params DoubleRange[] doubleRanges)
        {
            if(doubleRanges == null)
            {
                doubleRanges = Array.Empty<DoubleRange>();
            }

            var valueType = _searchContext.GetFieldValueType(field) as IIndexFacetValueType;
            var facet = new FacetDoubleField(field, doubleRanges, GetFacetField(field), isTaxonomyIndexed: valueType.IsTaxonomyFaceted);

            _facetFields.Add(facet);

            return new LuceneFacetOperation(this);
        }

        internal IFacetOperations FacetInternal(string field, params FloatRange[] floatRanges)
        {
            if (floatRanges == null)
            {
                floatRanges = Array.Empty<FloatRange>();
            }

            var valueType = _searchContext.GetFieldValueType(field) as IIndexFacetValueType;
            var facet = new FacetFloatField(field, floatRanges, GetFacetField(field), isTaxonomyIndexed: valueType.IsTaxonomyFaceted);

            _facetFields.Add(facet);

            return new LuceneFacetOperation(this);
        }

        internal IFacetOperations FacetInternal(string field, params Int64Range[] longRanges)
        {
            if(longRanges == null)
            {
                longRanges = Array.Empty<Int64Range>();
            }

            var valueType = _searchContext.GetFieldValueType(field) as IIndexFacetValueType;
            var facet = new FacetLongField(field, longRanges, GetFacetField(field), isTaxonomyIndexed: valueType.IsTaxonomyFaceted);

            _facetFields.Add(facet);

            return new LuceneFacetOperation(this);
        }

        private string GetFacetField(string field)
        {
            if (_facetsConfig.DimConfigs.ContainsKey(field))
            {
                return _facetsConfig.DimConfigs[field].IndexFieldName;
            }
            return ExamineFieldNames.DefaultFacetsName;
        }
        private bool GetFacetFieldIsMultiValued(string field)
        {
            if (_facetsConfig.DimConfigs.ContainsKey(field))
            {
                return _facetsConfig.DimConfigs[field].IsMultiValued;
            }
            return false;
        }
        private bool GetFacetFieldIsHierarchical(string field)
        {
            if (_facetsConfig.DimConfigs.ContainsKey(field))
            {
                return _facetsConfig.DimConfigs[field].IsHierarchical;
            }
            return false;
        }
        #region IFilter
        public override IBooleanFilterOperation ChainFilters(Action<IFilterChainStart> chain)
        {
            throw new NotImplementedException();
        }

        public override IBooleanFilterOperation TermFilter(FilterTerm term)
        {
            throw new NotImplementedException();
        }

        public override IBooleanFilterOperation TermsFilter(IEnumerable<FilterTerm> terms) => TermsInternal(terms);

        internal IBooleanFilterOperation TermsInternal(IEnumerable<FilterTerm> terms, Occur occurance = Occur.MUST)
        {
            if (terms is null)
            {
                throw new ArgumentNullException(nameof(terms));
            }

            if (!terms.Any() || terms.Any(x=> string.IsNullOrWhiteSpace(x.FieldName)))
            {
                throw new ArgumentOutOfRangeException(nameof(terms));
            }

            var luceneTerms = terms.Select(x => new Term(x.FieldName, x.FieldValue)).ToArray();
            var filterToAdd = new TermsFilter(luceneTerms)
            if (filterToAdd != null)
            {
                Filter.Add(filterToAdd, occurance);
            }

            return CreateOp();
        }
        public override IBooleanFilterOperation TermPrefixFilter(FilterTerm term) => throw new NotImplementedException();
        public override IBooleanFilterOperation FieldValueExistsFilter(string field) => throw new NotImplementedException();
        public override IBooleanFilterOperation FieldValueNotExistsFilter(string field) => throw new NotImplementedException();
        public override IBooleanFilterOperation QueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.And) => throw new NotImplementedException();
        public override IBooleanFilterOperation RangeFilter<T>(string field, T min, T max, bool minInclusive = true, bool maxInclusive = true) => throw new NotImplementedException();
        #endregion

        #region INestedFilter
        protected override INestedBooleanFilterOperation NestedChainFilters(Action<IFilterChainStart> chain) => throw new NotImplementedException();
        protected override INestedBooleanFilterOperation NestedTermFilter(FilterTerm term) => throw new NotImplementedException();
        protected override INestedBooleanFilterOperation NestedTermsFilter(IEnumerable<FilterTerm> terms) => throw new NotImplementedException();
        protected override INestedBooleanFilterOperation NestedTermPrefixFilter(FilterTerm term) => throw new NotImplementedException();
        protected override INestedBooleanFilterOperation NestedFieldValueExistsFilter(string field) => throw new NotImplementedException();
        protected override INestedBooleanFilterOperation NestedFieldValueNotExistsFilter(string field) => throw new NotImplementedException();
        protected override INestedBooleanFilterOperation NestedQueryFilter(Func<INestedQuery, INestedBooleanOperation> inner, BooleanOperation defaultOp) => throw new NotImplementedException();
        protected override INestedBooleanFilterOperation NestedRangeFilter<T>(string field, T min, T max, bool minInclusive, bool maxInclusive) => throw new NotImplementedException();
        #endregion
    }
}
