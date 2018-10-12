using StandardCollections.ObjectModel;
using StandardCollections.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace StandardCollections
{
    /// <summary>
    /// Represents a read-only collection of objects that is maintained in sorted order and can be accessed by index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [ DebuggerTypeProxy(typeof(ReadOnlyCollectionProxy<>)), DebuggerDisplay("Count = {Count}")]
    public class ReadOnlySortedSet<T> : IReadOnlyList<T>, ISortedCollection<T>, ISet<T>, IIndexedSet<T>, ICommonSortedSet<T>, ICollection
    {
        private object _syncRoot;
        private IComparer<T> _comparer;
        private T[] _elements;

        internal ReadOnlySortedSet()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> class that contains 
        /// elements copied from the especified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.</param>
        public ReadOnlySortedSet(IEnumerable<T> collection)
            : this(collection, null)
        {

        }
        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> class that contains 
        /// elements copied from the especified collection and that uses a specified comparer.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.</param>
        /// <param name="comparer">The default comparer to use for compare objects.</param>
        public ReadOnlySortedSet(IEnumerable<T> collection, IComparer<T> comparer)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            this._comparer = comparer ?? Comparer<T>.Default;
            List<T> list = SetHelper<T>.GetSortedListSet(collection, this._comparer);
            this._elements = list.ToArray();
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get
            {
                int num = index + StartIndex;
                if (num < 0 || num >= EndIndex)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.index, string.Empty);
                }
                return this._elements[num];
            }
        }
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.
        /// </summary>
        public int Count
        {
            get
            {
                int count = EndIndex - StartIndex;
                return (count < 0) ? 0 : count;
            }
        }
        /// <summary>
        /// Gets the default comparer used to compare stored elements.
        /// </summary>
        public IComparer<T> Comparer
        {
            get { return _comparer; }
        }
        /// <summary>
        /// Gets the minimum value in the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>, as defined by the comparer.
        /// </summary>
        public T MinValue
        {
            get
            {
                if (Count == 0)
                {
                    return default(T);
                }
                return _elements[StartIndex];
            }
        }
        /// <summary>
        /// Gets the maximum value in the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>, as defined by the comparer.
        /// </summary>
        public T MaxValue
        {
            get
            {
                if (Count == 0)
                {
                    return default(T);
                }
                return _elements[EndIndex - 1];
            }
        }

        internal virtual int StartIndex
        {
            get
            {
                return 0;
            }
        }
        internal virtual int EndIndex
        {
            get
            {
                return _elements.Length;
            }
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>. The value can be null for reference types.</param>
        /// <returns>true if the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> contains <paramref name="value"/>; otherwise, false.</returns>
        public bool Contains(T value)
        {
            return IndexOf(value) >= 0;
        }
        /// <summary>
        /// Copies the complete <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.</param>
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, Count);
        }
        /// <summary>
        /// Copies the complete <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> to a compatible one-dimensional array, starting at the specific array index.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the 
        /// <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>. The array must have zero-based indexing</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, Count);
        }
        /// <summary>
        /// Copies a specific number of elements from the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> 
        /// to a compatible one-dimensional array, starting at the specific array index.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the 
        /// <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>. The array must have zero-based indexing</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopyTo(T[] array, int index, int count)
        {
            CollectionHelper.CopyTo<T>(array, index, count, this);
        }
        /// <summary>
        /// Searches for the specified object and returns the zero-based index where the object is located in the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.</param>
        /// <returns>The zero-based index where the specified object is located in the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>, if found; otherwise, –1.</returns>
        public int IndexOf(T value)
        {
            int index = Array.BinarySearch<T>(_elements, StartIndex, Count, value);
            return index;
        }
        /// <summary>
        /// Returns an enumerator that iterates through <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.
        /// </summary>
        /// <returns>Returns an enumerator that iterates through <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        /// <summary>
        /// Determines whether the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> is a proper (strict) subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns> true if the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> is a proper subset of <paramref name="other"/>; otherwise, false.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                Thrower.ArgumentNullException(ArgumentType.other);
            }
            if (CollectionHelper.IsWellKnownCollection<T>(other, out int num))
            {
                if (this.Count == 0)
                {
                    return num > 0;
                }
            }
            ICommonSortedSet<T> sortedset = SetHelper<T>.GetSortedSetIfSameComparer(this, Comparer);
            if (sortedset != null)
            {
                if (this.Count >= sortedset.Count)
                {
                    return false;
                }
                ICommonSortedSet<T> subset = sortedset.GetViewBetween(MinValue, MaxValue);
                foreach (T item in this)
                {
                    if (!subset.Contains(item))
                    {
                        return false;
                    }
                }
                return true;
            }
            return SetHelper<T>.IsProperSubsetOf(this, other);
        }
        /// <summary>
        /// Determines whether the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> is a proper (strict) superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>true if the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet"/> is a proper superset of <paramref name="other"/>; otherwise, false.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                Thrower.ArgumentNullException(ArgumentType.other);
            }
            if (Count == 0)
            {
                return false;
            }
            ICommonSortedSet<T> sortedset = SetHelper<T>.GetSortedSetIfSameComparer(this, Comparer);
            if (sortedset != null)
            {
                if (sortedset.Count >= Count)
                {
                    return false;
                }
                ReadOnlySortedSet<T> subset = this.GetViewBetween(sortedset.MinValue, sortedset.MaxValue);
                foreach (T item in sortedset)
                {
                    if (subset.Contains(item))
                    {
                        return false;
                    }
                }
                return true;
            }
            return SetHelper<T>.IsProperSubsetOf(this, other);
        }
        /// <summary>
        /// Determines whether the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> is a subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>true if the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> is a subset of the specified collection; otherwise, false.</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                Thrower.ArgumentNullException(ArgumentType.other);
            }
            if (this.Count == 0)
            {
                return true;
            }
            ICommonSortedSet<T> sortedset = SetHelper<T>.GetSortedSetIfSameComparer(this, Comparer);
            if (sortedset != null)
            {
                if (this.Count > sortedset.Count)
                {
                    return false;
                }
                ICommonSortedSet<T> subset = sortedset.GetViewBetween(MinValue, MaxValue);
                foreach (T item in this)
                {
                    if (!subset.Contains(item))
                    {
                        return false;
                    }
                }
                return true;
            }
            return SetHelper<T>.IsSubsetOf(this, other);
        }
        /// <summary>
        /// Determines whether the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> is a superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>true if the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> is a superset of <paramref name="other"/>; otherwise, false.</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
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
                if (SetHelper<T>.IsWellKnownSet(other))
                {
                    if (this.Count < num)
                    {
                        return false;
                    }
                }
            }
            ICommonSortedSet<T> sortedset = SetHelper<T>.GetSortedSet(other);
            if ((sortedset == null) || !AreEqualComparers(this._comparer, sortedset.Comparer))
            {
                return this.ContainsSequence(other);
            }
            ReadOnlySortedSet<T> subset = this.GetViewBetween(sortedset.MinValue, sortedset.MaxValue);
            foreach (T item in sortedset)
            {
                if (!subset.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Determines whether the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>true if the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> and <paramref name="other"/> share at least one common element; otherwise, false.</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
            {
                Thrower.ArgumentNullException(ArgumentType.other);
            }
            if (Count == 0)
            {
                return false;
            }
            if (CollectionHelper.IsWellKnownCollection<T>(other, out int num))
            {
                if (num == 0)
                {
                    return false;
                }
                ICommonSortedSet<T> sortedset = SetHelper<T>.GetSortedSetIfSameComparer(this, Comparer);
                if (sortedset != null)
                {
                    if ((_comparer.Compare(MinValue, sortedset.MaxValue) > 0) || (_comparer.Compare(MaxValue, sortedset.MinValue) < 0))
                    {
                        return false;
                    }
                }
                foreach (T item in other)
                {
                    if (Contains(item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Determines whether the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>true if the current <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/> is equal to <paramref name="other"/>; otherwise, false.</returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
            {
                Thrower.ArgumentNullException(ArgumentType.other);
            }
            ICommonSortedSet<T> sortedset = SetHelper<T>.GetSortedSetIfSameComparer(this, Comparer);
            if (sortedset != null)
            {
                IEnumerator<T> e1 = this.GetEnumerator();
                IEnumerator<T> e2 = sortedset.GetEnumerator();
                bool canMove1 = e1.MoveNext();
                bool canMove2 = e2.MoveNext();
                while (canMove1 && canMove2)
                {
                    if (_comparer.Compare(e1.Current, e2.Current) != 0)
                    {
                        return false;
                    }
                    canMove1 = e1.MoveNext();
                    canMove2 = e2.MoveNext();
                }
                return (canMove1 == canMove2);
            }
            return SetHelper<T>.SetEquals(this, other);
        }
        /// <summary>
        /// Return a view of a subset in a <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.
        /// </summary>
        /// <param name="lowerValue">The lowest desired value in the view.</param>
        /// <param name="upperValue">The highest desired value in the view.</param>
        /// <returns>A subset view that contains only the values in the specified range.</returns>
        public ReadOnlySortedSet<T> GetViewBetween(T lowerValue, T upperValue)
        {
            if (_comparer.Compare(lowerValue, upperValue) > 0)
            {
                Thrower.ArgumentException(ArgumentType.empty, Resources.Argument_lowerBiggerThanUpper);
            }
            return GetSubset(lowerValue, upperValue);
        }

        internal virtual ReadOnlySortedSet<T> GetSubset(T lowerValue, T upperValue)
        {
            int leftBound = FindLeftBound(lowerValue, upperValue);
            int rightBound = FindRightBound(lowerValue, upperValue);
            if (leftBound == -1)
            {
                leftBound = 0;
            }
            return new SubSetView(this, leftBound, rightBound);
        }
        private static bool AreEqualComparers(IComparer<T> comparer1, IComparer<T> comparer2)
        {
            return comparer1.Equals(comparer2);
        }
        private bool ContainsSequence(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                if (!this.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }
        private int FindLeftBound(T lowerValue, T upperValue)
        {
            int left = 0;
            int right = _elements.Length - 1;
            int foundIndex = -1;
            while (left <= right)
            {
                int num = _comparer.Compare(_elements[left], lowerValue);
                int num2 = _comparer.Compare(_elements[right], lowerValue);
                if (num == 0)//left item equals to lowerValue
                {
                    return left;
                }
                if (num2 >= 0)//right item bigger than lowerValue
                {
                    foundIndex = right;
                }
                int mid = left + ((right - left) >> 1);
                int num3 = _comparer.Compare(_elements[mid], lowerValue);
                int num4 = _comparer.Compare(_elements[mid], upperValue);
                if ((num3 >= 0) && (num4 <= 0))//mid item inside lowerValue-upperValue range.
                {
                    foundIndex = mid;
                }
                if (num3 >= 0)
                {
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            return foundIndex;
        }
        private int FindRightBound(T lowerValue, T upperValue)
        {
            int left = 0;
            int right = _elements.Length - 1;
            int foundIndex = -1;
            while (left <= right)
            {
                int num = _comparer.Compare(_elements[right], upperValue);
                int num2 = _comparer.Compare(_elements[left], upperValue);
                if (num == 0)
                {
                    return right;
                }
                if (num2 <= 0)
                {
                    foundIndex = left;
                }
                int mid = left + ((right - left) >> 1);
                int num3 = _comparer.Compare(_elements[mid], lowerValue);
                int num4 = _comparer.Compare(_elements[mid], upperValue);
                if ((num3 >= 0) && (num4 <= 0))
                {
                    foundIndex = mid;
                }
                if (num4 > 0)
                {
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            return foundIndex;
        }

        /// <summary>
        ///Enumerates the elements of a <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private ReadOnlySortedSet<T> set;
            private T current;
            private int index;

            internal Enumerator(ReadOnlySortedSet<T> set)
            {
                this.set = set;
                this.current = default(T);
                this.index = set.StartIndex;
            }
            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public T Current
            {
                get
                {
                    return current;
                }
            }
            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="T:Academy.Collections.Generic.ReadOnlySortedSet`1"/>
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (index < set.EndIndex)
                {
                    this.current = set._elements[index];
                    index++;
                    return true;
                }
                return false;
            }

            void IDisposable.Dispose()
            {

            }
            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
            void IEnumerator.Reset()
            {
                this.current = default(T);
                this.index = set.StartIndex;
            }
        }

        #region Explicit
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        void ICollection.CopyTo(Array array, int index)
        {
            CollectionHelper.CopyTo<T>(array, index, Count, this);
        }
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }
        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }
        bool ISet<T>.Add(T item)
        {
            Thrower.NotSupportedException();
            return false;
        }
        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            Thrower.NotSupportedException();
        }
        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            Thrower.NotSupportedException();
        }
        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            Thrower.NotSupportedException();
        }
        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            Thrower.NotSupportedException();
        }
        void ICollection<T>.Add(T item)
        {
            Thrower.NotSupportedException();
        }
        void ICollection<T>.Clear()
        {
            Thrower.NotSupportedException();
        }
        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }
        bool ICollection<T>.Remove(T item)
        {
            Thrower.NotSupportedException();
            return false;
        }
        int IIndexedSet<T>.IndexOf(T item)
        {
            return Array.BinarySearch(_elements, item, _comparer);
        }
        ICommonSortedSet<T> ICommonSortedSet<T>.GetViewBetween(T min, T max)
        {
            return GetViewBetween(min, max);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        internal class SubSetView : ReadOnlySortedSet<T>
        {
            private int _lowerIndex;
            private int _upperIndex;
            private ReadOnlySortedSet<T> _underlying;

            internal override int StartIndex
            {
                get
                {
                    return _lowerIndex;
                }
            }
            internal override int EndIndex
            {
                get
                {
                    return _upperIndex + 1;
                }
            }

            public SubSetView(ReadOnlySortedSet<T> collection, int lowerIndex, int upperIndex)
            {
                this._underlying = collection;
                this._lowerIndex = lowerIndex;
                this._upperIndex = upperIndex;
                this._comparer = collection._comparer;
                this._elements = collection._elements;
            }

            internal override ReadOnlySortedSet<T> GetSubset(T lowerValue, T upperValue)
            {
                if (Count == 0 || _comparer.Compare(_elements[_lowerIndex], lowerValue) > 0)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.lowerValue, Resources.ArgumentOutOfRange_ViewValue);
                }
                if (_comparer.Compare(_elements[_upperIndex], lowerValue) < 0)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.upperValue, Resources.ArgumentOutOfRange_ViewValue);
                }
                return _underlying.GetViewBetween(lowerValue, upperValue);
            }

           
        }
    }
}
