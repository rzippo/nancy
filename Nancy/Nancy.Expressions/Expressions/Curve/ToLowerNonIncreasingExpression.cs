using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the lower non-increasing closure, $\mathrm{LNI}$, of a curve
/// (<see cref="Curve.ToLowerNonIncreasing"/>)
/// </summary>
public record ToLowerNonIncreasingExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the lower non-increasing closure, $\mathrm{LNI}$, expression
    /// </summary>
    public ToLowerNonIncreasingExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the lower non-increasing closure, $\mathrm{LNI}$, of a curve
    /// (<see cref="Curve.ToLowerNonIncreasing"/>)
    /// </summary>
    public ToLowerNonIncreasingExpression(
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
