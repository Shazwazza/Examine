using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Lucene.Indexing;
using Lucene.Net.Index;

namespace Examine.Lucene.Suggest
{
    public class SuggesterContext : ISuggesterContext
    {
        private ReaderManager _readerManager;
        private FieldValueTypeCollection _fieldValueTypeCollection;

        public SuggesterContext(ReaderManager readerManager, FieldValueTypeCollection fieldValueTypeCollection)
        {
            _readerManager = readerManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
        }

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
