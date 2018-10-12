using StandardCollections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StandardCollections
{
    /// <summary>
    /// Represents a strongly typed matrix of objects that can be accessed by index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the matrix.</typeparam>
    [Serializable, DebuggerTypeProxy(typeof(MatrixProxy<>)), DebuggerDisplay("Count = {Count}")]
    public class Matrix<T> : IMatrix<T>
    {
        #region Private region
        private const int DefaultDimension = 4;
        private const int MaxDimension = 65519;
        private const int MaxArraySize = 2146402440;

        private T[] _elements;
        private int _rowDim;
        private int _colDim;
        private long _count;
        private MatrixDataOrder _dataOrder;
        private int _version;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.Matrix`1"/> class 
        /// with the specified dimensions.
        /// </summary>
        /// <param name="rowCount">The number of rows in the matrix.</param>
        /// <param name="columnCount">The number of columns in the matrix.</param>
        public Matrix(int rowCount, int columnCount)
            : this(rowCount, columnCount, MatrixDataOrder.Default)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Academy.Collections.Generic.Matrix`1"/> class 
        /// with the specified dimensions and order.
        /// </summary>
        /// <param name="rowCount">The number of rows in the matrix.</param>
        /// <param name="columnCount">The number of columns in the matrix.</param>
        /// <param name="dataOrder">The order the matrix will store elements in memory. 'Default' and 'Merged' values are used as 'Row' value.</param>
        public Matrix(int rowCount, int columnCount, MatrixDataOrder dataOrder)
        {
            if (rowCount < 0)
            {
                rowCount = 0;
            }
            if (columnCount < 0)
            {
                columnCount = 0;
            }
            if (dataOrder != MatrixDataOrder.Column)
            {
                dataOrder = MatrixDataOrder.Row;
            }
            int num = MaxArraySize / rowCount;
            if (rowCount > num)
            {
                rowCount = num;
            }
            this._rowDim = rowCount;
            this._colDim = columnCount;
            this._dataOrder = dataOrder;
            this._version = 0;
            this.CalculateCount();
            this._elements = new T[_count];
        }

        private int GetIndex(int rowIndex, int columnIndex)
        {
            return GetIndex(rowIndex, columnIndex, _rowDim, _colDim, _dataOrder);
        }
        private void CalculateCount()
        {
            this._count = (long)_rowDim * (long)_colDim;
        }
        private static int GetIndex(int rowIndex, int columnIndex, int rowCount, int columnCount, MatrixDataOrder order)
        {
            if (order == MatrixDataOrder.Column)
            {
                return (int)((long)(rowCount) * columnIndex + rowIndex);
            }
            return (int)((long)columnCount * rowIndex + columnIndex);
        }

        private void DirectlySet(int rowIndex, int columnIndex, T value)
        {
            int indexOnArray = GetIndex(rowIndex, columnIndex);
            _elements[indexOnArray] = value;
            this._version++;
        }
        private T DirectlyGet(int rowIndex, int columnIndex)
        {
            int indexOnArray = GetIndex(rowIndex, columnIndex);
            return _elements[indexOnArray];
        }

        /// <summary>
        /// Gets the order the <see cref="T:Academy.Collections.Generic.Matrix`1"/> stores elements in memory.
        /// </summary>
        public MatrixDataOrder DataOrder
        {
            get
            {
                return _dataOrder;
            }
        }
        /// <summary>
        /// Gets or sets the element at the specified indices.
        /// </summary>
        /// <param name="rowIndex">The zero-based row index of the element to get or set.</param>
        /// <param name="columnIndex">The zero-based column index of the element to get or set.</param>
        public T this[int rowIndex, int columnIndex]
        {
            get
            {
                if ((rowIndex < 0) || (rowIndex >= this.RowCount))
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                if ((columnIndex < 0) || (columnIndex >= this.ColumnCount))
                {
                    throw new ArgumentOutOfRangeException("columnIndex");
                }
                return DirectlyGet(rowIndex, columnIndex);
            }
            set
            {
                if ((rowIndex < 0) || (rowIndex >= this.RowCount))
                {
                    throw new ArgumentOutOfRangeException("rowIndex");
                }
                if ((columnIndex < 0) || (columnIndex >= this.ColumnCount))
                {
                    throw new ArgumentOutOfRangeException("columnIndex");
                }
                DirectlySet(rowIndex, columnIndex, value);
            }
        }
        /// <summary>
        /// Gets the number of elements in the <see cref="T:Academy.Collections.Generic.Matrix`1"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return (int)_count;
            }
        }
        /// <summary>
        /// Gets a 64-bit integer that represents the number of elements in the <see cref="T:Academy.Collections.Generic.Matrix`1"/>.
        /// </summary>
        public long LongCount
        {
            get
            {
                return _count;
            }
        }
        /// <summary>
        /// Gets the number of columns contained in the <see cref="T:Academy.Collections.Generic.Matrix`1"/>.
        /// </summary>
        public int ColumnCount
        {
            get
            {
                return _colDim;
            }
        }
        /// <summary>
        /// Gets the number of rows contained in the <see cref="T:Academy.Collections.Generic.Matrix`1"/>.
        /// </summary>
        public int RowCount
        {
            get
            {
                return _rowDim;
            }
        }
        /// <summary>
        /// Determines whether a value is in the <see cref="T:Academy.Collections.Generic.Matrix`1"/>.
        /// </summary>
        /// <param name="value">The value to locate in the matrix</param>
        /// <returns>True if the value is contained in the matrix; otherwise, false.</returns>
        public bool Contains(T value)
        {
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
            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _elements.Length; i++)
            {
                if (comparer.Equals(_elements[i], value))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="T:Academy.Collections.Generic.Matrix`1"/>.
        /// </summary>
        /// <returns>An <see cref="T:Academy.Collections.Generic.Matrix`1.Enumerator"/> for the <see cref="T:Academy.Collections.Generic.Matrix`1"/>.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        /// <summary>
        /// Resizes the matrix to the specified new dimension. If new dimension provokes a smaller matrix, out-bounds values will be ignored. 
        /// Otherwise, if new dimension provokes a bigger matrix, new elements will have its type default value.
        /// </summary>
        /// <param name="newRowCount">The new number of rows.</param>
        /// <param name="newColumnCount">The new number of columns.</param>
        public void Resize(int newRowCount, int newColumnCount)
        {
            this.Resize(newRowCount, newColumnCount, DataOrder);
        }
        /// <summary>
        /// Resizes the matrix to the specified new dimension, specifying new dimensions and order. If new dimension provokes a smaller matrix, out-bounds values will be ignored. 
        /// Otherwise, if new dimension provokes a bigger matrix, new elements will have its type default value.
        /// </summary>
        /// <param name="newRowCount">The new number of rows.</param>
        /// <param name="newColumnCount">The new number of columns.</param>
        /// <param name="dataOrder">The order the matrix will use to store the elements in the operation.</param>
        public void Resize(int newRowCount, int newColumnCount, MatrixDataOrder dataOrder)
        {
            if (newRowCount < 0)
            {
                newRowCount = 0;
            }
            if (newColumnCount < 0)
            {
                newColumnCount = 0;
            }
            if (dataOrder == MatrixDataOrder.Default)
            {
                dataOrder = this.DataOrder;
            }
            if ((newRowCount != _rowDim) || (newColumnCount != _colDim) || (_dataOrder != dataOrder))
            {
                int newSize = newRowCount * newColumnCount;
                T[] local = new T[newSize];
                int rowMin = Math.Min(_rowDim, newRowCount);
                int colMin = Math.Min(_colDim, newColumnCount);
                for (int i = 0; i < rowMin; i++)
                {
                    for (int j = 0; j < colMin; j++)
                    {
                        int num = GetIndex(i, j);
                        int num2 = GetIndex(i, j, newRowCount, newColumnCount, dataOrder);
                        local[num2] = _elements[num];
                    }
                }
                this._elements = local;
                this._dataOrder = dataOrder;
                this._rowDim = newRowCount;
                this._colDim = newColumnCount;
                this.CalculateCount();
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

        /// <summary>
        /// Enumerates the elements of a <see cref="T:Academy.Collections.Generic.Matrix`1"/>.
        /// </summary>
        [Serializable]
        public class Enumerator : IEnumerator<MatrixEntry<T>>
        {
            private Matrix<T> matrix;
            private int currentRow;
            private int currentColumn;
            private MatrixEntry<T> current;
            private int count;

            internal Enumerator(Matrix<T> matrix)
            {
                this.matrix = matrix;
                this.count = 0;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public MatrixEntry<T> Current
            {
                get
                {
                    return current;
                }
            }
            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="T:Academy.Collections.Generic.Matrix`1"/>
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (count < matrix.Count)
                {
                    current = new MatrixEntry<T>(currentRow, currentColumn, matrix._elements[count]);
                    MoveToNextIndex();
                    return true;
                }
                current = default(MatrixEntry<T>);
                return false;
            }
            private void MoveToNextIndex()
            {
                count++;
                if (matrix._dataOrder == MatrixDataOrder.Column)
                {
                    currentRow = count / matrix._colDim;
                    currentColumn = count % matrix._colDim;
                }
                else
                {
                    currentRow = count % matrix._colDim;
                    currentColumn = count / matrix._colDim;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
            void IDisposable.Dispose()
            {

            }
            void IEnumerator.Reset()
            {
                this.count = 0;
                this.currentRow = 0;
                this.currentColumn = 0;
                this.current = default(MatrixEntry<T>);
            }
        }
    }
}
