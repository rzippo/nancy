using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the operation to scale a curve by a certain rational number
/// </summary>
public record ScaleExpression : CurveBinaryExpression<Curve, Rational>
{
    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    public ScaleExpression(
        Curve curveL,
        string nameL,
        Rational delay,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curveL, nameL), new RationalNumberExpression(delay), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    public ScaleExpression(
        Curve curveL,
        string nameL,
        RationalExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    public ScaleExpression(
        CurveExpression leftExpression,
        RationalExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(leftExpression, rightExpression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}