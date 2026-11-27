using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the lower non-decreasing closure, $\mathrm{LND}$, of a curve
/// (<see cref="Curve.ToLowerNonDecreasing"/>)
/// </summary>
public record ToLowerNonDecreasingExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the lower non-decreasing closure, $\mathrm{LND}$, expression
    /// </summary>
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public ToLowerNonDecreasingExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the lower non-decreasing closure, $\mathrm{LND}$, of a curve
    /// (<see cref="Curve.ToLowerNonDecreasing"/>)
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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