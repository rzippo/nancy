using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the max-plus deconvolution
/// </summary>
public record MaxPlusDeconvolutionExpression : CurveBinaryExpression<Curve, Curve>
{
    /// <summary>
    /// Creates a max-plus deconvolution expression
    /// </summary>
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="curveR">The right operand curve.</param>
    /// <param name="nameR">The name to assign to the right operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public MaxPlusDeconvolutionExpression(
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
    /// Creates a max-plus deconvolution expression
    /// </summary>
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public MaxPlusDeconvolutionExpression(
        Curve curveL,
        string nameL,
        CurveExpression rightOperand,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightOperand, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a max-plus deconvolution expression
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public MaxPlusDeconvolutionExpression(
        CurveExpression leftOperand,
        CurveExpression rightOperand,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftOperand, rightOperand, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}