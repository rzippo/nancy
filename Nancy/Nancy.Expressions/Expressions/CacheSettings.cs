namespace Unipi.Nancy.Expressions;

/// <summary>
/// Settings governing how an expression's own caches are managed.
/// </summary>
public record CacheSettings
{
    /// <summary>
    /// The largest element count a <see cref="CurveExpression"/>'s cached value can have while <see cref="CurveExpression.ValueCacheIsCheap"/> still counts it as cheap to keep.
    /// </summary>
    /// <remarks>
    /// A curve of <c>m</c> segments has about <c>2m</c> elements, so the default of 40 corresponds to roughly 20 segments.
    /// It is an arbitrary starting point, chosen without profiling; tune it to your own workload's curve sizes.
    /// Real data on typical sizes would settle it properly.
    /// </remarks>
    public int CheapCacheElementThreshold { get; init; } = 40;

    /// <inheritdoc cref="CheapCacheElementThreshold"/>
    /// <remarks>
    /// A curve of <c>m</c> segments has about <c>2m</c> elements, so a segment threshold of <c>n</c> is kept as an element threshold of <c>2n</c>.
    /// </remarks>
    [Obsolete("Renamed to CheapCacheElementThreshold, which counts elements rather than segments.")]
    public int CheapCacheSegmentThreshold
    {
        // Both thresholds are plain counts, not measures: halving is meant to truncate.
#pragma warning disable NANCY0005
        get => CheapCacheElementThreshold / 2;
#pragma warning restore NANCY0005
        init => CheapCacheElementThreshold = 2 * value;
    }
}
