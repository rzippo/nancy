using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the floor of a curve, $\lfloor f(t) \rfloor$
/// (<see cref="Curve.Floor"/>)
/// </summary>
public record FloorExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the floor expression
    /// </summary>
    public FloorExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the floor of a curve, $\lfloor f(t) \rfloor$
    /// (<see cref="Curve.Floor"/>)
    /// </summary>
    public FloorExpression(
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
