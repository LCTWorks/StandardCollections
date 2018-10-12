using System;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections.ObjectModel
{
    /// <summary>
    /// Represents a collection of objects maintained in sorted order that can perform fast search operations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sorted collection.</typeparam>
    public interface ISortedCollection<T> : IEnumerable<T>
    {
        /// <summary>
        /// The default comparer used to compare stored elements.
        /// </summary>
        IComparer<T> Comparer { get; }
        /// <summary>
        /// Returns the smallest value contained in the collection.
        /// </summary>
        T MinValue { get; }
        /// <summary>
        /// Returns the largest value contained in the collection.
        /// </summary>
        T MaxValue { get; }
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:Academy.Collections.Generic.ISortedCollection`1"/>.
        /// </summary>
        int Count
        {
            get;
        }
        /// <summary>
        /// Determinates whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if item is found in the collection; otherwise,false.</returns>
        bool Contains(T item);
    }
}
