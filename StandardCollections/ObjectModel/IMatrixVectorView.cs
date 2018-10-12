using System;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections.ObjectModel
{
    /// <summary>
    /// Represents a personalized view of a vector in an <see cref="T:Academy.Collections.Generic.IMatrix`1"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the <see cref="T:Academy.Collections.Generic.IMatrix`1"/> associated.</typeparam>
    public interface IMatrixVectorView<T> : IEnumerable<MatrixEntry<T>>
    {
        /// <summary>
        /// Gets the index of the viewing vector in the <see cref="T:Academy.Collections.Generic.IMatrix`1"/> associated.
        /// </summary>
        int Index { get; }
        /// <summary>
        /// Gets the matrix that contains the viewing vector.
        /// </summary>
        IMatrix<T> Matrix { get; }
        /// <summary>
        /// Gets or sets the elements in the viewing matrix at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        T this[int index] { get; set; }
        /// <summary>
        /// Gets the number of elements managed by the <see cref="T:Academy.Collections.Generic.IMatrixVectorView`1"/>.
        /// </summary>
        int Count
        {
            get;
        }
    }
}
