using System;
using System.Collections.Generic;
using System.Linq;

namespace RegexRetrieval
{
    internal static class Util
    {

        public static IEnumerable<T> Index<T>(this IList<T> list, IEnumerable<int> indexes)
        {
            foreach (var i in indexes)
                yield return list[i];
        }

        #region Array

        public static T[] Set<T>(this T[] array, Func<int, T> setter)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = setter(i);
            return array;
        }

        public static T[] SetParallel<T>(this T[] array, Func<int, T> setter)
            => SetParallel(array, -1, setter);
        public static T[] SetParallel<T>(this T[] array, int degreeOfParallelism, Func<int, T> setter)
        {
            var parallel = Enumerable.Range(0, array.Length).AsParallel();
            if (degreeOfParallelism > 0) parallel = parallel.WithDegreeOfParallelism(degreeOfParallelism);

            parallel.ForAll(i =>
            {
                array[i] = setter(i);
            });

            return array;
        }

        #endregion

        public static string Reverse(this string s)
        {
            if (s.Length <= 1) return s;
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static IEnumerable<int> Iterate(int toExclusive)
            => Iterate(0, toExclusive);
        public static IEnumerable<int> Iterate(int fromInclusive, int toExclusive)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
                yield return i;
        }

    }
}
