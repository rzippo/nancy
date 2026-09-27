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

    /// <summary>
    /// If true, a node that has just computed its value clears the cached values of the operands it read, unless they are named or cheap.
    /// </summary>
    /// <remarks>
    /// An operand's value is of no further use to its parent once the parent has computed, so a computation holds only the values still waiting for a sibling, and leaves no unnamed node below the root holding one.
    /// A named operand keeps its value, as does a leaf, whose value is its input, and one of at most <see cref="CheapCacheElementThreshold"/> elements.
    /// An unnamed operand shared with another parent that has yet to compute is computed again for it.
    /// Given to <see cref="IGenericExpression{TExpressionResult}.Compute"/>, it applies to every node of the computation.
    /// </remarks>
    public bool ClearOperandsWhenComputed { get; init; }

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
