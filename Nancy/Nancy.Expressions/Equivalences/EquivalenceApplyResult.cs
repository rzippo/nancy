
namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// The result of applying an equivalence.
/// </summary>
public record EquivalenceApplyResult
{
    /// <summary>
    /// The expression the equivalence produced, or <see langword="null"/> if it did not match.
    /// </summary>
    /// <remarks>
    /// Its value type is the one the equivalence's own sides are written over, so a caller casts where it knows what it asked for.
    /// </remarks>
    public IExpression? NewExpression { get; init; }

    /// <summary>
    /// True if the equivalence matched.
    /// </summary>
    public bool IsMatch { get; init; }

    /// <summary>
    /// What the equivalence's placeholders bound to, by name.
    /// </summary>
    public IReadOnlyDictionary<string, IExpression> Bindings { get; init; }
        = new Dictionary<string, IExpression>();

    /// <summary>
    /// The unmatched operands of a partial n-ary match, to be reattached to the new expression.
    /// </summary>
    public IReadOnlyList<IExpression>? NotMatchedExpressions { get; init; }
}
