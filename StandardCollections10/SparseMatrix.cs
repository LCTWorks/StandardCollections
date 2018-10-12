using StandardCollections.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using StandardCollections.ObjectModel;

namespace StandardCollections
{
    /// <summary>
    /// Represents a big dimensions matrix of objects that contains only few values different from a specified default value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the matrix.</typeparam>
    [DebuggerTypeProxy(typeof(MatrixProxy<>)), DebuggerDisplay("Count = {Count}")]
    public class SparseMatrix<T> : IMatrix<T>, ICollection, IReadOnlyCollection<MatrixEntry<T>>
    {
        #region Private and Internal Fields
        private static readonly Index FirstIndex = new Index { row = 0, col = 0 };
        private const int DefaultDimension = 1000;
        internal SortedDictionary<Index, T> dict;
        internal ValueEqualityComparer comparer;
        internal NonDefaultEntriesCollection nonDefaultEntries;
        internal T def;
        internal int rowDim;
        internal int colDim;
        internal int version;
        internal long count;
        internal MatrixDataOrder order;
        private static readonly Type type = typeof(SparseMatrix<T>);
        private static readonly MatrixEntry<T> lowerEntry = new MatrixEntry<T>(0, 0, default(T));
        #endregion

        #region Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/> class with specified dimensions.
        /// </summary>
        /// <param name="rowCount">The number of rows in the matrix.</param>
        /// <param name="columnCount">The number of columns in the matrix.</param>
        public SparseMatrix(int rowCount, int columnCount)
            : this(rowCount, columnCount, default(T), MatrixDataOrder.Default)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/> class with specified dimensions and default value.
        /// </summary>
        /// <param name="rowCount">The number of rows in the matrix.</param>
        /// <param name="columnCount">The number of columns in the matrix.</param>
        /// <param name="defaultValue">The value the matrix will assume as the default value.</param>
        public SparseMatrix(int rowCount, int columnCount, T defaultValue)
            : this(rowCount, columnCount, defaultValue, MatrixDataOrder.Default)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/> class with specified dimensions and data order.
        /// </summary>
        /// <param name="rowCount">The number of rows in the matrix.</param>
        /// <param name="columnCount">The number of columns in the matrix.</param>
        /// <param name="order">The order the matrix will store elements in memory. 'Default' and 'Merged' values are used as 'Row' value.</param>
        public SparseMatrix(int rowCount, int columnCount, MatrixDataOrder order)
            : this(rowCount, columnCount, default(T), order)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/> class with specified dimensions, data order and default value.
        /// </summary>
        /// <param name="rowCount">The number of rows in the matrix.</param>
        /// <param name="columnCount">The number of columns in the matrix.</param>
        /// <param name="defaultValue">The value the matrix will assume as the default value.</param>
        /// <param name="dataOrder">The order the matrix will store elements in memory. 'Default' and 'Merged' values are used as 'Row' value.</param>
        public SparseMatrix(int rowCount, int columnCount, T defaultValue, MatrixDataOrder dataOrder)
        {
            if (dataOrder == MatrixDataOrder.Default)
            {
                dataOrder = MatrixDataOrder.Row;
            }
            this.order = dataOrder;
            IComparer<Index> comparer;
            if (dataOrder == MatrixDataOrder.Row)
            {
                comparer = new RowFirstIndexComparer();
            }
            else
            {
                comparer = new ColumnFirstIndexComparer();
            }
            dict = new SortedDictionary<Index, T>(comparer);
            this.comparer = new ValueEqualityComparer();
            this.rowDim = rowCount;
            this.colDim = columnCount;
            this.count = (long)rowCount * (long)columnCount;
            this.def = defaultValue;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/> class with specified dimensions, data order, 
        /// default value and elements copied from the specified collection and positioned in the indices specified by the respective entries.
        /// </summary>
        /// <param name="entries">The collection whose elements are copied to the new matrix.</param>
        /// <param name="rowCount">The amount of rows of the matrix.</param>
        /// <param name="columnDimension">The amount of columns of the matrix.</param>
        /// <param name="defaultValue">The value the matrix will assume as the default value.</param>
        /// <param name="order">The order the matrix will store elements in memory. 'Default' and 'Merged' values are used as 'Row' value.</param>
        public SparseMatrix(IEnumerable<MatrixEntry<T>> entries, int rowCount, int columnDimension, T defaultValue, MatrixDataOrder order)
        {
            if (order != MatrixDataOrder.Column)
            {
                order = MatrixDataOrder.Row;
            }
            this.order = order;
            IComparer<Index> comparer;
            if (order == MatrixDataOrder.Row)
            {
                comparer = new RowFirstIndexComparer();
            }
            else
            {
                comparer = new ColumnFirstIndexComparer();
            }
            dict = new SortedDictionary<Index, T>(comparer);
            MatrixEntry<T> upperEntry = new MatrixEntry<T>(rowCount - 1, columnDimension - 1, default(T));
            this.def = defaultValue;
            this.comparer = new ValueEqualityComparer();
            this.rowDim = rowCount;
            this.colDim = columnDimension;
            this.count = (long)rowCount * (long)columnDimension;
            foreach (MatrixEntry<T> entry in entries)
            {
                int num = entry.CompareTo(lowerEntry);
                int num2 = entry.CompareTo(upperEntry);
                if ((num < 0) || (num2 > 0))
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.empty, Resources.ArgumentOutOfRange_EntryOutOfBounds);
                }
                DirectlySet(entry.RowIndex, entry.ColumnIndex, entry.Value);
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the element at the specified indices.
        /// </summary>
        /// <param name="rowIndex">The zero-based row index of the element to get or set.</param>
        /// <param name="columnIndex">The zero-based column index of the element to get or set.</param>
        public T this[int rowIndex, int columnIndex]
        {
            get
            {
                if (rowIndex < 0 || rowIndex >= rowDim)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
                }
                if (columnIndex < 0 && columnIndex >= colDim)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.columnIndex, Resources.ArgumentOutOfRange_ColIndex);
                }
                return DirectlyGet(rowIndex, columnIndex);
            }
            set
            {
                if (rowIndex < 0 || rowIndex >= rowDim)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
                }
                if (columnIndex < 0 && columnIndex >= colDim)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.columnIndex, Resources.ArgumentOutOfRange_ColIndex);
                }
                DirectlySet(rowIndex, columnIndex, value);
                version++;
            }
        }
        /// <summary>
        /// Gets the number of columns contained in the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        public int ColumnCount
        {
            get { return colDim; }
        }
        /// <summary>
        /// Gets the number of rows contained in the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        public int RowCount
        {
            get { return rowDim; }
        }
        /// <summary>
        /// Gets the number of elements in the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        public int Count
        {
            get { return (int)count; }
        }
        /// <summary>
        /// Gets a 64-bit integer that represents the number of elements in the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        public long LongCount
        {
            get
            {
                return count;
            }
        }
        /// <summary>
        /// Gets the order the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/> stores elements in memory.
        /// </summary>
        public MatrixDataOrder DataOrder
        {
            get
            {
                return order;
            }
        }
        /// <summary>
        /// Gets a collection of <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> containing the elements that are different from the default value.
        /// </summary>
        public MatrixEntryCollection<T> NonDefaultEntries
        {
            get
            {
                if (nonDefaultEntries == null)
                {
                    nonDefaultEntries = new NonDefaultEntriesCollection(this, order);
                }
                return nonDefaultEntries;
            }
        }
        /// <summary>
        /// Returns if the current instance of the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/> represents a dense matrix. That is that more than the half of the elements in the matrix are different from the default value.
        /// </summary>
        public bool IsDense
        {
            get
            {
                long num = LongCount / 2L;
                return (dict.Count >= num);
            }
        }
        /// <summary>
        /// Gets the value this matrix assumes as default value.
        /// </summary>
        public T DefaultValue
        {
            get
            {
                return def;
            }
        }

        /// <summary>
        /// Gets a view of the row at the specified index in the current instance of <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        /// <param name="rowIndex">The zero-based index of the row to view.</param>
        /// <returns>Returns a view of the row at the specified index in the matrix.</returns>
        public IMatrixVectorView<T> GetRowViewAt(int rowIndex)
        {
            if ((rowIndex < 0) || (rowIndex >= rowDim))
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
            }
            return new VectorView(this, rowIndex, true);
        }
        /// <summary>
        /// Gets a view of the column at the specified index in the current instance of <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        /// <param name="columnIndex">The zero-based index of the column to view.</param>
        /// <returns>Returns a view of the column at the specified index in the matrix.</returns>
        public IMatrixVectorView<T> GetColumnViewAt(int columnIndex)
        {
            if ((columnIndex < 0) || (columnIndex >= colDim))
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_ColIndex);
            }
            return new VectorView(this, columnIndex, false);
        }
        /// <summary>
        /// Determines whether a value is in the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        /// <param name="value">The value to locate in the matrix</param>
        /// <returns>True if the value is contained in the matrix; otherwise, false.</returns>
        public bool Contains(T value)
        {
            if (IsDefault(value))
            {
                return (dict.Count != LongCount);
            }
            return dict.ContainsValue(value);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        /// <returns>A <see cref="T:Academy.Collections.Generic.SparseMatrix`1.Enumerator"/> for the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, FirstIndex, rowDim, colDim, (order == MatrixDataOrder.Row), dict);
        }
        /// <summary>
        /// Gets the matrix entry for the specified value.
        /// </summary>
        /// <param name="value">The value of the entry.</param>
        /// <param name="entry">When this method returns, the entry for the specified value, if the value is found; otherwise the first entry for a matrix.</param>
        /// <returns>true if the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/> contains the specified value; otherwise, false.</returns>
        public bool TryGetEntry(T value, out MatrixEntry<T> entry)
        {
            if (IsDefault(value))
            {
                if (dict.Count < LongCount)
                {
                    entry = this.First((x) => IsDefault(x.Value));
                    return true;
                }
                entry = default(MatrixEntry<T>);
                return false;
            }
            if (value == null)
            {
                foreach (var item in dict)
                {
                    if (item.Value == null)
                    {
                        entry = new MatrixEntry<T>(item.Key.row, item.Key.col, item.Value);
                        return true;
                    }
                }
            }
            else
            {
                foreach (var item in dict)
                {
                    if (EqualityComparer<T>.Default.Equals(item.Value, value))
                    {
                        entry = new MatrixEntry<T>(item.Key.row, item.Key.col, item.Value);
                        return true;
                    }
                }
            }
            entry = default(MatrixEntry<T>);
            return false;
        }
        /// <summary>
        /// Resizes the matrix to the specified new dimension. If new dimension provokes a smaller matrix, out-bounds values will be ignored. 
        /// Otherwise, if new dimension provokes a bigger matrix, new elements will have its type default value.
        /// </summary>
        /// <param name="newRowCount">The new number of rows.</param>
        /// <param name="newColumnCount">The new number of columns.</param>
        public void Resize(int newRowCount, int newColumnCount)
        {
            if (newRowCount < 0)
            {
                newRowCount = 0;
            }
            if (newColumnCount < 0)
            {
                newColumnCount = 0;
            }
            if ((newRowCount != rowDim) && (newColumnCount != colDim))
            {
                List<Index> keysCollection = new List<Index>(dict.Count);
                foreach (var item in dict)
                {
                    if ((item.Key.row >= newRowCount) || (item.Key.col >= newColumnCount))
                    {
                        keysCollection.Add(item.Key);
                    }
                }
                foreach (var key in keysCollection)
                {
                    dict.Remove(key);
                }
            }
            this.rowDim = newRowCount;
            this.colDim = newColumnCount;
            this.CalculateCount();
            this.version++;
        }

        private bool IsDefault(T value)
        {
            return ((value == null) && (def == null)) || (comparer.Equals(value, def));
        }
        private void DirectlySet(int rowIndex, int colIndex, T value)
        {
            Index index = new Index { row = rowIndex, col = colIndex };
            if (IsDefault(value))
            {
                dict.Remove(index);
            }
            else
            {
                dict[index] = value;
            }
        }
        private T DirectlyGet(int rowIndex, int colIndex)
        {
            Index index = new Index { row = rowIndex, col = colIndex };
            if (!dict.TryGetValue(index, out T value))
            {
                value = def;
            }
            return value;
        }
        private void CalculateCount()
        {
            this.count = (long)rowDim * (long)colDim;
        }
        private SortedSet<Index> GetIndicesSet()
        {
            SortedSet<Index> set = new SortedSet<Index>(new RowFirstIndexComparer());
            foreach (MatrixEntry<T> entry in this.NonDefaultEntries)
            {
                Index index = new Index { row = entry.RowIndex, col = entry.ColumnIndex };
                set.Add(index);
            }
            return set;
        }

        private static IComparer<Index> GetIndexComparer(MatrixDataOrder order)
        {
            IComparer<Index> comparer;
            if (order == MatrixDataOrder.Row)
            {
                comparer = new RowFirstIndexComparer();
            }
            else
            {
                comparer = new ColumnFirstIndexComparer();
            }
            return comparer;
        }
        private static IEnumerable<KeyValuePair<Index, T>> DictionaryRange(SparseMatrix<T> matrix, bool isRow, int index)
        {
            Func<KeyValuePair<Index, T>, bool> inRange;
            if (isRow)
            {
                inRange = (x => (x.Key.row == index));
            }
            else
            {
                inRange = (x => (x.Key.col == index));
            }
            return matrix.dict.SkipWhile(x => !inRange(x)).TakeWhile(inRange);
        }
        private static IEnumerable<KeyValuePair<Index, T>> InvertedDictionaryRange(SparseMatrix<T> matrix, bool isRow, int index)
        {
            Func<KeyValuePair<Index, T>, bool> inRange;
            if (isRow)
            {
                inRange = (x => (x.Key.row == index));
            }
            else
            {
                inRange = (x => (x.Key.col == index));
            }
            return matrix.dict.Where(inRange);
        }

        #region Inner Types and Enumerator

        /// <summary>
        /// Enumerates the elements of a <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>.
        /// </summary>
        public class Enumerator : IEnumerator<MatrixEntry<T>>, IEnumerator<T>
        {
            private SparseMatrix<T> matrix;
            private Index startIndex;
            private readonly int rowCount;
            private readonly int colCount;
            private readonly bool isRowOrder;
            private IEnumerator<KeyValuePair<Index, T>> enumerator;
            private int i;
            private int j;
            private KeyValuePair<Index, T> next;
            private MatrixEntry<T> current;
            private IndexEqualityComparer comparer;
            private readonly int version;

            internal Enumerator(SparseMatrix<T> matrix, Index start, int rowCount, int colCount, bool isRowOrder, IEnumerable<KeyValuePair<Index, T>> e)
            {
                this.matrix = matrix;
                this.version = matrix.version;
                this.startIndex = start;
                this.rowCount = rowCount;
                this.colCount = colCount;
                this.isRowOrder = isRowOrder;
                this.enumerator = e.GetEnumerator();
                this.Initialize();
            }
            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public T Current
            {
                get { return current.Value; }
            }
            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="T:Academy.Collections.Generic.SparseMatrix`1"/>
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (version != matrix.version)
                {
                    Thrower.EnumeratorCorrupted();
                }
                if (MoveToNextIndex())
                {
                    Index index = new Index { row = i, col = j };
                    T value = default(T);
                    if (comparer.Equals(index, next.Key))
                    {
                        value = next.Value;
                        SetNext();
                    }
                    current = new MatrixEntry<T>(index.row, index.col, value);
                    return true;
                }
                i = matrix.RowCount + 1;
                j = matrix.ColumnCount + 1;
                return false;
            }

            void IDisposable.Dispose()
            {

            }
            object IEnumerator.Current
            {
                get
                {
                    if ((i == startIndex.row - 1) || (j == rowCount + startIndex.row - 1))
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                    }
                    if ((i == matrix.RowCount + 1) || (j == matrix.ColumnCount + 1))
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                    }
                    return Current;
                }
            }
            void IEnumerator.Reset()
            {
                this.enumerator.Reset();
                this.Initialize();
            }
            MatrixEntry<T> IEnumerator<MatrixEntry<T>>.Current
            {
                get { return current; }
            }
            private void Initialize()
            {
                if (isRowOrder)
                {
                    this.i = startIndex.row - 1;
                    this.j = (colCount + startIndex.col - 1);
                }
                else
                {
                    this.i = (rowCount + startIndex.row - 1);
                    this.j = startIndex.col - 1;
                }
                this.comparer = new IndexEqualityComparer();
                this.current = new MatrixEntry<T>(0, 0, default(T));
                this.SetNext();
            }
            private void SetNext()
            {
                try
                {
                    if (enumerator.MoveNext())
                    {
                        next = enumerator.Current;
                    }
                    else
                    {
                        next = new KeyValuePair<Index, T>(new Index { col = matrix.colDim + 1, row = matrix.rowDim + 1 }, default(T));
                    }
                }
                catch (InvalidOperationException)
                {
                    Thrower.EnumeratorCorrupted();
                }
            }
            private bool MoveToNextIndex()
            {

                if (isRowOrder)
                {
                    j = (j + 1) % (startIndex.col + colCount);
                    if (j == 0)
                    {
                        i++;
                        j += startIndex.col;
                    }
                    return (i < (startIndex.row + rowCount));
                }
                i = (i + 1) % (startIndex.row + rowCount);
                if (i == 0)
                {
                    j++;
                    i += startIndex.row;
                }
                return (j < matrix.ColumnCount);
            }
        }

        internal class VectorView : IMatrixVectorView<T>, IReadOnlyList<T>, ICollection
        {
            internal SparseMatrix<T> matrix;
            internal int containedIndex;
            internal bool isRow;

            public int Index
            {
                get
                {
                    return containedIndex;
                }
            }
            public T this[int index]
            {
                get
                {
                    CheckIndexConsistence();
                    CheckIndex(index);
                    if (isRow)
                    {
                        return matrix[this.containedIndex, index];
                    }
                    return matrix[index, this.containedIndex];
                }
                set
                {
                    CheckIndexConsistence();
                    CheckIndex(index);
                    if (isRow)
                    {
                        matrix[this.containedIndex, index] = value;
                    }
                    matrix[index, this.containedIndex] = value;
                }
            }
            public int Count
            {
                get
                {
                    if (isRow)
                    {
                        return matrix.colDim;
                    }
                    return matrix.rowDim;
                }
            }
            public bool UsesMatrixIndices
            {
                get
                {
                    return true;
                }
                set
                {

                }
            }
            public IMatrix<T> Matrix
            {
                get { return matrix; }
            }

            internal VectorView(SparseMatrix<T> matrix, int index, bool row)
            {
                this.matrix = matrix;
                this.containedIndex = index;
                this.isRow = row;
            }

            public void FillWith(IEnumerable<T> collection)
            {
                if (collection == null)
                {
                    Thrower.ArgumentNullException(ArgumentType.collection);
                }
                int num = 0;
                if (isRow)
                {
                    foreach (var item in collection.Take(matrix.colDim))
                    {
                        matrix.DirectlySet(containedIndex, num++, item);
                    }
                }
                else
                {
                    foreach (var item in collection.Take(matrix.rowDim))
                    {
                        matrix.DirectlySet(num++, containedIndex, item);
                    }
                }
            }
            public Enumerator GetEnumerator()
            {
                Index start;
                int rowCount;
                int colCount;
                IEnumerable<KeyValuePair<Index, T>> e;
                bool flag = (matrix.order == MatrixDataOrder.Row);
                if (isRow)
                {
                    rowCount = 1;
                    colCount = matrix.colDim;
                    start = new Index { row = containedIndex, col = 0 };
                    e = (flag) ? DictionaryRange(this.matrix, isRow, containedIndex) : InvertedDictionaryRange(this.matrix, isRow, containedIndex);
                }
                else
                {
                    colCount = 1;
                    rowCount = matrix.rowDim;
                    start = new Index { row = 0, col = containedIndex };
                    e = (flag) ? InvertedDictionaryRange(this.matrix, isRow, containedIndex) : DictionaryRange(this.matrix, isRow, containedIndex);
                }
                return new Enumerator(matrix, start, rowCount, colCount, flag, e);
            }

            private void CheckIndexConsistence()
            {
                if (isRow)
                {
                    if (matrix.RowCount <= containedIndex)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_RowIndex);
                    }
                }
                else
                {
                    if (matrix.ColumnCount <= containedIndex)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_ColumnIndex);
                    }
                }
            }
            private void CheckIndex(int index)
            {
                if (isRow)
                {
                    if ((index < 0) && (index >= matrix.RowCount))
                    {
                        Thrower.ArgumentOutOfRangeException(ArgumentType.index, Resources.ArgumentOutOfRange_Index);
                    }
                }
            }

            #region Explicit
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
                    return this;
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerator<MatrixEntry<T>> IEnumerable<MatrixEntry<T>>.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }

        internal class NonDefaultEntriesCollection : MatrixEntryCollection<T>
        {
            internal SparseMatrix<T> matrix;
            public NonDefaultEntriesCollection(SparseMatrix<T> matrix, MatrixDataOrder order)
                : base(order)
            {
                this.matrix = matrix;
            }

            public override bool ContainsEntry(int rowIndex, int columnIndex)
            {
                if ((rowIndex < 0 || rowIndex >= matrix.rowDim) || (columnIndex < 0 || columnIndex >= matrix.colDim))
                {
                    return false;
                }
                return matrix.dict.ContainsKey(new Index { row = rowIndex, col = columnIndex });
            }
            public override int Count
            {
                get
                {
                    return (matrix as SparseMatrix<T>).dict.Count;
                }
            }
            public override MatrixDataOrder Order
            {
                get
                {
                    return order;
                }
                protected set
                {
                    order = value;
                }
            }
            public override IEnumerator<MatrixEntry<T>> GetEnumerator()
            {
                return (matrix.dict.Select<KeyValuePair<Index, T>, MatrixEntry<T>>(Selector)).GetEnumerator();
            }

            private static MatrixEntry<T> Selector(KeyValuePair<Index, T> pair)
            {
                return new MatrixEntry<T>(pair.Key.row, pair.Key.col, pair.Value);
            }
        }

        #endregion

        #region Explicit Implementations
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }
        void ICollection.CopyTo(Array array, int index)
        {
            CollectionHelper.CopyTo<T>(array, index, Count, this);
        }
        object ICollection.SyncRoot
        {
            get { return ((ICollection)dict).SyncRoot; }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator<MatrixEntry<T>> IEnumerable<MatrixEntry<T>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Nested Types
        internal class Index
        {
            internal int row;
            internal int col;
        }
        internal class RowFirstIndexComparer : IComparer<Index>
        {
            public RowFirstIndexComparer()
            {

            }

            public int Compare(Index x, Index y)
            {
                int rest = x.row - y.row;
                if (rest == 0)
                {
                    return x.col - y.col;
                }
                return rest;
            }
        }
        internal class ColumnFirstIndexComparer : IComparer<Index>
        {
            public ColumnFirstIndexComparer()
            {

            }

            public int Compare(Index x, Index y)
            {
                int rest = x.col - y.col;
                if (rest == 0)
                {
                    return x.row - y.row;
                }
                return rest;
            }
        }
        internal class IndexEqualityComparer : IEqualityComparer<Index>
        {
            public IndexEqualityComparer()
            {

            }

            public bool Equals(Index x, Index y)
            {
                return (x.row == y.row) && (x.col == y.col);
            }

            public int GetHashCode(Index obj)
            {
                return 0;
            }
        }
        internal class ValueEqualityComparer : IEqualityComparer<T>
        {
            private static readonly IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

            public bool Equals(T x, T y)
            {
                try
                {
                    return comparer.Equals(x, y);
                }
                catch
                {
                    return false;
                }
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }
        #endregion        
    }
}
