using System;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections
{
    /// <summary>
    /// Specifies a value to stablish the way a matrix stores data in memory and/or iterates through stored elements.
    /// </summary>
    public enum MatrixDataOrder
    {
        /// <summary>
        /// Specifies the default order for desired operation.
        /// </summary>
        Default,
        /// <summary>
        /// Specifies the matrix stores (or iterates through) row elements first.
        /// </summary>
        Row,
        /// <summary>
        /// Specifies the matrix stores (or iterates through) columns elements first.
        /// </summary>
        Column,
        /// <summary>
        /// Specifies the matrix stores (or iterates through) elements following no sorted order. 
        /// </summary>
        Merged
    }
}
