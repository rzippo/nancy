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
}