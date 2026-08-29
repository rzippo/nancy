using Unipi.Nancy.Expressions.Visitors;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class describing a placeholder for any curve expression (used for equivalences)
/// </summary>
public record CurvePlaceholderExpression : CurveExpression
{
    /// <summary>
    /// Class describing a placeholder for any curve expression (used for equivalences)
    /// </summary>
    public CurvePlaceholderExpression(
        string curveName,
        ExpressionSettings? settings = null) 
        : base(curveName, settings)
    {
        CurveName = curveName;
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    /// <summary>
    /// Name of the curve placeholder.
    /// </summary>
    public string CurveName { get; init; }

    /// <summary>
    /// True if <paramref name="other"/> is a placeholder with the same <see cref="CurveName"/>.
    /// </summary>
    /// <remarks>
    /// The one exception to "name does not matter".
    /// A placeholder has no operands and no computable value, so its name is its entire identity.
    /// </remarks>
    public virtual bool Equals(CurvePlaceholderExpression? other)
        => other is not null && base.Equals(other) && CurveName == other.CurveName;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), CurveName);
}