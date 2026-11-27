using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the operation which enforces a curve to assume a given value at time 0.
/// </summary>
public record WithOriginAtExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the "with-origin-at" expression.
    /// </summary>
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="value">The value at the origin.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public WithOriginAtExpression(
        Curve curve,
        string name,
        Rational value,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), value, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "with-origin-at" expression.
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="value">The value at the origin.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public WithOriginAtExpression(
        CurveExpression expression,
        Rational value,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
        OriginValue = value;
    }

    /// <summary>
    /// The value enforced at time 0.
    /// </summary>
    public Rational OriginValue { get; }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
