using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// An implementation of a generic ordered dictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    internal class OrderedDictionary<TKey, TVal> : KeyedCollection<TKey, KeyValuePair<TKey, TVal>>, IDictionary<TKey, TVal>
    {
        public OrderedDictionary()
        {
        }

        public OrderedDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }
        
        public TVal GetItem(int index)
        {
            if (index >= Count) throw new IndexOutOfRangeException();

            var found = base[index];

            return base[found.Key].Value;
        }

        public int IndexOf(TKey key)
        {
            if (base.Dictionary == null) return -1;
            if (base.Dictionary.TryGetValue(key, out KeyValuePair<TKey, TVal> found))
            {
                return base.Items.IndexOf(found);
            }
            return -1;
        }

        protected override TKey GetKeyForItem(KeyValuePair<TKey, TVal> item)
        {
            return item.Key;
        }

        public bool ContainsKey(TKey key)
        {            
            return base.Contains(key);
        }

        public void Add(TKey key, TVal value)
        {
            if (base.Contains(key)) throw new ArgumentException("The key " + key + " already exists in this collection");

            base.Add(new KeyValuePair<TKey, TVal>(key, value));
        }

        public bool TryGetValue(TKey key, out TVal value)
        {
            if (base.Dictionary == null)
            {
                value = default(TVal);
                return false;
            }

            if (base.Dictionary.TryGetValue(key, out KeyValuePair<TKey, TVal> found))
            {
                value = found.Value;
                return true;
            }

            value = default(TVal);
            return false;
        }

        TVal IDictionary<TKey, TVal>.this[TKey key]
        {
            get
            {
                if (base.Dictionary.TryGetValue(key, out KeyValuePair<TKey, TVal> found))
                {
                    return found.Value;
                }
                return default(TVal);
            }
            set
            {
                if (base.Dictionary.TryGetValue(key, out KeyValuePair<TKey, TVal> found))
                {
                    var index = base.Items.IndexOf(found);
                    base.SetItem(index, new KeyValuePair<TKey, TVal>(key, value));
                }
                else
                {
                    base.Add(new KeyValuePair<TKey, TVal>(key, value));
                }
            }
        }

        public ICollection<TKey> Keys => base.Dictionary.Keys;

        public ICollection<TVal> Values
        {
            get { return base.Dictionary.Values.Select(x => x.Value).ToArray(); }
        }
    }
}