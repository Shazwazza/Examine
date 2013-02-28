using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Faceting
{
    public class ReaderDataCollection : IEnumerable<ReaderData>
    {
        private Dictionary<IndexReader, DataWithOffset> _readerData;

        private DataWithOffset[] _readers;


        public int DocumentCount { get; set; }

        public ISearcherContext SearcherContext { get; private set; }

        private ReaderDataCollection()
        {
            _readerData = new Dictionary<IndexReader, DataWithOffset>();
        }

        
        private void Initialize()
        {
            _readers = _readerData.Values.ToArray();
        }

        internal static ReaderDataCollection FromReader(IndexReader reader, ISearcherContext context, ReaderDataCollection old = null)
        {
            var data = new ReaderDataCollection();

            data.SearcherContext = context;

            data.AugmentReader(reader, old, 0);

            data.DocumentCount = reader.MaxDoc();

            data.Initialize();
            return data;
        }

        void AugmentReader(IndexReader reader, ReaderDataCollection oldData, int offset)
        {
            var subReaders = reader.GetSequentialSubReaders();
            if (subReaders != null
                && subReaders.All(sr => sr != reader)) // <- Why/why not? If a reader for some reason should return it self this check avoids infinite recursion.
            {
                //Data is added for each segment reader. Normal an IndexReader consists of one or more segment readers.
                //Those are the readers collectors etc. will meet.                
                var subOffset = offset;
                foreach (var r in subReaders)
                {
                    AugmentReader(r, oldData, offset + subOffset);                    
                    subOffset += r.MaxDoc();
                }
                return;
            }

            //If we got here, the reader doesn't have any/is a sequential sub readers. Treat it as one reader.

            ReaderData data;
            if (oldData == null || (data = oldData[reader]) == null)
            {
                data = new ReaderData(this, reader);
            }


            _readerData.Add(reader, new DataWithOffset(offset, reader.MaxDoc(), data));
        }

        internal static ReaderDataCollection Join(IEnumerable<ReaderDataCollection> data, ISearcherContext context)
        {
            var joinedData = new Dictionary<IndexReader, DataWithOffset>();
            var offset = 0;
            foreach (var d in data)
            {                
                foreach (var kv in d._readerData)
                {
                    joinedData[kv.Key] = new DataWithOffset(kv.Value.Offset + offset, kv.Value.Length, kv.Value.Data);
                }
                offset += d.DocumentCount;
            }            

            var collection = new ReaderDataCollection { _readerData = joinedData };
            collection.SearcherContext = context;
            collection.DocumentCount = offset;
            collection.Initialize();
            return collection;
        }

        public ReaderData GetData(int docId, out int localDocId)
        {
            foreach( var data in _readers )
            {
                if( docId >= data.Offset && docId < data.Offset + data.Length )
                {
                    localDocId = docId - data.Offset;
                    return data.Data;
                }
            }

            throw new ArgumentOutOfRangeException("docId");
        }

        public FacetLevel[] GetFacets(int docId)
        {
            int localId;
            return GetData(docId, out localId).FacetLevels[localId];
        }

        public long GetExternalId(int docId)
        {
            int localId;
            return GetData(docId, out localId).ExternalIds[localId];
        }


        public ReaderData this[IndexReader reader]
        {
            get
            {
                DataWithOffset data;
                return _readerData.TryGetValue(reader, out data) ? data.Data : null;
            }            
        }

      
        struct DataWithOffset
        {
            public int Offset;
            public int Length;
            public ReaderData Data;

            public DataWithOffset(int offset, int length, ReaderData data)
            {
                Offset = offset;
                Length = length;
                Data = data;
            }
        }

        public IEnumerator<ReaderData> GetEnumerator()
        {
            return _readers.Select(v => v.Data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
