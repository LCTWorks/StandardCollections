using StandardCollections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace StandardCollections
{
    internal class CollectionProxy<T>
    {
        private ICollection<T> collection;
        public CollectionProxy(ICollection<T> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = new T[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }
        }
    }
    internal class ReadOnlyCollectionProxy<T>
    {
        private IReadOnlyCollection<T> collection;
        public ReadOnlyCollectionProxy(IReadOnlyCollection<T> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                List<T> list = new List<T>(collection.Count);
                foreach (T item in this.collection)
                {
                    list.Add(item);
                }
                return list.ToArray();
            }
        }
    }
    internal class EnumerableProxy<T>
    {
        private IEnumerable<T> collection;
        public EnumerableProxy(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = collection.ToArray();
                return array;
            }
        }
    }
    internal class MatrixProxy<T>
    {
        private IMatrix<T> collection;
        public MatrixProxy(IMatrix<T> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public MatrixEntry<T>[] Items
        {
            get
            {
                MatrixEntry<T>[] array = collection.ToArray();
                return array;
            }
        }
    }
    internal class LookupProxy<TKey, TValue>
    {
        private ILookup<TKey, TValue> collection;
        public LookupProxy(ILookup<TKey, TValue> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public string[] Items
        {
            get
            {
                List<string> array = new List<string>(collection.Count);
                foreach (var group in collection)
                {
                    string key = (group.Key == null) ? string.Empty : group.Key.ToString();
                    foreach (var item in group)
                    {
                        string local = string.Format("Key={0},  Value={1}", key, item.ToString());
                        array.Add(local);
                    }
                }
                return array.ToArray();
            }
        }
    }
}
