using System;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Index;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Taxonomy Search Context
    /// </summary>
    public class TaxonomySearchContext : ITaxonomySearchContext
    {
        private readonly SearcherTaxonomyManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly bool _isNrt;
        private string[]? _searchableFields;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonomySearchContext(SearcherTaxonomyManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection, bool isNrt)
        {
            _searcherManager = searcherManager ?? throw new ArgumentNullException(nameof(searcherManager));
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _isNrt = isNrt;
        }

        /// <inheritdoc/>
        public ISearcherReference GetSearcher()
        {
            if (!_isNrt)
            {
                _searcherManager.MaybeRefresh();
            }
            return new TaxonomySearcherReference(_searcherManager);
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
    }
}
