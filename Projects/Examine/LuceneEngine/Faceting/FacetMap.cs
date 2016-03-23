using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// Maps facets to indices
    /// </summary>
    /// <remarks>
    /// A FacetMap is attached to the FacetConfiguration. It is possible that a FacetMap could be shared among multiple configurations and thus 
    /// multiple searchers
    /// </remarks>
    public class FacetMap : IEnumerable<KeyValuePair<int, FacetKey>>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FacetMap()
        {
            Keys = new List<FacetKey>();
        }        

        /// <summary>
        /// Gets the list of facet keys for this map
        /// </summary>
        internal List<FacetKey> Keys { get; }
        
        private readonly ConcurrentDictionary<FacetKey, int> _indices = new ConcurrentDictionary<FacetKey, int>();

        private readonly SortedDictionary<string, List<FacetKey>> _keysByFieldName = new SortedDictionary<string, List<FacetKey>>(StringComparer.OrdinalIgnoreCase);
        
        private readonly ConcurrentDictionary<long, FacetReferenceInfo[]> _referenceInfo = new ConcurrentDictionary<long, FacetReferenceInfo[]>();

        /// <summary>
        /// Returns the index for the facet key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetIndex(FacetKey key)
        {
            int index;
            return _indices.TryGetValue(key, out index) ? index : -1;
        }

        public bool TryGetReferenceInfo(long reference, out FacetReferenceInfo[] info)
        {
            return _referenceInfo.TryGetValue(reference, out info);
        }

        /// <summary>
        /// Registers a facet key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the field names
        /// </summary>
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
            var keysList = new List<List<FacetKey>>(fieldNames.Length);
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
