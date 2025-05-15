using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the non-negative closure of a curve
/// (<see cref="Curve.ToNonNegative"/>)
/// </summary>
public record ToNonNegativeExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the non-negative closure expression
    /// </summary>
    public ToNonNegativeExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the non-negative closure of a curve
    /// (<see cref="Curve.ToNonNegative"/>)
    /// </summary>
    public ToNonNegativeExpression(
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