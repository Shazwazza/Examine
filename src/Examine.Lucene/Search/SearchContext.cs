using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{

    /// <inheritdoc/>
    public class SearchContext : ISearchContext
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private string[]? _searchableFields;
        private readonly IndexSimilarityCollection? _indexSimilarityCollection;

        /// <inheritdoc/>
        public SearchContext(SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _indexSimilarityCollection = null;
        }

        /// <inheritdoc/>
        public SearchContext(SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection, IndexSimilarityCollection? indexSimilarityCollection)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _indexSimilarityCollection = indexSimilarityCollection;
        }

        /// <summary>
        /// Get Searcher Reference
        /// </summary>
        /// <returns></returns>
        public ISearcherReference GetSearcher() => new SearcherReference(_searcherManager);

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
#pragma warning disable IDE0007 // Use implicit type
                    IndexSearcher searcher = _searcherManager.Acquire();
#pragma warning restore IDE0007 // Use implicit type
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

        /// <inheritdoc/>
        public IIndexSimilarityType? GetSimilarity(string similarityName)
        {
            //Get the value type for the field, or use the default if not defined
            if(_indexSimilarityCollection == null || string.IsNullOrEmpty(similarityName))
            {
                return null;
            }

            return _indexSimilarityCollection.GetIndexSimilarity(similarityName);
        }

        /// <inheritdoc/>
        public IIndexSimilarityType? GetDefaultSimilarity()
        {
            //Get the value type for the field, or use the default if not defined
            if (_indexSimilarityCollection == null || string.IsNullOrEmpty(_indexSimilarityCollection.DefaultSimilarityName))
            {
                return null;
            }

            return _indexSimilarityCollection.GetIndexSimilarity(_indexSimilarityCollection.DefaultSimilarityName);
        }
    }
}
