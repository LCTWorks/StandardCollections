using StandardCollections.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections
{
    internal class HashCollection<T> : ICollection<T>, ISet<T>, IIndexedSet<T>
    {
        #region Private
        private const int max = 2147483647;

        internal IEqualityComparer<T> comparer;

        private int[] indices;
        private HashEntry[] entries;
        private int freeIndex;
        internal int lastIndex;

        private int version;
        private int count;
        #endregion

        public int BlankCount
        {
            get
            {
                return indices.Length - count;
            }
        }
        public int Count
        {
            get
            {
                return count;
            }
        }

        public HashCollection()
            : this(0, EqualityComparer<T>.Default)
        {

        }
        public HashCollection(int capacity)
            : this(capacity, EqualityComparer<T>.Default)
        {

        }
        public HashCollection(IEqualityComparer<T> comparer)
            : this(0, comparer)
        {

        }
        public HashCollection(int capacity, IEqualityComparer<T> comparer)
        {
            if (capacity < 0)
            {
                capacity = 0;
            }
            Initialize(capacity);
            this.comparer = comparer;
        }

        public bool Add(T item)
        {
            return InternalAdd(item, false);
        }
        public bool AddOrModify(T item)
        {
            return InternalAdd(item, true);
        }

        private bool InternalAdd(T item, bool modifyIfFound)
        {
            if (indices == null)
            {
                Initialize(0);
            }
            int hashCode = GetHashCode(item);
            int index = hashCode % indices.Length;
            for (int i = indices[index] - 1; i >= 0; i = entries[i].nextIndex)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].item, item))
                {
                    if (modifyIfFound)
                    {
                        entries[i].item = item;
                    }
                    return false;
                }
            }
            int index2;
            if (freeIndex >= 0)
            {
                index2 = freeIndex;
                freeIndex = entries[index2].nextIndex;
            }
            else
            {
                if (lastIndex == entries.Length)
                {
                    GrowSize();
                    index = hashCode % indices.Length;
                }
                index2 = lastIndex;
                lastIndex++;
            }
            entries[index2].item = item;
            entries[index2].hashCode = hashCode;
            entries[index2].nextIndex = indices[index] - 1;
            indices[index] = index2 + 1;
            version++;
            count++;
            return true;
        }
        public void Clear()
        {
            if (count > 0)
            {
                Array.Clear(entries, 0, lastIndex);
                Array.Clear(indices, 0, indices.Length);
                freeIndex = -1;
                lastIndex = 0;
                count = 0;
            }
            version++;
        }
        public void ToZeroCapacity()
        {
            this.Initialize(0);
            this.version++;
        }
        public void ClearBySteps(Action<T> onRemove)
        {
            for (int i = 0; i < lastIndex; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    T obj = entries[i].item;
                    onRemove?.Invoke(obj);
                    entries[i].item = default(T);
                }
            }
            int primeNum = MathHelper.MinPrimeSize;
            entries = new HashEntry[primeNum];
            indices = new int[primeNum];
            freeIndex = -1;
            lastIndex = 0;
        }
        public bool Contains(T item)
        {
            return Contains(item, comparer);
        }
        public bool Contains(T item, IEqualityComparer<T> comparer)
        {
            return FindEntry(item, comparer).HasValue;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, count);
        }
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            CollectionHelper.CopyTo(array, arrayIndex, count, this);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
        public IEnumerable<HashEntry> GetEntries()
        {
            foreach (HashEntry item in entries)
            {
                if (item.hashCode >= 0 && item.item != null)
                {
                    yield return item;
                }
            }
        }
        public IEnumerable<HashEntry> GetEntries(T item, IEqualityComparer<T> comparer)
        {
            int hashCode = GetHashCode(item);
            int index = hashCode % indices.Length;
            for (int i = indices[index] - 1; i >= 0; i = entries[i].nextIndex)
            {
                if ((entries[i].hashCode == hashCode) && comparer.Equals(entries[i].item, item))
                {
                    yield return entries[i];
                }
            }
        }
        public bool GetIndexOrCreate(T item, out int index)
        {
            if (indices == null)
            {
                Initialize(0);
            }
            int hashCode = GetHashCode(item);
            int index3 = hashCode % indices.Length;
            for (int i = indices[index3] - 1; i >= 0; i = entries[i].nextIndex)
            {
                if ((entries[i].hashCode == hashCode) && (comparer.Equals(entries[i].item, item)))
                {
                    index = i;
                    return false;
                }
            }
            int index2;
            if (freeIndex >= 0)
            {
                index2 = freeIndex;
                freeIndex = entries[index2].nextIndex;
            }
            else
            {
                if (lastIndex == entries.Length)
                {
                    GrowSize();
                    index3 = hashCode % indices.Length;
                }
                index2 = lastIndex;
                lastIndex++;
            }
            entries[index2].item = item;
            entries[index2].hashCode = hashCode;
            entries[index2].nextIndex = indices[index3] - 1;
            indices[index3] = index2 + 1;
            version++;
            count++;
            index = index2;
            return true;
        }
        public T GetItemAt(int index)
        {
            return entries[index].item;
        }
        public int IndexOf(T item)
        {
            int hashCode = GetHashCode(item);
            int index = hashCode % indices.Length;
            for (int i = indices[index] - 1; i >= 0; i = entries[i].nextIndex)
            {
                if ((entries[i].hashCode == hashCode) && (comparer.Equals(entries[i].item, item)))
                {
                    return i;
                }
            }
            return -1;
        }
        public bool Remove(T item)
        {
            return Remove(item, comparer);
        }
        public bool Remove(T item, IEqualityComparer<T> comparer)
        {
            if (this.indices != null)
            {
                int hashCode = GetHashCode(item);
                int index = hashCode % indices.Length;
                int previous = -1;
                for (int i = indices[index] - 1; i >= 0; i = entries[i].nextIndex)
                {
                    bool flag = comparer.Equals(entries[i].item, item);
                    if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].item, item))
                    {
                        if (previous < 0)
                        {
                            indices[index] = entries[i].nextIndex + 1;
                        }
                        else
                        {
                            entries[previous].nextIndex = entries[i].nextIndex;
                        }
                        entries[i].hashCode = -1;
                        entries[i].nextIndex = freeIndex;
                        entries[i].item = default(T);
                        freeIndex = i;
                        version++;
                        count--;
                        if (count == 0)
                        {
                            freeIndex = -1;
                            lastIndex = 0;
                        }
                        else
                        {
                            freeIndex = i;
                        }
                        return true;
                    }
                    previous = i;
                }
            }
            return false;
        }
        public HashEntry RemoveEntry(T item)
        {
            return RemoveEntry(item, comparer);
        }
        public HashEntry RemoveEntry(T item, IEqualityComparer<T> comparer)
        {
            if (this.indices != null)
            {
                int hashCode = GetHashCode(item);
                int index = hashCode % indices.Length;
                int previous = -1;
                for (int i = indices[index] - 1; i >= 0; i = entries[i].nextIndex)
                {
                    bool flag = comparer.Equals(entries[i].item, item);
                    if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].item, item))
                    {
                        if (previous < 0)
                        {
                            indices[index] = entries[i].nextIndex + 1;
                        }
                        else
                        {
                            entries[previous].nextIndex = entries[i].nextIndex;
                        }
                        HashEntry entry = entries[i];
                        entries[i].hashCode = -1;
                        entries[i].nextIndex = freeIndex;
                        entries[i].item = default(T);
                        freeIndex = i;
                        version++;
                        count--;
                        if (count == 0)
                        {
                            freeIndex = -1;
                            lastIndex = 0;
                        }
                        else
                        {
                            freeIndex = i;
                        }
                        return entry;
                    }
                    previous = i;
                }
            }
            return HashEntry.EmptySearchValue;
        }
        public int RemoveAll(T item, Action<T> onRemove)
        {
            int hashCode = GetHashCode(item);
            int index = hashCode % indices.Length;
            int previous = -1;
            int nextIndex = -1;
            int count = 0;
            for (int i = indices[index] - 1; i >= 0; i = nextIndex)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].item, item))
                {
                    if (previous < 0)
                    {
                        indices[index] = entries[i].nextIndex;
                    }
                    else
                    {
                        entries[previous].nextIndex = entries[i].nextIndex;
                    }
                    onRemove?.Invoke(entries[i].item);
                    nextIndex = entries[i].nextIndex;
                    entries[i].hashCode = -1;
                    entries[i].nextIndex = freeIndex;
                    entries[i].item = default(T);
                    freeIndex = i;
                    count--;
                    if (count == 0)
                    {
                        freeIndex = -1;
                        lastIndex = 0;
                    }
                    else
                    {
                        freeIndex = i;
                    }
                }
                previous = i;
            }
            version++;
            return count;
        }
        public void TrimExcess()
        {
            int num = (int)((entries.Length - 1) * (0.9));
            if (this.Count < num)
            {
                int capacity = MathHelper.GetNextPrime(this.count);
                SetCapacity(capacity);
            }
        }
        public T GetFirst()
        {
            int index = 0;
            while ((index < this.lastIndex))
            {
                if (this.entries[index].hashCode >= 0)
                {
                    return entries[index].item;
                }
                index++;
            }
            return default(T);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (count != 0)
            {
                if (other == this)
                {
                    Clear();
                }
                else
                {
                    foreach (var item in other)
                    {
                        this.Remove(item);
                    }
                }
            }
        }
        public void IntersectWith(IEnumerable<T> other)
        {
            if (count > 0)
            {
                if (IsWellKnownCollection(other, out int num))
                {
                    if (num > 0)
                    {
                        this.Clear();
                        return;
                    }
                    HashCollection<T> collection = GetHashCollectionWithSameEC(other);
                    if (collection != null)
                    {
                        for (int i = 0; i < lastIndex; i++)
                        {
                            if (entries[i].hashCode >= 0)
                            {
                                T obj = entries[i].item;
                                if (!collection.Contains(obj))
                                {
                                    this.Remove(obj);
                                }
                            }
                        }
                        return;
                    }
                }
                BitArray bits = new BitArray(Count);
                foreach (T item in other)
                {
                    int index = IndexOf(item);
                    if (index >= 0)
                    {
                        bits[index] = true;
                    }
                }
                for (int j = 0; j < lastIndex; j++)
                {
                    if ((entries[j].hashCode >= 0) && (bits[j]))
                    {
                        Remove(entries[j].item);
                    }
                }
            }
        }
        private static bool IsWellKnownCollection(IEnumerable<T> collection, out int count)
        {
            if (collection is ICollection<T> ic)
            {
                count = ic.Count;
                return true;
            }
            if (collection is IReadOnlyCollection<T> ic2)
            {
                count = ic2.Count;
                return true;
            }
            if (collection is ICollection ic3)
            {
                count = ic3.Count;
                return true;
            }
            count = -1;
            return false;
        }
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (CollectionHelper.IsWellKnownCollection(other, out int num))
            {
                if (count == 0)
                {
                    return (num > 0);
                }
                HashCollection<T> collection = GetHashCollectionWithSameEC(other);
                if (collection != null)
                {
                    if (this.count >= collection.count)
                    {
                        return false;
                    }
                    foreach (T item in this)
                    {
                        if (!collection.Contains(item))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            IndexedSetHelper<T> setHelper = new IndexedSetHelper<T>(this, lastIndex);
            return setHelper.IsProperSubsetOf(other);
        }
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (this.count > 0)
            {
                return false;
            }
            if (CollectionHelper.IsWellKnownCollection(other, out int num))
            {
                if (num == 0)
                {
                    return true;
                }
                HashCollection<T> collection = GetHashCollectionWithSameEC(other);
                if (collection != null)
                {
                    if (this.count <= collection.Count)
                    {
                        return false;
                    }
                    foreach (T item in other)
                    {
                        if (!this.Contains(item))
                        {
                            return false;
                        }
                    }
                }
            }
            IndexedSetHelper<T> setHelper = new IndexedSetHelper<T>(this, lastIndex);
            return setHelper.IsProperSupersetOf(other);
        }
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (this.count == 0)
            {
                return true;
            }
            HashCollection<T> collection = GetHashCollectionWithSameEC(other);
            if (collection != null)
            {
                if (this.count > collection.Count)
                {
                    return false;
                }
                foreach (T item in this)
                {
                    if (!collection.Contains(item))
                    {
                        return false;
                    }
                }
                return true;
            }
            IndexedSetHelper<T> setHelper = new IndexedSetHelper<T>(this, lastIndex);
            return setHelper.IsSubsetOf(other);
        }
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (CollectionHelper.IsWellKnownCollection(other, out int num))
            {
                if (num == 0)
                {
                    return true;
                }
                HashCollection<T> collection = GetHashCollectionWithSameEC(other);
                if ((collection != null) && (this.count < collection.Count))
                {
                    return false;
                }
            }
            foreach (T item in other)
            {
                if (!this.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }
        public bool Overlaps(IEnumerable<T> other)
        {
            if (this.count > 0)
            {
                foreach (T item in other)
                {
                    if (this.Contains(item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool SetEquals(IEnumerable<T> other)
        {
            HashCollection<T> collection = GetHashCollectionWithSameEC(other);
            if (collection != null)
            {
                if (this.count != collection.Count)
                {
                    return false;
                }
                foreach (T item in other)
                {
                    if (!this.Contains(item))
                    {
                        return false;
                    }
                }
                return true;
            }
            if ((CollectionHelper.IsWellKnownCollection(other, out int num)) && (this.count == 0) && (num > 0))
            {
                return false;
            }
            IndexedSetHelper<T> setHelper = new IndexedSetHelper<T>(this, lastIndex);
            return setHelper.SetEquals(other);
        }
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (count == 0)
            {
                UnionWith(other);
            }
            else if (other == this)
            {
                Clear();
            }
            else
            {
                if ((other is HashCollection<T> collection) && (this.comparer.Equals(collection.comparer)))
                {
                    foreach (T item in other)
                    {
                        if (!this.Remove(item))
                        {
                            this.Add(item);
                        }
                    }
                }
                else
                {
                    int lastIndex = this.lastIndex;
                    BitArray flags = new BitArray(lastIndex);
                    BitArray flags2 = new BitArray(lastIndex);
                    foreach (var item in other)
                    {
                        if (GetIndexOrCreate(item, out int index))
                        {
                            flags2[index] = true;
                        }
                        else if ((index < lastIndex) && (!flags2[index]))
                        {
                            flags[index] = true;
                        }
                    }
                    for (int i = 0; i < lastIndex; i++)
                    {
                        if (flags[i])
                        {
                            Remove(entries[i].item);
                        }
                    }
                }
            }
        }
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                this.Add(item);
            }
        }

        internal HashEntry FindEntry(T item)
        {
            return FindEntry(item, this.comparer);
        }
        internal HashEntry FindEntry(T item, IEqualityComparer<T> comparer)
        {
            int hashCode = GetHashCode(item);
            int index = hashCode % indices.Length;
            for (int i = indices[index] - 1; i >= 0; i = entries[i].nextIndex)
            {
                if ((entries[i].hashCode == hashCode) && (comparer.Equals(entries[i].item, item)))
                {
                    return entries[i];
                }
            }
            return HashEntry.EmptySearchValue;
        }

        private void Initialize(int capacity)
        {
            int primeNum = MathHelper.GetNextPrime(capacity);
            entries = new HashEntry[primeNum];
            indices = new int[primeNum];
            freeIndex = -1;
            lastIndex = 0;
        }
        private int GetHashCode(T item)
        {
            if (item == null)
            {
                return 0;
            }
            return (comparer.GetHashCode(item)) & max;
        }
        private void SetCapacity(int capacity)
        {
            HashEntry[] newEntries = new HashEntry[capacity];
            int[] newIndices = new int[capacity];
            if (this.entries != null)
            {
                Array.Copy(entries, 0, newEntries, 0, lastIndex);
            }
            for (int i = 0; i < lastIndex; i++)
            {
                int newIndex = newEntries[i].hashCode % capacity;
                newEntries[i].nextIndex = newIndices[newIndex] - 1;
                newIndices[newIndex] = i + 1;
            }
            this.indices = newIndices;
            this.entries = newEntries;
        }
        private void GrowSize()
        {
            this.SetCapacity(MathHelper.GetGrowedPrime(this.count));
        }
        private HashCollection<T> GetHashCollectionWithSameEC(IEnumerable<T> other)
        {
            if ((other is HashCollection<T> collection) && (this.comparer.Equals(collection.comparer)))
            {
                return collection;
            }
            return null;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        public struct HashEntry
        {
            internal static readonly HashEntry EmptySearchValue = new HashEntry { item = default(T), hashCode = -1, nextIndex = -1 };

            internal T item;
            internal int hashCode;
            internal int nextIndex;

            public bool HasValue
            {
                get
                {
                    return this.hashCode != (-1);
                }
            }

        }

        public struct Enumerator : IEnumerator<T>
        {
            internal HashCollection<T> collection;
            internal T current;
            internal int index;
            internal int version;

            public Enumerator(HashCollection<T> collection)
            {
                this.collection = collection;
                this.version = collection.version;
                this.index = 0;
                this.current = default(T);
            }

            public T Current
            {
                get
                {
                    return current;
                }
            }
            public void Dispose()
            {

            }
            public bool MoveNext()
            {
                if (version != collection.version)
                {
                    Thrower.EnumeratorCorrupted();
                }
                while ((index < collection.lastIndex))
                {
                    if (collection.entries[index].hashCode >= 0)
                    {
                        current = collection.entries[index].item;
                        index++;
                        return true;
                    }
                    index++;
                }
                index = collection.lastIndex + 1;
                current = default(T);
                return false;
            }

            public void Reset()
            {
                if (version != collection.version)
                {
                    Thrower.EnumeratorCorrupted();
                }
                index = 0;
                current = default(T);
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                    }
                    if (index == (collection.count + 1))
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                    }
                    return current;
                }
            }
        }

    }
}
