using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression that computes the infimum value attained by a curve expression, $\inf_{t \ge 0} f(t)$
/// (<see cref="Curve.InfValue"/>).
/// </summary>
public record InfValueExpression : RationalUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the "infimum value" expression
    /// </summary>
    public InfValueExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "infimum value" expression
    /// </summary>
    public InfValueExpression(
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
