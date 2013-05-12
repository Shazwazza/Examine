using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Examine.LuceneEngine.Faceting
{
    public class ReaderData
    {
        public IndexReader Reader { get; set; }
        public static readonly string ExternalIdFieldName = "__ExternalId";

        public FacetLevel[][] FacetLevels { get; private set; }

        public Dictionary<int, Filter> FacetFilters { get; private set; }

        /// <summary>
        /// This can be used in scorers for fast retrieval of external IDs.        
        /// </summary>
        public long[] ExternalIds { get; private set; }


        public object[] ExternalData { get; private set; }

        public ReaderData(FacetConfiguration config, IndexReader reader)
        {
            Reader = reader;
            //Read external IDs from index
            ReadExternalIds(reader);

            //Attach external data (if any)
            ReadExternalData(config);

            //Load facet ID's and levels from index reader.            
            ReadFacets(config, reader);
        }

        private void ReadExternalIds(IndexReader reader)
        {
            var buffer = new byte[16];
            ExternalIds = new long[reader.MaxDoc()];

            var tp = reader.TermPositions(ExternalIdField.Term);
            try
            {
                while (tp.Next())
                {
                    int doc = tp.Doc();
                    tp.NextPosition();
                    tp.GetPayload(buffer, 0);

                    var id = PayloadDataTokenStream.GetLongValue(buffer);

                    ExternalIds[doc] = id;
                }
            }
            finally
            {
                tp.Close();
            }

        }

        private void ReadExternalData(FacetConfiguration config)
        {
            if (config == null) return;

            var provider = config.ExternalDataProvider;
            if (provider != null)
            {
                ExternalData = new object[ExternalIds.Length];
                var ids = ExternalIds;
                for (int i = 0, n = ids.Length; i < n; i++)
                {
                    ExternalData[i] = provider.GetData(ids[i]);
                }
            }
        }

        private void ReadFacets(FacetConfiguration config, IndexReader reader)
        {
            if (config == null) return;
            var unbackedFacets = new HashSet<int>();

            var levels = new List<FacetLevel>[reader.MaxDoc()];
            var map = config.FacetMap;
            foreach (var fe in config.FacetExtractors)
            {
                foreach (var df in fe.GetDocumentFacets(reader, config))
                {
                    if (levels[df.DocumentId] == null) levels[df.DocumentId] = new List<FacetLevel>();

                    var facetId = map.Register(df.Key);
                    if (!df.TermBased || config.CacheAllQueryFilters)
                    {
                        unbackedFacets.Add(facetId);
                    }

                    levels[df.DocumentId].Add(new FacetLevel { FacetId = facetId, Level = df.Level });
                }
            }

            FacetLevels = new FacetLevel[levels.Length][];
            for (int i = 0, n = levels.Length; i < n; i++)
            {
                var levelsArray = levels[i] == null ? new FacetLevel[0] : levels[i].ToArray();
                foreach (var c in config.FacetCombs)
                {
                    levelsArray = c.Comb(levelsArray);
                }
                FacetLevels[i] = levelsArray;
            }


            //Intialize doc id sets for each used facet
            var facetFilters = new Dictionary<int, DocCollector>();

            for (int i = 0; i < FacetLevels.Length; i++)
            {
                foreach (var facetLevel in FacetLevels[i])
                {
                    DocCollector docs;
                    if (!facetFilters.TryGetValue(facetLevel.FacetId, out docs))
                    {
                        //If the facet is backed by a term there may only be performance reasons to cache the filter.
                        facetFilters.Add(facetLevel.FacetId, docs =
                            unbackedFacets.Contains(facetLevel.FacetId) ?
                                                                            new DocCollector(FacetLevels.Length) : null);
                    }
                    if (docs != null) docs.Add(i);
                }
            }

            FacetFilters = new Dictionary<int, Filter>(facetFilters.Count);
            foreach (var kv in facetFilters)
            {
                if (kv.Value == null)
                {
                    var facet = map.Keys[kv.Key];
                    FacetFilters.Add(kv.Key,
                        //new CachingWrapperFilter( <-- The CachingWrapperFilter is a way to still store the doc sets but in a garbage collectable way.
                        new QueryWrapperFilter(new TermQuery(new Term(facet.FieldName, facet.Value))))
                        //)
                        ;
                }
                else
                {
                    FacetFilters.Add(kv.Key, kv.Value.GetFilter());
                }
            }

        }

        /// <summary>
        /// Internal class to construct DocIdFilters for Lucene. 
        /// If only a few documents have a facet SortedVIntList is used to save memory. Otherwise OpenBitSet
        /// </summary>
        class DocCollector
        {
            private readonly int _bitSetLimit;
            private readonly int _maxDoc;

            private OpenBitSet _bitSet;
            private List<int> _docs;

            public DocCollector(int maxDoc)
            {
                _bitSetLimit = 3 * ((maxDoc / 32) / 4) / 4; //~Size of bitset in bytes divided by size of int. A little less because bit sets are faster.
                _maxDoc = maxDoc;
                _docs = new List<int>(_bitSetLimit);
            }

            /// <summary>
            /// Docs must be added in increasing order
            /// </summary>
            /// <param name="doc"></param>
            public void Add(int doc)
            {
                if (_bitSet != null) _bitSet.FastSet(doc);
                else
                {
                    _docs.Add(doc);
                    if (_docs.Count == _bitSetLimit)
                    {
                        _bitSet = new OpenBitSet(_maxDoc);
                        foreach (var d in _docs)
                        {
                            _bitSet.FastSet(d);
                        }
                        _docs = null;
                    }
                }
            }

            public Filter GetFilter()
            {
                return new SimpleFilter(GetDocSet());
            }

            private DocIdSet GetDocSet()
            {
                return _bitSet ?? (DocIdSet)new SortedVIntList(_docs.ToArray());
            }



            class SimpleFilter : Filter
            {
                private readonly DocIdSet _docs;

                public SimpleFilter(DocIdSet docs)
                {
                    _docs = docs;
                }

                public override DocIdSet GetDocIdSet(IndexReader reader)
                {
                    return _docs;
                }
            }
        }
    }
}
