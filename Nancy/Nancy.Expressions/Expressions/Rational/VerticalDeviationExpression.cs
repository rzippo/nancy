using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the vertical deviation between two curve expressions
/// </summary>
public class VerticalDeviationExpression(
    CurveExpression leftExpression,
    CurveExpression rightExpression,
    string expressionName = "",
    ExpressionSettings? settings = null)
    : RationalBinaryExpression<Curve, Curve>(leftExpression, rightExpression, expressionName, settings)
{
    /// <summary>
    /// Creates a "vertical deviation expression"
    /// </summary>
    public VerticalDeviationExpression(Curve curveL, string nameL, Curve curveR, string nameR,
        string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), new ConcreteCurveExpression(curveR, nameR), expressionName,
            settings)
    {
    }

    /// <summary>
    /// Creates a "vertical deviation expression"
    /// </summary>
    public VerticalDeviationExpression(Curve curveL, string nameL, CurveExpression rightExpression,
        string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}