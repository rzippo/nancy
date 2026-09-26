namespace Unipi.Nancy.Expressions;

/// <summary>
/// A concrete leaf expression, whose value is compared by the value's own equivalence rather than by record equality.
/// </summary>
/// <remarks>
/// The comparison lives on the leaf so a matcher can use it without naming the value type.
/// </remarks>
internal interface IExpressionLeaf : IExpression
{
    /// <summary>
    /// True if this leaf's value matches the value of <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The expression to compare against.</param>
    bool ValueMatches(IExpression other);
}
