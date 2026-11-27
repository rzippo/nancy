using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the horizontal deviation between two curve expressions
/// </summary>
public record HorizontalDeviationExpression : RationalBinaryExpression<Curve, Curve>
{
    /// <summary>
    /// Creates a "horizontal deviation expression"
    /// </summary>
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="curveR">The right operand curve.</param>
    /// <param name="nameR">The name to assign to the right operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public HorizontalDeviationExpression(
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
    /// Creates a "horizontal deviation expression"
    /// </summary>
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public HorizontalDeviationExpression(
        Curve curveL,
        string nameL,
        CurveExpression rightOperand,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightOperand, expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the horizontal deviation between two curve expressions
    /// </summary>
    /// <param name="LeftExpression">The left operand expression.</param>
    /// <param name="RightExpression">The right operand expression.</param>
    /// <param name="ExpressionName">The name of the expression.</param>
    /// <param name="Settings">Settings for the expression definition and evaluation.</param>
    public HorizontalDeviationExpression(
        CurveExpression LeftExpression,
        CurveExpression RightExpression,
        string ExpressionName = "",
        ExpressionSettings? Settings = null)
        : base(LeftExpression, RightExpression, ExpressionName, Settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}