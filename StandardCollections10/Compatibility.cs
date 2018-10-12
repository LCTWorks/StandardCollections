using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using StandardCollections.ObjectModel;

namespace StandardCollections
{
    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods that supports compatibility views between <see cref="System.Collections.Generic"/> collection interfaces.
    /// </summary>
    public static class Compatibility
    {
        /// <summary>
        /// Returns the specified <see cref="T:System.Collections.Generic.ICollection`1"/> wrapped in a <see cref="T:System.Collections.Generic.IReadOnlyCollection`1"/> object.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection to wrap in the <see cref="T:System.Collections.Generic.IReadOnlyCollection`1"/> object.</param>
        /// <returns>Returns a <see cref="T:System.Collections.Generic.IReadOnlyCollection`1"/> wrapper of the specified collection.</returns>
        public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(ICollection<T> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            if (collection is IReadOnlyCollection<T>)
            {
                return (collection as IReadOnlyCollection<T>);
            }
            return new CompatibleCollection<T>(collection);
        }
        /// <summary>
        /// Returns the specified <see cref="T:System.Collections.Generic.IList`1"/> wrapped in a <see cref="T:System.Collections.Generic.IReadOnlyList`1"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to wrap in the <see cref="T:System.Collections.Generic.IReadOnlyList`1"/>.</param>
        /// <returns>Returns a <see cref="T:System.Collections.Generic.IReadOnlyList`1"/> wrapper of the specified list.</returns>
        public static IReadOnlyList<T> AsReadOnlyList<T>(IList<T> list)
        {
            if (list == null)
            {
                Thrower.ArgumentNullException(ArgumentType.list);
            }
            if (list is IReadOnlyList<T>)
            {
                return (list as IReadOnlyList<T>);
            }
            return new CompatibileList<T>(list);
        }
        /// <summary>
        /// Returns the specified <see cref="T:System.Collections.Generic.IDictionary`2"/> wrapped in a <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"/> object.
        /// </summary>
        /// <typeparam name="TKey">The type of the _keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to wrap in the <see cref="T:System.Collections.Generic.IReadOnlyCollection`1"/> object.</param>
        /// <returns>Returns a <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"/> wrapper of the specified dictionary.</returns>
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                Thrower.ArgumentNullException(ArgumentType.dictionary);
            }
            if (dictionary is IReadOnlyDictionary<TKey, TValue>)
            {
                return (dictionary as IReadOnlyDictionary<TKey, TValue>);
            }
            return new CompatibleDictionary<TKey, TValue>(dictionary);
        }
        /// <summary>
        /// Returns the specified <see cref="T:System.Collections.Generic.IReadOnlyCollection`1"/> wrapped in a read-only <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the read-only collection.</typeparam>
        /// <param name="collection">The read-only collection to wrap.</param>
        /// <returns>Returns a read-only <see cref="T:System.Collections.Generic.ICollection`1"/> wrapper of the specified read-only collection.</returns>
        public static ICollection<T> AsCollection<T>(IReadOnlyCollection<T> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            return new CompatibleCollection<T>(collection);
        }
        /// <summary>
        /// Returns the specified <see cref="T:System.Collections.Generic.IReadOnlyList`1"/> wrapped in a read-only <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the read-only list.</typeparam>
        /// <param name="list">The read-only list to wrap.</param>
        /// <returns>Returns a read-only <see cref="T:System.Collections.Generic.IList`1"/> wrapper of the specified read-only list.</returns>
        public static IList<T> AsList<T>(IReadOnlyList<T> list)
        {
            if (list == null)
            {
                Thrower.ArgumentNullException(ArgumentType.list);
            }
            return new CompatibileList<T>(list);
        }
        /// <summary>
        /// Returns the specified <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"/> wrapped in a read-only <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of _keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The read-only dictionary to wrap.</param>
        /// <returns>Returns a read-only <see cref="T:System.Collections.Generic.IDictionary`2"/> wrapper of the specified read-only dictionary.</returns>
        public static IDictionary<TKey, TValue> AsDictionary<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                Thrower.ArgumentNullException(ArgumentType.dictionary);
            }
            return new CompatibleDictionary<TKey, TValue>(dictionary);
        }
        /// <summary>
        /// Returns an <see cref="T:Academy.Collections.Generic.ISortedCollection`1"/> implementation based on the specified <see cref="T:System.Collections.Generic.SortedSet`1"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sorted set.</typeparam>
        /// <param name="collection">The sorted set to wrap in the <see cref="T:Academy.Collections.Generic.ISortedCollection`1"/> implementation.</param>
        /// <returns>a <see cref="T:Academy.Collections.Generic.ISortedCollection`1"/> implementation that wraps the specified sorted set.</returns>
        public static ISortedCollection<T> AsSortedCollection<T>(SortedSet<T> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            return new SortedSetWrapper<T>(collection);
        }
        /// <summary>
        /// Returns an <see cref="T:Academy.Collections.Generic.IMatrix`1"/> implementation based on the specified two dimensional <see cref="T:System.Array"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to wrap.</param>
        /// <returns>a <see cref="T:Academy.Collections.Generic.IMatrix`1"/> implementation that wraps the specified two dimensional array.</returns>
        public static IMatrix<T> AsMatrix<T>(T[,] array)
        {
            if (array == null)
            {
                Thrower.ArgumentNullException(ArgumentType.array);
            }
            return new ArrayGenericMatrix<T>(array);
        }

        #region Private Region

        [DebuggerTypeProxy(typeof(CollectionProxy<>)), DebuggerDisplay("Count = {Count}")]
        private class CompatibleCollection<T> : ICollection<T>, IReadOnlyCollection<T>
        {
            private IReadOnlyCollection<T> collection;

            public CompatibleCollection(IReadOnlyCollection<T> collection)
            {
                this.collection = collection;
            }
            public CompatibleCollection(ICollection<T> collection)
            {
                this.collection = new CollectionWrapper<T>(collection);
            }

            public void Add(T item)
            {
                Thrower.NotSupportedException();
            }
            public void Clear()
            {
                Thrower.NotSupportedException();
            }
            public bool Contains(T item)
            {
                return collection.Contains(item);
            }
            public void CopyTo(T[] array, int arrayIndex)
            {
                CollectionHelper.CopyTo(array, arrayIndex, Count, this);
            }
            public int Count
            {
                get { return collection.Count; }
            }
            public bool IsReadOnly
            {
                get { return true; }
            }
            public bool Remove(T item)
            {
                Thrower.NotSupportedException();
                return false;
            }
            public IEnumerator<T> GetEnumerator()
            {
                return collection.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

         
        }

        private class CollectionWrapper<T> : IReadOnlyCollection<T>
        {
            internal ICollection<T> collection;
            public CollectionWrapper(ICollection<T> collection)
            {
                this.collection = collection;
            }

            public int Count
            {
                get { return collection.Count; }
            }
            public IEnumerator<T> GetEnumerator()
            {
                return collection.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [DebuggerTypeProxy(typeof(CollectionProxy<>)), DebuggerDisplay("Count = {Count}")]
        private class CompatibileList<T> : IList<T>, IReadOnlyList<T>
        {
            private IReadOnlyList<T> list;

            public CompatibileList(IReadOnlyList<T> list)
            {
                this.list = list;
            }
            public CompatibileList(IList<T> list)
            {
                this.list = new ListWrapper<T>(list);
            }

            public int IndexOf(T item)
            {
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                for (int i = 0; i < Count; i++)
                {
                    if (comparer.Equals(list[i], item))
                    {
                        return i;
                    }
                }
                return -1;
            }
            public void Insert(int index, T item)
            {
                Thrower.NotSupportedException();
            }
            public void RemoveAt(int index)
            {
                Thrower.NotSupportedException();
            }
            public T this[int index]
            {
                get
                {
                    return list[index];
                }
                set
                {
                    Thrower.NotSupportedException();
                }
            }
            public void Add(T item)
            {
                Thrower.NotSupportedException();
            }
            public void Clear()
            {
                Thrower.NotSupportedException();
            }
            public bool Contains(T item)
            {
                return list.Contains(item);
            }
            public void CopyTo(T[] array, int arrayIndex)
            {
                CollectionHelper.CopyTo(array, arrayIndex, Count, this);
            }
            public int Count
            {
                get { return list.Count; }
            }
            public bool IsReadOnly
            {
                get { return true; }
            }
            public bool Remove(T item)
            {
                Thrower.NotSupportedException();
                return false;
            }
            public IEnumerator<T> GetEnumerator()
            {
                return list.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        private class ListWrapper<T> : IReadOnlyList<T>
        {
            private IList<T> list;

            public ListWrapper(IList<T> list)
            {
                this.list = list;
            }
            public T this[int index]
            {
                get { return list[index]; }
            }
            public int Count
            {
                get { return list.Count; }
            }
            public IEnumerator<T> GetEnumerator()
            {
                return list.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [DebuggerTypeProxy(typeof(CollectionProxy<>)), DebuggerDisplay("Count = {Count}")]
        private class CompatibleDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        {
            private IReadOnlyDictionary<TKey, TValue> dictionary;

            public CompatibleDictionary(IReadOnlyDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }
            public CompatibleDictionary(IDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = new DictionaryWrapper<TKey, TValue>(dictionary);
            }

            public void Add(TKey key, TValue value)
            {
                Thrower.NotSupportedException();
            }
            public bool ContainsKey(TKey key)
            {
                return dictionary.ContainsKey(key);
            }
            public ICollection<TKey> Keys
            {
                get
                {
                    IEnumerable<TKey> keys = dictionary.Keys;
                    if ((keys is ICollection<TKey>))
                    {
                        return (keys as ICollection<TKey>);
                    }
                    if (keys is IReadOnlyCollection<TKey>)
                    {
                        return new CompatibleCollection<TKey>(keys as IReadOnlyCollection<TKey>);
                    }
                    return new EnumerableCollection<TKey>(keys);
                }
            }
            public bool Remove(TKey key)
            {
                Thrower.NotSupportedException();
                return false;
            }
            public bool TryGetValue(TKey key, out TValue value)
            {
                return dictionary.TryGetValue(key, out value);
            }
            public ICollection<TValue> Values
            {
                get
                {
                    IEnumerable<TValue> values = dictionary.Values;
                    if ((values is ICollection<TValue>))
                    {
                        return (values as ICollection<TValue>);
                    }
                    if (values is IReadOnlyCollection<TValue>)
                    {
                        return new CompatibleCollection<TValue>(values as IReadOnlyCollection<TValue>);
                    }
                    return new EnumerableCollection<TValue>(values);
                }
            }
            public TValue this[TKey key]
            {
                get
                {
                    return dictionary[key];
                }
                set
                {
                    Thrower.NotSupportedException();
                }
            }
            public void Add(KeyValuePair<TKey, TValue> item)
            {
                Thrower.NotSupportedException();
            }
            public void Clear()
            {
                Thrower.NotSupportedException();
            }
            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return dictionary.Contains(item);
            }
            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                CollectionHelper.CopyTo(array, arrayIndex, Count, this);
            }
            public int Count
            {
                get { return dictionary.Count; }
            }
            public bool IsReadOnly
            {
                get { return true; }
            }
            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                Thrower.NotSupportedException();
                return false;
            }
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return dictionary.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
            {
                get
                {
                    return dictionary.Keys;
                }
            }
            IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
            {
                get
                {
                    return dictionary.Values;
                }
            }
        }

        private class DictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        {
            private IDictionary<TKey, TValue> dictionary;

            public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }

            public bool ContainsKey(TKey key)
            {
                return dictionary.ContainsKey(key);
            }
            public IEnumerable<TKey> Keys
            {
                get { return dictionary.Keys; }
            }
            public bool TryGetValue(TKey key, out TValue value)
            {
                return dictionary.TryGetValue(key, out value);
            }
            public IEnumerable<TValue> Values
            {
                get { return dictionary.Values; }
            }
            public TValue this[TKey key]
            {
                get { return dictionary[key]; }
            }
            public int Count
            {
                get { return dictionary.Count; }
            }
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return dictionary.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [DebuggerTypeProxy(typeof(EnumerableProxy<>)), DebuggerDisplay("Count = {Count}")]
        private class SortedSetWrapper<T> : ISortedCollection<T>
        {
            private readonly SortedSet<T> sortedSet;
            public SortedSetWrapper(SortedSet<T> sortedSet)
            {
                this.sortedSet = sortedSet;
            }
            public IComparer<T> Comparer
            {
                get { return this.sortedSet.Comparer; }
            }
            public T MinValue
            {
                get { return this.sortedSet.Min; }
            }
            public T MaxValue
            {
                get { return this.sortedSet.Max; }
            }
            public int Count
            {
                get { return this.sortedSet.Count; }
            }
            public bool Contains(T item)
            {
                return this.sortedSet.Contains(item);
            }
            public IEnumerator<T> GetEnumerator()
            {
                return this.sortedSet.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [DebuggerTypeProxy(typeof(CollectionProxy<>)), DebuggerDisplay("Count = {Count}")]
        private class EnumerableCollection<T> : ICollection<T>, IReadOnlyCollection<T>
        {
            private IEnumerable<T> sequence;
            public EnumerableCollection(IEnumerable<T> sequence)
            {
                this.sequence = sequence;
            }

            public void Add(T item)
            {
                Thrower.NotSupportedException();
            }
            public void Clear()
            {
                Thrower.NotSupportedException();
            }
            public bool Contains(T item)
            {
                return sequence.Contains(item);
            }
            public void CopyTo(T[] array, int arrayIndex)
            {
                CollectionHelper.CopyTo(array, arrayIndex, Count, this);
            }
            public int Count
            {
                get { return sequence.Count(); }
            }
            public bool IsReadOnly
            {
                get { return true; }
            }
            public bool Remove(T item)
            {
                Thrower.NotSupportedException();
                return false;
            }
            public IEnumerator<T> GetEnumerator()
            {
                return sequence.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        [DebuggerTypeProxy(typeof(MatrixProxy<>)), DebuggerDisplay("Count = {Count}")]
        private class ArrayGenericMatrix<T> : IMatrix<T>
        {
            private Array array;
            public ArrayGenericMatrix(Array array)
            {
                this.array = array;
            }
            public int Count
            {
                get
                {
                    return array.Length;
                }
            }
            public T this[int rowIndex, int columnIndex]
            {
                get
                {
                    return (T)array.GetValue(rowIndex, columnIndex);
                }
                set
                {
                    array.SetValue(value, rowIndex, columnIndex);
                }
            }
            public int ColumnCount
            {
                get { return array.GetLength(1); }
            }
            public int RowCount
            {
                get { return array.GetLength(0); }
            }
            public bool Contains(T item)
            {
                if (item == null)
                {
                    foreach (var local in array)
                    {
                        if (local == null)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
                    foreach (var local2 in array)
                    {
                        if (comparer.Equals((T)local2, item))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            public IEnumerator<MatrixEntry<T>> GetEnumerator()
            {
                int row = 0;
                int col = 0;
                int count = 0;
                int colCount = ColumnCount;
                foreach (var local in array)
                {
                    row = count % colCount;
                    col = count / colCount;
                    yield return new MatrixEntry<T>(row, col, (T)local);
                    count++;
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion
    }
}
