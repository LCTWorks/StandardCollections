using StandardCollections.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StandardCollections
{
    /// <summary>
    /// Represents an object that relates a generic value to a row-column index system.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [DebuggerDisplay("{Text}")]
    public struct MatrixEntry<T> : IEquatable<MatrixEntry<T>>, IComparable<MatrixEntry<T>>
    {
        private readonly int _rowIndex;
        private readonly int _colIndex;
        private readonly T _value;

        /// <summary>
        /// Initializes a new instance of <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> structure with specified indices and value.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="columnIndex">The column index</param>
        /// <param name="value">The value associated to the specified indices.</param>
        public MatrixEntry(int rowIndex, int columnIndex, T value)
        {
            if (rowIndex < 0)
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.rowIndex, Resources.ArgumentOutOfRange_RowIndex);
            }
            if (columnIndex < 0)
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.columnIndex, Resources.ArgumentOutOfRange_ColIndex);
            }

            this._rowIndex = rowIndex;
            this._colIndex = columnIndex;
            this._value = value;
        }

        /// <summary>
        /// Gets the row index.
        /// </summary>
        public int RowIndex
        {
            get
            {
                return _rowIndex;
            }
        }
        /// <summary>
        /// Gets the column index.
        /// </summary>
        public int ColumnIndex
        {
            get
            {
                return _colIndex;
            }
        }
        /// <summary>
        /// Gets the value associated.
        /// </summary>
        public T Value
        {
            get
            {
                return _value;
            }
        }
        /// <summary>
        /// Returns a string representation of the <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> using the string representation of the value. 
        /// </summary>
        /// <returns>A string representation of the <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> which includes the string representation of the value.</returns>
        public override string ToString()
        {
            return Text;
        }
        /// <summary>
        /// Gets current entry hash code using the row index, the column index and the associated value's hash code.
        /// </summary>
        /// <returns>returns a hash code for the current entry.</returns>
        public override int GetHashCode()
        {
            return _rowIndex ^ _colIndex ^ _value.GetHashCode();
        }
        /// <summary>
        /// Indicates whether the current entry is equals to the specified entry.
        /// </summary>
        /// <param name="other">The other <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> object.</param>
        /// <returns>true if the current and the specified entries are equals; otherwise false.</returns>
        public bool Equals(MatrixEntry<T> other)
        {
            return (_rowIndex == other._rowIndex) && (_colIndex == other._colIndex);
        }
        /// <summary>
        /// Compares the current <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> with another entry by comparing the row index and the column index.
        /// </summary>
        /// <param name="other">A <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> to compare with this entry.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// If the value is less than zero this entry is less than the <paramref name="other"/> parameter. 
        /// If value is zero this entry and the <paramref name="other"/> parameter are equals. 
        /// And if the value is greater than zero this entry is bigger than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(MatrixEntry<T> other)
        {
            int num = this._rowIndex.CompareTo(other._rowIndex);
            int num2 = this._colIndex.CompareTo(other._colIndex);
            return (num | num2);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.Generic.IComparer`1"/> that can be used to 
        /// compare <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> instances based on the specified order.
        /// </summary>
        /// <param name="order">The order used to create the comparer.</param>
        /// <returns>A comparer used to compare <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> instances.</returns>
        public static IComparer<MatrixEntry<T>> CreateEntryComparer(MatrixDataOrder order)
        {
            if (order == MatrixDataOrder.Column)
            {
                return new ColumnFirstComparer();
            }
            if (order == MatrixDataOrder.Merged)
            {
                return new MergedComparer();
            }
            return new RowFirstComparer();
        }
        /// <summary>
        /// Returns an <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> that can be used for equality 
        /// testing of <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/>.
        /// </summary>
        /// <returns>A comparer used for equality testing in <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> instances.</returns>
        public static IEqualityComparer<MatrixEntry<T>> CreateEntryEqualityComparer()
        {
            return new EntryEqComparer(null);
        }
        /// <summary>
        /// Returns an <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> that can be used for equality 
        /// testing of <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> instances using the specifying 
        /// comparer for the value type.
        /// </summary>
        /// <param name="comparer">The comparer to use for creating the returned comparer.</param>
        /// <returns>A comparer used for equality testing in <see cref="T:Academy.Collections.Generic.MatrixEntry`1"/> instances.</returns>
        public static IEqualityComparer<MatrixEntry<T>> CreateEntryEqualityComparer(IEqualityComparer<T> comparer)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            return new EntryEqComparer(comparer);
        }

        #region Comparers
        private class RowFirstComparer : IComparer<MatrixEntry<T>>
        {
            public RowFirstComparer()
            {
            }
            public int Compare(MatrixEntry<T> x, MatrixEntry<T> y)
            {
                int num = x.RowIndex.CompareTo(y.RowIndex);
                if (num == 0)
                {
                    return x.ColumnIndex.CompareTo(y.ColumnIndex);
                }
                return num;
            }
        }
        private class ColumnFirstComparer : IComparer<MatrixEntry<T>>
        {
            public ColumnFirstComparer()
            {
            }
            public int Compare(MatrixEntry<T> x, MatrixEntry<T> y)
            {
                int num = x.ColumnIndex.CompareTo(y.ColumnIndex);
                if (num == 0)
                {
                    return x.RowIndex.CompareTo(y.RowIndex);
                }
                return num;
            }
        }
        private class MergedComparer : IComparer<MatrixEntry<T>>
        {
            public MergedComparer()
            {
            }
            public int Compare(MatrixEntry<T> x, MatrixEntry<T> y)
            {
                return 0;
            }
        }
        private class EntryEqComparer : IEqualityComparer<MatrixEntry<T>>
        {
            private IEqualityComparer<T> comparer;
            public EntryEqComparer(IEqualityComparer<T> comparer)
            {
                this.comparer = comparer;
            }

            public bool Equals(MatrixEntry<T> x, MatrixEntry<T> y)
            {
                if (x.Equals(y))
                {
                    if (comparer == null)
                    {
                        return true;
                    }
                    return comparer.Equals(x.Value, y.Value);
                }
                return false;
            }

            public int GetHashCode(MatrixEntry<T> obj)
            {
                return obj.GetHashCode();
            }
        }
        #endregion

        internal string Text
        {
            get
            {
                return string.Format("[{0},{1}] | {2}", _rowIndex, _colIndex, _value);
            }
        }
    }
}
