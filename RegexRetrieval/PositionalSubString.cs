using System;
using System.Collections;
using System.Collections.Generic;

namespace RegexRetrieval
{
    public struct PositionalSubString : IReadOnlyList<char>, IEnumerable<char>
    {
        public readonly int LeftMin;
        public readonly int LeftMax;
        public readonly int RightMin;
        public readonly int RightMax;
        public readonly SubString Value;

        public bool LeftFixed => LeftMin == LeftMax;
        public bool RightFixed => RightMin == RightMax;
        public int Left => LeftFixed ? LeftMin : throw new InvalidOperationException();
        public int Right => RightFixed ? RightMin : throw new InvalidOperationException();

        public int Length => Value.Length;
        int IReadOnlyCollection<char>.Count => Length;

        public char this[int index] => Value[index];

        public PositionalSubString(SubString value, int leftMin, int leftMax, int rightMin, int rightMax)
        {
            LeftMin = leftMin;
            LeftMax = leftMax;
            RightMin = rightMin;
            RightMax = rightMax;
            Value = value;
        }

        public IEnumerator<char> GetEnumerator()
            => Value.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
            => Value.ToString();
    }

    public static class PositionedSubStringExtensions
    {
        public static PositionalSubString ToPositional(this SubString value, int leftMin = 0, int leftMax = int.MaxValue, int rightMin = 0, int rightMax = int.MaxValue)
            => new PositionalSubString(value, leftMin, leftMax, rightMin, rightMax);
        public static PositionalSubString ToPositional(this string value, int leftMin = 0, int leftMax = int.MaxValue, int rightMin = 0, int rightMax = int.MaxValue)
            => new PositionalSubString(value.ToSubString(), leftMin, leftMax, rightMin, rightMax);
    }
}
