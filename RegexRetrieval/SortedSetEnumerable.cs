using System.Collections.Generic;

namespace RegexRetrieval
{
    public static class SortedSetEnumerable
    {

        /// <summary>
        /// Returns a new sorted set in ascending order which is the intersection of the two given sets.
        /// </summary>
        /// <param name="a">A sorted set in ascending order.</param>
        /// <param name="b">A sorted set in ascending order.</param>
        /// <returns></returns>
        public static IEnumerable<int> SetIntersect(this IEnumerable<int> a, IEnumerable<int> b)
        {
            var e = b.GetEnumerator();
            if (!e.MoveNext()) yield break;

            foreach (var i in a)
            {
                if (i < e.Current) continue;

                while (i > e.Current)
                {
                    if (!e.MoveNext()) yield break;
                }

                if (i == e.Current)
                {
                    yield return i;

                    if (!e.MoveNext()) yield break;
                }
            }
        }

        /// <summary>
        /// Returns a new sorted set in union order which is the intersection of the two given sets.
        /// </summary>
        /// <param name="a">A sorted set in ascending order.</param>
        /// <param name="b">A sorted set in ascending order.</param>
        /// <returns></returns>
        public static IEnumerable<int> SetUnion(this IEnumerable<int> a, IEnumerable<int> b)
        {
            var e = b.GetEnumerator();

            bool hasNext = e.MoveNext();
            foreach (var i in a)
            {
                if (hasNext)
                {
                    bool doReturn = true;
                    while (hasNext && e.Current <= i)
                    {
                        yield return e.Current;
                        if (i == e.Current)
                            doReturn = false;
                        hasNext = e.MoveNext();
                    }
                    if (doReturn)
                        yield return i;
                }
                else
                    yield return i;
            }

            if (hasNext)
            {
                while (e.MoveNext())
                    yield return e.Current;
            }
        }

        /// <summary>
        /// Creates the complement of the given set in the universe described by the given range.
        /// </summary>
        /// <param name="a">A sorted set in ascending order.</param>
        /// <param name="fromInclusive"></param>
        /// <param name="toExclusive"></param>
        /// <returns></returns>
        public static IEnumerable<int> SetComplement(this IEnumerable<int> a, int fromInclusive, int toExclusive)
        {
            var current = fromInclusive;
            foreach (var i in a)
            {
                if (current < i)
                    for (int j = current; j < i; j++)
                        yield return j;
                current = i + 1;
            }
            for (; current < toExclusive; current++)
                yield return current;
        }

        public static IEnumerable<int> SetWithoutSubSet(this IEnumerable<int> set, IEnumerable<int> subSet)
        {
            var e = subSet.GetEnumerator();
            if (!e.MoveNext())
                return set;
            else
                return SetWithoutNonEmptySubSet(set, e);
        }
        private static IEnumerable<int> SetWithoutNonEmptySubSet(IEnumerable<int> set, IEnumerator<int> e)
        {
            // whether all elements of the subset have been precessed.
            var subSetReady = false;
            var current = e.Current;

            foreach (var i in set)
            {
                if (subSetReady)
                {
                    yield return i;
                }
                else
                {
                    if (current < i)
                    {
                        if (e.MoveNext())
                            current = e.Current;
                        else
                            subSetReady = true;
                    }

                    if (subSetReady || i != current)
                        yield return i;
                }
            }
        }

        /// <summary>
        /// Returns a new sorted set which contains only the first occurrence of every value.
        /// <para>
        /// The sorting direction of the returned set is the same as <paramref name="collection"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">A sorted collection.</param>
        /// <returns></returns>
        public static IEnumerable<T> Unique<T>(this IEnumerable<T> collection)
            where T : struct
        {
            var e = collection.GetEnumerator();

            if (!e.MoveNext())
                yield break;
            var last = e.Current;
            yield return last;

            while (e.MoveNext())
            {
                if (!last.Equals(e.Current))
                {
                    last = e.Current;
                    yield return last;
                }
            }
        }

        #region selection

        public static Selection<int> SetComplement(this Selection<int> selection, int fromInclusive, int toExclusive)
        {
            if (selection.IsAll) return Selection<int>.Empty;
            if (selection.IsEmpty) return Selection<int>.All;

            var count = toExclusive - selection.Count - fromInclusive;
            return new Selection<int>(selection.Value.SetComplement(fromInclusive, toExclusive), count);
        }

        public static Selection<int> SetWithoutSubSet(this Selection<int> selection, Selection<int> subsetSelection)
        {
            if (subsetSelection.IsEmpty) return selection;
            if (subsetSelection.IsAll) return Selection<int>.Empty;
            if (selection.IsAll || selection.IsEmpty) return selection;

            var count = selection.Count - subsetSelection.Count;
            return new Selection<int>(selection.Value.SetWithoutSubSet(subsetSelection.Value), count);
        }

        #endregion

    }
}
