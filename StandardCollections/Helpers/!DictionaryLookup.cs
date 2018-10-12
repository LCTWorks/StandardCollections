using System;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections
{
    internal class DictionaryLookup<TKey, TValue>
    {
        internal Dictionary<TKey, List<TValue>> dict;

        public DictionaryLookup(IEqualityComparer<TKey> comparer)
        {
            dict = new Dictionary<TKey, List<TValue>>(comparer);
        }

        public bool ContainsKey(TKey key)
        {
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// If the key is not present, creates a new entry with the key and returns a new associated 
        /// list. Else if the key is present returns the list associated (if associated list is null 
        /// returns a new list after associating it).
        /// </summary>
        public List<TValue> GetOrCreate(TKey key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            var list = new List<TValue>();
            dict[key] = list;
            return list;
        }
        public List<TValue> GetIfExists(TKey key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return null;
        }
        public bool Create(TKey key)
        {
            if (dict.ContainsKey(key))
            {
                return false;
            }
            var list = new List<TValue>();
            dict[key] = list;
            return true;
        }

        public int RemoveKey(TKey key)
        {
            if (!dict.ContainsKey(key))
            {
                return -1;
            }
            var list = dict[key];
            dict.Remove(key);
            return list.Count;
        }
    }
}
