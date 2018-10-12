using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StandardCollections
{
    internal interface IIndexedSet<T> : ISet<T>
    {
        int IndexOf(T item);
    }
    internal interface ICommonSortedSet<T> : IEnumerable<T>
    {
        int Count { get; }
        T MinValue { get; }
        T MaxValue { get; }
        IComparer<T> Comparer { get; }
        ICommonSortedSet<T> GetViewBetween(T min, T max);
        bool Contains(T value);
    }

    internal class IndexedSetHelper<T>
    {
        private readonly IIndexedSet<T> set;
        private int overlapCount;
        private int exceptCount;
        private readonly int count;

        public IndexedSetHelper(IIndexedSet<T> set, int count)
        {
            this.set = set;
            this.count = count;
        }

        private void CalculateMatches(IEnumerable<T> other)
        {
            int num = 0;
            int num2 = 0;
            if (this.count > 0)
            {
                BitArray flags = new BitArray(count);
                foreach (T item in other)
                {
                    int index = this.set.IndexOf(item);
                    if (index >= 0)
                    {
                        if (!flags[index])
                        {
                            flags[index] = true;
                            num++;
                        }
                    }
                    else
                    {
                        num2++;
                    }
                }
            }
            else if (other.Any())
            {
                num2++;
            }
            this.overlapCount = num;
            this.exceptCount = num2;
        }
        private void CalculateExcept(IEnumerable<T> other)
        {
            int num = 0;
            int num2 = 0;
            if (count > 0)
            {
                BitArray flags = new BitArray(count);
                using (IEnumerator<T> enumerator = other.GetEnumerator())
                {
                    while ((enumerator.MoveNext()) && (num2 == 0))
                    {
                        T item = enumerator.Current;
                        int index = this.set.IndexOf(item);
                        if (index >= 0)
                        {
                            if (!flags[index])
                            {
                                flags[index] = true;
                                num++;
                            }
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
            }
            else if (other.Any())
            {
                num2++;
            }
            this.overlapCount = num;
            this.exceptCount = num2;
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            this.CalculateMatches(other);
            return (this.overlapCount == this.set.Count) && (this.exceptCount >= 0);
        }
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            this.CalculateExcept(other);
            return (this.overlapCount < this.set.Count) && (this.exceptCount == 0);
        }
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            this.CalculateMatches(other);
            return (this.overlapCount == this.set.Count) && (this.exceptCount > 0);
        }
        public bool SetEquals(IEnumerable<T> other)
        {
            this.CalculateExcept(other);
            return (this.overlapCount == this.set.Count) && (this.exceptCount == 0);
        }
    }
    internal class SetHelper<T>
    {
        private static ValuePair CalculateMatches(IIndexedSet<T> set, IEnumerable<T> other, int count)
        {
            int num = 0;
            int num2 = 0;
            if (count > 0)
            {
                BitArray flags = new BitArray(count);
                foreach (T item in other)
                {
                    int index = set.IndexOf(item);
                    if (index >= 0)
                    {
                        if (!flags[index])
                        {
                            flags[index] = true;
                            num++;
                        }
                    }
                    else
                    {
                        num2++;
                    }
                }
            }
            else if (other.Any())
            {
                num2++;
            }
            return new ValuePair
            {
                overlapCount = num,
                exceptCount = num2,
            };
        }
        private static ValuePair CalculateExcept(IIndexedSet<T> set, IEnumerable<T> other, int count)
        {
            int num = 0;
            int num2 = 0;
            if (count > 0)
            {
                BitArray flags = new BitArray(count);
                using (IEnumerator<T> enumerator = other.GetEnumerator())
                {
                    while ((enumerator.MoveNext()) && (num2 == 0))
                    {
                        T item = enumerator.Current;
                        int index = set.IndexOf(item);
                        if (index >= 0)
                        {
                            if (!flags[index])
                            {
                                flags[index] = true;
                                num++;
                            }
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
            }
            else if (other.Any())
            {
                num2++;
            }
            return new ValuePair
            {
                overlapCount = num,
                exceptCount = num2,
            };
        }

        public static bool IsSubsetOf(IIndexedSet<T> set, IEnumerable<T> other)
        {
            return IsSubsetOf(set, other, set.Count);
        }
        public static bool IsSubsetOf(IIndexedSet<T> set, IEnumerable<T> other, int count)
        {
            ValuePair pair = CalculateMatches(set, other, count);
            return (pair.overlapCount == set.Count) && (pair.exceptCount >= 0);
        }
        public static bool IsProperSupersetOf(IIndexedSet<T> set, IEnumerable<T> other)
        {
            return IsProperSupersetOf(set, other, set.Count);
        }
        public static bool IsProperSupersetOf(IIndexedSet<T> set, IEnumerable<T> other, int count)
        {
            ValuePair pair = CalculateExcept(set, other, count);
            return (pair.overlapCount < set.Count) && (pair.exceptCount == 0);
        }
        public static bool IsProperSubsetOf(IIndexedSet<T> set, IEnumerable<T> other)
        {
            return IsProperSubsetOf(set, other, set.Count);
        }
        public static bool IsProperSubsetOf(IIndexedSet<T> set, IEnumerable<T> other, int count)
        {
            ValuePair pair = CalculateMatches(set, other, count);
            return (pair.overlapCount == set.Count) && (pair.exceptCount > 0);
        }
        public static bool SetEquals(IIndexedSet<T> set, IEnumerable<T> other)
        {
            return SetEquals(set, other, set.Count);
        }
        public static bool SetEquals(IIndexedSet<T> set, IEnumerable<T> other, int count)
        {
            ValuePair pair = CalculateExcept(set, other, count);
            return (pair.overlapCount == set.Count) && (pair.exceptCount == 0);
        }
        public static List<T> GetSortedListSet(IEnumerable<T> collection, IComparer<T> comparer)
        {
            List<T> list = new List<T>(collection);
            list.Sort(comparer);
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (comparer.Compare(list[i], list[i + 1]) == 0)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
            return list;
        }
        public static ICommonSortedSet<T> GetSortedSetIfSameComparer(IEnumerable<T> other, IComparer<T> comparer)
        {
            if ((other is ICommonSortedSet<T> set) && comparer.Equals(set.Comparer))
            {
                return set;
            }
            if ((other is SortedSet<T> set2) && comparer.Equals(set2.Comparer))
            {
                return new SortedSetWrapper(set2);
            }
            return null;
        }
        public static ICommonSortedSet<T> GetSortedSet(IEnumerable<T> other)
        {
            if (other is ICommonSortedSet<T> set)
            {
                return set;
            }
            if (other is SortedSet<T> set2)
            {
                return new SortedSetWrapper(set2);
            }
            return null;
        }
        public static bool IsWellKnownSet(IEnumerable<T> other)
        {
            if ((other is IIndexedSet<T>) || (other is HashSet<T>) || (other is SortedSet<T>))
            {
                return true;
            }
            return false;
        }

        private struct ValuePair
        {
            internal int overlapCount;
            internal int exceptCount;
        }
        private class SortedSetWrapper : ICommonSortedSet<T>
        {
            private SortedSet<T> set;

            public SortedSetWrapper(SortedSet<T> set)
            {
                this.set = set;
            }
            public int Count
            {
                get
                {
                    return set.Count;
                }
            }
            public T MinValue
            {
                get { return set.Min; }
            }
            public T MaxValue
            {
                get { return set.Max; }
            }
            public IComparer<T> Comparer
            {
                get
                {
                    return set.Comparer;
                }
            }
            public bool Contains(T value)
            {
                return set.Contains(value);
            }
            public ICommonSortedSet<T> GetViewBetween(T min, T max)
            {
                return new SortedSetWrapper(set.GetViewBetween(min, max));
            }
            public IEnumerator<T> GetEnumerator()
            {
                return set.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
