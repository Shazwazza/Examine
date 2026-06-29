using System;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{

    public sealed class SearchContext : ISearchContext
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly bool _isNrt;
        private string[] _searchableFields;

        // Cached factory for the default FullText value type — avoids a ConcurrentDictionary lookup
        // on every GetFieldValueType call. Written at most once (same value); volatile ensures
        // visibility across threads without a lock, matching the pattern used in LuceneIndex.
        private volatile IFieldValueTypeFactory _defaultFactory;

        [Obsolete("Use ctor with all dependencies")]
        public SearchContext(SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
        }

        public SearchContext(SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection, bool isNrt)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _isNrt = isNrt;
        }

        // TODO: Do we want to create a new searcher every time? I think so, but we shouldn't allocate so much
        public ISearcherReference GetSearcher()
        {
            if (!_isNrt)
            {
                _searcherManager.MaybeRefresh();
            }

            return new SearcherReference(_searcherManager);
        }

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
                        // Single-pass: select + filter in one LINQ chain to avoid
                        // materialising an intermediate List<string>.
                        var filtered = MultiFields.GetMergedFieldInfos(searcher.IndexReader)
                                    .Select(x => x.Name)
                                    .Where(x => !x.StartsWith(ExamineFieldNames.SpecialFieldPrefix, StringComparison.Ordinal))
                                    .ToArray();

                        // Only cache non-empty results so that an initially empty index
                        // will re-read fields once documents have been indexed.
                        if (filtered.Length > 0)
                        {
                            _searchableFields = filtered;
                        }

                        return filtered;
                    }
                    finally
                    {
                        _searcherManager.Release(searcher);
                    }
                }

                return _searchableFields;
            }
        }

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName,
                _defaultFactory ??= _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}
