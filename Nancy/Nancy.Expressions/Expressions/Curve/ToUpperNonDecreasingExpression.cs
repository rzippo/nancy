using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the upper non-decreasing closure of a curve
/// (<see cref="Curve.ToUpperNonDecreasing"/>)
/// </summary>
public record ToUpperNonDecreasingExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the upper non-decreasing closure expression
    /// </summary>
    public ToUpperNonDecreasingExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the upper non-decreasing closure expression
    /// </summary>
    public ToUpperNonDecreasingExpression(
        CurveExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}