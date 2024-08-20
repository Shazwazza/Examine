using System;
using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.Lucene.Search
{

    public class SearchContext : ISearchContext
    {
        private readonly SearcherManager _searcherManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private readonly Lazy<ISearcherReference> _searcherReference;
        private string[] _searchableFields;

        public SearchContext(SearcherManager searcherManager, FieldValueTypeCollection fieldValueTypeCollection)
        {
            _searcherManager = searcherManager;            
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new ArgumentNullException(nameof(fieldValueTypeCollection));
            _searcherReference = new Lazy<ISearcherReference>(() =>
            {
                // TODO: Only if NRT is disabled?
                //_searcherManager.MaybeRefresh();

                return new SearcherReference(_searcherManager);
            });
        }

        // TODO: Do we want to create a new searcher every time? I think so, but we shouldn't allocate so much
        public ISearcherReference GetSearcher()
        {
            //return _searcherReference.Value;

            // TODO: Only if NRT is disabled?
            //_searcherManager.MaybeRefresh();

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
                    IndexSearcher searcher = _searcherManager.Acquire();
                    //var searcher = GetSearcher().IndexSearcher;

                    try
                    {
                        var fields = MultiFields.GetMergedFieldInfos(searcher.IndexReader)
                                    .Select(x => x.Name)
                                    .ToList();

                        //exclude the special index fields
                        _searchableFields = fields
                            .Where(x => !x.StartsWith(ExamineFieldNames.SpecialFieldPrefix))
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

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName, 
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}
