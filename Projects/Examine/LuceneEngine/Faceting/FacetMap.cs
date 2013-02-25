using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// Maps facets to indices. Global for all searchers.
    /// </summary>
    public class FacetMap : IEnumerable<KeyValuePair<int, FacetKey>>
    {                

        public List<FacetKey> Keys { get; private set; }


        private ConcurrentDictionary<FacetKey, int> _indices = new ConcurrentDictionary<FacetKey, int>();

        public FacetMap()
        {
            Keys = new List<FacetKey>();
        }

        public int GetIndex(FacetKey key)
        {
            int index;
            return _indices.TryGetValue(key, out index) ? index : -1;
        }

        public int Register(FacetKey key)
        {
            return _indices.GetOrAdd(key, k =>
                {
                    lock (Keys)
                    {
                        Keys.Add(k);
                        return Keys.Count;
                    }
                });
        }

        public IEnumerator<KeyValuePair<int, FacetKey>> GetEnumerator()
        {
            var n = Keys.Count;
            for( var i = 0; i < n; i++ )
            {
                yield return new KeyValuePair<int, FacetKey>(i, Keys[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
