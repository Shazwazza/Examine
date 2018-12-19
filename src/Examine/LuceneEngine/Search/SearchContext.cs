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

        public IEnumerable<IIndexValueType> ValueTypes => _fieldValueTypeCollection.ValueTypes;

        public IIndexValueType GetValueType(string fieldName)
        {
            return _fieldValueTypeCollection.GetValueType(fieldName);
        }
    }
}