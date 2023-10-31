using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{

    /// <inheritdoc/>
    public class SearchContext : ISearchContext
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private string[]? _searchableFields;
        private readonly SimilarityDefinitionCollection _similarityDefinitionCollection;

        /// <inheritdoc/>
        public SearchContext(SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _similarityDefinitionCollection = null;
        }

        /// <inheritdoc/>
        public SearchContext(SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection, SimilarityDefinitionCollection similarityDefinitionCollection)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _similarityDefinitionCollection = similarityDefinitionCollection;
        }

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
                    IndexSearcher searcher = _searcherManager.Acquire();
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
        public SimilarityDefinition? GetSimilarity(string similarityName)
        {
            //Get the value type for the field, or use the default if not defined
            if(_similarityDefinitionCollection == null || string.IsNullOrEmpty(similarityName))
            {
                return null;
            }

            if (_similarityDefinitionCollection.TryGetValue(similarityName, out var similarity))
            {
                return similarity;
            };
            return null;
        }

        /// <inheritdoc/>
        public SimilarityDefinition? GetDefaultSimilarity()
        {
            //Get the value type for the field, or use the default if not defined
            if (_similarityDefinitionCollection == null || string.IsNullOrEmpty(_similarityDefinitionCollection.DefaultSimilarityName))
            {
                return null;
            }

            if (_similarityDefinitionCollection.TryGetValue(_similarityDefinitionCollection.DefaultSimilarityName, out var similarity))
            {
                return similarity;
            };
            return null;
        }
    }
}
