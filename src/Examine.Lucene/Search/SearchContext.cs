using System;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{

    /// <inheritdoc/>
    public sealed class SearchContext : ISearchContext
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly bool _isNrt;
        private string[]? _searchableFields;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchContext"/> class.
        /// </summary>
        /// <param name="searcherManager">The manager responsible for managing the searcher instances.</param>
        /// <param name="fieldValueTypeCollection">The collection of field value types used for indexing and searching.</param>
        /// <param name="isNrt">Indicates whether the search context is using near real-time indexing.</param>
        public SearchContext(SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection, bool isNrt)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _isNrt = isNrt;
        }

        /// <inheritdoc/>
        public ISearcherReference GetSearcher()
        {
            // TODO: Do we want to create a new searcher every time? I think so, but we shouldn't allocate so much
            if (!_isNrt)
            {
                _searcherManager.MaybeRefresh();
            }

            return new SearcherReference(_searcherManager);
        }

        /// <inheritdoc/>
        public string[] SearchableFields
        {
            get
            {
                if (_searchableFields == null)
                {
                    // IMPORTANT! Do not resolve the IndexSearcher from the `IndexSearcher` property above since this
                    // will not release it from the searcher manager. When we are collecting fields, we are essentially
                    // performing a 'search'. We must ensure that the underlying reader has the correct reference counts.
                    var searcher = _searcherManager.Acquire();

                    try
                    {
                        var fields = MultiFields.GetMergedFieldInfos(searcher.IndexReader)
                                    .Select(x => x.Name)
                                    .ToList();

                        //exclude the special index fields
                        _searchableFields = fields
                            .Where(x => !x.StartsWith(ExamineFieldNames.SpecialFieldPrefix) && !x.Equals(ExamineFieldNames.DefaultFacetsName))
                            .ToArray();
                    }
                    finally
                    {
                        _searcherManager.Release(searcher);
                    }
                }

                return _searchableFields;
            }
        }

        /// <inheritdoc/>
        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName,
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}
