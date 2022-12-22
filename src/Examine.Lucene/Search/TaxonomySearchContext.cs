using System;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Index;

namespace Examine.Lucene.Search
{
    public class TaxonomySearchContext : ITaxonomySearchContext
    {
        private readonly SearcherTaxonomyManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private string[] _searchableFields;

        public TaxonomySearchContext(SearcherTaxonomyManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection)
        {
            _searcherManager = searcherManager;
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
        }

        public ISearcherReference GetSearcher() => new TaxonomySearcherReference(_searcherManager);

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

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName,
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }

        public ITaxonomySearcherReference GetTaxonomyAndSearcher() => new TaxonomySearcherReference(_searcherManager);
    }
}
