using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Examine.Lucene.Indexing;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Queries.Function;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial;
using Spatial4n.Distance;
using Spatial4n.Shapes;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// This class is used to query against Lucene.Net
    /// </summary>
    [DebuggerDisplay("Category: {Category}, LuceneQuery: {Query}")]
    public class LuceneSearchQuery : LuceneSearchQueryBase, IQueryExecutor
    {
        private readonly ISearchContext _searchContext;
        private ISet<string> _fieldsToLoad = null;

        public LuceneSearchQuery(
            ISearchContext searchContext,
            string category, Analyzer analyzer, LuceneSearchOptions searchOptions, BooleanOperation occurance)
            : base(CreateQueryParser(searchContext, analyzer, searchOptions), category, searchOptions, occurance)
        {
            _searchContext = searchContext;
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

        public virtual IBooleanOperation OrderBy(params SortableField[] fields) => OrderByInternal(false, fields);

        internal IOrdering OrderBy(Sorting[] sorts) => OrderByInternal(sorts);

        public virtual IBooleanOperation OrderByDescending(params SortableField[] fields) => OrderByInternal(true, fields);

        public override IBooleanOperation Field<T>(string fieldName, T fieldValue)
            => RangeQueryInternal<T>(new[] { fieldName }, fieldValue, fieldValue, true, true, Occurrence);

        public override IBooleanOperation ManagedQuery(string query, string[] fields = null)
            => ManagedQueryInternal(query, fields, Occurrence);

        public override IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true)
            => RangeQueryInternal(fields, min, max, minInclusive, maxInclusive, Occurrence);

        protected override INestedBooleanOperation FieldNested<T>(string fieldName, T fieldValue)
            => RangeQueryInternal<T>(new[] { fieldName }, fieldValue, fieldValue, true, true, Occurrence);

        protected override INestedBooleanOperation ManagedQueryNested(string query, string[] fields = null)
            => ManagedQueryInternal(query, fields, Occurrence);

        protected override INestedBooleanOperation RangeQueryNested<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true)
            => RangeQueryInternal(fields, min, max, minInclusive, maxInclusive, Occurrence);

        internal LuceneBooleanOperationBase ManagedQueryInternal(string query, string[] fields, Occur occurance)
        {
            Query.Add(new LateBoundQuery(() =>
            {
                //if no fields are specified then use all fields
                fields = fields ?? AllFields;

                var types = fields.Select(f => _searchContext.GetFieldValueType(f)).Where(t => t != null);

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
                    else if (typeof(T) == typeof(DateOnly) && valueType is IIndexRangeValueType<DateTime> dateOnlyType)
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
        public ISearchResults Execute(QueryOptions options = null) => Search(options);

        /// <summary>
        /// Performs a search with a maximum number of results
        /// </summary>
        private ISearchResults Search(QueryOptions options)
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
            var executor = new LuceneSearchExecutor(options, query, SortFields, _searchContext, _fieldsToLoad, Sortings);

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
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));

            foreach (var f in fields)
            {
                var fieldName = f.FieldName;

                var defaultSort = SortFieldType.STRING;

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

        /// <summary>
        /// Internal operation for adding the ordered results
        /// </summary>
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
        private LuceneBooleanOperationBase OrderByInternal(params Sorting[] sorting)
        {
            if (sorting == null)
                throw new ArgumentNullException(nameof(sorting));

            SortFields.Clear();
            Sortings.Clear();
            foreach (var item in sorting)
            {
                switch (item.Field.SortType)
                {
                    case SortType.SpatialDistance:
                    case SortType.Score:
                    case SortType.DocumentOrder:
                    case SortType.String:
                    case SortType.Int:
                    case SortType.Float:
                    case SortType.Long:
                    case SortType.Double:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Sortings.Add(item);
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

        public IBooleanOperation SelectAllFieldsInternal()
        {
            _fieldsToLoad = null;
            return CreateOp();
        }

        protected override LuceneBooleanOperationBase CreateOp() => new LuceneBooleanOperation(this);

        public override IBooleanOperation SpatialDistanceQuery(string field, Func<IExamineSpatialShapeFactory, IExamineSpatialPoint> point, ExamineSpatialDistanceComparison distanceComparison, double distance)
        {
            throw new NotImplementedException();
        }

        public override IBooleanOperation SpatialOperationQuery(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape)
        {
            var spatialField = _searchContext.GetFieldValueType(field) as ShapeIndexFieldValueTypeBase;
            var shapeVal = shape(spatialField.ExamineSpatialShapeFactory);
            SpatialOperation luceneSpatialOperation;
            switch (spatialOperation)
            {
                case ExamineSpatialOperation.Intersects:
                    luceneSpatialOperation = SpatialOperation.Intersects;
                    break;
                case ExamineSpatialOperation.Overlaps:
                    luceneSpatialOperation = SpatialOperation.Overlaps;
                    break;
                case ExamineSpatialOperation.IsWithin:
                    luceneSpatialOperation = SpatialOperation.IsWithin;
                    break;
                case ExamineSpatialOperation.BoundingBoxIntersects:
                    luceneSpatialOperation = SpatialOperation.BBoxIntersects;
                    break;
                case ExamineSpatialOperation.BoundingBoxWithin:
                    luceneSpatialOperation = SpatialOperation.BBoxWithin;
                    break;
                case ExamineSpatialOperation.Contains:
                    luceneSpatialOperation = SpatialOperation.Contains;
                    break;
                case ExamineSpatialOperation.IsDisjointTo:
                    luceneSpatialOperation = SpatialOperation.IsDisjointTo;
                    break;
                case ExamineSpatialOperation.IsEqualTo:
                    luceneSpatialOperation = SpatialOperation.IsEqualTo;
                    break;
                default:
                    throw new NotSupportedException(nameof(spatialOperation));
            }
            var spatial4nShape = (shapeVal as ExamineLuceneShape)?.Shape;
            var spatialArgs = new SpatialArgs(luceneSpatialOperation, spatial4nShape);
            var queryToAdd = spatialField.SpatialStrategy.MakeQuery(spatialArgs);
            var occurrence = Occur.SHOULD;
            if (queryToAdd != null)
            {
                Query.Add(queryToAdd, occurrence);
            }

            return CreateOp();
        }

        public override IBooleanOperation SpatialOperationQuery(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IEnumerable<IExamineSpatialShape>> shapes)
        {
            var spatialField = _searchContext.GetFieldValueType(field) as ShapeIndexFieldValueTypeBase;
            var shapeVals = shapes(spatialField.ExamineSpatialShapeFactory);
            SpatialOperation luceneSpatialOperation;
            switch (spatialOperation)
            {
                case ExamineSpatialOperation.Intersects:
                    luceneSpatialOperation = SpatialOperation.Intersects;
                    break;
                case ExamineSpatialOperation.Overlaps:
                    luceneSpatialOperation = SpatialOperation.Overlaps;
                    break;
                case ExamineSpatialOperation.IsWithin:
                    luceneSpatialOperation = SpatialOperation.IsWithin;
                    break;
                case ExamineSpatialOperation.BoundingBoxIntersects:
                    luceneSpatialOperation = SpatialOperation.BBoxIntersects;
                    break;
                case ExamineSpatialOperation.BoundingBoxWithin:
                    luceneSpatialOperation = SpatialOperation.BBoxWithin;
                    break;
                case ExamineSpatialOperation.Contains:
                    luceneSpatialOperation = SpatialOperation.Contains;
                    break;
                case ExamineSpatialOperation.IsDisjointTo:
                    luceneSpatialOperation = SpatialOperation.IsDisjointTo;
                    break;
                case ExamineSpatialOperation.IsEqualTo:
                    luceneSpatialOperation = SpatialOperation.IsEqualTo;
                    break;
                default:
                    throw new NotSupportedException(nameof(spatialOperation));
            }
            var shapeList = shapeVals.Select(x => x as ExamineLuceneShape).Select(x => x.Shape).ToList();
            var shapeCollection = new ShapeCollection(shapeList, spatialField.SpatialStrategy.SpatialContext);
            var spatialArgs = new SpatialArgs(luceneSpatialOperation, shapeCollection);
            var queryToAdd = spatialField.SpatialStrategy.MakeQuery(spatialArgs);
            var occurrence = Occur.SHOULD;
            if (queryToAdd != null)
            {
                Query.Add(queryToAdd, occurrence);
            }

            return CreateOp();
        }
    }
}
