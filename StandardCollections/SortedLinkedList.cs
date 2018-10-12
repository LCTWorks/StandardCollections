using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using StandardCollections.Properties;
using System.Threading;
using StandardCollections.ObjectModel;

namespace StandardCollections
{
    /// <summary>
    /// Represents a linked list of objects that are maintained in sorted order. Repetead values are allowed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the linked list.</typeparam>
    [Serializable, DebuggerTypeProxy(typeof(CollectionProxy<>)), DebuggerDisplay("Count = {Count}")]
    public class SortedLinkedList<T> : ICollection<T>, ISortedCollection<T>, ISerializable, IDeserializationCallback, ICollection
    {
        #region Private Region
        internal delegate bool FindMatchPredicate(Node node);
        private object _syncRoot;
        private int _version;
        private SerializationInfo _sinfo;
        private Node _head;
        private int _count;
        private Random _random;
        internal IComparer<T> comparer;

        private const int DefaultHeight = 4;
        private const double ProbFactor = 0.5d;
        #endregion

        #region Ctors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> class that has the default comparer.
        /// </summary>
        public SortedLinkedList()
            : this(Comparer<T>.Default)
        {

        }
        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> class that uses a specified comparer.
        /// </summary>
        /// <param name="comparer">The default comparer to use for compare objects.</param>
        public SortedLinkedList(IComparer<T> comparer)
        {
            this.comparer = comparer ?? Comparer<T>.Default;
            _head = new Node();
            _random = new Random();
            _count = 0;
            _version = 0;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> class that contains 
        /// elements copied from the especified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.</param>
        public SortedLinkedList(IEnumerable<T> collection)
            : this(collection, Comparer<T>.Default)
        {

        }
        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> class that contains 
        /// elements copied from the especified collection and that uses a specified comparer.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.</param>
        /// <param name="comparer">The default comparer to use for compare objects.</param>
        public SortedLinkedList(IEnumerable<T> collection, IComparer<T> comparer)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            _random = new Random();
            this.comparer = comparer ?? Comparer<T>.Default;
            ReconstructFrom(collection);
        }
        /// <summary>
        /// Initialize a new instance of the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> class that contains serialized data.
        /// </summary>
        /// <param name="info">The object that contains the information that is required to serialize the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> object.</param>
        /// <param name="context">The structure that contains the source and destination of the serialized stream associated with the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> object.</param>
        protected SortedLinkedList(SerializationInfo info, StreamingContext context)
        {
            _sinfo = info;
        }
        #endregion

        /// <summary>
        /// Gets the default comparer used to compare stored elements.
        /// </summary>
        public IComparer<T> Comparer
        {
            get
            {
                return comparer;
            }
        }
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.
        /// </summary>
        public int Count
        {
            get
            {
                InternalCheckVersion();
                return _count;
            }
        }
        /// <summary>
        /// Gets the minimum value in the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>, as defined by the comparer.
        /// </summary>
        public T MinValue
        {
            get
            {
                InternalCheckVersion();
                Node first = StartNode();
                if (first != null)
                {
                    return first.Value;
                }
                return default(T);
            }
        }
        /// <summary>
        /// Gets the maximum value in the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>, as defined by the comparer.
        /// </summary>
        public T MaxValue
        {
            get
            {
                //InternalCheckVersion() Implicit on Count
                if (Count == 0)
                {
                    return default(T);
                }
                Node n = _head;
                for (int i = n.Count - 1; i >= 0; i--)
                {
                    while (n.Nodes[i] != null && IsInRange(n.Nodes[i].Value))
                    {
                        n = n.Nodes[i];
                    }
                }
                return n.Value;
            }
        }

        /// <summary>
        /// Adds an element to the list.
        /// </summary>
        /// <param name="value">The element to add to the list.</param>
        public void Add(T value)
        {
            InternalAdd(value);
        }
        /// <summary>
        /// Removes all elements from the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.
        /// </summary>
        public virtual void Clear()
        {
            _head = new Node();
            _version++;
        }
        /// <summary>
        /// Determines whether an element is in the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>. The value can be null for reference types.</param>
        /// <returns>true if the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> contains <paramref name="value"/>; otherwise, false.</returns>
        public bool Contains(T value)
        {
            Node current = _head;
            for (int i = _head.Count - 1; i >= 0; i--)
            {
                while (current.Nodes[i] != null && IsInRange(current.Nodes[i].Value))
                {
                    int results = comparer.Compare(current.Nodes[i].Value, value);
                    if (results < 0)
                        current = current.Nodes[i];
                    else if (results == 0)
                        return true;
                    else break;
                }
            }
            return false;
        }
        /// <summary>
        /// Determines whether an element is in the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> by searching every element that matches the specified value by using the
        /// <see cref="T:System.Collections.Generic.IComparer`1"/> implementation provided by the Comparer property, and the specified <see cref="T:System.Predicate`1"/>.
        /// </summary>
        /// <param name="value">The first value to locate in the list.</param>
        /// <param name="match">The predicate used to determinate if an element exists.</param>
        /// <returns>true if any value matches both, the  <see cref="T:System.Collections.Generic.IComparer`1"/> provided by the Comparer property and the specified <see cref="T:System.Predicate`1"/>.</returns>
        public bool Contains(T value, Predicate<T> match)
        {
            if (match == null)
            {
                Thrower.ArgumentNullException(ArgumentType.match);
            }
            if (IsInRange(value))
            {
                Node current = _head;
                Node node = null;
                for (int i = _head.Count - 1; i >= 0; i--)
                {
                    while (current.Nodes[i] != null && IsInRange(current.Nodes[i].Value))
                    {
                        int results = this.comparer.Compare(current.Nodes[i].Value, value);
                        if (results < 0)
                        {
                            current = current.Nodes[i];
                        }
                        else
                        {
                            if (results == 0)
                            {
                                if (match(current.Nodes[i].Value))
                                {
                                    return true;
                                }
                                node = current.Nodes[i];
                            }
                            break;
                        }
                    }
                }
                while ((node != null) && (this.comparer.Compare(node.Value, value) == 0))
                {
                    if (match(node.Value))
                    {
                        return true;
                    }
                    node = node.Nodes[0];
                }
            }
            return false;
        }
        /// <summary>
        /// Copies the complete <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.</param>
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }
        /// <summary>
        /// Copies the complete <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> to a compatible one-dimensional array, starting at the specific array index.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the 
        /// <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>. The array must have zero-based indexing</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, _count);
        }
        /// <summary>
        /// Copies a specific number of elements from the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> 
        /// to a compatible one-dimensional array, starting at the specific array index.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the 
        /// <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>. The array must have zero-based indexing</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopyTo(T[] array, int index, int count)
        {
            CollectionHelper.CopyToCheck(array, index, count);
            ListWalk(x =>
            {
                if (index >= count)
                {
                    return false;
                }
                array[index] = x.Value;
                index++;
                return true;
            });
        }
        /// <summary>
        /// Returns an enumerator that iterates through <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.
        /// </summary>
        /// <returns>Returns an enumerator that iterates through <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.</returns>
        public Enumerator GetEnumerator()
        {
            InternalCheckVersion();
            return new Enumerator(this);
        }
        /// <summary>
        /// Removes a specified value from the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.
        /// </summary>
        /// <param name="value">The element to remove.</param>
        /// <returns>Returns true if the element was removed, otherwise returns false.</returns>
        public bool Remove(T value)
        {
            return InternalRemove(value);
        }
        /// <summary>
        /// Removes a specified value from the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> by searching every element that matches the value by using the
        /// <see cref="T:System.Collections.Generic.IComparer`1"/> implementation provided by the Comparer property, and the specified <see cref="T:System.Predicate`1"/>.
        /// </summary>
        /// <param name="value">The value to locate in the list.</param>
        /// <param name="match">The predicate used to determinate if an element exists.</param>
        /// <returns>true if any value matches both, the <see cref="T:System.Collections.Generic.IComparer`1"/> provided by the Comparer property and the specified <see cref="T:System.Predicate`1"/> and the found value was also successfully removed.</returns>
        public bool Remove(T value, Predicate<T> match)
        {
            if (match == null)
            {
                Thrower.ArgumentNullException(ArgumentType.match);
            }
            return InternalRemove(value, match);
        }
        /// <summary>
        /// Removes all the values that match the conditions defined by the specified predicate from a <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.
        /// </summary>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements that were removed from the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> collection.</returns>
        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
            {
                Thrower.ArgumentNullException(ArgumentType.match);
            }
            List<T> matchesList = new List<T>(Count);
            ListWalk(x =>
            {
                if (match(x.Value))
                {
                    matchesList.Add(x.Value);
                }
                return true;
            });
            return matchesList.Count(InternalRemove);
        }
        /// <summary>
        /// Return a view of a sublist in a <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.
        /// </summary>
        /// <param name="lowerValue">The lowest desired value in the view.</param>
        /// <param name="upperValue">The highest desired value in the view.</param>
        /// <returns>A sublist view that contains only the values in the specified range.</returns>
        public virtual SortedLinkedList<T> GetViewBetween(T lowerValue, T upperValue)
        {
            if (comparer.Compare(lowerValue, upperValue) > 0)
            {
                Thrower.ArgumentException(ArgumentType.empty, Resources.Argument_lowerBiggerThanUpper);
            }
            return new SubListView(this, lowerValue, upperValue);
        }
        /// <summary>
        /// Return a view of a sublist in a <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> containing the range where the stored values matches the specified value
        /// using the comparer provided by the <see cref="Comparer"/> property.
        /// </summary>
        /// <param name="value">The value used to determine the range.</param>
        /// <returns>A sublist view that contains only the values that matches the specified value.</returns>
        public SortedLinkedList<T> GetMatchView(T value)
        {
            return GetViewBetween(value, value);
        }
        /// <summary>
        /// Implements the <see cref="T:System.Runtime.Serialization.ISerializable"/> interface and returns the data that you must have to serialize an <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that contains the information that is required to serialize the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> object.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure that contains the source and destination of the serialized stream associated with the  <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/> object.</param>
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                Thrower.ArgumentNullException(ArgumentType.info);
            }
            info.AddValue(SerializationString.Version, _version);
            info.AddValue(SerializationString.Count, _count);
            info.AddValue(SerializationString.Comparer, comparer, typeof(IComparer<T>));
            if (_count != 0)
            {
                T[] local = new T[Count];
                int index = 0;
                foreach (T item in this)
                {
                    local[index++] = item;
                }
                info.AddValue(SerializationString.Data, local, typeof(T[]));
            }
        }
        /// <summary>
        /// Implements the <see cref="T:System.Runtime.Serialization.ISerializable"/> interface, and raises the deserialization event when the deserialization is complete.
        /// </summary>
        /// <param name="sender">The source of the deserialization event.</param>
        protected virtual void OnDeserialization(object sender)
        {
            if (comparer == null)
            {
                if (_sinfo == null)
                {
                    Thrower.SerializationException(Resources.Serialization_InvalidOnDeserialization);
                }
                _random = new Random();
                comparer = (IComparer<T>)_sinfo.GetValue(SerializationString.Comparer, typeof(IComparer<T>));
                _count = _sinfo.GetInt32(SerializationString.Count);
                _head = new Node(default(T), MathHelper.Log2N(_count));
                if (_count > 0)
                {
                    T[] data = (T[])_sinfo.GetValue(SerializationString.Data, typeof(T[]));
                    if (data == null)
                    {
                        Thrower.SerializationException(Resources.Serialization_ValuesMissing);
                    }
                    foreach (T t in data)
                    {
                        InternalAdd(t);
                    }
                }
                _version = _sinfo.GetInt32(SerializationString.Version);
                _sinfo = null;
            }
        }

        #region Explicit Region
        void ICollection.CopyTo(Array array, int index)
        {
            CollectionHelper.CopyTo<T>(array, index, _count, this);
        }
        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
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
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            OnDeserialization(sender);
        }
        #endregion

        #region Internal
        internal virtual void InternalAdd(T item)
        {
            int height = _head.Count;
            int num = GetRandomHeight(height + 1);
            Node n = new Node(item, num);
            if (n.Count > _head.Count)
            {
                _head.IncHeight();
                _head.Nodes[_head.Count - 1] = n;
                num--;
            }
            Node current = _head;
            for (int i = height - 1; i >= 0; i--)
            {
                while (current.Nodes[i] != null && comparer.Compare(current.Nodes[i].Value, item) <= 0)
                {
                    current = current.Nodes[i];
                }
                if (i < num)
                {
                    n.Nodes[i] = current.Nodes[i];
                    current.Nodes[i] = n;
                }
            }
            _count++;
            _version++;
        }
        internal bool ListWalk(FindMatchPredicate predicate)
        {
            Node current = StartNode();
            while (current != null && IsInRange(current.Value))
            {
                if (!predicate(current))
                {
                    return false;
                }
                current = current.Nodes[0];
            }
            return true;
        }
        internal Node FindNode(T item)
        {
            Node current = _head;
            Node node = null;
            for (int i = _head.Count - 1; i >= 0; i--)
            {
                while (current.Nodes[i] != null && IsInRange(current.Nodes[i].Value))
                {
                    int results = comparer.Compare(current.Nodes[i].Value, item);
                    if (results < 0)
                    {
                        current = current.Nodes[i];
                    }
                    else
                    {
                        if (results == 0)
                        {
                            node = current.Nodes[i];
                        }
                        break;
                    }
                }
            }
            return node;
        }
        internal Node[] FindIndexNode(T item)
        {
            Node[] index = new Node[_head.Count];
            Node node = _head;
            for (int i = _head.Count - 1; i >= 0; i--)
            {
                while ((node.Nodes[i] != null) && (comparer.Compare(node.Nodes[i].Value, item) < 0))
                {
                    node = node.Nodes[i];
                }
                index[i] = node;
            }
            return index;
        }
        internal Node FindBefore(T min)
        {
            Node node = _head;
            Node node2 = null;
            for (int i = _head.Count - 1; i >= 0; i--)
            {
                while ((node.Nodes[i] != null) && (comparer.Compare(node.Nodes[i].Value, min) < 0))
                {
                    node = node.Nodes[i];
                }
                node2 = node.Nodes[i];
            }
            return node2;
        }
        internal void ReconstructFrom(IEnumerable<T> collection)
        {
            T[] list = collection.ToArray();
            Array.Sort(list, comparer);
            _head = ConstructHeadFrom(list);
            _count = list.Length;
            _version++;
        }
        internal int GetRandomHeight(int maxLevel)
        {
            int level = 1;
            while (_random.NextDouble() < ProbFactor && level < maxLevel)
                level++;
            return level;
        }
        internal virtual bool InternalRemove(T item)
        {
            Node[] index = FindIndexNode(item);
            Node current = index[0].Nodes[0];

            if (current != null && comparer.Compare(current.Value, item) == 0)
            {
                for (int i = 0; (i < _head.Count) && ((index[i].Nodes[i] == current)); i++)
                {
                    index[i].Nodes[i] = current.Nodes[i];
                }

                if (_head.Nodes[_head.Count - 1] == null)
                    _head.DecHeight();

                TrimHeight();
                _count--;
                _version++;
                return true;
            }
            return false;
        }
        internal virtual bool InternalRemove(T item, Predicate<T> match)
        {
            Node[] index = FindIndexNode(item);
            Node current = index[0];
            if (current.Nodes[0] != null && comparer.Compare(current.Nodes[0].Value, item) == 0)
            {
                while ((comparer.Compare(current.Nodes[0].Value, item) == 0) && (!match(current.Nodes[0].Value)))
                {
                    current = current.Nodes[0];
                    if (current.Nodes[0] == null)
                    {
                        return false;
                    }
                }
                Node matchNode = current.Nodes[0];
                current.Nodes[0] = matchNode.Nodes[0];
                for (int i = 1; i < _head.Count; i++)
                {
                    while ((index[i].Nodes[i] != null) && (comparer.Compare(current.Value, item) == 0))
                    {
                        if (index[i].Nodes[i] == matchNode)
                        {
                            index[i].Nodes[i] = matchNode.Nodes[i];
                            break;
                        }
                        index[i] = index[i].Nodes[i];
                    }
                }

                if (_head.Nodes[_head.Count - 1] == null)
                    _head.DecHeight();

                TrimHeight();
                _count--;
                _version++;
                return true;
            }
            return false;
        }

        private void TrimHeight()
        {
            int height = MathHelper.Log2N(Count);
            if (((int)(height * 1.5f)) < _head.Count)
            {
                Node node = _head;
                while (node != null)
                {
                    node.Resize(height);
                    node = node.Nodes[height - 1];
                }
            }
        }
        internal virtual void InternalCheckVersion()
        {
        }
        internal virtual bool IsInRange(T item)
        {
            return true;
        }
        internal static Node ConstructHeadFrom(IList<T> list)
        {
            int length = list.Count;
            int height = MathHelper.Log2N(length);
            Node[] array2 = new Node[length];
            int jump = (int)Math.Pow(2, height - 1);
            Node head = new Node(default(T), height);
            for (int i = height - 1; i >= 0; i--)
            {
                Node last = head;
                int index = jump - 1;
                do
                {
                    Node current = array2[index] ?? (array2[index] = new Node(list[index], i + 1));
                    last.Nodes[i] = current;
                    last = current;
                    index += jump;

                } while (index < length);
                jump >>= 1;
            }
            return head;
        }
        internal virtual Node StartNode()
        {
            return _head.Nodes[0];
        }
        #endregion

        [Serializable]
        internal class SubListView : SortedLinkedList<T>
        {
            private SortedLinkedList<T> _underlying;
            private Node _start;
            private T _max;
            private T _min;

            public SubListView(SortedLinkedList<T> list, T min, T max)
            {
                this._underlying = list;
                this._version = list._version - 1;
                this._min = min;
                this._max = max;
                this.InternalCheckVersion();
            }
            public SubListView(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }

            public override void Clear()
            {
                if (Count == _underlying.Count)
                {
                    _underlying.Clear();
                }
                else
                {
                    _underlying.RemoveWhere(IsInRange);
                }
                _count = 0;
                UpdateSubList();
            }
            public override SortedLinkedList<T> GetViewBetween(T lowerValue, T upperValue)
            {
                if (comparer.Compare(_min, lowerValue) > 0)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.lowerValue, Resources.ArgumentOutOfRange_ViewValue);
                }
                if (comparer.Compare(_max, upperValue) < 0)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.upperValue, Resources.ArgumentOutOfRange_ViewValue);
                }
                return _underlying.GetViewBetween(lowerValue, upperValue);
            }

            internal override Node StartNode()
            {
                InternalCheckVersion();
                return _start;
            }
            internal override void InternalAdd(T item)
            {
                if (!IsInRange(item))
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.item, null);
                }
                _underlying.InternalAdd(item);
                _count++;
            }
            internal override void InternalCheckVersion()
            {
                if (_version != _underlying._version)
                {
                    UpdateSubList();
                    CalculateCount();
                }
            }
            internal override bool IsInRange(T item)
            {
                int low = comparer.Compare(_min, item);
                if (low > 0)
                {
                    return false;
                }
                return comparer.Compare(_max, item) >= 0;
            }
            internal override bool InternalRemove(T item)
            {
                if (IsInRange(item))
                {
                    bool flagResult = _underlying.InternalRemove(item);
                    if (flagResult)
                    {
                        _count--;
                    }
                    return flagResult;
                }
                return false;
            }
            internal override bool InternalRemove(T item, Predicate<T> match)
            {
                if (IsInRange(item))
                {
                    bool flagResult = _underlying.InternalRemove(item, match);
                    if (flagResult)
                    {
                        _count--;
                    }
                    return flagResult;
                }
                return false;
            }

            protected override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.info);
                }
                info.AddValue(SerializationString.Max, _max);
                info.AddValue(SerializationString.Min, _min);
                base.GetObjectData(info, context);
            }
            protected override void OnDeserialization(object sender)
            {
                if (_sinfo != null)
                {
                    comparer = (IComparer<T>)_sinfo.GetValue(SerializationString.Comparer, typeof(IComparer<T>));
                    _max = (T)_sinfo.GetValue(SerializationString.Max, typeof(T));
                    _min = (T)_sinfo.GetValue(SerializationString.Min, typeof(T));
                    int count = _sinfo.GetInt32(SerializationString.Count);
                    if (count != 0)
                    {
                        T[] data = (T[])_sinfo.GetValue(SerializationString.Data, typeof(T[]));
                        if (data == null)
                        {
                            Thrower.SerializationException(Resources.Serialization_ValuesMissing);
                        }
                        _underlying = new SortedLinkedList<T>(data);
                    }
                    else
                    {
                        _underlying = new SortedLinkedList<T>();
                    }
                    if (count != _underlying._count)
                    {
                        Thrower.SerializationException(Resources.Serialization_CountNotMatch);
                    }
                    _underlying._version = _sinfo.GetInt32(SerializationString.Version);
                    _count = _underlying._count;
                    UpdateSubList();
                    _sinfo = null;
                }
            }

            private void CalculateCount()
            {
                int count = 0;
                ListWalk(x =>
                {
                    count++;
                    return true;
                });
                _count = count;
            }
            private void UpdateSubList()
            {
                _head = _underlying._head;
                _start = _underlying.FindBefore(_min);
                _version = _underlying._version;
            }
        }

        internal class Node
        {
            internal T Value;
            internal int Count;
            internal Node[] Nodes;

            internal Node()
            {
                this.Value = default(T);
                this.Count = 1;
                this.Nodes = new Node[4];
            }
            internal Node(T value, int height)
            {
                this.Value = value;
                Nodes = new Node[height];
                Count = height;
            }
            internal void IncHeight()
            {
                if (Count == Nodes.Length)
                {
                    Resize(Count + 2);
                }
                Count++;
            }
            internal void DecHeight()
            {
                Count--;
            }
            internal void Resize(int newSize)
            {
                Node[] array = new Node[newSize];
                int length = Math.Min(Count, newSize);
                Array.Copy(Nodes, 0, array, 0, length);
                Nodes = array;
            }
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>.
        /// </summary>
        [Serializable, DebuggerDisplay("Current = {Current}")]
        public struct Enumerator : IEnumerator<T>, IEnumerator, ISerializable, IDeserializationCallback
        {
            private SortedLinkedList<T> _list;
            private int _version;
            private int _index;
            private T _current;
            private Node _node;
            private int steps;
            private SerializationInfo _info;

            private void SetNode()
            {
                if (_index != _list.Count + 1)
                {
                    if (_index == 0)
                    {
                        _node = _list._head.Nodes[0];
                    }
                    else
                    {
                        Node node = _list.FindNode(_current);
                        while (steps > 0)
                        {
                            node = node.Nodes[0];
                            steps--;
                        }
                        _node = node;
                    }
                }
            }
            private void CheckVersion()
            {
                if (_version != _list._version)
                {
                    Thrower.EnumeratorCorrupted();
                }
            }

            internal Enumerator(SortedLinkedList<T> list)
            {
                _list = list;
                _version = list._version;
                _index = 0;
                _current = default(T);
                _node = list.StartNode();
                steps = -1;
                _info = null;
            }
            private Enumerator(SerializationInfo info, StreamingContext context)
            {
                _list = null;
                _version = 0;
                _index = 0;
                _current = default(T);
                _node = null;
                steps = -1;
                _info = info;
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
            /// Advances the enumerator to the next element of the <see cref="T:Academy.Collections.Generic.SortedLinkedList`1"/>
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                CheckVersion();
                if (_node == null || !_list.IsInRange(_node.Value))
                {
                    _index = _list.Count + 1;
                    return false;
                }
                _index++;
                T before = Current;
                _current = _node.Value;
                _node = _node.Nodes[0];

                if (before != null)
                {
                    int num = _list.comparer.Compare(_current, before);
                    if ((num == 0) && (_index != 1))
                    {
                        steps++;
                    }
                    else
                    {
                        steps = 0;
                    }
                }
                return true;
            }

            void IDisposable.Dispose()
            {
            }
            object IEnumerator.Current
            {
                get
                {
                    if (this._index == 0)
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                    if (this._index == this._list.Count + 1)
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                    return _current;
                }
            }
            void IEnumerator.Reset()
            {
                CheckVersion();
                _index = 0;
                _current = default(T);
                _node = null;
            }
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.info);
                }
                info.AddValue(SerializationString.Collection, _list);
                info.AddValue(SerializationString.Version, _version);
                info.AddValue(SerializationString.Index, _index);
                info.AddValue(SerializationString.Current, _current);
            }
            void IDeserializationCallback.OnDeserialization(object sender)
            {
                if (_list == null)
                {
                    if (_info == null)
                    {
                        Thrower.SerializationException(Resources.Serialization_InvalidOnDeserialization);
                    }
                    _list = (SortedLinkedList<T>)_info.GetValue(SerializationString.Collection, typeof(SortedLinkedList<T>));
                    _version = _info.GetInt32(SerializationString.Version);
                    _index = _info.GetInt32(SerializationString.Index);
                    _current = (T)_info.GetValue(SerializationString.Current, typeof(T));
                    if (_list._sinfo != null)
                    {
                        _list.OnDeserialization(sender);
                    }
                    SetNode();
                }
                _info = null;
            }
        }
    }
}
