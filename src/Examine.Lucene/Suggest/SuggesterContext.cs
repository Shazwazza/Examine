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
        private readonly SuggesterDefinitionCollection _suggesterDefinitions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="readerManager">Reader Manager for IndexReader on the Suggester Index</param>
        /// <param name="fieldValueTypeCollection">Fields of Suggester Index</param>
        public SuggesterContext(ReaderManager readerManager, FieldValueTypeCollection fieldValueTypeCollection, SuggesterDefinitionCollection suggesterDefinitions)
        {
            _readerManager = readerManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
            _suggesterDefinitions = suggesterDefinitions;
        }

        /// <inheritdoc>/>
        public IIndexReaderReference GetIndexReader() => new IndexReaderReference(_readerManager);

        public SuggesterDefinitionCollection GetSuggesterDefinitions() => _suggesterDefinitions;

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName,
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}
