using System;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections
{
    internal interface IMatrixBehavior<T>
    {
        bool AreValidIndices(int rowIndex, int columnIndex);
        int GetIndexOnArray(int rowIndex, int columnIndex);
        T[] GetResizedArray(int newDimension);
        IIndexEnumerator GetEnumerator();
        IIndexEnumerator GetInvertedOrderEnumerator();
    }

    internal interface IIndexEnumerator
    {
        void Reset();
        void MoveNext();
        int Row { get; }
        int Column { get; }
    }
}
