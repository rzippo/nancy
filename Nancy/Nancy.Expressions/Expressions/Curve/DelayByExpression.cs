using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is to delay a curve by a certain time,
/// i.e. computing $f(t - T)$.
/// </summary>
public record DelayByExpression : CurveBinaryExpression<Curve, Rational>
{
/// <summary>
    /// Creates the $f(t - T)$ expression.
    /// </summary>
    public DelayByExpression(
        Curve curveL,
        string nameL,
        Rational delay,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new RationalNumberExpression(delay), expressionName, settings)
    {
    }

/// <summary>
    /// Creates the $f(t - T)$ expression.
    /// </summary>
    public DelayByExpression(
        Curve curveL,
        string nameL,
        RationalExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

/// <summary>
    /// Creates the $f(t - T)$ expression.
    /// </summary>
    public DelayByExpression(
        CurveExpression leftExpression,
        RationalExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftExpression, rightExpression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
    
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}