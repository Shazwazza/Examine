using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Search
{
    public class SearchContext : ISearchContext
    {
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;

        public SearchContext(FieldValueTypeCollection fieldValueTypeCollection, Searcher searcher)
        {
            _fieldValueTypeCollection = fieldValueTypeCollection;
            Searcher = searcher;
        }

        public Searcher Searcher { get; }

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName, 
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}