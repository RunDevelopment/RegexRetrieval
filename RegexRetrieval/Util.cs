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
        public static T[] Set<T>(this T[] array, Func<int, T, T> setter)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = setter(i, array[i]);
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

        public static IList<char> GetAllCharacters(string[] words)
        {
            bool[] contains = new bool[0x1_0000];
            foreach (var w in words)
                foreach (var c in w)
                    contains[c] = true;

            var chars = new List<char>();
            for (var i = 0; i < 0x1_0000; i++)
                if (contains[i])
                    chars.Add((char) i);
            return chars;
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
