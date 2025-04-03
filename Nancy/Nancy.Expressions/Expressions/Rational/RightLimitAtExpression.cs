using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression that computes the right-limit value of the curve at a specific time.
/// </summary>
public record RightLimitAtExpression : RationalBinaryExpression<Curve, Rational>
{
    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
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
    public RightLimitAtExpression(
        CurveExpression curve,
        RationalExpression x,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(curve, x, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}