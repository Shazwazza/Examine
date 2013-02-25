using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;

namespace Examine.LuceneEngine.Faceting
{
    public class IndexReaderDataCollection : IEnumerable<KeyValuePair<IndexReader, IndexReaderData>>
    {
        private Dictionary<IndexReader, IndexReaderData> _readerData;

        public ISearcherContext SearcherContext { get; private set; }

        private IndexReaderDataCollection()
        {
            _readerData = new Dictionary<IndexReader, IndexReaderData>();
        }

        internal static IndexReaderDataCollection FromReader(IndexReader reader, ISearcherContext context, IndexReaderDataCollection old = null)
        {
            var data = new IndexReaderDataCollection();

            data.SearcherContext = context;

            data.AugmentReader(reader, old);

            return data;
        }

        void AugmentReader(IndexReader reader, IndexReaderDataCollection oldData)
        {
            var subReaders = reader.GetSequentialSubReaders();
            if (subReaders != null
                && subReaders.All(sr => sr != reader)) // <- Why/why not? If a reader for some reason should return it self this check avoids infinite recursion.
            {
                //Data is added for each segment reader. Normal an IndexReader consists of one or more segment readers.
                //Those are the readers collectors etc. will meet.                
                foreach (var r in subReaders)
                {
                    AugmentReader(r, oldData);
                }
                return;
            }

            //If we got here, the reader doesn't have any/is a sequential sub readers. Treat it as one reader.

            IndexReaderData data;
            if (oldData == null || (data = oldData[reader]) == null)
            {
                data = new IndexReaderData(this, reader);
            }

            this[reader] = data;
        }

        internal static IndexReaderDataCollection Join(IEnumerable<IndexReaderDataCollection> data, ISearcherContext context)
        {
            var joinedData = new Dictionary<IndexReader, IndexReaderData>();
            foreach (var d in data)
            {
                foreach (var kv in d._readerData)
                {
                    joinedData[kv.Key] = kv.Value;
                }
            }

            var collection = new IndexReaderDataCollection { _readerData = joinedData };
            collection.SearcherContext = context;
            return collection;
        }

        public IndexReaderData this[IndexReader reader]
        {
            get
            {
                IndexReaderData data;
                return _readerData.TryGetValue(reader, out data) ? data : null;
            }
            internal set { _readerData[reader] = value; }
        }

        public IEnumerator<KeyValuePair<IndexReader, IndexReaderData>> GetEnumerator()
        {
            return _readerData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
