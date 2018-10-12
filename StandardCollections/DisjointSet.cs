using StandardCollections.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace StandardCollections
{
    /// <summary>
    /// Represents a set of values as a collection of disjoint subsets represented each by a key. The subsets can be accessed by the representing key.
    /// </summary>
    /// <typeparam name="TKey">The representation key type for the subsets.</typeparam>
    /// <typeparam name="TValue">The value type to store in the subsets.</typeparam>
    [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(LookupProxy<,>))]
    public class DisjointSet<TKey, TValue> : ILookup<TKey, TValue>, IEnumerable<IGrouping<TKey, TValue>>, IEnumerable
    {
        internal HashCollection<Entry> _set;
        internal EntryEqComparer _entryComparer;
        internal IEqualityComparer<TValue> _valueComparer;
        internal KeySet _keys;
        internal ValueSet _values;
        internal int _version;
        internal int _count;

        /// <summary>
        /// Gets the subset represented by the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        /// The subset represented by the specified key. If the specified key is not found throws a 
        /// <see cref="T:System.Collections.Generic.KeyNotFoundException>"/>.
        /// </returns>
        public Subset this[TKey key]
        {
            get
            {
                Entry entry = FindEntry(key, false);
                if (entry == null)
                {
                    Thrower.KeyNotFoundException(Resources.KeyNotFound_DSetInvalidKey);
                }
                return new Subset(this, entry);
            }
        }
        /// <summary>
        /// Gets a set containing the subsets representation _keys in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> for key management operations.
        /// </summary>
        public KeySet SubsetKeys
        {
            get
            {
                if (this._keys == null)
                {
                    this._keys = new KeySet(this);
                }
                return this._keys;
            }
        }
        /// <summary>
        /// Gets a set containing the values in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> for value management operations.
        /// </summary>
        public ValueSet Values
        {
            get
            {
                if (this._values == null)
                {
                    this._values = new ValueSet(this);
                }
                return this._values;
            }
        }
        /// <summary>
        /// Gets the number of subsets contained in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return this._set.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> class 
        /// that is empty and uses the default equality comparers for the key and value types.
        /// </summary>
        public DisjointSet() : this(null, null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> class 
        /// that is empty and uses the specified equality comparers for both, the key and value type.
        /// </summary>
        /// <param name="keyComparer">
        /// The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use when comparing 
        /// _keys in the disjoint set, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"/> 
        /// implementation for the key type.
        /// </param>
        /// <param name="valueComparer">
        /// The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use when comparing 
        /// values in the disjoint set, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"/> 
        /// implementation for the value type.
        /// </param>
        public DisjointSet(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            this._entryComparer = new EntryEqComparer(keyComparer);
            this._valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
            this._set = new HashCollection<Entry>(1, this._entryComparer);
            this._version = 0;
            this._count = 0;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> class 
        /// that uses the specified equality comparers for both, the key and value type and contains elements copied from the 
        /// specified <see cref="T:System.Linq.ILookup`2"/> object.
        /// </summary>
        /// <param name="lookup">
        /// The <see cref="T:System.Linq.ILookup`2"/> object whose _keys and elements are copied to the new disjoint set.
        /// </param>
        /// <param name="keyComparer">
        /// The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use when comparing 
        /// _keys in the disjoint set, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"/> 
        /// implementation for the key type.
        /// </param>
        /// <param name="valueComparer">
        /// The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> implementation to use when comparing 
        /// values in the disjoint set, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1"/> 
        /// implementation for the value type.
        /// </param>
        public DisjointSet(ILookup<TKey, TValue> lookup, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            if (lookup == null)
            {
                Thrower.ArgumentNullException(ArgumentType.lookup);
            }
            this._entryComparer = new EntryEqComparer(keyComparer);
            this._valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
            this._set = new HashCollection<Entry>(lookup.Count, this._entryComparer);
            foreach (IGrouping<TKey, TValue> group in lookup)
            {
                this._set.GetIndexOrCreate(CreateEntry(group.Key, valueComparer), out int num);
                Entry entry = this._set.GetItemAt(num);
                Subset subset = new Subset(this, entry);
                subset.UnionWith(group);
            }
            this._version = 0;
        }

        /// <summary>
        /// Gets whether the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> contains a subset represented by the specified key.
        /// </summary>
        /// <param name="key">The subset representation key.</param>
        /// <returns>true if the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> contains subset represntd by the specified key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            return this.SubsetKeys.Contains(key);
        }
        /// <summary>
        /// Gets whether the specified value is contained in one of the subsets in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.
        /// </summary>
        /// <param name="value">The value to find in the disjoint set.</param>
        /// <returns>true if the specified value is found; otherwise, false.</returns>
        public bool ContainsValue(TValue value)
        {
            return this.Values.Contains(value);
        }
        /// <summary>
        /// Gets whether the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> has a subset represented by the specified key that contains the specified value.
        /// </summary>
        /// <param name="key">The subset representation key.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>true if the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> contains a subset represented by the key specified and that subset contains the specified value; otherwise false.</returns>
        public bool AreRelated(TKey key, TValue value)
        {
            Entry entry = FindEntry(key, false);
            if (entry != null)
            {
                return entry.subset.Contains(value);
            }
            return false;
        }
        /// <summary>
        /// Gets the contained subset that contains the specified value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.</param>
        /// <returns>A <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> representing the subset that contains the specified value. If no subset contains the value then returns null.</returns>
        public Subset GetSubset(TValue value)
        {
            Entry entry = FindEntryByValue(value);
            if (entry != null)
            {
                return new Subset(this, entry.key);
            }
            return null;
        }
        /// <summary>
        /// Gets an enumerator that iterates through the subsets of the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.
        /// </summary>
        /// <returns>An <see cref="T:Academy.Collections.Generic.DisjointSet`2.Enumerator"/> that iterates through the subsets of the disjoint set.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        #region KeySet, ValueSet and Subset Implementations
        /// <summary>
        /// Represents the set of _keys representing the subsets contained in the 
        /// <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>. This class cannot be inherited.
        /// </summary>
        [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(CollectionProxy<>))]
        public sealed class KeySet : ISet<TKey>, ICollection
        {
            private DisjointSet<TKey, TValue> disjointset;
            private IEnumerable<Entry> AsEntryEnumerable(IEnumerable<TKey> collection)
            {
                return from x in collection select DisjointSet<TKey, TValue>.CreateEntry(x);
            }

            internal KeySet(DisjointSet<TKey, TValue> set)
            {
                this.disjointset = set;
            }

            /// <summary>
            /// Gets the number of _keys contained in the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>.
            /// </summary>
            public int Count
            {
                get
                {
                    return this.disjointset._set.Count;
                }
            }
            /// <summary>
            /// Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> object that is used to determine equality for the _keys in the set.
            /// </summary>
            public IEqualityComparer<TKey> Comparer
            {
                get
                {
                    return disjointset._entryComparer.keyComparer;
                }
            }

            /// <summary>
            /// Adds an empty subset represented by the specified key to the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.
            /// </summary>
            /// <param name="item"></param>
            /// <returns>
            /// true if the subset is added to the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>; otherwise false.
            /// </returns>
            public bool Add(TKey item)
            {
                bool flag = this.disjointset._set.Add(CreateEntry(item, disjointset._valueComparer));
                this.disjointset.Update(0);
                return flag;
            }
            /// <summary>
            /// Removes all subsets from the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>. Values associated will be also removed.
            /// </summary>
            public void Clear()
            {
                this.disjointset._set.Clear();
                this.disjointset.Update(-this.disjointset._count);
            }
            /// <summary>
            /// Gets whether the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> contains a subset represented by the specified key.
            /// </summary>
            /// <param name="key">The representative key.</param>
            /// <returns>true if the collection contains a subset represented by the specified key; otherwise false.</returns>
            public bool Contains(TKey key)
            {
                return this.disjointset._set.Contains(CreateEntry(key));
            }
            /// <summary>
            /// Copies the complete <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> to a compatible one-dimensional array, starting at the beginning of the target array.
            /// </summary>
            /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>.</param>
            public void CopyTo(TKey[] array)
            {
                CopyTo(array, 0, Count);
            }
            /// <summary>
            /// Copies the complete <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> to a compatible one-dimensional array, starting at the specific array index.
            /// </summary>
            /// <param name="array">
            /// A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>. The array must have zero-based indexing
            /// </param>
            /// <param name="index">The zero-based index in array at which copying begins.</param>
            public void CopyTo(TKey[] array, int index)
            {
                CopyTo(array, index, Count);
            }
            /// <summary>
            /// Copies a specific number of elements from the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>
            /// to a compatible one-dimensional array, starting at the specific array index.
            /// </summary>
            /// <param name="array">
            /// A one-dimensional array that is the destination of the elements copied from the 
            /// <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>. The array must have zero-based indexing
            /// </param>
            /// <param name="index">The zero-based index in array at which copying begins.</param>
            /// <param name="count">The number of elements to copy.</param>
            public void CopyTo(TKey[] array, int index, int count)
            {
                CollectionHelper.CopyTo<TKey>(array, index, count, this);
            }

            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>.
            /// </summary>
            /// <returns>A <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet.Enumerator"/> for the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>.</returns>
            public Enumerator GetEnumerator()
            {
                return new Enumerator(this.disjointset);
            }
            /// <summary>
            /// Removes the entire subset represented by the specified key from the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.
            /// </summary>
            /// <param name="key">The representative key.</param>
            /// <returns>true if the key was successfully found and the subset removed; otherwise false.</returns>
            public bool Remove(TKey key)
            {
                HashCollection<Entry>.HashEntry hashEntry = this.disjointset._set.RemoveEntry(CreateEntry(key));
                if (hashEntry.HasValue)
                {
                    this.disjointset.Update(-hashEntry.item.subset.Count);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Removes all _keys in the specified collection from the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>. Values associated with the deleted _keys will be also removed.
            /// </summary>
            /// <param name="other">The collection of _keys to remove from the set.</param>
            public void ExceptWith(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (other == this)
                {
                    this.Clear();
                    this.disjointset.Update(-this.disjointset._count);
                }
                else
                {
                    IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                    this.disjointset._set.ExceptWith(other2);
                    this.disjointset.UpdateCount();
                }
            }
            /// <summary>
            /// Modifies the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> so that it contains only _keys that are also in a specified collection. Values associated with the deleted _keys will be also removed.
            /// </summary>
            /// <param name="other">The collection to compare to the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>.</param>
            public void IntersectWith(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (other != this)
                {
                    IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                    this.disjointset._set.IntersectWith(other2);
                    this.disjointset.UpdateCount();
                }
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is a proper (strict) subset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns> true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is a proper subset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsProperSubsetOf(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (this == other)
                {
                    return false;
                }
                IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                return this.disjointset._set.IsProperSubsetOf(other2);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is a proper (strict) superset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is a proper superset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsProperSupersetOf(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (this == other)
                {
                    return false;
                }
                IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                return this.disjointset._set.IsProperSupersetOf(other2);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is a subset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is a subset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsSubsetOf(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (this == other)
                {
                    return true;
                }
                IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                return this.disjointset._set.IsSubsetOf(other2);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is a superset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is a superset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsSupersetOf(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (this == other)
                {
                    return true;
                }
                IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                return this.disjointset._set.IsSupersetOf(other2);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> overlaps with the specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> and <paramref name="other"/> share at least one common key; otherwise, false.</returns>
            public bool Overlaps(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (this == other)
                {
                    return true;
                }
                IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                return this.disjointset._set.Overlaps(other2);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> and the specified collection contain the same elements.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> is equal to <paramref name="other"/>; otherwise, false.</returns>
            public bool SetEquals(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (this == other)
                {
                    return true;
                }
                IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                return this.disjointset._set.SetEquals(other2);
            }
            /// <summary>
            /// Modifies the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> so that it contains only _keys that are present either in the current set or in the specified collection, but not both.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void SymmetricExceptWith(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (other == this)
                {
                    this.Clear();
                    this.disjointset.Update(-this.disjointset._count);
                }
                else
                {
                    IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                    this.disjointset._set.SymmetricExceptWith(other2);
                    this.disjointset.UpdateCount();
                }
            }
            /// <summary>
            /// Modifies the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/> so that it contains all _keys that are present in either the current set or the specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void UnionWith(IEnumerable<TKey> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (other != this)
                {
                    IEnumerable<Entry> other2 = AsEntryEnumerable(other);
                    this.disjointset._set.UnionWith(other2);
                    this.disjointset.UpdateCount();
                }
            }

            #region Explicit
            void ICollection<TKey>.Add(TKey item)
            {
                this.Add(item);
            }
            bool ICollection<TKey>.IsReadOnly
            {
                get { return false; }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return GetEnumerator();
            }
            void ICollection.CopyTo(Array array, int index)
            {
                CollectionHelper.CopyTo<TKey>(array, index, Count, this);
            }
            bool ICollection.IsSynchronized
            {
                get { return false; }
            }
            object ICollection.SyncRoot
            {
                get { return ((ICollection)disjointset).SyncRoot; }
            }
            #endregion

            /// <summary>
            /// An enumerator that iterates through the elements of a <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>.
            /// </summary>
            [Serializable]
            public struct Enumerator : IEnumerator<TKey>, ISerializable
            {
                private DisjointSet<TKey, TValue> disjointset;
                private IEnumerator<Entry> enumerator;
                private readonly int version;
                private TKey current;

                internal Enumerator(DisjointSet<TKey, TValue> disjointset)
                {
                    this.disjointset = disjointset;
                    this.version = disjointset._version;
                    this.enumerator = disjointset._set.GetEnumerator();
                    this.current = default(TKey);
                }
                internal Enumerator(SerializationInfo info, StreamingContext context)
                {
                    this.disjointset = (DisjointSet<TKey, TValue>)info.GetValue(SerializationString.Collection, typeof(DisjointSet<TKey, TValue>));
                    this.current = (TKey)info.GetValue(SerializationString.Current, typeof(TKey));
                    this.enumerator = disjointset._set.GetEnumerator();
                    IEqualityComparer<TKey> comparer = disjointset._entryComparer.keyComparer;
                    while (enumerator.MoveNext() && (!comparer.Equals(enumerator.Current.Key, current)))
                    {

                    }
                    this.version = info.GetInt32(SerializationString.Version);
                }

                /// <summary>
                /// Gets the key at the current position of the enumerator.
                /// </summary>
                public TKey Current
                {
                    get
                    {
                        return current;
                    }
                }
                /// <summary>
                /// Releases all resources used by the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet.Enumerator"/>.
                /// </summary>
                public void Dispose()
                {

                }
                /// <summary>
                /// Advances the enumerator to the next element of the <see cref="T:Academy.Collections.Generic.DisjointSet`2.KeySet"/>.
                /// </summary>
                /// <returns>true if the enumerator was successfully advanced to the next key; false if the enumerator has passed the end of the collection.</returns>
                public bool MoveNext()
                {
                    if (disjointset._version != version)
                    {
                        Thrower.EnumeratorCorrupted();
                    }
                    if (enumerator.MoveNext())
                    {
                        this.current = enumerator.Current.Key;
                        return true;
                    }
                    return false;
                }

                void IEnumerator.Reset()
                {
                    if (disjointset._version != version)
                    {
                        Thrower.EnumeratorCorrupted();
                    }
                    this.enumerator.Reset();
                }
                object IEnumerator.Current
                {
                    get { return Current; }
                }
                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.AddValue(SerializationString.Collection, disjointset);
                    info.AddValue(SerializationString.Version, version);
                    info.AddValue(SerializationString.Current, current);
                }
            }
        }
        /// <summary>
        /// Represents the set of values contained in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>. 
        /// This class cannot be inherited.
        /// </summary>
        [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(CollectionProxy<>))]
        public sealed class ValueSet : ISet<TValue>, IEnumerable<TValue>, ICollection
        {
            private DisjointSet<TKey, TValue> disjointset;
            internal ValueSet(DisjointSet<TKey, TValue> set)
            {
                this.disjointset = set;
            }

            /// <summary>
            /// Gets the number of values contained in the <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>.
            /// </summary>
            public int Count
            {
                get { return disjointset._count; }
            }
            /// <summary>
            /// Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> object that is used to determine equality for the values in the set.
            /// </summary>
            public IEqualityComparer<TValue> Comparer
            {
                get
                {
                    return disjointset._valueComparer;
                }
            }

            /// <summary>
            /// Adds a specified value to the subset represented by the specified key in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>. 
            /// If no subset with the specified key exists, the method creates it and then adds the value.
            /// </summary>
            /// <param name="key">The subset representation key.</param>
            /// <param name="value">The value to add to the subset.</param>
            /// <returns>true if the value is not contained in any subset in the disjoint set and successfully added; otherwise false.</returns>
            public bool AddTo(TKey key, TValue value)
            {
                if (!this.Contains(value))
                {
                    Entry entry = disjointset.FindEntry(key, true);
                    if (entry.subset.Add(value))
                    {
                        this.disjointset.Update(1);
                        return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// Removes all values from the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>. The operation maintains all contained subsets.
            /// </summary>
            public void Clear()
            {
                foreach (Entry item in disjointset._set)
                {
                    item.subset.ToZeroCapacity();
                }
                disjointset.Update(-disjointset._count);
            }
            /// <summary>
            /// Determines whether the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> contains the specified value.
            /// </summary>
            /// <param name="value">The value to find in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.</param>
            /// <returns>true if the specified value is contained in any subset in the disjoint set; otherwise false.</returns>
            public bool Contains(TValue value)
            {
                return disjointset.FindEntryByValue(value) != null;
            }
            /// <summary>
            /// Copies the complete <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> to a compatible one-dimensional array, starting at the beginning of the target array.
            /// </summary>
            /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>.</param>
            public void CopyTo(TValue[] array)
            {
                CopyTo(array, 0, Count);
            }
            /// <summary>
            /// Copies the complete <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> to a compatible one-dimensional array, starting at the specific array index.
            /// </summary>
            /// <param name="array">
            /// A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>. The array must have zero-based indexing
            /// </param>
            /// <param name="index">The zero-based index in array at which copying begins.</param>
            public void CopyTo(TValue[] array, int index)
            {
                CopyTo(array, index, Count);
            }
            /// <summary>
            /// Copies a specific number of elements from the <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>
            /// to a compatible one-dimensional array, starting at the specific array index.
            /// </summary>
            /// <param name="array">
            /// A one-dimensional array that is the destination of the elements copied from the 
            /// <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>. The array must have zero-based indexing
            /// </param>
            /// <param name="index">The zero-based index in array at which copying begins.</param>
            /// <param name="count">The number of elements to copy.</param>
            public void CopyTo(TValue[] array, int index, int count)
            {
                CollectionHelper.CopyTo<TValue>(array, index, count, this);
            }
            /// <summary>
            /// Removes the specified value from the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.
            /// </summary>
            /// <param name="value">The value to remove.</param>
            /// <returns>true if the specified value is contained in any subset in the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> and successfully removed; otherwise false.</returns>
            public bool Remove(TValue value)
            {
                foreach (Entry item in disjointset._set)
                {
                    if (item.subset.Remove(value))
                    {
                        return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>.
            /// </summary>
            /// <returns>A <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet.Enumerator"/> for the <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>.</returns>
            public Enumerator GetEnumerator()
            {
                return new Enumerator(this.disjointset);
            }

            /// <summary>
            /// Removes all values in the specified collection from the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>.
            /// </summary>
            /// <param name="other">The collection of values to remove from the current set.</param>
            public void ExceptWith(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (other == this)
                {
                    this.Clear();
                }
                Subset subset = GetIfContainedSubset(other);
                if (subset != null)
                {
                    int num = subset.Count;
                    subset.Clear();
                    disjointset.Update(-num);
                }
                else
                {
                    foreach (TValue value in other)
                    {
                        this.Remove(value);
                    }
                }
            }
            /// <summary>
            /// Modifies the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> so that it contains only values that are also in a specified collection
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void IntersectWith(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (Count > 0)
                {
                    if (CollectionHelper.IsWellKnownCollection(other, out int num))
                    {
                        if (num == 0)
                        {
                            this.Clear();
                            return;
                        }
                        Subset subset = GetIfContainedSubset(other);
                        if (subset != null)
                        {
                            foreach (Entry entry in disjointset._set)
                            {
                                if (entry != subset.entry)
                                {
                                    entry.subset.ToZeroCapacity();
                                }
                            }
                            disjointset.Update(Count - disjointset._count);
                            return;
                        }
                    }
                    DisjointSet<TKey, TValue> set2 = new DisjointSet<TKey, TValue>(disjointset.SubsetKeys.Comparer, this.Comparer);
                    foreach (TValue value in other)
                    {
                        foreach (Entry entry2 in disjointset._set)
                        {
                            if (entry2.subset.Remove(value))
                            {
                                this.AddTo(entry2.key, value);
                                break;
                            }
                        }
                    }
                    disjointset._set = set2._set;
                    disjointset.Update(set2._count - disjointset._count);
                }
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is a proper (strict) subset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns> true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is a proper subset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsProperSubsetOf(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (CollectionHelper.IsWellKnownCollection(other, out int num))
                {
                    if (disjointset._count == 0)
                    {
                        return (num > 0);
                    }
                    Subset subset = GetIfContainedSubset(other);
                    if (subset != null)
                    {
                        return false;
                    }
                    ISet<TValue> set = GetIfWellKnownSetImpWithSameEC(other);
                    if (set != null)
                    {
                        if (Count >= set.Count)
                        {
                            return false;
                        }
                        return set.IsProperSupersetOf(this);
                    }
                }
                HashCollection<TValue> collection = new HashCollection<TValue>(this.Count, this.Comparer);
                foreach (TValue item in this)
                {
                    collection.Add(item);
                }
                return SetHelper<TValue>.IsProperSubsetOf(collection, other, collection.lastIndex);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is a proper (strict) superset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is a proper superset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsProperSupersetOf(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (this.Count == 0)
                {
                    return false;
                }
                if (CollectionHelper.IsWellKnownCollection(other, out int num))
                {
                    if (num == 0)
                    {
                        return false;
                    }
                    Subset subset = GetIfContainedSubset(other);
                    if (subset != null)
                    {
                        return subset.Count < this.Count;
                    }
                    ISet<TValue> set = GetIfWellKnownSetImpWithSameEC(other);
                    if (set != null)
                    {
                        if (set.Count >= Count)
                        {
                            return false;
                        }
                        return set.IsProperSubsetOf(this);
                    }
                }
                HashCollection<TValue> collection = new HashCollection<TValue>(Count, Comparer);
                foreach (TValue item in this)
                {
                    collection.Add(item);
                }
                return SetHelper<TValue>.IsProperSupersetOf(collection, other, collection.lastIndex);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is a subset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is a subset of the specified collection; otherwise, false.</returns>
            public bool IsSubsetOf(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (this.Count == 0)
                {
                    return true;
                }
                if (CollectionHelper.IsWellKnownCollection(other, out int num))
                {
                    if (num == 0)
                    {
                        return false;
                    }
                    Subset subset = GetIfContainedSubset(other);
                    if (subset != null)
                    {
                        return Count == subset.Count;
                    }
                    ISet<TValue> set = GetIfWellKnownSetImpWithSameEC(other);
                    if (set != null)
                    {
                        if (Count > set.Count)
                        {
                            return false;
                        }
                        return (other as ISet<TValue>).IsSupersetOf(this);
                    }
                }
                HashCollection<TValue> collection = new HashCollection<TValue>(Count, Comparer);
                foreach (TValue item in this)
                {
                    collection.Add(item);
                }
                return SetHelper<TValue>.IsSubsetOf(collection, other, collection.lastIndex);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is a superset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is a superset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsSupersetOf(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (CollectionHelper.IsWellKnownCollection(other, out int num))
                {
                    if (num == 0)
                    {
                        return true;
                    }
                    Subset subset = GetIfContainedSubset(other);
                    if (subset != null)
                    {
                        return true;
                    }
                    ISet<TValue> set = GetIfWellKnownSetImpWithSameEC(other);
                    if (set != null)
                    {
                        if (Count < set.Count)
                        {
                            return false;
                        }
                        return set.IsSubsetOf(this);
                    }
                }
                HashCollection<TValue> collection = new HashCollection<TValue>(Count, Comparer);
                foreach (TValue item in this)
                {
                    collection.Add(item);
                }
                foreach (TValue item2 in other)
                {
                    if (!collection.Contains(item2))
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> overlaps with the specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> and <paramref name="other"/> share at least one common key; otherwise, false.</returns>
            public bool Overlaps(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (Count == 0)
                {
                    return false;
                }
                if (this == other)
                {
                    return true;
                }
                if (CollectionHelper.IsWellKnownCollection(other, out int num))
                {
                    if (num == 0)
                    {
                        return false;
                    }
                    Subset subset = GetIfContainedSubset(other);
                    if (subset != null)
                    {
                        return (subset.Count > 0);
                    }
                    ISet<TValue> set = GetIfWellKnownSetImpWithSameEC(other);
                    if (set != null)
                    {
                        return set.Overlaps(this);
                    }
                }
                HashCollection<TValue> collection = new HashCollection<TValue>(Count, Comparer);
                foreach (TValue item in this)
                {
                    collection.Add(item);
                }
                return collection.Overlaps(other);
            }
            /// <summary>
            /// Determines whether the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> and the specified collection contain the same elements.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> is equal to <paramref name="other"/>; otherwise, false.</returns>
            public bool SetEquals(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (other == this)
                {
                    return true;
                }
                Subset subset = GetIfContainedSubset(other);
                if (subset != null)
                {
                    return (this.Count == subset.Count);
                }
                ISet<TValue> set = GetIfWellKnownSetImpWithSameEC(other);
                if (set != null)
                {
                    return set.SetEquals(this);
                }
                HashCollection<TValue> collection = new HashCollection<TValue>(Count, Comparer);
                foreach (TValue item in this)
                {
                    collection.Add(item);
                }
                return collection.SetEquals(other);
            }
            /// <summary>
            /// Modifies the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> so that it contains only values that are present either in the current set or in the specified collection, but not both.
            /// New values will be added to the subset represented by the key type default value.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void SymmetricExceptWith(IEnumerable<TValue> other)
            {
                this.SymmetricExceptWith(other, default(TKey));
            }
            /// <summary>
            /// Modifies the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> so that it contains only values that are present either in the current set or in the specified collection, but not both.
            /// New values will be added to the subset represented by the specified key.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <param name="defaultKey">The key which subset will contain the new values added by the current operation.</param>
            public void SymmetricExceptWith(IEnumerable<TValue> other, TKey defaultKey)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (Count == 0)
                {
                    if (disjointset.Values.Count <= 0)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_NoSubsetContained);
                    }
                    Entry entry = disjointset.FindEntry(defaultKey, true);
                    entry.subset.UnionWith(other);
                }
                else
                {
                    if (other == this)
                    {
                        this.Clear();
                    }
                    Subset subset = GetIfContainedSubset(other);
                    if (subset != null)
                    {
                        subset.Clear();
                    }
                    else
                    {
                        ISet<TValue> set = GetIfWellKnownSetImpWithSameEC(other) ?? new HashSet<TValue>(other, Comparer);
                        Entry entry2 = disjointset.FindEntry(defaultKey, true);
                        foreach (TValue item in set)
                        {
                            if (!this.Remove(item))
                            {
                                entry2.subset.Add(item);
                                disjointset.Update(1);
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// Modifies the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> so that it contains all values that are present in either the current set or the specified collection.
            /// New values will be added to the subset represented by the key type default value.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void UnionWith(IEnumerable<TValue> other)
            {
                this.UnionWith(other, default(TKey));
            }
            /// <summary>
            /// Modifies the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/> so that it contains all values that are present in either the current set or the specified collection.
            /// New values will be added to the subset represented by the specified key.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <param name="defaultKey">The key which subset will contain the new values added by the current operation.</param>
            public void UnionWith(IEnumerable<TValue> other, TKey defaultKey)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                if (other != this)
                {
                    if ((!(other is Subset subset)) || (subset.parent != disjointset))
                    {
                        if (disjointset._set.Count == 0)
                        {
                            Thrower.InvalidOperationException(Resources.InvalidOperation_NoSubsetContained);
                        }
                        Entry entry = disjointset.FindEntry(defaultKey, true);
                        HashCollection<TValue> set = entry.subset;
                        int count = disjointset._count;
                        foreach (TValue value in other)
                        {
                            if (!this.Contains(value))
                            {
                                entry.subset.Add(value);
                                count++;
                            }
                        }
                        disjointset.Update(count);
                    }
                }
            }

            private bool IsCompatibleValueSet(IEnumerable<TValue> other)
            {
                if (other is ValueSet values)
                {
                    return values.Comparer.Equals(Comparer);
                }
                return false;
            }
            private ISet<TValue> GetIfWellKnownSetImpWithSameEC(IEnumerable<TValue> other)
            {
                if ((other is ValueSet valueset) && (AreEqualityComparerEquals(Comparer, valueset.Comparer)))
                {
                    return valueset;
                }
                if ((other is HashSet<TValue> hashSet) && (AreEqualityComparerEquals(Comparer, hashSet.Comparer)))
                {
                    return hashSet;
                }
                if ((other is Subset subset) && (AreEqualityComparerEquals(Comparer, subset.parent._valueComparer)))
                {
                    return subset;
                }
                return null;
            }
            private bool AreEqualityComparerEquals(IEqualityComparer<TValue> comparer1, IEqualityComparer<TValue> comparer2)
            {
                return comparer1.Equals(comparer2);
            }
            private Subset GetIfContainedSubset(IEnumerable<TValue> other)
            {
                if ((other is Subset subset) && (subset.parent == disjointset))
                {
                    return subset;
                }
                return null;
            }

            bool ISet<TValue>.Add(TValue item)
            {
                Thrower.NotSupportedException();
                return false;
            }
            void ICollection<TValue>.Add(TValue item)
            {
                Thrower.NotSupportedException();
            }
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            bool ICollection<TValue>.IsReadOnly
            {
                get { return false; }
            }
            void ICollection.CopyTo(Array array, int index)
            {
                CollectionHelper.CopyTo<TValue>(array, index, Count, this);
            }
            bool ICollection.IsSynchronized
            {
                get { return false; }
            }
            object ICollection.SyncRoot
            {
                get { return ((ICollection)disjointset).SyncRoot; }
            }

            /// <summary>
            /// An enumerator that iterates through the elements of a <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>.
            /// </summary>
            [Serializable]
            public struct Enumerator : IEnumerator<TValue>, ISerializable, IDeserializationCallback
            {
                internal DisjointSet<TKey, TValue> disjointset;
                internal int version;
                internal bool notStarted;
                internal bool finished;
                internal IEnumerator<IGrouping<TKey, TValue>> groupingEnumerator;
                internal IEnumerator<TValue> valuesEnumerator;
                internal TValue current;
                internal SerializationInfo sinfo;

                internal Enumerator(DisjointSet<TKey, TValue> set)
                {
                    this.disjointset = set;
                    this.version = set._version;
                    this.groupingEnumerator = this.disjointset.GetEnumerator();
                    this.valuesEnumerator = Enumerable.Empty<TValue>().GetEnumerator();
                    this.current = default(TValue);
                    this.notStarted = true;
                    this.finished = false;
                    this.sinfo = null;
                }
                private Enumerator(SerializationInfo info, StreamingContext context)
                {
                    this.sinfo = info;
                    this.disjointset = null;
                    this.groupingEnumerator = null;
                    this.valuesEnumerator = null;
                    this.version = 0;
                    this.notStarted = false;
                    this.finished = false;
                    this.current = default(TValue);
                }

                /// <summary>
                /// Gets the value at the current position of the enumerator.
                /// </summary>
                public TValue Current
                {
                    get
                    {
                        return current;
                    }
                }
                /// <summary>
                /// Advances the enumerator to the next element of the <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet"/>.
                /// </summary>
                /// <returns>true if the enumerator was successfully advanced to the next value; false if the enumerator has passed the end of the collection.</returns>
                public bool MoveNext()
                {
                    if (this.version != this.disjointset._version)
                    {
                        Thrower.EnumeratorCorrupted();
                    }
                    if (valuesEnumerator.MoveNext())
                    {
                        return SetCurrentValue();
                    }
                    while (groupingEnumerator.MoveNext())
                    {
                        valuesEnumerator = groupingEnumerator.Current.GetEnumerator();
                        if (valuesEnumerator.MoveNext())
                        {
                            return SetCurrentValue();
                        }
                    }
                    valuesEnumerator = Enumerable.Empty<TValue>().GetEnumerator();
                    finished = true;
                    return false;
                }
                /// <summary>
                /// Releases all resources used by the <see cref="T:Academy.Collections.Generic.DisjointSet`2.ValueSet.Enumerator"/>.
                /// </summary>
                public void Dispose()
                {

                }

                private bool SetCurrentValue()
                {
                    this.current = valuesEnumerator.Current;
                    notStarted = false;
                    return true;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if (notStarted)
                        {
                            Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                        }
                        if (finished)
                        {
                            Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                        }
                        return Current;
                    }
                }
                void IEnumerator.Reset()
                {
                    if (this.version != this.disjointset._version)
                    {
                        Thrower.EnumeratorCorrupted();
                    }
                    this.groupingEnumerator = this.disjointset.GetEnumerator();
                    this.valuesEnumerator = Enumerable.Empty<TValue>().GetEnumerator();
                    this.current = default(TValue);
                    this.notStarted = true;
                    this.finished = false;
                }
                void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    if (info == null)
                    {
                        Thrower.ArgumentNullException(ArgumentType.info);
                    }
                    info.AddValue(SerializationString.Collection, this.disjointset, typeof(DisjointSet<TKey, TValue>));
                    info.AddValue(SerializationString.Current, current, typeof(TValue));
                    info.AddValue(SerializationString.NotStarted, notStarted);
                    info.AddValue(SerializationString.Version, version);
                }
                void IDeserializationCallback.OnDeserialization(object sender)
                {
                    if (this.groupingEnumerator == null)
                    {
                        if (this.sinfo == null)
                        {
                            Thrower.SerializationException(Resources.Serialization_InvalidOnDeserialization);
                        }
                        this.disjointset = (DisjointSet<TKey, TValue>)this.sinfo.GetValue(SerializationString.Collection, typeof(DisjointSet<TKey, TValue>));
                        this.version = this.sinfo.GetInt32(SerializationString.Version);
                        this.groupingEnumerator = this.disjointset.GetEnumerator();
                        this.valuesEnumerator = Enumerable.Empty<TValue>().GetEnumerator();
                        TValue value = (TValue)this.sinfo.GetValue(SerializationString.Current, typeof(TValue));
                        IEqualityComparer<TValue> comparer = this.disjointset._valueComparer;
                        this.notStarted = this.sinfo.GetBoolean(SerializationString.NotStarted);
                        if (!this.notStarted)
                        {
                            while (this.MoveNext())
                            {
                                if (comparer.Equals(this.Current, value))
                                {
                                    break;
                                }
                            }
                        }
                    }

                }
            }
        }
        /// <summary>
        /// Represents a set of values mapped to a key contained in a <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.
        /// </summary>
        [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(CollectionProxy<>))]
        public sealed class Subset : ISet<TValue>, IGrouping<TKey, TValue>
        {
            internal TKey key;
            internal DisjointSet<TKey, TValue> parent;
            [NonSerialized]
            internal Entry entry;
            internal int version;

            /// <summary>
            /// Gets the number of values in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    CheckPersistance();
                    return entry.subset.Count;
                }
            }
            /// <summary>
            /// Gets the key associated to the current collection.
            /// </summary>
            public TKey Key
            {
                get
                {
                    CheckPersistance();
                    return this.key;
                }
            }

            internal Subset(DisjointSet<TKey, TValue> set, TKey key)
            {
                this.parent = set;
                this.key = key;
                this.version = this.parent._version - 1;
                this.CheckPersistance();
            }
            internal Subset(DisjointSet<TKey, TValue> set, Entry entry)
            {
                this.parent = set;
                this.key = entry.key;
                this.version = this.parent._version;
                this.entry = entry;
            }

            internal void CheckPersistance()
            {
                if (this.parent._version != this.version)
                {
                    this.entry = this.parent.FindEntry(this.key, false);
                    if (entry == null)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_SetKeyMissed);
                    }
                    this.version = this.parent._version;
                }
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                version--;
            }
            private bool IsSiblingSubSetOf(IEnumerable<TValue> other)
            {
                if (other is Subset subset)
                {
                    subset.CheckPersistance();
                    return (subset.parent == this.parent);
                }
                return false;
            }
            private void UnionWithSibling(Subset other)
            {
                Entry entry = other.entry;
                if (this.entry != entry)
                {
                    if (this.entry.UnionWith(entry.subset))
                    {
                        entry.subset = new HashCollection<TValue>(entry.subset.comparer);
                    }
                }
            }
            private bool GetIndexOrCreate(TValue value, out int index)
            {
                if (this.parent.FindEntryByValue(value) == null)
                {
                    if (entry.subset.GetIndexOrCreate(value, out index))
                    {
                        this.version++;
                        return true;
                    }
                    return false;
                }
                index = -1;
                return false;
            }

            /// <summary>
            /// Adds a value to the subset represented by the managed key in the associated <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>.
            /// </summary>
            /// <param name="value">The value to add</param>
            /// <returns>true if the value was successfully added; otherwise false.</returns>
            public bool Add(TValue value)
            {
                CheckPersistance();
                if (!this.parent.Values.Contains(value))
                {
                    entry.subset.Add(value);
                    this.parent._count++;
                    this.parent._version++;
                    this.version++;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Removes all values in the specified collection from the managed subset.
            /// </summary>
            /// <param name="other">The collection of values to remove from the current set.</param>
            public void ExceptWith(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if (!IsSiblingSubSetOf(other))
                {
                    this.entry.subset.ExceptWith(other);
                }
            }
            /// <summary>
            /// Modifies the managed subset so that it contains only values that are also in a specified collection. 
            /// Values contained in the associated <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> subsets 
            /// will be assumed as part of the collection for Contains checks.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void IntersectWith(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if ((!(other is DisjointSet<TKey, TValue> set)) || (this.parent != set))
                {
                    if (!IsSiblingSubSetOf(other))
                    {
                        this.entry.subset.IntersectWith(other);
                    }
                }
            }
            /// <summary>
            /// Determines whether the managed subset is a proper (strict) subset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns> true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> is a proper subset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsProperSubsetOf(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if (other is Subset subset)
                {
                    subset.CheckPersistance();
                    if ((subset.parent == this.parent))
                    {
                        return (this.Count == 0) && (subset.Count > 0);
                    }
                }
                else
                {
                    if ((other is DisjointSet<TKey, TValue> parent) && (this.parent == parent))
                    {
                        return (parent._count > this.entry.subset.Count);
                    }
                }
                return this.entry.subset.IsProperSubsetOf(other);
            }
            /// <summary>
            /// Determines whether the managed subset is a proper (strict) superset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> is a proper superset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsProperSupersetOf(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if (other is Subset subset)
                {
                    subset.CheckPersistance();
                    if ((subset.parent == this.parent))
                    {
                        return (this.Count > 0) && (subset.Count == 0);
                    }
                }
                if ((other is DisjointSet<TKey, TValue> parent) && (this.parent == parent))
                {
                    return false;
                }
                return this.entry.subset.IsProperSupersetOf(other);
            }
            /// <summary>
            /// Determines whether the managed subset is a subset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> is a subset of the specified collection; otherwise, false.</returns>
            public bool IsSubsetOf(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if (other is Subset subset)
                {
                    subset.CheckPersistance();
                    if ((subset.parent == this.parent))
                    {
                        return (this.Count == 0) && (subset.Count == 0);
                    }
                }
                else
                {
                    if ((other is DisjointSet<TKey, TValue> parent) && (this.parent == parent))
                    {
                        return true;
                    }
                }
                return this.entry.subset.IsSubsetOf(other);
            }
            /// <summary>
            /// Determines whether the managed subset is a superset of a specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> is a superset of <paramref name="other"/>; otherwise, false.</returns>
            public bool IsSupersetOf(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if (other is Subset subset)
                {
                    subset.CheckPersistance();
                    if ((subset.parent == this.parent))
                    {
                        return (this.Count == 0) && (subset.Count == 0);
                    }
                }
                else
                {
                    if ((other is DisjointSet<TKey, TValue> parent) && (this.parent == parent))
                    {
                        return (parent.Values.Count == 0);
                    }
                }
                return this.entry.subset.IsSupersetOf(other);
            }
            /// <summary>
            /// Determines whether the managed subset overlaps with the specified collection.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> and <paramref name="other"/> share at least one common key; otherwise, false.</returns>
            public bool Overlaps(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                CheckPersistance();
                if ((other is DisjointSet<TKey, TValue> parent) && (this.parent == parent))
                {
                    return (parent.Values.Count > 0) && (this.Count > 0);
                }
                if (!IsSiblingSubSetOf(other))
                {
                    return this.entry.subset.Overlaps(other);
                }
                return false;
            }
            /// <summary>
            /// Determines whether the managed subset and the specified collection contain the same elements.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>true if the current <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> is equal to <paramref name="other"/>; otherwise, false.</returns>
            public bool SetEquals(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if ((other is DisjointSet<TKey, TValue> parent) && (this.parent == parent))
                {
                    if ((parent.Values.Count == this.Count) || (parent._set.Count == 1))
                    {
                        return true;
                    }
                    return this.parent.GetSingleFilledEntry() == this.entry;
                }
                if (!IsSiblingSubSetOf(other))
                {
                    return this.entry.subset.SetEquals(other);
                }
                return false;
            }
            /// <summary>
            /// Modifies the managed subset so that it contains only values that are present either in the current set or in the specified collection, but not both. 
            /// Values contained in the associated <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> subsets 
            /// will be assumed as part of the collection for Contains checks.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void SymmetricExceptWith(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if (entry.subset.Count == 0)
                {
                    this.UnionWith(other);
                }
                else if (other == this)
                {
                    this.Clear();
                }
                else
                {
                    if ((other is DisjointSet<TKey, TValue> set) && (this.parent == set))
                    {
                        set.ToSingleSet(this.entry);
                        return;
                    }
                    HashCollection<TValue> collection = null;
                    if (other is Subset subset)
                    {
                        subset.CheckPersistance();
                        if (subset.parent == this.parent)
                        {
                            UnionWithSibling(subset);
                            return;
                        }
                        collection = subset.entry.subset;
                    }
                    HashCollection<TValue> localSubset = this.entry.subset;
                    if ((collection != null) && localSubset.comparer.Equals(collection.comparer))
                    {
                        foreach (TValue value in collection)
                        {
                            if (!this.Remove(value))
                            {
                                this.Add(value);
                            }
                        }
                    }
                    else
                    {
                        HashCollection<TValue> subset2 = this.entry.subset;
                        int lastIndex = subset2.lastIndex;
                        BitArray flags = new BitArray(lastIndex);
                        BitArray flags2 = new BitArray(lastIndex);
                        foreach (TValue item in other)
                        {
                            if (GetIndexOrCreate(item, out int index))
                            {
                                if (index >= 0)
                                {
                                    flags2[index] = true;
                                }
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
                                Remove(subset2.GetItemAt(i));
                            }
                        }
                    }
                }
            }
            /// <summary>
            /// Modifies the managed subset so that it contains all values that are present in either the current set or the specified collection. 
            /// Values contained in the associated <see cref="T:Academy.Collections.Generic.DisjointSet`2"/> subsets 
            /// will be assumed as part of the collection for Contains checks.
            /// </summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void UnionWith(IEnumerable<TValue> other)
            {
                if (other == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.other);
                }
                this.CheckPersistance();
                if (other is Subset subset)
                {
                    subset.CheckPersistance();
                    if ((subset.parent == this.parent))
                    {
                        UnionWithSibling(subset);
                        return;
                    }
                }
                if ((other is DisjointSet<TKey, TValue> set) && (this.parent == set))
                {
                    set.ToSingleSet(this.entry);
                    return;
                }
                int num = this.parent._count;
                foreach (TValue value in other)
                {
                    if (this.parent.Values.Contains(value))
                    {
                        entry.subset.Add(value);
                        num++;
                    }
                }
                this.parent._version++;
                this.version++;
                this.parent._count = num;
            }

            /// <summary>
            /// Removes all elements from the managed subset.
            /// </summary>
            public void Clear()
            {
                CheckPersistance();
                entry.subset.Clear();
            }
            /// <summary>
            /// Determines whether the managed subset contains the specified value.
            /// </summary>
            /// <param name="value">The value to find in the subset.</param>
            /// <returns>true if the value is contained in the subset; otherwise false.</returns>
            public bool Contains(TValue value)
            {
                CheckPersistance();
                return this.entry.subset.Contains(value);
            }
            /// <summary>
            /// Copies the complete <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> to a compatible one-dimensional array, starting at the beginning of the target array.
            /// </summary>
            /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/>.</param>
            public void CopyTo(TValue[] array)
            {
                CopyTo(array, 0, Count);
            }
            /// <summary>
            /// Copies the complete <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> to a compatible one-dimensional array, starting at the specific array index.
            /// </summary>
            /// <param name="array">A one-dimensional array that is the destination of the elements copied from the 
            /// <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/>. The array must have zero-based indexing</param>
            /// <param name="index">The zero-based index in array at which copying begins.</param>
            public void CopyTo(TValue[] array, int index)
            {
                CopyTo(array, index, Count);
            }
            /// <summary>
            /// Copies a specific number of elements from the <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/> 
            /// to a compatible one-dimensional array, starting at the specific array index.
            /// </summary>
            /// <param name="array">A one-dimensional array that is the destination of the elements copied from the 
            /// <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/>. The array must have zero-based indexing</param>
            /// <param name="index">The zero-based index in array at which copying begins.</param>
            /// <param name="count">The number of elements to copy.</param>
            public void CopyTo(TValue[] array, int index, int count)
            {
                CheckPersistance();
                entry.subset.CopyTo(array, index, count);
            }
            /// <summary>
            /// Removes the specified value from the managed subset.
            /// </summary>
            /// <param name="item">The value to remove</param>
            /// <returns>true if the value was found and successfully removed; otherwise false.</returns>
            public bool Remove(TValue item)
            {
                CheckPersistance();
                if (this.entry.subset.Remove(item))
                {
                    this.parent._count--;
                    this.parent._version++;
                    this.version++;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="T:Academy.Collections.Generic.DisjointSet`2.Subset"/>.
            /// </summary>
            /// <returns>An enumerator that iterates through the managed subset.</returns>
            public IEnumerator<TValue> GetEnumerator()
            {
                CheckPersistance();
                return this.entry.subset.GetEnumerator();
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            void ICollection<TValue>.Add(TValue item)
            {
                this.Add(item);
            }
        }

        #endregion

        #region Entry and Comparer
        [Serializable]
        internal class Entry : IGrouping<TKey, TValue>
        {
            internal TKey key;
            internal HashCollection<TValue> subset;

            public Entry(TKey key)
            {
                this.key = key;
                this.subset = new HashCollection<TValue>();
            }
            public Entry(TKey key, IEqualityComparer<TValue> comparer)
            {
                this.key = key;
                this.subset = new HashCollection<TValue>(comparer);
            }

            public TKey Key
            {
                get { return key; }
            }

            public bool UnionWith(HashCollection<TValue> other)
            {
                if ((this.subset.Count == 0) && (other.Count > 0))
                {
                    this.subset = other;
                    return true;
                }
                else
                {
                    HashCollection<TValue> set1;
                    HashCollection<TValue> set2;
                    if (other.BlankCount >= this.subset.Count)
                    {
                        set1 = other;
                        set2 = this.subset;
                    }
                    else
                    {
                        set1 = this.subset;
                        set2 = other;
                    }
                    foreach (var item in set2)
                    {
                        set1.Add(item);
                    }
                    this.subset = set1;
                    return false;
                }
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return subset.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        [Serializable]
        internal class EntryEqComparer : IEqualityComparer<Entry>
        {
            internal IEqualityComparer<TKey> keyComparer;

            public EntryEqComparer(IEqualityComparer<TKey> keyComparer)
            {
                this.keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            }

            public bool Equals(Entry x, Entry y)
            {
                return keyComparer.Equals(x.key, y.key);
            }
            public int GetHashCode(Entry obj)
            {
                return keyComparer.GetHashCode(obj.key);
            }
        }
        #endregion

        #region Internal and Explicit
        internal Entry GetSingleFilledEntry()
        {
            //Gets the only node that contains elements, if more than one or non node contains elements, returns null.
            int count = 0;
            Entry filled = null;
            foreach (Entry entry in this._set)
            {
                int num = entry.subset.Count;
                if ((count > 0) && (num > 0))
                {
                    return null;
                }
                count = entry.subset.Count;
                if (count > 0)
                {
                    filled = entry;
                }
            }
            return filled;
        }
        internal void RemoveSet(Entry entry)
        {
            this._set.Remove(entry);
            this._count -= entry.subset.Count;
            this._version++;
        }
        internal int GetKeyHashCode(TKey key)
        {
            return _entryComparer.keyComparer.GetHashCode(key) & int.MaxValue;
        }
        internal int GetValueHashCode(TValue value)
        {
            return _valueComparer.GetHashCode(value) & int.MaxValue;
        }
        internal Entry FindEntryByValue(TValue value)
        {
            foreach (Entry item in this._set)
            {
                if (item.subset.Contains(value))
                {
                    return item;
                }
            }
            return null;
        }
        internal Entry FindEntry(TKey key, bool create)
        {
            Entry entry = CreateEntry(key, _valueComparer);
            HashCollection<Entry>.HashEntry outEntry;
            if (create)
            {
                _set.GetIndexOrCreate(entry, out int num);
                return _set.GetItemAt(num);
            }
            outEntry = _set.FindEntry(entry, _entryComparer);
            if (outEntry.HasValue)
            {
                return outEntry.item;
            }
            return null;
        }
        internal static Entry CreateEntry(TKey key)
        {
            return new Entry(key);
        }
        internal static Entry CreateEntry(TKey key, IEqualityComparer<TValue> comparer)
        {
            return new Entry(key, comparer);
        }
        internal void Update(int count)
        {
            this._count += count;
            this._version++;
        }
        internal void UpdateCount()
        {
            int count = 0;
            foreach (Entry item in this._set)
            {
                count += item.subset.Count;
            }
            this.Update(count);
        }
        internal void ToSingleSet(Entry entry)
        {
            foreach (var item in this._set)
            {
                if (item != entry)
                {
                    entry.subset.UnionWith(item.subset);
                }
            }
            this._count = entry.subset.Count;
        }

        bool ILookup<TKey, TValue>.Contains(TKey key)
        {
            return ContainsKey(key);
        }
        int ILookup<TKey, TValue>.Count
        {
            get { return this.Count; }
        }
        IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator<IGrouping<TKey, TValue>> IEnumerable<IGrouping<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Enumerates the subsets of a <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>, as a sequence of <see cref="T:System.Linq.IGrouping`2"/>.
        /// </summary>
        [Serializable]
        public struct Enumerator : IEnumerator<IGrouping<TKey, TValue>>, ISerializable
        {
            private DisjointSet<TKey, TValue> disjointset;
            private readonly int version;
            private IEnumerator<Entry> enumerator;
            private IGrouping<TKey, TValue> current;
            private bool finishedOrNotStarted;

            internal Enumerator(DisjointSet<TKey, TValue> disjointset)
            {
                this.disjointset = disjointset;
                this.version = disjointset._version;
                this.enumerator = disjointset._set.GetEnumerator();
                this.finishedOrNotStarted = true;
                this.current = null;
            }
            internal Enumerator(SerializationInfo info, StreamingContext context)
            {
                this.disjointset = (DisjointSet<TKey, TValue>)info.GetValue(SerializationString.Collection, typeof(DisjointSet<TKey, TValue>));
                this.version = info.GetInt32(SerializationString.Version);
                this.finishedOrNotStarted = info.GetBoolean(SerializationString.FinishedOrNotStarted);
                this.enumerator = disjointset._set.GetEnumerator();
                if (this.finishedOrNotStarted)
                {
                    this.current = null;
                }
                else
                {
                    IEqualityComparer<TKey> comparer = disjointset._entryComparer.keyComparer;
                    TKey currentKey = (TKey)info.GetValue(SerializationString.Current, typeof(TKey));
                    bool flag = true;
                    while ((flag = enumerator.MoveNext()) && (!comparer.Equals(enumerator.Current.key, currentKey)))
                    {

                    }
                    current = flag ? (IGrouping<TKey, TValue>)enumerator.Current : null;
                }
            }

            /// <summary>
            /// Gets an <see cref="T:System.Linq.IGrouping`2"/> implementation that represents the subset at the current position of the enumerator.
            /// </summary>
            public IGrouping<TKey, TValue> Current
            {
                get
                {
                    return current;
                }
            }
            /// <summary>
            /// Releases all resources used by the <see cref="T:Academy.Collections.Generic.DisjointSet`2.Enumerator"/>.
            /// </summary>
            public void Dispose()
            {
            }
            /// <summary>
            /// Advances the enumerator to the next subset of the <see cref="T:Academy.Collections.Generic.DisjointSet`2"/>
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next subset; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (this.version != disjointset._version)
                {
                    Thrower.EnumeratorCorrupted();
                }
                if (!(finishedOrNotStarted = !enumerator.MoveNext()))
                {
                    current = (IGrouping<TKey, TValue>)enumerator.Current;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Resets the enumerator to its initial position, which is before the first subset in the disjointset.
            /// </summary>
            public void Reset()
            {
                if (this.version != disjointset._version)
                {
                    Thrower.EnumeratorCorrupted();
                }
                this.enumerator = disjointset._set.GetEnumerator();
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(SerializationString.Collection, disjointset, typeof(DisjointSet<TKey, TValue>));
                info.AddValue(SerializationString.Version, version);
                info.AddValue(SerializationString.FinishedOrNotStarted, finishedOrNotStarted);
                if (!finishedOrNotStarted)
                {
                    info.AddValue(SerializationString.Current, current.Key);
                }
            }
        }
    }
}
