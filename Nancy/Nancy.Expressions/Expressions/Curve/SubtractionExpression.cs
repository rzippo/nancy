using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the subtraction
/// </summary>
public record SubtractionExpression : CurveBinaryExpression<Curve, Curve>
{
    public bool NonNegative { get; init; }
    
    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    public SubtractionExpression(
        Curve curveL, 
        string nameL, 
        Curve curveR, 
        string nameR, 
        bool nonNegative = false,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new ConcreteCurveExpression(curveR, nameR), nonNegative, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    public SubtractionExpression(
        Curve curveL, 
        string nameL, 
        CurveExpression rightExpression, 
        bool nonNegative = false,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, nonNegative, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    public SubtractionExpression(
        CurveExpression leftExpression,
        CurveExpression rightExpression,
        bool nonNegative = false,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftExpression, rightExpression, expressionName, settings)
    {
        NonNegative = nonNegative;
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
    
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}