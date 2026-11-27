using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

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
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="curveR">The right operand curve.</param>
    /// <param name="nameR">The name to assign to the right operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SubtractionExpression(
        Curve curveL, 
        string nameL, 
        CurveExpression rightOperand,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightOperand, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SubtractionExpression(
        CurveExpression leftOperand,
        CurveExpression rightOperand,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftOperand, rightOperand, expressionName, settings)
    {
        NonNegative = false;
    }
    
    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="curveR">The right operand curve.</param>
    /// <param name="nameR">The name to assign to the right operand.</param>
    /// <param name="nonNegative">If true, replace negative values of the result with zero.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="nonNegative">If true, replace negative values of the result with zero.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public SubtractionExpression(
        Curve curveL, 
        string nameL, 
        CurveExpression rightOperand, 
        bool nonNegative,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightOperand, nonNegative, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="nonNegative">If true, replace negative values of the result with zero.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public SubtractionExpression(
        CurveExpression leftOperand,
        CurveExpression rightOperand,
        bool nonNegative,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftOperand, rightOperand, expressionName, settings)
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