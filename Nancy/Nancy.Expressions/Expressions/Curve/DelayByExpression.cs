using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the operation delay a curve by a certain time
/// </summary>
public class DelayByExpression(
    CurveExpression leftExpression,
    RationalExpression rightExpression,
    string expressionName = "", ExpressionSettings? settings = null)
    : CurveBinaryExpression<Curve, Rational>(leftExpression, rightExpression, expressionName, settings)
{
    /// <summary>
    /// Creates the "delay-by" expression
    /// </summary>
    public DelayByExpression(Curve curveL, string nameL, Rational delay, string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), new RationalNumberExpression(delay), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "delay-by" expression
    /// </summary>
    public DelayByExpression(Curve curveL, string nameL, RationalExpression rightExpression,
        string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}