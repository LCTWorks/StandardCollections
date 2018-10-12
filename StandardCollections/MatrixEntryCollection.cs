using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace StandardCollections
{
    /// <summary>
    /// Represents a collection of <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the entries contained in the collection.</typeparam>
    [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ReadOnlyCollectionProxy<>))]
    public abstract class MatrixEntryCollection<T> : IReadOnlyCollection<MatrixEntry<T>>, ICollection
    {
        [NonSerialized]
        internal object syncRoot;
        internal MatrixDataOrder order;

        /// <summary>
        /// When overrided in a derived class, gets the number of elements in the collection.
        /// </summary>
        public virtual int Count
        {
            get
            {
                return this.Count();
            }
        }
        /// <summary>
        /// Gets or sets the order the <see cref="T:Academy.Collections.Generic.MatrixEntryCollection`1"/> iterates through the contained elements.
        /// </summary>
        public virtual MatrixDataOrder Order
        {
            get
            {
                return order;
            }
            protected set
            {
                if (value == MatrixDataOrder.Default)
                {
                    value = MatrixDataOrder.Row;
                }
                this.order = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.MatrixEntryCollection`1"/> class with the specified order.
        /// </summary>
        /// <param name="order">The order used to enumerate through the elements contained in the collection.</param>
        protected MatrixEntryCollection(MatrixDataOrder order)
        {
            this.order = order;
        }

        /// <summary>
        /// Gets whether the collection contains an entry with the specified indices.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="columnIndex">the column index.</param>
        /// <returns>true if the collection contains an entry with the specified row and column indices.</returns>
        public virtual bool ContainsEntry(int rowIndex, int columnIndex)
        {
            foreach (MatrixEntry<T> entry in this)
            {
                if ((entry.RowIndex == rowIndex) && (entry.ColumnIndex == columnIndex))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// When implemented in a derived class, gets an enumerator that iterates through the elements of the <see cref="T:Academy.Collections.Generic.MatrixEntryCollection`1"/>.
        /// </summary>
        /// <returns>returns an enumerator that iterates through the elements of the <see cref="T:Academy.Collections.Generic.MatrixEntryCollection`1"/>.</returns>
        public abstract IEnumerator<MatrixEntry<T>> GetEnumerator();

        /// <summary>
        /// Creates a new instance of <see cref="T:Academy.Collections.Generic.MatrixEntryCollection`1"/> that contains 
        /// elements copied from the specified collection and sorted by the specified order.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:Academy.Collections.Generic.MatrixEntryCollection`1"/>.</param>
        /// <param name="order">The order the collection presents the entries.</param>
        /// <returns>a new instance of <see cref="T:Academy.Collections.Generic.MatrixEntryCollection`1"/> containing elements copied from the specified collection 
        /// and provides the specified order.</returns>
        public static MatrixEntryCollection<T> Create(IEnumerable<MatrixEntry<T>> collection, MatrixDataOrder order)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            if (order == MatrixDataOrder.Default)
            {
                order = MatrixDataOrder.Row;
            }
            return new EntryFixedCollection(collection, order);
        }

        #region Explicit Implementation
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        void ICollection.CopyTo(Array array, int index)
        {
            CollectionHelper.CopyTo<T>(array, index, Count, this);
        }
        int ICollection.Count
        {
            get { return Count; }
        }
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }
        object ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref syncRoot, new object(), null);
                }
                return syncRoot;
            }
        }
        #endregion

        private class EntryFixedCollection : MatrixEntryCollection<T>
        {
            private readonly IComparer<MatrixEntry<T>> comparer;
            private List<MatrixEntry<T>> list;

            public EntryFixedCollection(IEnumerable<MatrixEntry<T>> collection, MatrixDataOrder order)
                : base(order)
            {
                list = new List<MatrixEntry<T>>(collection);
                if (order != MatrixDataOrder.Merged)
                {
                    IComparer<MatrixEntry<T>> comparer = MatrixEntry<T>.CreateEntryComparer(order);
                    list.Sort(comparer);
                }
                else
                {
                    comparer = null;
                }
            }

            public override int Count
            {
                get
                {
                    return list.Count;
                }
            }
            public override bool ContainsEntry(int rowIndex, int columnIndex)
            {
                MatrixEntry<T> entry = new MatrixEntry<T>(rowIndex, columnIndex, default(T));
                if (comparer != null)
                {
                    int index = list.BinarySearch(entry, comparer);
                    return index >= 0;
                }
                return list.Contains(entry);
            }
            public override IEnumerator<MatrixEntry<T>> GetEnumerator()
            {
                return list.GetEnumerator();
            }
        }

    }
}
