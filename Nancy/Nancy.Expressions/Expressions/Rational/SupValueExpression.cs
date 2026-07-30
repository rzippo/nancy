using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression that computes the supremum value attained by a curve expression, $\sup_{t \ge 0} f(t)$
/// (<see cref="Curve.SupValue"/>).
/// </summary>
public record SupValueExpression : RationalUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the "supremum value" expression
    /// </summary>
    public SupValueExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "supremum value" expression
    /// </summary>
    public SupValueExpression(
        CurveExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
