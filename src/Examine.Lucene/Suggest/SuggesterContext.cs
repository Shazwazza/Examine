using Examine.Lucene.Indexing;
using Lucene.Net.Index;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Suggester Context for LuceneSuggester
    /// </summary>
    public class SuggesterContext : ISuggesterContext
    {
        private ReaderManager _readerManager;
        private FieldValueTypeCollection _fieldValueTypeCollection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="readerManager">Reader Manager for IndexReader on the Suggester Index</param>
        /// <param name="fieldValueTypeCollection">Fields of Suggester Index</param>
        public SuggesterContext(ReaderManager readerManager, FieldValueTypeCollection fieldValueTypeCollection)
        {
            _readerManager = readerManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
        }

        /// <inheritdoc>/>
        public IIndexReaderReference GetIndexReader() => new IndexReaderReference(_readerManager);

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName,
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}
