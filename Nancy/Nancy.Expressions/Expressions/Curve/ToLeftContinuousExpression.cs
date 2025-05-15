using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the conversion of a curve to be left-continuous
/// (<see cref="Curve.ToLeftContinuous"/>)
/// </summary>
public record ToLeftContinuousExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the "to-left-continuous" expression
    /// </summary>
    public ToLeftContinuousExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the conversion of a curve to be left-continuous
    /// (<see cref="Curve.ToLeftContinuous"/>)
    /// </summary>
    public ToLeftContinuousExpression(
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