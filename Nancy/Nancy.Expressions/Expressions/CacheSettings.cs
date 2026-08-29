namespace Unipi.Nancy.Expressions;

/// <summary>
/// Settings governing how an expression's own caches are managed.
/// </summary>
public record CacheSettings
{
    /// <summary>
    /// The largest segment count a <see cref="CurveExpression"/>'s cached value can have while <see cref="CurveExpression.ValueCacheIsCheap"/> still counts it as cheap to keep.
    /// </summary>
    /// <remarks>
    /// The default of 20 is an arbitrary starting point, chosen without profiling; tune it to your own workload's curve sizes.
    /// Real data on typical sizes would settle it properly.
    /// </remarks>
    public int CheapCacheSegmentThreshold { get; init; } = 20;
}
