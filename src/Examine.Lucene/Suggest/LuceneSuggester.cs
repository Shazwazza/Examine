using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.Lucene.Suggest
{
    public class LuceneSuggester : BaseLuceneSuggester, IDisposable
    {
        private readonly ReaderManager _readerManager;
        private readonly FieldValueTypeCollection _fieldValueTypeCollection;
        private bool _disposedValue;
        private ControlledRealTimeReopenThread<DirectoryReader> _nrtSuggesterReopenThread;
        public LuceneSuggester(string name, ReaderManager readerManager, FieldValueTypeCollection fieldValueTypeCollection, Analyzer suggestionSearchAnalyzer = null)
            : base(name, suggestionSearchAnalyzer)
        {
            _readerManager = readerManager;
            _fieldValueTypeCollection = fieldValueTypeCollection;
            _nrtSuggesterReopenThread = null;
        }

        public LuceneSuggester(string name, LuceneIndex luceneIndex, FieldValueTypeCollection fieldValueTypeCollection, Analyzer suggestionSearchAnalyzer = null)
         : base(name, suggestionSearchAnalyzer)
        {
            TrackingIndexWriter writer = luceneIndex.IndexWriter;
            var suggesterManager = new ReaderManager(writer.IndexWriter, true);
            suggesterManager.AddListener(luceneIndex);

            _nrtSuggesterReopenThread = new ControlledRealTimeReopenThread<DirectoryReader>(writer, suggesterManager, 5.0, 1.0)
            {
                Name = $"{Name} Suggester Index {luceneIndex.Name} NRT Reopen Thread",
                IsBackground = true
            };

            _nrtSuggesterReopenThread.Start();

            // wait for most recent changes when first creating the suggester
            luceneIndex.WaitForChanges();
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
                    if (_nrtSuggesterReopenThread != null)
                    {
                        _nrtSuggesterReopenThread.Interrupt();
                        _nrtSuggesterReopenThread.Dispose();
                    }
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
