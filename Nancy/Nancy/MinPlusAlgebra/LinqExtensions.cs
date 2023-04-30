using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

#if DO_LOG
using NLog;
#endif

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Provides generic LINQ extension methods.
/// </summary>
public static class LinqExtensions
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    /// <summary>
    /// Retrieves a range of items from the list, via zero-based indexing
    /// </summary>
    /// <param name="list"></param>
    /// <param name="start">Zero-based index at which the range starts</param>
    /// <param name="end">Non-inclusive zero-based index at which the range ends</param>
    /// <returns>The range of items</returns>
    /// <exception cref="ArgumentException"><paramref name="start"/> and <paramref name="end"/> do not denote a valid range of items in the list</exception>
    public static IEnumerable<T> GetRangeEnumerable<T>(this IReadOnlyList<T> list, int start, int end)
    {
        if (start > end)
            throw new ArgumentException("Start must be lower than end");

        for (int i = start; i < end; i++)
            yield return list[i];
    }

    /// <summary>
    /// Shorthand method to serialize as JSON a collection of objects
    /// </summary>
    public static string ToJsonString<T>(this IEnumerable<T> items)
    {
        return JsonConvert.SerializeObject(items, new GenericCurveConverter(), new RationalConverter());
    }

    /// <summary>
    /// Associates each element with its index.
    /// Handy shortcut for foreach loops. 
    /// </summary>
    public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> items)
    {
        return items.Select((item, i) => (item, i));
    }

    /// <summary>
    /// Append all lines to the StringBuilder
    /// </summary>
    public static void AppendLines(this StringBuilder sb, IEnumerable<string> lines)
    {
        foreach (var line in lines)
            sb.AppendLine(line);
    }

    /// <summary>
    /// Optimized Distinct() which assumes the items are ordered by their <see cref="object.Equals(object?)"/> method
    /// </summary>
    /// <param name="items">The items ordered via their default comparer, i.e. the <see cref="object.Equals(object?)"/> method</param>
    /// <remarks>The result is NOT correct if <paramref name="items"/> are ordered via any other comparer</remarks>
    public static IEnumerable<TSource> OrderedDistinct<TSource>(this IOrderedEnumerable<TSource> items)
        => OrderedDistinct(items, i => i);

    /// <summary>
    /// Optimized Distinct() which assumes the items are already ordered according to the given key 
    /// </summary>
    /// <param name="items">The sequence ordered via the given key</param>
    /// <param name="keySelector">A function to extract a key from an element. Must be the same key the items have been ordered with.</param>
    /// <remarks>The result is NOT correct if <paramref name="items"/> are ordered via any other key or comparer</remarks>
    public static IEnumerable<TSource> OrderedDistinct<TSource, TKey>(this IOrderedEnumerable<TSource> items, Func<TSource, TKey> keySelector)
    {
        // this method could be improved by actually checking the comparer is the same and returning again an IOrderedEnumerable
        // for the purposes of this codebase this is enough

        bool first = true;
        TKey previousKey = default!; // never used before first assignment
        using var enumerator = items.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var currentKey = keySelector(enumerator.Current);
            if (!first && previousKey!.Equals(currentKey))
                continue;
            else
            {
                yield return enumerator.Current;
                previousKey = currentKey;
                first = false;
            }
        }
    }
}