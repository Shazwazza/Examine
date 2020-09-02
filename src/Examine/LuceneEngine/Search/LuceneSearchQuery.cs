using System;
using System.Diagnostics;
using System.Linq;
using Examine.LuceneEngine.Indexing;
using Examine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Search
{
    /// <summary>
    /// This class is used to query against Lucene.Net
    /// </summary>
    [DebuggerDisplay("Category: {Category}, LuceneQuery: {Query}")]
    public class LuceneSearchQuery : LuceneSearchQueryBase, IQueryExecutor
    {
        private readonly ISearchContext _searchContext;

        public LuceneSearchQuery(
            ISearchContext searchContext,
            string category, Analyzer analyzer, string[] fields, LuceneSearchOptions searchOptions, BooleanOperation occurance)
            : base(CreateQueryParser(searchContext, fields, analyzer), category, fields, searchOptions, occurance)
        {   
            _searchContext = searchContext;
        }

        private static CustomMultiFieldQueryParser CreateQueryParser(ISearchContext searchContext, string[] fields, Analyzer analyzer) 
            => new ExamineMultiFieldQueryParser(searchContext, LuceneVersion, fields, analyzer);

        public IBooleanOperation OrderBy(params SortableField[] fields) => OrderByInternal(false, fields);

        public IBooleanOperation OrderByDescending(params SortableField[] fields) => OrderByInternal(true, fields);

        public override IBooleanOperation Field<T>(string fieldName, T fieldValue) 
            => RangeQueryInternal<T>(new[] { fieldName }, fieldValue, fieldValue);

        public override IBooleanOperation ManagedQuery(string query, string[] fields = null)
            => ManagedQueryInternal(query, fields);

        public override IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true)
            => RangeQueryInternal(fields, min, max, minInclusive, maxInclusive);

        protected override INestedBooleanOperation FieldNested<T>(string fieldName, T fieldValue) 
            => RangeQueryInternal<T>(new[] { fieldName }, fieldValue, fieldValue);

        protected override INestedBooleanOperation ManagedQueryNested(string query, string[] fields = null)
            => ManagedQueryInternal(query, fields);

        protected override INestedBooleanOperation RangeQueryNested<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true)
            => RangeQueryInternal(fields, min, max, minInclusive, maxInclusive);

        internal LuceneBooleanOperation ManagedQueryInternal(string query, string[] fields = null)
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
                    var q = type.GetQuery(query, _searchContext.Searcher);
                    if (q != null)
                    {
                        //CriteriaContext.ManagedQueries.Add(new KeyValuePair<IIndexFieldValueType, Query>(type, q));
                        inner.Add(q, Occur.SHOULD);
                    }

                }

                outer.Add(inner, Occur.SHOULD);

                return outer;
            }), Occurrence);


            return new LuceneBooleanOperation(this);
        }

        internal LuceneBooleanOperation RangeQueryInternal<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true)
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
                    else
                    {
                        throw new InvalidOperationException($"Could not perform a range query on the field {f}, it's value type is {valueType?.GetType()}");
                    }
                }
                outer.Add(inner, Occur.SHOULD);

                return outer;
            }), Occurrence);


            return new LuceneBooleanOperation(this);
        }


        /// <inheritdoc />
        public ISearchResults Execute(int maxResults = 500) => Search(maxResults);


        /// <summary>
        /// Performs a search with a maximum number of results
        /// </summary>
        private ISearchResults Search(int maxResults = 500)
        {
            var searcher = _searchContext.Searcher;
            if (searcher == null) return EmptySearchResults.Instance;

            // capture local
            var query = Query;

            if (!string.IsNullOrEmpty(Category))
            {
                // rebuild the query
                var existingClauses = query.Clauses.ToList();
                query = new BooleanQuery
                {
                    // prefix the category field query as a must
                    { GetFieldInternalQuery(Providers.LuceneIndex.CategoryFieldName, new ExamineValue(Examineness.Explicit, Category), false), Occur.MUST }
                };
                // add the ones that we're already existing
                foreach (var c in existingClauses)
                {
                    query.Add(c);
                }
            }

            var pagesResults = new LuceneSearchResults(query, SortFields, searcher, maxResults);
            return pagesResults;
        }


        /// <summary>
        /// Internal operation for adding the ordered results
        /// </summary>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        /// <param name="fields">The field names.</param>
        /// <returns>A new <see cref="IBooleanOperation"/> with the clause appended</returns>
        private LuceneBooleanOperation OrderByInternal(bool descending, params SortableField[] fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            foreach (var f in fields)
            {
                var fieldName = f.FieldName;

                var defaultSort = SortField.STRING;

                switch (f.SortType)
                {
                    case SortType.Score:
                        defaultSort = SortField.SCORE;
                        break;
                    case SortType.DocumentOrder:
                        defaultSort = SortField.DOC;
                        break;
                    case SortType.String:
                        defaultSort = SortField.STRING;
                        break;
                    case SortType.Int:
                        defaultSort = SortField.INT;
                        break;
                    case SortType.Float:
                        defaultSort = SortField.FLOAT;
                        break;
                    case SortType.Long:
                        defaultSort = SortField.LONG;
                        break;
                    case SortType.Double:
                        defaultSort = SortField.DOUBLE;
                        break;
                    case SortType.Short:
                        defaultSort = SortField.SHORT;
                        break;
                    case SortType.Byte:
                        defaultSort = SortField.BYTE;
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

            return new LuceneBooleanOperation(this);
        }

        protected override LuceneBooleanOperationBase CreateOp() => new LuceneBooleanOperation(this);
    }
}
