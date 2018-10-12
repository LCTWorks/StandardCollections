using StandardCollections.Events;
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
    /// Represents a first-in, first-out collection managing a fixed amount of objects. 
    /// At maximum capacity, the collection deletes the first item when a new item is been inserted.
    /// </summary>
    /// <typeparam name="T">The type of objects in the queue.</typeparam>
    [DebuggerTypeProxy(typeof(EnumerableProxy<>)), DebuggerDisplay("Count = {Count}")]
    public class EndlessQueue<T> : IEnumerable<T>, IReadOnlyCollection<T>, ICollection
    {
        private const int DefSize = 4;
        private int head;
        private int tail;
        private T[] items;
        private int count;
        private int version;
        private object _syncRoot;

        public event ItemReplacingEventHandler<T> ItemReplacing;
        public event ItemReplacedEventHandler<T> ItemReplaced;

        /// <summary>
        /// The number of objects contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
        }
        /// <summary>
        /// Gets or sets the total number of elements the internal 
        /// data structure can hold before start deleting.
        /// </summary>
        public int Capacity
        {
            get
            {
                return items.Length;
            }
            set
            {
                int num = value;
                if (value < 0)
                {
                    num = 0;
                }
                if (num != items.Length)
                {
                    T[] newArray = new T[num];
                    if (num > items.Length)
                    {
                        if (this.count > 0)
                        {
                            if (this.head < this.tail)
                            {
                                Array.Copy(this.items, this.head, newArray, 0, this.tail);
                            }
                            else
                            {
                                Array.Copy(this.items, this.head, newArray, 0, this.items.Length - this.head);
                                Array.Copy(this.items, 0, newArray, this.items.Length - this.head, this.tail);
                            }
                        }
                    }
                    else
                    {
                        var e = GetEnumerator();
                        int index = 0;
                        while (e.MoveNext() && index < num)
                        {
                            newArray[index++] = e.Current;
                        }
                        this.count = num;
                    }
                    this.items = newArray;
                    this.head = 0;
                    this.tail = (this.count >= num) ? 0 : this.count;
                    this.version++;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/> 
        /// class that is empty and has the specified capacity.
        /// </summary>
        /// <param name="capacity">The number of elements the collection can contain.</param>
        public EndlessQueue(int capacity)
        {
            if (capacity < 0)
            {
                capacity = DefSize;
            }
            this.head = 0;
            this.tail = 0;
            this.items = new T[capacity];
            this.count = 0;
        }
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/> 
        /// class that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.</param>
        public EndlessQueue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            if (!CollectionHelper.IsWellKnownCollection(collection, out int size))
            {
                size = DefSize;
            }
            this.head = 0;
            this.tail = 0;
            this.items = new T[DefSize];
            this.count = 0;
            foreach (var item in collection)
            {
                if (this.Count == Capacity)
                {
                    Capacity = Capacity * 2;
                }
                Enqueue(item);
            }
            Capacity = Count;
        }

        /// <summary>
        /// Adds an object to the end of the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
        /// </summary>
        /// <param name="item">
        /// The object to add to the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>. 
        /// The value can be null for reference types.
        /// </param>
        public void Enqueue(T item)
        {
            //Responding to replacING events.
            bool isReplacing = Count == Capacity;
            if ((isReplacing) && (ItemReplacing != null))
            {
                var args = new ItemReplacingEventArgs<T>(item, items[tail]);
                ItemReplacing(this, args);
                if (args.Handled)
                {
                    return;
                }
            }
            //
            if (tail == head && count > 0)
            {
                head = NextIndex(head);
            }
            var oldItem = items[tail];
            items[tail] = item;
            tail = NextIndex(tail);
            //Responding to replacED event.
            if (isReplacing && ItemReplaced != null)
            {
                var args2 = new ItemReplacedEventArgs<T>(item, oldItem);
                ItemReplaced(this, args2);
            }
            //
            if (items.Length >= (count + 1))
            {
                count++;
            }
            version++;
        }
        /// <summary>
        /// Inserts an object at the end of the <see cref="EndlessQueue{T}"/> and 
        /// returns whether the collection was full, in which case the first item will be deleted allowing an 
        /// empty space for the new item.
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="EndlessQueue{T}"/>. 
        /// The value can be null for reference types.
        /// </param>
        /// <param name="deletedItem">
        /// When this method returns, contains the deleted value at the beginning of the collection 
        /// if the collection was full; otherwise contains the type default value.
        /// </param>
        /// <returns>
        /// true if the number of items in the collection reached the maximum capacity before 
        /// the operation performed, otherwise returns false.
        /// </returns>
        public bool Enqueue(T item, out T deletedItem)
        {
            bool flag = false;
            deletedItem = default(T);

            //Responding to replacING event.
            bool isReplacing = Count == Capacity;
            if ((isReplacing) && (ItemReplacing != null))
            {
                var args = new ItemReplacingEventArgs<T>(item, items[tail]);
                ItemReplacing(this, args);
                if (args.Handled)
                {
                    return false;
                }
            }
            //
            if (tail == head && count > 0)
            {
                deletedItem = items[head];
                head = NextIndex(head);
                flag = true;
            }
            this.items[tail] = item;
            //Responding to replacED event.
            if (flag && ItemReplaced != null)
            {
                var oldItem = deletedItem;
                var args2 = new ItemReplacedEventArgs<T>(item, oldItem);
                ItemReplaced(this, args2);
            }
            //
            tail = NextIndex(tail);
            if (items.Length >= (count + 1))
            {
                count++;
            }
            version++;
            return flag;
        }
        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.</returns>
        public T Dequeue()
        {
            if (count == 0)
            {
                Thrower.InvalidOperationException(Resources.InvalidOperation_EmptyCollection);
            }

            T item = this.items[head];
            head = (head + 1) % items.Length;
            count--;
            version++;
            return item;
        }
        /// <summary>
        /// Returns the object at the beginning of the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.</returns>
        public T Peek()
        {
            if (count == 0)
            {
                Thrower.InvalidOperationException(Resources.InvalidOperation_EmptyCollection);
            }
            return items[head];
        }
        /// <summary>
        /// Removes all objects from the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
        /// </summary>
        public void Clear()
        {
            if (head < tail)
            {
                Array.Clear(items, head, items.Length);
            }
            else
            {
                Array.Clear(items, head, items.Length - head);
                Array.Clear(items, 0, tail);
            }
            head = 0;
            tail = 0;
            count = 0;
            version++;
        }
        /// <summary>
        /// Determines whether an element is in the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>. 
        /// The value can be null for reference types
        /// </param>
        /// <returns>
        /// true if the item was found in the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>; 
        /// otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            int index = this.head;
            int num = this.count;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (num-- > 0)
            {
                if (item == null)
                {
                    if (this.items[index] == null)
                    {
                        return true;
                    }
                }
                else if ((this.items[index] != null) && comparer.Equals(this.items[index], item))
                {
                    return true;
                }
                index = NextIndex(index);
            }
            return false;
        }
        /// <summary>
        /// Sets the collection capacity to the number of elements in the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>,
        /// if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess()
        {
            int num = (int)(this.items.Length * 0.9);
            if (this.count < num)
            {
                Capacity = count;
            }
        }
        /// <summary>
        /// Copies the entire <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/> 
        /// to a new array.
        /// </summary>
        /// <returns>
        /// A new array containing copies of the elements of the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
        /// </returns>
        public T[] ToArray()
        {
            T[] array = new T[count];
            if (count > 0)
            {
                if (head < tail)
                {
                    Array.Copy(this.items, this.head, array, 0, this.count);
                    return array;
                }
                Array.Copy(this.items, this.head, array, 0, this.items.Length - this.head);
                Array.Copy(this.items, 0, array, this.items.Length - this.head, this.tail);
            }
            return array;
        }

        /// <summary>
        /// Copies the complete <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/> to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.</param>
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, this.Count);
        }
        /// <summary>
        /// Copies the complete <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/> to a compatible one-dimensional array, starting at the specific array index.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from the 
        /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>. The array must have zero-based indexing</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, this.Count);
        }
        /// <summary>
        /// Copies a specific number of elements from the complete <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/> to a compatible one-dimensional array, starting at the specific array index.
        /// </summary>
        /// <param name="array">A one-dimensional array that is the destination of the elements copied from 
        /// the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>. The array must have zero-based indexing</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopyTo(T[] array, int index, int count)
        {
            CollectionHelper.CopyTo(array, index, count, this);
        }

        /// <summary>
        /// Returns an enumerator for the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:Academy.Collections.Generic.EndlessQueue`1.Enumerator"/> 
        /// for the <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        private int NextIndex(int index)
        {
            return (index + 1) % items.Length;
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
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        internal T GetElementAt(int index)
        {
            return this.items[(head + index) % items.Length];
        }

        /// <summary>
        /// Enumerates the elements of an <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
        /// </summary>
        [DebuggerDisplay("Current = {Current}")]
        public struct Enumerator : IEnumerator<T>
        {
            private int index;
            private int version;
            private EndlessQueue<T> queue;
            private T current;

            internal Enumerator(EndlessQueue<T> queue)
            {
                this.queue = queue;
                this.version = queue.version;
                this.current = default(T);
                this.index = -1;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public T Current
            {
                get
                {
                    if (index < 0)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumInvalid);
                    }
                    return current;
                }
            }
            /// <summary>
            /// Releases all resources used by the 
            /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1.Enumerator"/>.
            /// </summary>
            public void Dispose()
            {
                this.index = -2;
                this.current = default(T);
            }
            object IEnumerator.Current
            {
                get { return Current; }
            }
            /// <summary>
            /// Advances the enumerator to the next element of the 
            /// <see cref="T:Academy.Collections.Generic.EndlessQueue`1"/>.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the 
            /// next element; false if the enumeration has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                CheckVersion();
                if (index == -2)
                {
                    return false;
                }
                index++;
                if (index == queue.count)
                {
                    index = -2;
                    current = default(T);
                    return false;
                }
                current = queue.GetElementAt(index);
                return true;
            }
            void IEnumerator.Reset()
            {
                CheckVersion();
                this.index = -1;
                this.current = default(T);
            }
            private void CheckVersion()
            {
                if (version != queue.version)
                {
                    Thrower.InvalidOperationException(Resources.InvalidOperation_EnumCorrupt);
                }
            }
        }
    }
}
