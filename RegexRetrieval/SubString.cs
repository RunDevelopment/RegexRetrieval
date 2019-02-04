using System;
using System.Collections;
using System.Collections.Generic;

namespace RegexRetrieval
{
    public readonly struct SubString : IReadOnlyList<char>, IEnumerable<char>
    {
        private readonly int index;
        private readonly int length;
        private readonly string str;

        public int Index => index;
        public int Length => length;
        int IReadOnlyCollection<char>.Count => Length;

        public char this[int index] => str[this.index + index];


        public SubString(string str) : this(str, 0) { }
        public SubString(string str, int index) : this(str, index, (str?.Length ?? 0) - index) { }
        public SubString(string str, int index, int length)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (index < 0 || index > str.Length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "index out of range");
            if (length < 0 || index + length > str.Length)
                throw new ArgumentOutOfRangeException(nameof(length), length, "length out of range");

            this.str = str;
            this.index = index;
            this.length = length;
        }

        public SubString ToSubString(int startIndex)
            => startIndex == 0 ? this : ToSubString(startIndex, length - startIndex);
        public SubString ToSubString(int startIndex, int count)
        {
            if (startIndex < 0 || startIndex > length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "start index out of range");
            if (count < 0 || startIndex + count > length)
                throw new ArgumentOutOfRangeException(nameof(count), count, "count out of range");

            return new SubString(str, index + startIndex, count);
        }

        public int IndexOf(char value)
            => str.IndexOf(value, 0, Length);
        public int IndexOf(string value)
            => str.IndexOf(value, 0, Length, StringComparison.Ordinal);
        public int IndexOf(string value, int startIndex)
            => str.IndexOf(value, index + startIndex, Length - startIndex, StringComparison.Ordinal);
        public int IndexOf(string value, int startIndex, int count)
            => str.IndexOf(value, index + startIndex, count, StringComparison.Ordinal);
        public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
            => str.IndexOf(value, index + startIndex, count, comparisonType);

        public IEnumerator<char> GetEnumerator()
        {
            for (int i = 0; i < length; i++)
                yield return str[i + index];
        }
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
            => index == 0 && length == str.Length ? str : str.Substring(index, length);

    }

    public static class SubStringExtension
    {
        public static SubString ToSubString(this string str)
            => new SubString(str);
        public static SubString ToSubString(this string str, int startIndex)
            => new SubString(str, startIndex);
        public static SubString ToSubString(this string str, int startIndex, int count)
            => new SubString(str, startIndex, count);
        public static SubString ToSubString(this SubString str)
            => str;
    }
}
