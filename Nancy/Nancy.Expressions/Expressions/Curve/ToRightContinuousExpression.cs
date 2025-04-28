using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the conversion of a curve to be right-continuous
/// (<see cref="Curve.ToRightContinuous"/>)
/// </summary>
public record ToRightContinuousExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the "to-right-continuous" expression
    /// </summary>
    public ToRightContinuousExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the conversion of a curve to be right-continuous
    /// (<see cref="Curve.ToRightContinuous"/>)
    /// </summary>
    public ToRightContinuousExpression(
        CurveExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
    
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}