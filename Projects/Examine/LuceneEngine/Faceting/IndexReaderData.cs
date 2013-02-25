using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Examine.LuceneEngine.Faceting
{
    public class IndexReaderData
    {
        public FacetLevel[][] FacetLevels { get; private set; }

        public Dictionary<int, DocIdSet> FacetFilters { get; private set; }

        public IndexReaderData(IndexReaderDataCollection collection, IndexReader reader)
        {
            //Load facet id's and levels from index reader.

            var levels = new List<FacetLevel>[reader.MaxDoc()];
            var config = collection.SearcherContext.Searcher.FacetConfiguration;
            if (config != null)
            {
                foreach (var fe in config.FacetExtractors)
                {
                    foreach (var df in fe.GetDocumentFacets(reader, config))
                    {
                        if (levels[df.DocumentId] == null) levels[df.DocumentId] = new List<FacetLevel>();

                        levels[df.DocumentId].Add(new FacetLevel { FacetId = config.FacetMap.Register(df.Key), Level = df.Level });
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
                    foreach (var facet in FacetLevels[i])
                    {
                        DocCollector docs;
                        if (!facetFilters.TryGetValue(facet.FacetId, out docs))
                        {
                            facetFilters.Add(facet.FacetId, docs = new DocCollector(FacetLevels.Length));
                        }
                        docs.Add(i);
                    }
                }

                FacetFilters = new Dictionary<int, DocIdSet>(facetFilters.Count);
                foreach (var kv in facetFilters)
                {
                    FacetFilters[kv.Key] = kv.Value.GetDocSet();
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

            public DocIdSet GetDocSet()
            {
                return _bitSet ?? (DocIdSet)new SortedVIntList(_docs.ToArray());
            }
        }
    }
}
