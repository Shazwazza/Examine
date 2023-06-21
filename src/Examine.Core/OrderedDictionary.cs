using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Examine
{
    /// <summary>
    /// An implementation of a generic ordered dictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    public class OrderedDictionary<TKey, TVal> : KeyedCollection<TKey, KeyValuePair<TKey, TVal>>, IDictionary<TKey, TVal>, IReadOnlyDictionary<TKey, TVal> where TKey : notnull
    {
        /// <inheritdoc/>
        public OrderedDictionary()
        {
        }

        /// <inheritdoc/>
        public OrderedDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        /// <inheritdoc/>
        public TVal GetItem(int index)
        {
            if (index >= Count) throw new IndexOutOfRangeException();

            var found = base[index];

            return base[found.Key].Value;
        }

        /// <inheritdoc/>
        public int IndexOf(TKey key)
        {
            if (base.Dictionary == null) return -1;
            if (base.Dictionary.TryGetValue(key, out KeyValuePair<TKey, TVal> found))
            {
                return base.Items.IndexOf(found);
            }
            return -1;
        }

        /// <inheritdoc/>
        protected override TKey GetKeyForItem(KeyValuePair<TKey, TVal> item)
        {
            return item.Key;
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {            
            return base.Contains(key);
        }

        /// <inheritdoc/>
        public void Add(TKey key, TVal value)
        {
            if (base.Contains(key)) throw new ArgumentException("The key " + key + " already exists in this collection");

            base.Add(new KeyValuePair<TKey, TVal>(key, value));
        }

        /// <inheritdoc/>
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        // Justification for warning disabled: IDictionary is missing [MaybeNullWhen(false)] in Netstandard 2.1
        public bool TryGetValue(TKey key,
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#if !NETSTANDARD2_0
            [MaybeNullWhen(false)]
#endif
            out TVal value)
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

        TVal IReadOnlyDictionary<TKey, TVal>.this[TKey key] => ((IDictionary<TKey, TVal>)this)[key];

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TVal>.Keys => Keys;

        IEnumerable<TVal> IReadOnlyDictionary<TKey, TVal>.Values => Values;

        TVal IDictionary<TKey, TVal>.this[TKey key]
        {
            get
            {
                if (base.Dictionary != null && 
                    base.Dictionary.TryGetValue(key, out KeyValuePair<TKey, TVal> found))
                {
                    return found.Value;
                }
                throw new KeyNotFoundException();
            }
            set
            {
                if (base.Dictionary != null && 
                    base.Dictionary.TryGetValue(key, out KeyValuePair<TKey, TVal> found))
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

        private static readonly ICollection<TKey> EmptyCollection = new List<TKey>();
        private static readonly ICollection<TVal> EmptyValues = new List<TVal>();

        /// <inheritdoc/>
        public ICollection<TKey> Keys => base.Dictionary != null ? base.Dictionary.Keys : EmptyCollection;

        /// <inheritdoc/>
        public ICollection<TVal> Values => base.Dictionary != null ? base.Dictionary.Values.Select(x => x.Value).ToArray() : EmptyValues;
    }
}
