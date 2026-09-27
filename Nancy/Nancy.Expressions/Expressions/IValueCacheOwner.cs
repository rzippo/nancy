namespace Unipi.Nancy.Expressions;

/// <summary>
/// A node that caches its computed value, and can drop it once no longer needed.
/// </summary>
internal interface IValueCacheOwner : IExpression
{
    /// <summary>
    /// Drops the cached value if it has more than <paramref name="threshold"/> elements.
    /// </summary>
    void DropValueIfLargerThan(int threshold);
}
