using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression that computes the right-limit value of the curve at a specific time.
/// </summary>
public record RightLimitAtExpression : RationalBinaryExpression<Curve, Rational>
{
    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="x">The target time of the limit.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RightLimitAtExpression(
        Curve curve,
        string name,
        Rational x,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), new RationalNumberExpression(x), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="x">The target time of the limit.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RightLimitAtExpression(
        Curve curve,
        string name,
        RationalExpression x,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), x, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    /// <param name="curve">The operand curve expression.</param>
    /// <param name="x">The target time of the limit.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RightLimitAtExpression(
        CurveExpression curve,
        RationalExpression x,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(curve, x, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}