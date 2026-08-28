using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the upper non-increasing closure, $\mathrm{UNI}$, of a curve
/// (<see cref="Curve.ToUpperNonIncreasing"/>)
/// </summary>
public record ToUpperNonIncreasingExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the upper non-increasing closure, $\mathrm{UNI}$, expression
    /// </summary>
    public ToUpperNonIncreasingExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the upper non-increasing closure, $\mathrm{UNI}$, of a curve
    /// (<see cref="Curve.ToUpperNonIncreasing"/>)
    /// </summary>
    public ToUpperNonIncreasingExpression(
        CurveExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
