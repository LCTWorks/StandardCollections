using StandardCollections.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace StandardCollections
{
    /// <summary>
    /// Represents a collections of objects that is maintained arranged by priority.
    /// </summary>
    /// <typeparam name="T">The type of elements in the priority queue.</typeparam>
    [DebuggerTypeProxy(typeof(EnumerableProxy<>)), DebuggerDisplay("Count = {Count}")]
    public class PriorityQueue<T> : IReadOnlyCollection<T>, IEnumerable<T>, ICollection
    {
        #region Private Region
        private object _syncRoot;
        private int _version;
        private IComparer<T> _comparer;
        private static readonly T[] _defaultarray = new T[1];
        private int _currentIndex;
        private T[] _elements;
        private const int MaxLength = 2146435071;

        private void Swap(int first, int second)
        {
            T exchange = _elements[first];
            _elements[first] = _elements[second];
            _elements[second] = exchange;
        }
        private void Heapify(int index)
        {
            int son1;
            while ((son1 = LeftSon(index)) < _currentIndex)
            {
                int son2;
                if ((son2 = RightSon(index)) < _currentIndex - 1)
                {
                    if ((_comparer.Compare(_elements[son1], _elements[index]) > 0) || (_comparer.Compare(_elements[son2], _elements[index]) > 0))
                    {
                        if (_comparer.Compare(_elements[son1], _elements[son2]) > 0)
                        {
                            Swap(son1, index);
                            index = son1;
                        }
                        else
                        {
                            Swap(son2, index);
                            index = son2;
                        }
                    }
                    else
                        return;
                }
                else
                {
                    if (_comparer.Compare(_elements[son1], _elements[index]) > 0)
                    {
                        Swap(son1, index);
                    }
                    return;
                }
            }
        }
        private void SetCapacity(int capacity)
        {
            T[] destination = new T[capacity];
            if (_currentIndex > 1)
            {
                Array.Copy(_elements, 0, destination, 0, _currentIndex);
            }
            _elements = destination;
            _version++;
        }
        private static int LeftSon(int index)
        {
            return (2 * index);
        }
        private static int RightSon(int index)
        {
            return ((2 * index) + 1);
        }
        private static int Parent(int index)
        {
            return (index / 2);
        }
        #endregion

        private const int DefaultCapacity = 4;
        private const int MinimumCapacity = 0;

        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> class that is empty and has the default capacity and comparer.
        /// </summary>
        public PriorityQueue()
        {
            _comparer = Comparer<T>.Default;
            _elements = new T[DefaultCapacity + 1];
            _currentIndex = 1;
        }
        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> class that is 
        /// empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> can initially store.</param>e
        public PriorityQueue(int capacity)
            : this(capacity, Comparer<T>.Default)
        {

        }
        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> class that is 
        /// empty and has the specified initial capacity and comparer.
        /// </summary>
        /// <param name="capacity">The number of elements that the new <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> instance can initially store.</param>
        /// <param name="comparer">The default comparer to use for compare objects.</param>
        public PriorityQueue(int capacity, IComparer<T> comparer)
        {
            if (capacity < MinimumCapacity)
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.capacity, Resources.ArgumentOutOfRange_Negative);
            }
            if (capacity == int.MaxValue)
            {
                capacity--;
            }
            if (comparer == null)
            {
                comparer = Comparer<T>.Default;
            }
            _elements = new T[capacity + 1];
            _comparer = comparer;
            _currentIndex = 1;
            _elements[0] = default(T);
        }
        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> class that contains 
        /// elements copied from the especified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.</param>
        public PriorityQueue(IEnumerable<T> collection)
            : this(collection, Comparer<T>.Default)
        {
        }
        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> class that contains 
        /// elements copied from the especified collection and uses the specified comparer.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.</param>
        /// <param name="comparer">The default comparer to use for compare objects.</param>
        public PriorityQueue(IEnumerable<T> collection, IComparer<T> comparer)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            if (comparer == null)
            {
                comparer = Comparer<T>.Default;
            }
            _comparer = comparer;
            if (collection is PriorityQueue<T> heap && _comparer.Equals(heap.Comparer))
            {
                _elements = heap._elements.Clone() as T[];
                _currentIndex = heap._currentIndex;
            }
            else
            {
                if (!CollectionHelper.IsWellKnownCollection(collection, out int num))
                {
                    num = DefaultCapacity;
                }
                _elements = new T[num + 1];
                _comparer = comparer;
                _currentIndex = 1;
                _elements[0] = default(T);
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Enqueue(enumerator.Current);
                    }
                }
                TrimExcess();
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
        /// Gets the number of elements contained in the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return _currentIndex - 1;
            }
        }

        /// <summary>
        /// Inserts an element to the priority queue.
        /// </summary>
        /// <param name="item">The element to insert.</param>
        public void Enqueue(T item)
        {
            int index = _currentIndex;
            if (index == _elements.Length)
            {
                int num = (_elements.Length - 1) * 2;
                if (num > MaxLength)
                {
                    num = MaxLength;
                }
                int num2 = _currentIndex + 2;
                if (num < num2)
                {
                    num = num2;
                }
                SetCapacity(num + 1);
            }
            _elements[(_currentIndex++)] = item;
            int div;
            while (index > 1 && _comparer.Compare(_elements[index], _elements[div = Parent(index)]) > 0)
            {
                Swap(index, div);
                index = div;
            }
            _version++;
        }
        /// <summary>
        /// Removes all elements from the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.
        /// </summary>
        public void Clear()
        {
            _elements = _defaultarray;
            _currentIndex = 1;
        }
        /// <summary>
        /// Creates a new object that is a copy of the current instance of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.
        /// </summary>
        /// <returns>Returns a new object that is a copy of the current instance of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.</returns>
        public object Clone()
        {
            PriorityQueue<T> pqueue = new PriorityQueue<T>(0, _comparer)
            {
                _elements = (T[])this._elements.Clone(),
                _comparer = this._comparer,
                _currentIndex = _currentIndex,
                _version = 0
            };
            return pqueue;
        }
        /// <summary>
        /// Determines whether an element is contained in the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.</param>
        /// <returns>returns true if this <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> instance contains the object, otherwise returns false.</returns>
        public bool Contains(T item)
        {
            for (int i = 1; i < _currentIndex; i++)
            {
                if (_comparer.Compare(item, _elements[i]) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Copies the complete <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.</param>
        public void CopyTo(T[] array)
        {
            CollectionHelper.CopyTo(array, 0, Count, this);
        }
        /// <summary>
        /// Copies the complete <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> to a compatible one-dimensional array, starting at the specific array index.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the 
        /// <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>. The array must have zero-based indexing</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int index)
        {
            CollectionHelper.CopyTo(array, index, Count, this);
        }
        /// <summary>
        /// Copies a specific number of elements from the complete <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> to a compatible one-dimensional array, starting at the specific array index.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from 
        /// the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>. The array must have zero-based indexing</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopyTo(T[] array, int index, int count)
        {
            CollectionHelper.CopyTo(array, index, count, this);
        }
        /// <summary>
        /// Removes and returns the maximum priority element contained in the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.
        /// </summary>
        /// <returns>Returns the maximum priority element contained in the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/></returns>
        public T Dequeue()
        {
            if (_currentIndex == 1)
            {
                Thrower.InvalidOperationException(Resources.InvalidOperation_EmptyCollection);
            }
            T local = _elements[1];
            if (Count > 1)
            {
                Swap(1, _currentIndex - 1);
                _currentIndex--;
                Heapify(1);
            }
            else
            {
                Clear();
            }
            _version++;
            return local;
        }
        /// <summary>
        /// Gets the maximum priority element contained in the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/> without removing it.
        /// </summary>
        /// <returns>Returns the object at the beginning of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/></returns>
        public T Peek()
        {
            if (_currentIndex == 1)
            {
                Thrower.InvalidOperationException(Resources.InvalidOperation_EmptyCollection);
            }
            return _elements[1];
        }
        /// <summary>
        /// Returns an enumerator that iterates through <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.
        /// </summary>
        /// <returns>Returns an enumerator that iterates through <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        /// <summary>
        ///Sets the capacity to the actual number of elements in the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>,
        ///if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess()
        {
            int num = (int)((_elements.Length - 1) * 0.9);
            if (Count < num)
            {
                SetCapacity(_currentIndex);
            }
        }
        /// <summary>
        /// Copies the elements of the <see cref="T:Academy.Collections.Generics.PriorityQueue`1"/> to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the <see cref="T:Academy.Collections.Generics.PriorityQueue`1"/>.</returns>
        public T[] ToArray()
        {
            T[] local = new T[Count];
            Array.Copy(_elements, 0, local, 0, Count);
            Array.Sort(local, Comparer);
            return local;
        }
        /// <summary>
        /// Copies the elements of the <see cref="T:Academy.Collections.Generics.PriorityQueue`1"/> to a new <see cref="T:System.Collections.Generics.List`1"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generics.List`1"/> containing copies of the elements of the <see cref="T:Academy.Collections.Generics.PriorityQueue`1"/>.</returns>
        public List<T> ToList()
        {
            List<T> list = new List<T>(Count);
            for (int i = 0; i < _currentIndex; i++)
            {
                list.Add(_elements[i]);
            }
            list.Sort(Comparer);
            return list;
        }

        #region Explicit implementation
        void ICollection.CopyTo(Array array, int index)
        {
            CollectionHelper.CopyTo<T>(array, index, Count, this);
        }
        bool ICollection.IsSynchronized
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
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Enumerates the elements of a <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>.
        /// </summary>
        [DebuggerDisplay("Current = {Current}")]
        public struct Enumerator : IEnumerator<T>
        {
            private int _index;
            private T _current;
            private PriorityQueue<T> _clone;
            private PriorityQueue<T> _heap;
            private int _version;

            internal Enumerator(PriorityQueue<T> heap)
            {
                _heap = heap;
                _clone = heap.Clone() as PriorityQueue<T>;
                _version = _heap._version;
                _index = 0;
                _current = default(T);
            }
          
            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public T Current
            {
                get
                {
                    return _current;
                }
            }
            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="T:Academy.Collections.Generic.PriorityQueue`1"/>
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_version != _heap._version)
                {
                    Thrower.InvalidOperationException(Resources.InvalidOperation_EnumCorrupt);
                }
                if (_clone.Count > 0)
                {
                    _current = _clone.Dequeue();
                    _index++;
                    return true;
                }
                _index = _heap.Count + 1;
                return false;
            }
            /// <summary>
            /// Releases all resources used by the <see cref="T:Academy.Collections.Generic.PriorityQueue`1.Enumerator"/>.
            /// </summary>
            public void Dispose()
            {
            }
            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0)
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                    if (_index == _heap.Count + 1)
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                    return _current;
                }
            }
            void IEnumerator.Reset()
            {
                if (_version != _heap._version)
                {
                    Thrower.InvalidOperationException(Resources.InvalidOperation_EnumCorrupt);
                }
                _index = 0;
                _current = default(T);
            }
        }
    }
}
