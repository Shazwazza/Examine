using System;
using Lucene.Net.Index;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Reference to an instance of an IndexReader on a Lucene Index
    /// </summary>
    public class IndexReaderReference : IIndexReaderReference
    {
        private readonly ReaderManager _readerManager;
        private bool _disposedValue;
        private DirectoryReader _indexReader;

        public IndexReaderReference(ReaderManager readerManager)
        {
            _readerManager = readerManager;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
