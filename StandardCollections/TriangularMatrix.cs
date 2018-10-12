using StandardCollections.ObjectModel;
using StandardCollections.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace StandardCollections
{  
    /// <summary>
    /// Provides a collection of objects that can be accessed by a triangular index system.
    /// </summary>
    /// <typeparam name="T">The type of elements in the matrix.</typeparam>
    [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(MatrixProxy<>))]
    public class TriangularMatrix<T> : IMatrix<T>, IReadOnlyCollection<MatrixEntry<T>>, ICollection
    {
        #region Private and Internal Fields
        private const int DefaultDimension = 4;
        private const int MaxDimension = 65519;
        private const int MaxArraySize = 2146402440;
        private static readonly Type type = typeof(TriangularMatrix<T>);
        private static readonly Type genType = typeof(T);

        [NonSerialized]
        private object _syncRoot;
        private int _version;
        private readonly TriangularBase _mode;
        private readonly MatrixDataOrder _order;
        private T[] _elements;
        private int _dim;
        private bool _allowDefault;
        private T _def;
        private long _count;
        [NonSerialized]
        private IMatrixBehavior<T> _behavior;
        [NonSerialized]
        private EntriesCollection _nonDefaultEntries;
        [NonSerialized]
        private EntriesCollection _entries;

        private static int UpperTriangularSerie(long dim, long i)
        {

            double num = (i * i) / 2d;
            double result = (i * dim) - (num) - (i / 2d) + dim;
            return (int)result;
        }

        /// <summary>
        /// Gets the amount of items of any triangular matrix with specified dimension.
        /// </summary>
        /// <param name="dim">The dimension of the triangular matrix.</param>
        private static int TriangularCount(int dim)
        {
            long num = (long)dim;
            long num2 = num * num;
            return (int)(((num2) + num) / 2L);
        }
        private static int TriangularArrayDimension(int collectionCount)
        {
            long num = 8L * (long)collectionCount + 1L;
            double dimension = (Math.Sqrt(num) - 1) / 2;
            return (int)Math.Ceiling(dimension);
        }
        private static int GetScalableIndex(int row, int col, int dim)
        {
            int num = UpperTriangularSerie(dim, row - 1);
            return num + (col - row);
        }
        private static int GetStaticIndex(int row, int col)
        {
            return TriangularCount(row) + col;
        }

        private static IMatrixBehavior<T> GetBehavior(TriangularMatrix<T> matrix, MatrixDataOrder order, TriangularBase mode)
        {
            if (order == MatrixDataOrder.Column)
            {
                if (mode == TriangularBase.Upper)
                {
                    return new UpperColumnFirstBehavior(matrix);
                }
                return new LowerColumnFirstBehavior(matrix);
            }
            if (mode == TriangularBase.Upper)
            {
                return new UpperRowFirstBehavior(matrix);
            }
            return new LowerRowFirstBehavior(matrix);
        }
        private T DirectlyGet(int rowIndex, int columnIndex)
        {
            if (_behavior.AreValidIndices(rowIndex, columnIndex))
            {
                int index = _behavior.GetIndexOnArray(rowIndex, columnIndex);
                return _elements[index];
            }
            if (!_allowDefault)
            {
                Thrower.ArgumentException(ArgumentType.empty, Resources.ArgumentOutOfRange_TriangularIndex);
            }
            return _def;
        }
        private void DirectlySet(int rowIndex, int columnIndex, T value)
        {
            if (!_behavior.AreValidIndices(rowIndex, columnIndex))
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.index, Resources.ArgumentOutOfRange_TriangularIndex);
            }
            int index = _behavior.GetIndexOnArray(rowIndex, columnIndex);
            _elements[index] = value;
            _version++;
        }
        private bool InRange(int index)
        {
            return (index >= 0) && (index < _dim);
        }
        private void CalculateCount()
        {
            _count = (_allowDefault) ? (_dim * _dim) : _elements.Length;
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> class that has the specified dimension.
        /// </summary>
        /// <param name="dimension">The dimension of the matrix. Used to calculate the number of elements the new matrix can store.</param>
        public TriangularMatrix(int dimension)
            : this(dimension, TriangularBase.Default, MatrixDataOrder.Default, true, default(T))
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> class that has the specified dimension, mode and order.
        /// </summary>
        /// <param name="dimension">The dimension of the matrix. Used to calculate the number of elements the new matrix can store.</param>
        /// <param name="mode">The mode the new matrix will access stored elements by index.</param>
        /// <param name="dataOrder">The order the new matrix will store elements in memory. 'Default' and 'Merged' values are used as 'Row' value.</param>
        public TriangularMatrix(int dimension, TriangularBase mode, MatrixDataOrder dataOrder)
            : this(dimension, mode, dataOrder, true, default(T))
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> class that has the specified dimension, triangular mode, data order and default value settings.
        /// </summary>
        /// <param name="dimension">The dimension of the matrix. Used to calculate the number of elements the new matrix can store.</param>
        /// <param name="mode">The mode the new matrix will access stored elements by index.</param>
        /// <param name="dataOrder">The order the new matrix will store elements in memory. 'Default' and 'Merged' values are used as 'Row' value.</param>
        /// <param name="allowDefaultValues">Specifies whether the matrix bounds will allow defaults cells or not.</param>
        /// <param name="defaultValue">The value the matrix assumes as the default value. Specified value will be ignored if the <paramref name="allowDefaultValues"/> parameter has value <value>true</value>.</param>
        public TriangularMatrix(int dimension, TriangularBase mode, MatrixDataOrder dataOrder, bool allowDefaultValues, T defaultValue)
        {
            if (dimension < 0)
            {
                dimension = 0;
            }
            if (dimension > MaxDimension)
            {
                dimension = MaxDimension;
            }
            if (allowDefaultValues)
            {
                this._allowDefault = true;
                this._def = defaultValue;
            }
            this._behavior = GetBehavior(this, dataOrder, mode);
            this._def = defaultValue;
            this._dim = dimension;
            int count = TriangularCount(_dim);
            this._mode = (mode == TriangularBase.Default) ? TriangularBase.Lower : mode;
            this._order = (dataOrder == MatrixDataOrder.Default) ? MatrixDataOrder.Row : dataOrder;
            this._elements = new T[count];
            this.CalculateCount();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> class that contains
        /// elements copied from the specified collection and has the precise dimension to store the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new matrix.</param>
        /// <param name="defaultValue">The value the matrix assumes as the default value.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection"/>is null.</exception>
        public TriangularMatrix(IEnumerable<T> collection, T defaultValue)
            : this(collection, TriangularBase.Default, MatrixDataOrder.Default, true, defaultValue)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> class that contains
        /// elements copied from the specified collection and has the precise dimension to store the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new matrix.</param>
        /// <param name="mode">The mode the new matrix will access stored elements by index.</param>
        /// <param name="dataOrder">The order the new matrix will store elements in memory. 'Default' and 'Merged' values are used as 'Row' value.</param>
        /// <param name="allowDefaultValues">Specifies whether the matrix bounds will allow defaults cells or not.</param>
        /// <param name="defaultValue">The value the matrix assumes as the default value. Specified value will be ignored if the <paramref name="allowDefaultValues"/> parameter has value <value>true</value>.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection"/>is null.</exception>
        public TriangularMatrix(IEnumerable<T> collection, TriangularBase mode, MatrixDataOrder dataOrder, bool allowDefaultValues, T defaultValue)
        {
            if (collection == null)
            {
                Thrower.ArgumentNullException(ArgumentType.collection);
            }
            bool limited = false;
            int collectionCount = CollectionHelper.GetCountFromEnumerable(collection);
            if (collectionCount > MaxArraySize)
            {
                collectionCount = MaxArraySize;
                limited = true;
            }
            this._dim = TriangularArrayDimension(collectionCount);
            int num = TriangularCount(_dim);
            this._elements = new T[num];
            if (allowDefaultValues)
            {
                this._allowDefault = true;
                this._def = defaultValue;
            }
            this._mode = mode == TriangularBase.Default ? TriangularBase.Lower : mode;
            this._order = dataOrder == MatrixDataOrder.Default ? MatrixDataOrder.Row : dataOrder;
            this._behavior = GetBehavior(this, dataOrder, mode);
            int index = 0;
            if (!limited)
            {
                foreach (var item in collection)
                {
                    _elements[index++] = item;
                }
            }
            else
            {
                IEnumerator<T> enumerator = collection.GetEnumerator();
                while ((enumerator.MoveNext()) && (index < MaxArraySize))
                {
                    _elements[index++] = enumerator.Current;
                }
            }
            this.CalculateCount();
        }
        #endregion

        /// <summary>
        /// Gets or sets the element at the specified indices.
        /// </summary>
        /// <param name="rowIndex">The zero-based row index of the element to get or set.</param>
        /// <param name="columnIndex">The zero-based column index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">rowIndex and/or columnIndex are outside the bounds of the valid matrix values. Which variates according to the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1.Mode"/>property.</exception>
        public T this[int rowIndex, int columnIndex]
        {
            get
            {
                if (!InRange(rowIndex))
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
                }
                if (!InRange(columnIndex))
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.columnIndex, Resources.ArgumentOutOfRange_ColIndex);
                }
                return DirectlyGet(rowIndex, columnIndex);
            }
            set
            {
                if (rowIndex < 0 || rowIndex >= _dim)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
                }
                if (columnIndex < 0 || columnIndex >= _dim)
                {
                    Thrower.ArgumentOutOfRangeException(ArgumentType.columnIndex, Resources.ArgumentOutOfRange_ColIndex);
                }
                DirectlySet(rowIndex, columnIndex, value);
            }
        }
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return (int)_count;
            }
        }
        /// <summary>
        /// Gets a 64-bit integer that represents the number of elements in the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        public long LongCount
        {
            get
            {
                return _count;
            }
        }
        /// <summary>
        /// Gets or sets the dimension of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>. 
        /// While setting if new dimension is lesser than the old one, out-bound items will be ignored, otherwise, 
        /// if new dimension is bigger than the old, new items will have its default value.
        /// </summary>
        public int Dimension
        {
            get
            {
                return _dim;
            }
        }
        /// <summary>
        /// Gets the structure of the elements in the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        public TriangularBase Base
        {
            get
            {
                return _mode;
            }
        }
        /// <summary>
        /// Gets the order the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> stores elements in memory.
        /// </summary>
        public MatrixDataOrder DataOrder
        {
            get
            {
                return _order;
            }
        }
        /// <summary>
        /// Gets the value this matrix assumes as default value.
        /// </summary>
        public T DefaultValue
        {
            get
            {
                return _def;
            }
        }
        /// <summary>
        /// Gets whether the matrix bounds will allow default cells or not.
        /// </summary>
        public bool AllowDefaultValues
        {
            get
            {
                return _allowDefault;
            }
            set
            {
                if (this._allowDefault != value)
                {
                    this._allowDefault = value;
                    this.CalculateCount();
                    this._version++;
                }
            }
        }
        /// <summary>
        /// Gets all stored elements as a collection of <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/>.
        /// </summary>
        public MatrixEntryCollection<T> Entries
        {
            get
            {
                if (_entries == null)
                {
                    _entries = new EntriesCollection(this, _allowDefault, DataOrder);
                }
                return _entries;
            }
        }
        /// <summary>
        /// Gets a collection of <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> containing only the elements
        /// that are under or over the main diagonal as defined by the <see cref="Base"/> property.
        /// </summary>
        public MatrixEntryCollection<T> InBoundEntries
        {
            get
            {
                if (_nonDefaultEntries == null)
                {
                    _nonDefaultEntries = new EntriesCollection(this, false, DataOrder);
                }
                return _nonDefaultEntries;
            }
        }

        /// <summary>
        /// Determines whether a value is in the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        /// <param name="value">The value to locate in the matrix</param>
        /// <returns>True if the value is contained in the matrix; otherwise, false.</returns>
        public bool Contains(T value)
        {
            return Contains(value, null);
        }
        /// <summary>
        /// Determines whether a value is in the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> using the specified comparer.
        /// </summary>
        /// <param name="value">The value to locate in the matrix</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> used to compare the specified value and the stored elements.</param>
        /// <returns>True if the value is contained in the matrix; otherwise, false.</returns>
        public bool Contains(T value, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }
            if (((value == null) && (_def == null)) && (_allowDefault))
            {
                return true;
            }
            if (comparer.Equals(value, _def) && (_allowDefault))
            {
                return true;
            }
            if (value == null)
            {
                for (int i = 0; i < _elements.Length; i++)
                {
                    if (_elements[i] == null)
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _elements.Length; i++)
                {
                    if (comparer.Equals(_elements[i], value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        ///  Returns an enumerator that iterates through the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        /// <returns>An <see cref="T:Academy.Collections.Generic.TriangularMatrix`1.Enumerator"/> for the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, _allowDefault, false);
        }
        /// <summary>
        /// Gets a view of the row at the specified index in the current instance of <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        /// <param name="rowIndex">The zero-based index of the row to get the view.</param>
        /// <returns>Returns a view of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> at the specified row index.</returns>
        public IMatrixVectorView<T> GetRowViewAt(int rowIndex)
        {
            return GetRowViewAt(rowIndex, _allowDefault);
        }
        /// <summary>
        /// Gets a view of the row at the specified index in the current instance of <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        /// <param name="rowIndex">The zero-based index of the row to get the view.</param>
        /// <param name="allowOutBoundsEntries">Specifies whether the view groups also the entries that are out of the triangluar bounds as defined by the <see cref="Base"/> property .</param>
        /// <exception cref="T:System.ArgumentOutOfRangeExecption"><paramref name="rowIndex"/> is lesser than zero or bigger or equals than the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1.Dimension"/> property.</exception>
        /// <returns>Returns a view of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> at the specified row index.</returns>
        public IMatrixVectorView<T> GetRowViewAt(int rowIndex, bool allowOutBoundsEntries)
        {
            if (!InRange(rowIndex))
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
            }
            return new RowView(this, rowIndex, allowOutBoundsEntries);
        }
        /// <summary>
        /// Gets a view of the column at the specified index in the current instance of <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        /// <param name="columnIndex">The zero-based index of the column to get the view.</param>
        /// <returns>Returns a view of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> at the specified column index.</returns>
        public IMatrixVectorView<T> GetColumnViewAt(int columnIndex)
        {
            return GetColumnViewAt(columnIndex, _allowDefault);
        }
        /// <summary>
        /// Gets a view of the column at the specified index in the current instance of <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        /// <param name="columnIndex">The zero-based index of the column to get the view.</param>
        /// <param name="allowOutBoundsEntries">Specifies whether the view groups also the entries that are out of the triangluar bounds as defined by the <see cref="Base"/> property .</param>
        /// <exception cref="T:System.ArgumentOutOfRangeExecption"><paramref name="columnIndex"/> is lesser than zero or bigger or equals than the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1.Dimension"/> property.</exception>
        /// <returns>Returns a view of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> at the specified column index.</returns>
        public IMatrixVectorView<T> GetColumnViewAt(int columnIndex, bool allowOutBoundsEntries)
        {
            if (!InRange(columnIndex))
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_ColIndex);
            }
            return new ColumnView(this, columnIndex, allowOutBoundsEntries);
        }
        /// <summary>
        /// Resizes the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/> to a new specified dimension. 
        /// If dimension decreases, outside elements will be ignored, otherwise new elements will have the type default value.
        /// </summary>
        /// <param name="newDimension">The new dimension to resize the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.</param>
        public void Resize(int newDimension)
        {
            if (newDimension < 0)
            {
                newDimension = 0;
            }
            if (newDimension != _dim)
            {
                if (newDimension > MaxDimension)
                {
                    newDimension = MaxDimension;
                }
                T[] local = _behavior.GetResizedArray(newDimension);
                this._elements = local;
                this._dim = newDimension;
                this.CalculateCount();
                this._version++;
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this._behavior = GetBehavior(this, this._order, this._mode);
        }

        #region Enumerator
        /// <summary>
        /// Enumerates the elements of a <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>.
        /// </summary>
        [Serializable]
        public class Enumerator : IEnumerator<MatrixEntry<T>>
        {
            private TriangularMatrix<T> matrix;
            private IIndexEnumerator enumerator;
            private T current;
            private readonly int _version;
            private int _index;
            private readonly bool _allowDefault;
            private readonly bool _inverted;
            private readonly int _count;

            internal Enumerator(TriangularMatrix<T> matrix, bool allowDefault, bool inverted)
            {
                this.matrix = matrix;
                this._version = matrix._version;
                this._allowDefault = allowDefault;
                this._inverted = inverted;
                this.SetEnumerator();
                this.current = default(T);
                this._index = 0;
                this._count = (allowDefault) ? (this.matrix._dim * this.matrix._dim) : this.matrix._elements.Length;
            }
            private void SetEnumerator()
            {
                if (_allowDefault)
                {
                    bool isRow = this.matrix.DataOrder != MatrixDataOrder.Row;
                    bool flag = (this._inverted) ? !isRow : isRow;
                    enumerator = new SquaredIndexEnumerator(this.matrix.Dimension, flag);
                }
                else
                {
                    enumerator = (this._inverted) ? (this.matrix._behavior.GetInvertedOrderEnumerator()) : (this.matrix._behavior.GetEnumerator());
                }
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
            /// Advances the enumerator to the next element of the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1"/>
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (_version != matrix._version)
                {
                    Thrower.EnumeratorCorrupted();
                }
                if (_index < _count)
                {
                    if (_allowDefault || _inverted)
                    {
                        enumerator.MoveNext();
                        current = matrix.DirectlyGet(enumerator.Row, enumerator.Column);
                    }
                    else
                    {
                        current = matrix._elements[_index];
                        enumerator.MoveNext();
                    }
                    _index++;
                    return true;
                }
                _index = matrix.Count + 1;
                current = default(T);
                return false;
            }
            /// <summary>
            /// Releases all resources used by the <see cref="T:Academy.Collections.Generic.TriangularMatrix`1.Enumerator"/>.
            /// </summary>
            public void Dispose()
            {

            }
            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                    }
                    if (_index == matrix.Count + 1)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                    }
                    return Current;
                }
            }
            void IEnumerator.Reset()
            {
                if (_version != matrix._version)
                {
                    Thrower.EnumeratorCorrupted();
                }
                enumerator.Reset();
                current = default(T);
                _index = 0;
            }
            MatrixEntry<T> IEnumerator<MatrixEntry<T>>.Current
            {
                get
                {
                    if (_index == 0)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                    }
                    if (_index == matrix.Count + 1)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                    }
                    return new MatrixEntry<T>(enumerator.Row, enumerator.Column, current);
                }
            }
        }
        #endregion

        #region Others
        [Serializable]
        internal class EntriesCollection : MatrixEntryCollection<T>
        {
            private readonly bool _allowDefaults;
            private TriangularMatrix<T> matrix;
            public EntriesCollection(TriangularMatrix<T> matrix, bool allowDefaults, MatrixDataOrder order)
                : base(order)
            {
                this._allowDefaults = allowDefaults;
                this.matrix = matrix;
            }

            public override int Count
            {
                get
                {
                    if (_allowDefaults)
                    {
                        return matrix._dim * matrix._dim;
                    }
                    return matrix._elements.Length;
                }
            }
            public override MatrixDataOrder Order
            {
                get
                {
                    return this.order;
                }
                protected set
                {
                    this.order = value;
                }
            }
            public override IEnumerator<MatrixEntry<T>> GetEnumerator()
            {
                TriangularMatrix<T> matrix = (this.matrix as TriangularMatrix<T>);
                return new Enumerator(matrix, this._allowDefaults, (this.Order != matrix.DataOrder));
            }
            public override bool ContainsEntry(int rowIndex, int columnIndex)
            {
                if ((matrix.InRange(rowIndex)) && (matrix.InRange(columnIndex)))
                {
                    if ((matrix._behavior.AreValidIndices(rowIndex, columnIndex)) || _allowDefaults)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        [Serializable]
        private class SquaredIndexEnumerator : IIndexEnumerator
        {
            private int row;
            private int col;
            private readonly bool _isRowOrder;
            private readonly int _dim;

            public int Row
            {
                get { return row; }
            }
            public int Column
            {
                get { return col; }
            }

            public SquaredIndexEnumerator(int dim, bool isRowOrder)
            {
                this.row = -1;
                this.col = -1;
                this._dim = dim;
                this._isRowOrder = isRowOrder;
            }

            public void Reset()
            {
                this.row = -1;
                this.col = -1;
            }
            public void MoveNext()
            {
                if (_isRowOrder)
                {
                    this.row = (this.row + 1) % this._dim;
                    if (this.row == 0)
                    {
                        col++;
                    }
                }
                else
                {
                    this.col = (this.col + 1) % _dim;
                    if (this.col == 0)
                    {
                        this.row++;
                    }
                }
            }
        }
        #endregion

        #region Explicit Implementations
        void ICollection.CopyTo(Array array, int index)
        {
            CollectionHelper.CopyTo<MatrixEntry<T>>(array, index, Count, this);
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
        IEnumerator<MatrixEntry<T>> IEnumerable<MatrixEntry<T>>.GetEnumerator()
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
        int IMatrix<T>.ColumnCount
        {
            get { return Dimension; }
        }
        int IMatrix<T>.RowCount
        {
            get { return Dimension; }
        }
        #endregion

        #region Behaviors

        internal class LowerRowFirstBehavior : IMatrixBehavior<T>
        {
            private readonly TriangularMatrix<T> _matrix;

            public LowerRowFirstBehavior(TriangularMatrix<T> matrix)
            {
                _matrix = matrix;
            }

            public bool AreValidIndices(int rowIndex, int columnIndex)
            {
                return (rowIndex >= columnIndex);
            }

            public int GetIndexOnArray(int rowIndex, int columnIndex)
            {
                return GetStaticIndex(rowIndex, columnIndex);
            }
            public T[] GetResizedArray(int newDimension)
            {
                int count = TriangularMatrix<T>.TriangularCount(newDimension);
                var local = new T[count];
                int num = Math.Min(_matrix._elements.Length, count);
                Array.Copy(_matrix._elements, local, num);
                return local;
            }
            public bool UpdateIndicesOnEnumeration(ref int row, ref int col)
            {
                if (col < row)
                {
                    col++;
                    return true;
                }
                if (row < _matrix._dim - 1)
                {
                    row++;
                    return true;
                }
                return false;
            }
            public IIndexEnumerator GetEnumerator()
            {
                return new Enumerator(_matrix);
            }
            public IIndexEnumerator GetInvertedOrderEnumerator()
            {
                return new LowerColumnFirstBehavior.Enumerator(_matrix);
            }

            [Serializable]
            internal class Enumerator : IndicesEnumerator
            {
                public Enumerator(TriangularMatrix<T> matrix)
                    : base(matrix)
                {

                }
                public override void Reset()
                {
                    this.col = 0;
                    this.row = -1;
                }
                public override void MoveNext()
                {
                    col++;
                    if (col > row)
                    {
                        row++;
                        col = 0;
                    }
                }
            }
        }

        internal class LowerColumnFirstBehavior : IMatrixBehavior<T>
        {
            private readonly TriangularMatrix<T> _matrix;

            public LowerColumnFirstBehavior(TriangularMatrix<T> matrix)
            {
                _matrix = matrix;
            }

            public bool AreValidIndices(int rowIndex, int columnIndex)
            {
                return (rowIndex >= columnIndex);
            }
            public int GetIndexOnArray(int rowIndex, int columnIndex)
            {
                return GetScalableIndex(columnIndex, rowIndex, _matrix._dim);
            }
            public T[] GetResizedArray(int newDimension)
            {
                int count = TriangularCount(newDimension);
                T[] local = new T[count];
                int min = Math.Min(_matrix._dim, newDimension);
                for (int i = 0; i < min; i++)
                {
                    int end = i + 1;
                    for (int j = 0; j < end; j++)
                    {
                        int index = GetScalableIndex(j, i, _matrix._dim);
                        int index2 = GetScalableIndex(j, i, newDimension);
                        local[index2] = _matrix._elements[index];
                    }
                }
                return local;
            }
            public IIndexEnumerator GetEnumerator()
            {
                return new Enumerator(_matrix);
            }
            public IIndexEnumerator GetInvertedOrderEnumerator()
            {
                return new LowerRowFirstBehavior.Enumerator(_matrix);
            }

            [Serializable]
            internal class Enumerator : IndicesEnumerator
            {
                public Enumerator(TriangularMatrix<T> matrix)
                    : base(matrix)
                {

                }
                public override void Reset()
                {
                    this.col = 0;
                    this.row = -1;
                }
                public override void MoveNext()
                {
                    row++;
                    if (row >= dim)
                    {
                        col++;
                        row = col;
                    }
                }
            }
        }

        internal class UpperRowFirstBehavior : IMatrixBehavior<T>
        {
            private readonly TriangularMatrix<T> _matrix;

            public UpperRowFirstBehavior(TriangularMatrix<T> matrix)
            {
                _matrix = matrix;
            }
            public bool AreValidIndices(int rowIndex, int columnIndex)
            {
                return (rowIndex <= columnIndex);
            }
            public int GetIndexOnArray(int rowIndex, int columnIndex)
            {
                return GetScalableIndex(rowIndex, columnIndex, _matrix._dim);
            }
            public T[] GetResizedArray(int newDimension)
            {
                var count = TriangularCount(newDimension);
                var local = new T[count];
                var min = Math.Min(_matrix._dim, newDimension);
                for (var i = 0; i < min; i++)
                {
                    for (var j = i; j < min; j++)
                    {
                        int index = GetScalableIndex(i, j, _matrix._dim);
                        int index2 = GetScalableIndex(i, j, newDimension);
                        local[index2] = _matrix._elements[index];
                    }
                }
                return local;
            }
            public IIndexEnumerator GetEnumerator()
            {
                return new Enumerator(_matrix);
            }
            public IIndexEnumerator GetInvertedOrderEnumerator()
            {
                return new UpperColumnFirstBehavior.Enumerator(_matrix);
            }

            [Serializable]
            internal class Enumerator : IndicesEnumerator
            {
                public Enumerator(TriangularMatrix<T> matrix)
                    : base(matrix)
                {

                }
                public override void Reset()
                {
                    this.col = -1;
                    this.row = 0;
                }
                public override void MoveNext()
                {
                    col++;
                    if (col >= dim)
                    {
                        row++;
                        col = row;
                    }
                }
            }
        }

        internal class UpperColumnFirstBehavior : IMatrixBehavior<T>
        {
            private readonly TriangularMatrix<T> _matrix;

            public UpperColumnFirstBehavior(TriangularMatrix<T> matrix)
            {
                _matrix = matrix;
            }

            public bool AreValidIndices(int rowIndex, int columnIndex)
            {
                return (rowIndex <= columnIndex);
            }
            public int GetIndexOnArray(int rowIndex, int columnIndex)
            {
                return GetStaticIndex(columnIndex, rowIndex);
            }
            public T[] GetResizedArray(int newDimension)
            {
                int count = TriangularMatrix<T>.TriangularCount(newDimension);
                T[] local = new T[count];
                int num = Math.Min(_matrix._elements.Length, count);
                Array.Copy(_matrix._elements, local, num);
                return local;
            }
            public IIndexEnumerator GetEnumerator()
            {
                return new Enumerator(_matrix);
            }
            public IIndexEnumerator GetInvertedOrderEnumerator()
            {
                return new UpperRowFirstBehavior.Enumerator(_matrix);
            }
            [Serializable]
            internal class Enumerator : IndicesEnumerator
            {
                public Enumerator(TriangularMatrix<T> matrix)
                    : base(matrix)
                {

                }
                public override void Reset()
                {
                    this.col = -1;
                    this.row = -1;
                }
                public override void MoveNext()
                {
                    row++;
                    if (row > col)
                    {
                        col++;
                        row = 0;
                    }
                }
            }
        }

        [Serializable]
        internal abstract class IndicesEnumerator : IIndexEnumerator
        {
            internal int row;
            internal int col;
            internal int dim;

            public IndicesEnumerator(TriangularMatrix<T> matrix)
            {
                this.dim = matrix._dim;
                this.Reset();
            }
            public virtual void Reset()
            {
                this.row = 0;
                this.col = 0;
            }

            public abstract void MoveNext();

            public int Row
            {
                get
                {
                    return row;
                }
            }
            public int Column
            {
                get
                {
                    return col;
                }
            }
        }
        #endregion

        #region Vector Views

        [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(IEnumerable<>))]
        internal class RowView : IMatrixVectorView<T>, IReadOnlyList<T>, ICollection
        {
            private readonly int _rowIndex;
            private TriangularMatrix<T> _matrix;
            private bool _allowDefaults;
            private object _syncRoot;
            private int _version;

            public RowView(TriangularMatrix<T> matrix, int rowIndex, bool allowDefault)
            {
                _rowIndex = rowIndex;
                _matrix = matrix;
                _allowDefaults = allowDefault;
                _version = matrix._version;
            }

            public T this[int index]
            {
                get
                {
                    CheckVersion();
                    if (!_matrix.InRange(index))
                    {
                        Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
                    }
                    if (!_allowDefaults)
                    {
                        if (!_matrix._behavior.AreValidIndices(_rowIndex, index))
                        {
                            Thrower.ArgumentOutOfRangeException(ArgumentType.index, Resources.ArgumentOutOfRange_RowIndex);
                        }
                    }
                    return _matrix.DirectlyGet(_rowIndex, index);
                }
                set
                {
                    CheckVersion();
                    if (!_matrix.InRange(index))
                    {
                        Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
                    }
                    _matrix.DirectlySet(_rowIndex, index, value);
                }
            }
            public int Count
            {
                get
                {
                    CheckVersion();
                    if (_allowDefaults)
                    {
                        return _matrix._dim;
                    }
                    return (_matrix.Base == TriangularBase.Lower) ? (_rowIndex + 1) : (_matrix._dim - _rowIndex);
                }
            }

            public Enumerator GetEnumerator()
            {
                CheckVersion();
                return new Enumerator(_matrix, _rowIndex, _allowDefaults);
            }

            private void CheckVersion()
            {
                if (this._version != _matrix._version)
                {
                    if (_matrix._dim <= _rowIndex)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_RowIndex);
                    }
                    this._allowDefaults = _matrix._allowDefault;
                    this._version = _matrix._version;
                }
            }

            public class Enumerator : IEnumerator<MatrixEntry<T>>, IEnumerator<T>
            {
                private TriangularMatrix<T> _matrix;
                private readonly int _version;
                private int _startIndex;
                private int _index;
                private int _count;
                private readonly int _rowIndex;
                private MatrixEntry<T> _current;
                private readonly bool _allowDefaults;
                private readonly bool _isRowOrder;
                private readonly bool _isLowerMode;

                public Enumerator(TriangularMatrix<T> matrix, int rowIndex, bool allowDefaults)
                {
                    this._matrix = matrix;
                    this._version = matrix._version;
                    this._rowIndex = rowIndex;
                    this._allowDefaults = allowDefaults;
                    this._count = 0;
                    this._current = default(MatrixEntry<T>);
                    this._isRowOrder = matrix.DataOrder == MatrixDataOrder.Row;
                    this._isLowerMode = matrix.Base == TriangularBase.Lower;
                    int num = (_isLowerMode) ? 0 : _rowIndex;
                    this._count = (allowDefaults) ? 0 : num;
                    this._startIndex = matrix._behavior.GetIndexOnArray(_rowIndex, num);
                    this._index = _startIndex;
                }

                public MatrixEntry<T> Current
                {
                    get { return _current; }
                }
                public void Dispose()
                {

                }
                object IEnumerator.Current
                {
                    get
                    {
                        if (_count == 0)
                        {
                            Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                        }
                        if (_count == _matrix._dim + 1)
                        {
                            Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                        }
                        return Current;
                    }
                }
                public bool MoveNext()
                {
                    if (this._version != _matrix._version)
                    {
                        Thrower.EnumeratorCorrupted();
                    }
                    if ((_count < _matrix._dim))
                    {
                        if (InNonDefaultRange())
                        {
                            _current = new MatrixEntry<T>(_rowIndex, _index, _matrix._elements[_index]);
                            _count++;
                            MoveToNextIndex();
                            return true;
                        }
                        if (_allowDefaults)
                        {
                            _current = new MatrixEntry<T>(_rowIndex, _index, _matrix._def);
                            _count++;
                            return true;
                        }
                    }
                    _current = default(MatrixEntry<T>);
                    _count = _matrix._dim + 1;
                    return false;
                }
                public void Reset()
                {
                    if (this._version != _matrix._version)
                    {
                        Thrower.EnumeratorCorrupted();
                    }
                    this._startIndex = _matrix._behavior.GetIndexOnArray(_rowIndex, 0);
                    this._count = 0;
                    this._index = _startIndex;
                    this._current = default(MatrixEntry<T>);
                }

                private void MoveToNextIndex()
                {
                    if (_isRowOrder)
                    {
                        _index++;
                    }
                    else
                    {
                        _index = _matrix._behavior.GetIndexOnArray(_rowIndex, _count);
                    }
                }
                private bool InNonDefaultRange()
                {
                    return (_isLowerMode) ? (_count < _rowIndex + 1) : (_count >= _rowIndex);
                }

                T IEnumerator<T>.Current
                {
                    get { return Current.Value; }
                }
            }

            #region Explicit
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerator<MatrixEntry<T>> IEnumerable<MatrixEntry<T>>.GetEnumerator()
            {
                return GetEnumerator();
            }
            int IMatrixVectorView<T>.Index
            {
                get { return _rowIndex; }
            }
            IMatrix<T> IMatrixVectorView<T>.Matrix
            {
                get { return _matrix; }
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
            #endregion
        }
        internal class ColumnView : IMatrixVectorView<T>, IReadOnlyList<T>, ICollection
        {
            private readonly int _colIndex;
            private TriangularMatrix<T> _matrix;
            private bool _allowDefaults;
            private object _syncRoot;
            private int _version;

            public ColumnView(TriangularMatrix<T> matrix, int colIndex, bool allowDefault)
            {
                _colIndex = colIndex;
                _matrix = matrix;
                _allowDefaults = allowDefault;
                _version = matrix._version;
            }

            public T this[int index]
            {
                get
                {
                    CheckVersion();
                    if (!_matrix.InRange(index))
                    {
                        Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_ColIndex);
                    }
                    if (!_allowDefaults)
                    {
                        if (!_matrix._behavior.AreValidIndices(index, _colIndex))
                        {
                            Thrower.ArgumentOutOfRangeException(ArgumentType.index, Resources.ArgumentOutOfRange_ColIndex);
                        }
                    }
                    return _matrix.DirectlyGet(index, _colIndex);
                }
                set
                {
                    CheckVersion();
                    if (!_matrix.InRange(index))
                    {
                        Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_ColIndex);
                    }
                    _matrix.DirectlySet(index, _colIndex, value);
                }
            }
            public int Count
            {
                get
                {
                    CheckVersion();
                    if (_allowDefaults)
                    {
                        return _matrix._dim;
                    }
                    return (_matrix._dim - _colIndex);
                }
            }

            public Enumerator GetEnumerator()
            {
                CheckVersion();
                return new Enumerator(_matrix, _colIndex, _allowDefaults);
            }

            private void CheckVersion()
            {
                if (this._version != _matrix._version)
                {
                    if (_matrix._dim <= _colIndex)
                    {
                        Thrower.InvalidOperationException(Resources.InvalidOperation_ColumnIndex);
                    }
                    this._allowDefaults = _matrix._allowDefault;
                    this._version = _matrix._version;
                }
            }

            public class Enumerator : IEnumerator<MatrixEntry<T>>, IEnumerator<T>
            {
                private TriangularMatrix<T> _matrix;
                private readonly int _version;
                private int _startIndex;
                private int _index;
                private int _count;
                private readonly int _colIndex;
                private MatrixEntry<T> _current;
                private readonly bool _allowDefaults;
                private readonly bool isRowOrder;
                private readonly bool isLowerMode;

                public Enumerator(TriangularMatrix<T> matrix, int colIndex, bool allowDefaults)
                {
                    this._matrix = matrix;
                    this._version = matrix._version;
                    this._colIndex = colIndex;
                    this._allowDefaults = allowDefaults;
                    this._current = default(MatrixEntry<T>);
                    this.isRowOrder = matrix.DataOrder == MatrixDataOrder.Row;
                    this.isLowerMode = matrix.Base == TriangularBase.Lower;
                    this.SetStartIndex();
                    this._index = _startIndex;
                }

                public MatrixEntry<T> Current
                {
                    get { return _current; }
                }
                public void Dispose()
                {

                }
                object IEnumerator.Current
                {
                    get
                    {
                        if (_count == 0)
                        {
                            Thrower.InvalidOperationException(Resources.InvalidOperation_EnumNotStarted);
                        }
                        if (_count == _matrix._dim + 1)
                        {
                            Thrower.InvalidOperationException(Resources.InvalidOperation_EnumEnded);
                        }
                        return Current;
                    }
                }
                public bool MoveNext()
                {
                    if (this._version != _matrix._version)
                    {
                        Thrower.EnumeratorCorrupted();
                    }
                    if (_count < (_matrix._dim))
                    {
                        if (!InDefaultRange())
                        {
                            _current = new MatrixEntry<T>(_index, _colIndex, _matrix._elements[_index]);
                            _count++;
                            MoveToNextIndex();
                            return true;
                        }
                        if (_allowDefaults)
                        {
                            _current = new MatrixEntry<T>(_index, _colIndex, _matrix._def);
                            _count++;
                            return true;
                        }
                    }
                    _current = default(MatrixEntry<T>);
                    _count = _matrix._dim + 1;
                    return false;
                }
                public void Reset()
                {
                    if (this._version != _matrix._version)
                    {
                        Thrower.EnumeratorCorrupted();
                    }
                    this._startIndex = _matrix._behavior.GetIndexOnArray(_colIndex, 0);
                    this._count = 0;
                    this._index = _startIndex;
                    this._current = default(MatrixEntry<T>);
                }

                private void MoveToNextIndex()
                {
                    if (isRowOrder)
                    {
                        _index = _matrix._behavior.GetIndexOnArray(_count, _colIndex);
                    }
                    else
                    {
                        _index++;
                    }
                }
                private bool InDefaultRange()
                {
                    return (isLowerMode) ? (_count < _colIndex) : (_count > _colIndex);
                }
                private void SetStartIndex()
                {
                    int num = (isLowerMode) ? _colIndex : 0;
                    this._startIndex = _matrix._behavior.GetIndexOnArray(num, _colIndex);
                    this._count = (!_allowDefaults) ? num : 0;
                }

                T IEnumerator<T>.Current
                {
                    get { throw new NotImplementedException(); }
                }
            }

            #region Explicit
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }
            IEnumerator<MatrixEntry<T>> IEnumerable<MatrixEntry<T>>.GetEnumerator()
            {
                return GetEnumerator();
            }
            int IMatrixVectorView<T>.Index
            {
                get { return _colIndex; }
            }
            IMatrix<T> IMatrixVectorView<T>.Matrix
            {
                get { return _matrix; }
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
            #endregion
        }

        #endregion
    }
    /// <summary>
    /// Specifies the base in a triangular matrix.
    /// </summary>
    public enum TriangularBase
    {
        /// <summary>
        /// Defines a triangular matrix as Upper. That is all the elements bellow the main diagonal has a default value and are considered default cells.
        /// </summary>
        Upper,
        /// <summary>
        /// Defines a triangular matrix as Lower. That is all the elements over the main diagonal has a default value and are considered default cells
        /// </summary>
        Lower,
        /// <summary>
        /// Specifies the default base for a triangular matrix. Which is <see cref="Lower"/>.
        /// </summary>
        Default
    }
}
