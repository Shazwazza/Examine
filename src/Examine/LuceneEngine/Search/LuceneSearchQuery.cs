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
    public class LuceneSearchQuery : LuceneSearchQueryBase, IQuery, IQueryExecutor
    {
        private readonly ISearchContext _searchContext;

        public LuceneSearchQuery(
            ISearchContext searchContext,
            string category, Analyzer analyzer, string[] fields, LuceneSearchOptions searchOptions, BooleanOperation occurance)
            : base(category, analyzer, fields, searchOptions, occurance)
        {   
            _searchContext = searchContext;
        }

        public override IBooleanOperation Field<T>(string fieldName, T fieldValue)
        {
            return RangeQuery<T>(new[] { fieldName }, fieldValue, fieldValue);
        }

        public IBooleanOperation OrderBy(params SortableField[] fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            return OrderByInternal(false, fields);
        }

        public IBooleanOperation OrderByDescending(params SortableField[] fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            return OrderByInternal(true, fields);
        }
        
        public override IBooleanOperation ManagedQuery(string query, string[] fields = null)
        {
            Query.Add(new LateBoundQuery(() =>
            {
                var types = fields != null
                                ? fields.Select(f => _searchContext.GetFieldValueType(f)).Where(t => t != null)
                                : _searchContext.FieldValueTypes;

                var bq = new BooleanQuery();
                foreach (var type in types)
                {
                    var q = type.GetQuery(query, _searchContext.Searcher);
                    if (q != null)
                    {
                        //CriteriaContext.ManagedQueries.Add(new KeyValuePair<IIndexFieldValueType, Query>(type, q));
                        bq.Add(q, Occur.SHOULD);
                    }

                }
                return bq;
            }), Occurrence);


            return new LuceneBooleanOperation(this);
        }

        public override IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) 
        {
            Query.Add(new LateBoundQuery(() =>
            {
                var bq = new BooleanQuery();
                foreach (var f in fields)
                {
                    if (_searchContext.GetFieldValueType(f) is IIndexRangeValueType<T> type)
                    {
                        var q = type.GetQuery(min, max, minInclusive, maxInclusive);
                        if (q != null)
                        {
                            //CriteriaContext.FieldQueries.Add(new KeyValuePair<IIndexFieldValueType, Query>(type, q));
                            bq.Add(q, Occur.SHOULD);
                        }
                    }
                    else
                    {
                        Trace.TraceError("Could not perform a range query on the field {0}, it's value type is {1}", f, _searchContext.GetFieldValueType(f).GetType());
                    }
                }
                return bq;
            }), Occurrence);


            return new LuceneBooleanOperation(this);
        }

        

        /// <inheritdoc />
        public ISearchResults Execute(int maxResults = 500)
        {
            return Search(maxResults);
        }

        

        /// <summary>
        /// Performs a search with a maximum number of results
        /// </summary>
        private ISearchResults Search(int maxResults = 500)
        {
            var searcher = _searchContext.Searcher;
            if (searcher == null) return EmptySearchResults.Instance;

            var pagesResults = new LuceneSearchResults(Query, SortFields, searcher, maxResults);
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

        protected override LuceneBooleanOperationBase CreateOp()
        {
            return new LuceneBooleanOperation(this);
        }
    }
}
