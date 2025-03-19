using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the vertical deviation between two curve expressions
/// </summary>
public record VerticalDeviationExpression : RationalBinaryExpression<Curve, Curve>
{
    /// <summary>
    /// Creates a "vertical deviation expression"
    /// </summary>
    public VerticalDeviationExpression(
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
    /// Creates a "vertical deviation expression"
    /// </summary>
    public VerticalDeviationExpression(
        Curve curveL,
        string nameL,
        CurveExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the vertical deviation between two curve expressions
    /// </summary>
    public VerticalDeviationExpression(
        CurveExpression LeftExpression,
        CurveExpression RightExpression,
        string ExpressionName = "",
        ExpressionSettings? Settings = null)
        : base(LeftExpression, RightExpression, ExpressionName, Settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}