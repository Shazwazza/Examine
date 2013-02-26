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

        private SortedDictionary<string, List<FacetKey>> _keysByFieldName = new SortedDictionary<string, List<FacetKey>>(StringComparer.OrdinalIgnoreCase);

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

                        List<FacetKey> list;
                        if (!_keysByFieldName.TryGetValue(key.FieldName, out list))
                        {
                            _keysByFieldName.Add(key.FieldName, list = new List<FacetKey>());
                        }
                        list.Add(k);

                        return Keys.Count - 1;
                    }
                });
        }

        public IEnumerable<string> FieldNames
        {
            get
            {
                lock (Keys)
                {
                    return _keysByFieldName.Keys.ToArray();
                }
            }
        }

        public IEnumerable<FacetKey> GetByFieldName(string fieldName)
        {
            List<FacetKey> keys;
            lock (Keys)
            {
                if (!_keysByFieldName.TryGetValue(fieldName, out keys))
                {
                    yield break;
                }
            }

            var n = keys.Count;
            for (var i = 0; i < n; i++)
            {
                yield return keys[i];
            }
        }

        public IEnumerator<KeyValuePair<int, FacetKey>> GetEnumerator()
        {
            var n = Keys.Count;
            for (var i = 0; i < n; i++)
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
