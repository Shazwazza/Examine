using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Examine.LuceneEngine.Cru;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Faceting
{
    public class FacetsLoader : ISearcherWarmer, IDisposable
    {
        public FacetConfiguration Configuration { get; private set; }

        readonly ConditionalWeakTable<object, ReaderData> _readerData = new ConditionalWeakTable<object, ReaderData>();        
        
        public FacetsLoader(FacetConfiguration configuration)
        {                        
            Configuration = configuration;            
        }

        public FacetMap FacetMap { get { return Configuration != null && !Configuration.IsEmpty ? Configuration.FacetMap : null; }}        


        public ReaderData GetReaderData(IndexReader reader)
        {
            return _readerData.GetValue(reader.GetFieldCacheKey(), key => new ReaderData(Configuration, reader));
        }

        public void Warm(IndexSearcher s)
        {
            foreach (var reader in s.GetIndexReader().GetAllSubReaders())
            {
                AugmentReader(reader);
            }
        }

        ReaderData AugmentReader(IndexReader reader)
        {
            var data = GetReaderData(reader);
            data.Reader = reader;

            return data;
        }

        public void Dispose()
        {           
        }
        
    }
}