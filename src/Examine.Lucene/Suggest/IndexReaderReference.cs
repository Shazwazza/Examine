using System;
using Lucene.Net.Index;

namespace Examine.Lucene.Suggest
{
    public class IndexReaderReference : IIndexReaderReference
    {
        private ReaderManager _readerManager;
        private bool _disposedValue;
        private DirectoryReader _indexReader;
        public IndexReaderReference(ReaderManager readerManager)
        {
            _readerManager = readerManager;
        }

        public DirectoryReader IndexReader
        {
            get
            {
                if (_disposedValue)
                {
                    throw new ObjectDisposedException($"{nameof(IndexReaderReference)} is disposed");
                }
                return _indexReader ?? (_indexReader = _readerManager.Acquire());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_indexReader != null)
                    {
                        _readerManager.Release(_indexReader);
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
