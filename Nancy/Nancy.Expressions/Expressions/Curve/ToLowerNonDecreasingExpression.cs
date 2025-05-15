using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the lower non-decreasing closure of a curve
/// (<see cref="Curve.ToLowerNonDecreasing"/>)
/// </summary>
public record ToLowerNonDecreasingExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the lower non-decreasing closure expression
    /// </summary>
    public ToLowerNonDecreasingExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the lower non-decreasing closure of a curve
    /// (<see cref="Curve.ToLowerNonDecreasing"/>)
    /// </summary>
    public ToLowerNonDecreasingExpression(
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