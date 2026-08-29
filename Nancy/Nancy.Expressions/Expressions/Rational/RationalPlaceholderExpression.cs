using Unipi.Nancy.Expressions.Visitors;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class describing a placeholder for any rational expression (used for equivalences)
/// </summary>
public record RationalPlaceholderExpression : RationalExpression
{
    /// <summary>
    /// Class describing a placeholder for any rational expression (used for equivalences)
    /// </summary>
    public RationalPlaceholderExpression(
        string rationalName,
        ExpressionSettings? settings = null)
        : base(rationalName, settings)
    {
        RationalName = rationalName;
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    /// <summary>
    /// Name of the rational placeholder.
    /// </summary>
    public string RationalName { get; init; }

    /// <summary>
    /// True if <paramref name="other"/> is a placeholder with the same <see cref="RationalName"/>.
    /// </summary>
    /// <remarks>
    /// The one exception to "name does not matter".
    /// A placeholder has no operands and no computable value, so its name is its entire identity.
    /// </remarks>
    public virtual bool Equals(RationalPlaceholderExpression? other)
        => other is not null && base.Equals(other) && RationalName == other.RationalName;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), RationalName);
}