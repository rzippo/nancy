using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the subtraction
/// </summary>
public record SubtractionExpression : CurveBinaryExpression<Curve, Curve>
{
    /// <summary>
    /// If set to true, the result is forced to be non-negative.
    /// </summary>
    public bool NonNegative { get; init; }
    
    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    public SubtractionExpression(
        Curve curveL, 
        string nameL, 
        Curve curveR, 
        string nameR,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new ConcreteCurveExpression(curveR, nameR), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    public SubtractionExpression(
        Curve curveL, 
        string nameL, 
        CurveExpression rightExpression,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    public SubtractionExpression(
        CurveExpression leftExpression,
        CurveExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftExpression, rightExpression, expressionName, settings)
    {
        NonNegative = false;
    }
    
    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public SubtractionExpression(
        Curve curveL, 
        string nameL, 
        Curve curveR, 
        string nameR, 
        bool nonNegative,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new ConcreteCurveExpression(curveR, nameR), nonNegative, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public SubtractionExpression(
        Curve curveL, 
        string nameL, 
        CurveExpression rightExpression, 
        bool nonNegative,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, nonNegative, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public SubtractionExpression(
        CurveExpression leftExpression,
        CurveExpression rightExpression,
        bool nonNegative,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftExpression, rightExpression, expressionName, settings)
    {
        NonNegative = nonNegative;
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}