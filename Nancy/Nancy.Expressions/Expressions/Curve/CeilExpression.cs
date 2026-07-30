using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the ceiling of a curve, $\lceil f(t) \rceil$
/// (<see cref="Curve.Ceil"/>)
/// </summary>
public record CeilExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the ceiling expression
    /// </summary>
    public CeilExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the ceiling of a curve, $\lceil f(t) \rceil$
    /// (<see cref="Curve.Ceil"/>)
    /// </summary>
    public CeilExpression(
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
