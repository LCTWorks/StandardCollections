using StandardCollections.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections
{
    internal static class CollectionHelper
    {
        public static bool IsWellKnownCollection<T>(IEnumerable<T> collection, out int count)
        {
            if (collection is ICollection<T> ic)
            {
                count = ic.Count;
                return true;
            }
            if (collection is IReadOnlyCollection<T> ic2)
            {
                count = ic2.Count;
                return true;
            }
            ICollection ic3 = collection as ICollection;
            if (ic3 != null)
            {
                count = ic3.Count;
                return true;
            }
            count = -1;
            return false;
        }
        public static int GetCountFromEnumerable<T>(IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection<T>)
            {
                return (enumerable as ICollection<T>).Count;
            }
            if (enumerable is ICollection)
            {
                return (enumerable as ICollection).Count;
            }
            if (enumerable is IReadOnlyCollection<T>)
            {
                return (enumerable as IReadOnlyCollection<T>).Count;
            }
            int count = 0;
            foreach (T item in enumerable)
            {
                count++;
            }
            return count;
        }
        public static void CopyToCheck<T>(T[] array, int index, int count)
        {
            if (array == null)
            {
                Thrower.ArgumentNullException(ArgumentType.array);
            }
            if (index < 0 || index > array.Length)
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.index, string.Format(Resources.ArgumentOutOfRange_Range_G, new object[] { 0, array.Length - 1 }));
            }
            if (array.Length - index < count)
            {
                Thrower.ArgumentException(ArgumentType.index, Resources.Argument_ArrayNotLongEnought);
            }
        }
        public static void CopyToCheck(Array array, int index, int count)
        {
            if (array == null)
            {
                Thrower.ArgumentNullException(ArgumentType.array);
            }
            if (array.Rank != 1)
            {
                Thrower.ArgumentException(ArgumentType.array, Resources.Argument_ArrayRank);
            }
            if (array.GetLowerBound(0) != 0)
            {
                Thrower.ArgumentException(ArgumentType.array, Resources.Argument_LowerBoundArrayNotZero);
            }
            if (index < 0 || index > array.Length)
            {
                Thrower.ArgumentOutOfRangeException(ArgumentType.index, string.Format(Resources.ArgumentOutOfRange_Range_G, new object[] { 0, array.Length - 1 }));
            }
            if ((array.Length - index) < count)
            {
                Thrower.ArgumentException(ArgumentType.empty, Resources.Argument_ArrayNotLongEnought);
            }
        }
        public static void CopyTo<T>(T[] array, int index, int count, IEnumerable<T> collection)
        {
            CopyToCheck(array, index, count);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext() && index < count)
            {
                array[index++] = enumerator.Current;
            }
        }
        public static void CopyTo<T>(Array array, int index, int count, IEnumerable collection)
        {
            CopyToCheck(array, index, count);
            T[] array2 = array as T[];
            if (collection is ICollection<T> c2 && array2 != null)
            {
                c2.CopyTo(array2, index);
            }
            else
            {
                object[] objs = array as object[];
                if (objs == null)
                {
                    Thrower.ArgumentException(ArgumentType.array, Resources.Argument_InvalidArrayType);
                }
                try
                {
                    var enumerator = collection.GetEnumerator();
                    while (enumerator.MoveNext() && index < count)
                    {
                        objs[index++] = enumerator.Current;
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    Thrower.ArgumentException(ArgumentType.array, Resources.Argument_InvalidArrayType);
                }
            }
        }
    }
}
