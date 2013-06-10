using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetsLoader : ISearcherWarmer, IDisposable
    {
        public FacetConfiguration Configuration { get; private set; }

        readonly ConcurrentDictionary<object, ReaderData> _readerData = new ConcurrentDictionary<object, ReaderData>();

        private readonly Thread _cleanThread;

        public FacetsLoader(FacetConfiguration configuration)
        {            
            Configuration = configuration;
            _cleanThread = new Thread(Clean);
            _cleanThread.Start();
        }

        public FacetMap FacetMap { get { return Configuration != null && !Configuration.IsEmpty ? Configuration.FacetMap : null; }}        


        public ReaderData GetReaderData(IndexReader reader)
        {
            return _readerData[reader.GetFieldCacheKey()];
        }

        public void Warm(IndexSearcher s)
        {
            foreach (var reader in s.GetIndexReader().GetAllSubReaders())
            {
                AugmentReader(reader);
            }
        }

        void AugmentReader(IndexReader reader)
        {            
            _readerData.GetOrAdd(reader.GetFieldCacheKey(), (key) => new ReaderData(Configuration, reader))
                       .Reader = reader;

            //In the very rare event that the cleaner didn't get the updated reader and removed the data, they are created again.
            _readerData.GetOrAdd(reader.GetFieldCacheKey(), (key) => new ReaderData(Configuration, reader));
        }

        public void Dispose()
        {
            _finish = true;
            _waitHandle.Set();

            _cleanThread.Join();
        }

        private ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);
        private bool _finish;

        public void Clean()
        {
            while (!_finish)
            {
                foreach (var data in _readerData.Values)
                {
                    if (data.Reader.GetRefCount() == 0)
                    {
                        if (data.Reader.GetRefCount() == 0)
                        {
                            ReaderData old;
                            _readerData.TryRemove(data.Reader.GetFieldCacheKey(), out old);
                        }

                    }
                }

                _waitHandle.Wait(TimeSpan.FromSeconds(.1d));
            }
        }
    }
}