using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the max-plus deconvolution
/// </summary>
public record MaxPlusDeconvolutionExpression : CurveBinaryExpression<Curve, Curve>
{
    /// <summary>
    /// Creates a max-plus deconvolution expression
    /// </summary>
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
    public MaxPlusDeconvolutionExpression(
        Curve curveL,
        string nameL,
        CurveExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a max-plus deconvolution expression
    /// </summary>
    public MaxPlusDeconvolutionExpression(
        CurveExpression leftExpression,
        CurveExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftExpression, rightExpression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}