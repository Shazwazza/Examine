using System;
using System.Collections;
using System.Collections.Generic;

namespace Examine
{
    internal class CaseInsensitiveKeyReadOnlyDictionary<TVal> : IReadOnlyDictionary<string, TVal>
    {
        public CaseInsensitiveKeyReadOnlyDictionary(ICollection<KeyValuePair<string, TVal>> dictionary)
        {
            var copied = new KeyValuePair<string, TVal>[dictionary.Count];
            dictionary.CopyTo(copied, 0);
            foreach (var c in copied)
            {
                _dictionary[c.Key] = c.Value;
            }
        }

        private readonly Dictionary<string, TVal> _dictionary = new Dictionary<string, TVal>(StringComparer.OrdinalIgnoreCase); 

        public IEnumerator<KeyValuePair<string, TVal>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(string key, out TVal value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TVal this[string key]
        {
            get { return _dictionary[key]; }
        }

        public IEnumerable<string> Keys
        {
            get { return _dictionary.Keys; }
        }

        public IEnumerable<TVal> Values
        {
            get { return _dictionary.Values; }
        }
    }
}