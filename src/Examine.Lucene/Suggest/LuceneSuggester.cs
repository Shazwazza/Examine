using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Index;

namespace Examine.Lucene.Suggest
{
    public class LuceneSuggester : BaseLuceneSuggester, IDisposable
    {
        private readonly ReaderManager _readerManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private bool _disposedValue;

        public LuceneSuggester(string name, ReaderManager readerManager, FieldValueTypeCollection fieldValueTypeCollection, Analyzer suggestionSearchAnalyzer = null)
            : base(name, suggestionSearchAnalyzer)
        {
            _readerManager = readerManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
        }
        public override ISuggesterContext GetSuggesterContext() => new SuggesterContext(_readerManager, _fieldValueTypeCollection);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _readerManager.Dispose();
                }

                _disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
