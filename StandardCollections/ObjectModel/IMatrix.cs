using System;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections.ObjectModel
{
    /// <summary>
    /// Represents a collection of objects that can be individually accessed by row and column indices.
    /// </summary>
    /// <typeparam name="T">The type of elements in the matrix.</typeparam>
    public interface IMatrix<T> : IEnumerable<MatrixEntry<T>>
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:Academy.Collections.Generic.IMatrix`1"/>.
        /// </summary>
        int Count
        {
            get;
        }
        /// <summary>
        /// Gets or sets the element at the specified indices.
        /// </summary>
        /// <param name="rowIndex">The zero-based row index of the element to get or set.</param>
        /// <param name="columnIndex">The zero-based column index of the element to get or set.</param>
        T this[int rowIndex, int columnIndex]
        {
            get;
            set;
        }
        /// <summary>
        /// Gets the number of columns in the <see cref="T:Academy.Collections.Generic.IMatrix`1"/>.
        /// </summary>
        int ColumnCount { get; }
        /// <summary>
        /// Gets the number of rows in the <see cref="T:Academy.Collections.Generic.IMatrix`1"/>.
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Determinates whether the matrix contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the matrix.</param>
        /// <returns>true if item is found in the matrix; otherwise,false.</returns>
        bool Contains(T item);
    }
}
