using System;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Index;
using Lucene.Net.Search.Similarities;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Taxonomy Search Context
    /// </summary>
    public class TaxonomySearchContext : ITaxonomySearchContext
    {
        private readonly SearcherTaxonomyManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private string[]? _searchableFields;
        private readonly IndexSimilarityCollection? _indexSimilarityCollection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searcherManager"></param>
        /// <param name="fieldValueTypeCollection"></param>
        /// <param name="indexSimilarityCollection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TaxonomySearchContext(SearcherTaxonomyManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection, IndexSimilarityCollection? indexSimilarityCollection)
        {
            _searcherManager = searcherManager ?? throw new ArgumentNullException(nameof(searcherManager));
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _indexSimilarityCollection = indexSimilarityCollection ?? throw new ArgumentNullException(nameof(indexSimilarityCollection));
        }

        /// <inheritdoc/>
        public ISearcherReference GetSearcher() => new TaxonomySearcherReference(_searcherManager);

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
                    var searcherAndTaxonomy = _searcherManager.Acquire();
                    try
                    {
                        var fields = MultiFields.GetMergedFieldInfos(searcherAndTaxonomy.Searcher.IndexReader)
                                    .Select(x => x.Name)
                                    .ToList();

                        //exclude the special index fields
                        _searchableFields = fields
                            .Where(x => !x.StartsWith(ExamineFieldNames.SpecialFieldPrefix) && !x.Equals(ExamineFieldNames.DefaultFacetsName))
                            .ToArray();
                    }
                    finally
                    {
                        _searcherManager.Release(searcherAndTaxonomy);
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
        public ITaxonomySearcherReference GetTaxonomyAndSearcher() => new TaxonomySearcherReference(_searcherManager);

        /// <inheritdoc/>
        public IIndexSimilarity? GetSimilarity(string similarityName)
        {
            //Get the value type for the field, or use the default if not defined
            if (_indexSimilarityCollection == null || string.IsNullOrEmpty(similarityName))
            {
                return null;
            }

            return _indexSimilarityCollection.GetIndexSimilarity(similarityName);
        }

        /// <inheritdoc/>
        public IIndexSimilarity? GetDefaultSimilarity()
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
