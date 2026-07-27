using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the z-deviation between two curve expressions
/// </summary>
public record ZDeviationExpression : RationalBinaryExpression<Curve, Curve>
{
    /// <summary>
    /// Creates a z-deviation expression
    /// </summary>
    public ZDeviationExpression(
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
    /// Creates a z-deviation expression
    /// </summary>
    public ZDeviationExpression(
        Curve curveL,
        string nameL,
        CurveExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the z-deviation between two curve expressions
    /// </summary>
    public ZDeviationExpression(
        CurveExpression LeftExpression,
        CurveExpression RightExpression,
        string ExpressionName = "",
        ExpressionSettings? Settings = null)
        : base(LeftExpression, RightExpression, ExpressionName, Settings)
    {
    }

    /// <summary>
    /// Creates a z-deviation expression
    /// </summary>
    public ZDeviationExpression(
        CurveExpression leftExpression,
        Curve curveR,
        string nameR,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(leftExpression, new ConcreteCurveExpression(curveR, nameR), expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
