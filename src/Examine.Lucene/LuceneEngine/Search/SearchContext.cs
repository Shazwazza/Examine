using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Search
{
    public class SearchContext : ISearchContext
    {
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;

        public SearchContext(FieldValueTypeCollection fieldValueTypeCollection, IndexSearcher searcher)
        {
            _fieldValueTypeCollection = fieldValueTypeCollection ?? throw new System.ArgumentNullException(nameof(fieldValueTypeCollection));
            Searcher = searcher ?? throw new System.ArgumentNullException(nameof(searcher));
        }

        public IndexSearcher Searcher { get; }

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName, 
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}