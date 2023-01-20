using System;
using System.Collections.Generic;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Util;

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
        private readonly Dictionary<string, Lookup> _suggesters;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="readerManager">Reader Manager for IndexReader on the Suggester Index</param>
        /// <param name="fieldValueTypeCollection">Fields of Suggester Index</param>
        public SuggesterContext(ReaderManager readerManager, FieldValueTypeCollection fieldValueTypeCollection, SuggesterDefinitionCollection suggesterDefinitions,
            Dictionary<string, Lookup> suggesters)
        {
            _readerManager = readerManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
            _suggesterDefinitions = suggesterDefinitions;
            _suggesters = suggesters;
        }

        /// <inheritdoc>/>
        public IIndexReaderReference GetIndexReader() => new IndexReaderReference(_readerManager);

        public SuggesterDefinitionCollection GetSuggesterDefinitions() => _suggesterDefinitions;

        public TLookup GetSuggester<TLookup>(string name) where TLookup : Lookup
        {
            if(_suggesters.TryGetValue(name, out var suggester))
            {
                return suggester as TLookup;
            }
            throw new ArgumentException(name, nameof(name));
        }

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return _fieldValueTypeCollection.GetValueType(
                fieldName,
                _fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }

        public LuceneVersion GetLuceneVersion() => LuceneVersion.LUCENE_48;
    }
}
