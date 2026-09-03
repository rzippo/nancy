using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the operation which enforces a curve to assume a given value at time 0.
/// </summary>
public record WithOriginAtExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the "with-origin-at" expression.
    /// </summary>
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
