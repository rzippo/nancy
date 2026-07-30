namespace Unipi.Nancy.Expressions.ExpressionsUtility;

/// <summary>
/// Class containing extension methods
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Returns all the different subsets of <paramref name="length"/> elements from a list.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> GetCombinations<T>(this IEnumerable<T> list, int length)
    {
        if (length == 1) return list.Select(t => new T[] { t });
        var enumerable = list.ToList();
        return GetCombinations(enumerable, length - 1)
            .SelectMany(t => enumerable.Where(o => !t.Contains(o)),
                (t1, t2) => t1.Concat(new[] { t2 }));
    }
}