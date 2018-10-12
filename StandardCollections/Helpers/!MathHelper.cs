using System;
using System.Collections.Generic;
using System.Text;

namespace StandardCollections
{
    internal class MathHelper
    {
        public const int MaxInt32Prime = 2146435069;
        public const int MinPrimeSize = 3;

        private static readonly int[] primesMap = new int[]
            {
                3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 83, 97, 113, 131, 149, 167,
                191, 223, 257, 293, 331, 383, 463, 563, 673, 787, 907, 1039, 1181,
                1361, 1657, 2011, 2437, 2953, 3557, 4177, 4987, 6011, 7069, 8501,
                10313, 12487, 15131, 18313, 21911, 26293, 32479, 39301, 47563, 57557,
                68659, 84011, 100823, 120997, 144611, 174257, 209123, 250949, 301141,
                352327, 414697, 515917, 624311, 749383, 899263, 1079123, 1294957, 1524763,
                1760203, 2134697, 2591891, 3156031, 3721537, 4620809, 5568287, 6681947, 8018347
            };

        public static int GetNextPrime(int value)
        {
            for (int i = 0; i < primesMap.Length; i++)
            {
                if (primesMap[i] >= value)
                {
                    return primesMap[i];
                }
            }
            for (int j = value | 1; j < int.MaxValue; j++)
            {
                if (IsPrime(j))
                {
                    return j;
                }
            }
            return value;
        }
        public static int GetGrowedPrime(int value)
        {
            long num = value * 2L;
            if ((num > MaxInt32Prime) && (value < MaxInt32Prime))
            {
                return MaxInt32Prime;
            }
            return GetNextPrime((int)num);
        }
        public static bool IsPrime(int value)
        {
            if (value % 2 == 0)
            {
                return (value == 2);
            }
            int limit = (int)Math.Sqrt(value);
            for (int i = 3; i <= limit; i += 2)
            {
                if ((value % i) == 0)
                {
                    return false;
                }
            }
            return true;
        }
        public static int Log2N(int n)
        {
            int value = 0;
            while (n > 0)
            {
                value++;
                n = n >> 1;
            }
            return value;
        }
    }
}
