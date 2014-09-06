using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
        
        private ConcurrentDictionary<long, FacetReferenceInfo[]> _referenceInfo = new ConcurrentDictionary<long, FacetReferenceInfo[]>();

        public FacetMap()
        {
            Keys = new List<FacetKey>();
        }        

        public int GetIndex(FacetKey key)
        {
            int index;
            return _indices.TryGetValue(key, out index) ? index : -1;
        }

        public bool TryGetReferenceInfo(long reference, out FacetReferenceInfo[] info)
        {
            return _referenceInfo.TryGetValue(reference, out info);
        }

        public int Register(FacetKey key)
        {
            key = key.Intern();

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

                        var id = Keys.Count - 1;
                        var reference = key as FacetReferenceKey;
                        if (reference != null)
                        {
                            var nameAndId = new FacetReferenceInfo(key.FieldName, id);
                            _referenceInfo.AddOrUpdate(reference.ReferenceId, _=>new[] {nameAndId}, (_, ids) =>
                                {
                                    var newIds = ids.ToList();
                                    newIds.Add(nameAndId);
                                    return newIds.ToArray();
                                });
                        }
                        return id;
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

        public IEnumerable<KeyValuePair<int, FacetKey>> GetByFieldNames(params string[] fieldNames)
        {
            List<List<FacetKey>> keysList = new List<List<FacetKey>>(fieldNames.Length);
            lock (Keys)
            {
                List<FacetKey> keys;
                foreach (var fieldName in fieldNames)
                {
                    if (_keysByFieldName.TryGetValue(fieldName, out keys))
                    {
                        keysList.Add(keys);
                    }
                }
            }

            foreach (var keys in keysList)
            {
                var n = keys.Count;
                for (var i = 0; i < n; i++)
                {
                    yield return new KeyValuePair<int, FacetKey>(i, keys[i]);
                }
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
