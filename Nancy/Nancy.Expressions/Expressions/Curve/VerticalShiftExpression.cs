using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression that computes the vertical shift of a curve,
/// i.e. $g(t) = f(t) + K$.
/// </summary>
/// <remarks>
/// The shift always moves the entire curve, including the point at the origin.
/// </remarks>
public record VerticalShiftExpression : CurveBinaryExpression<Curve, Rational>
{
    /// <summary>
    /// Creates the $g(t) = f(t) + K$ expression.
    /// </summary>
    public VerticalShiftExpression(
        Curve curveL, 
        string nameL, 
        Rational value, 
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new RationalNumberExpression(value), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the $g(t) = f(t) + K$ expression.
    /// </summary>
    public VerticalShiftExpression(
        Curve curveL, 
        string nameL, 
        RationalExpression rightExpression,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }
    
    /// <summary>
    /// Creates the $g(t) = f(t) + K$ expression.
    /// </summary>
    /// <param name="curveExpression">The curve to be shifted.</param>
    /// <param name="rationalExpression">The amount of the shift.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public VerticalShiftExpression(
        CurveExpression curveExpression,
        RationalExpression rationalExpression,
        string expressionName = "", 
        ExpressionSettings? settings = null)
        : base(curveExpression, rationalExpression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
    
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}