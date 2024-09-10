using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the horizontal deviation between two curve expressions
/// </summary>
public class HorizontalDeviationExpression(
    CurveExpression leftExpression,
    CurveExpression rightExpression,
    string expressionName = "",
    ExpressionSettings? settings = null)
    : RationalBinaryExpression<Curve, Curve>(leftExpression, rightExpression, expressionName, settings)
{
    /// <summary>
    /// Creates a "horizontal deviation expression"
    /// </summary>
    public HorizontalDeviationExpression(Curve curveL, string nameL, Curve curveR, string nameR,
        string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), new ConcreteCurveExpression(curveR, nameR), expressionName,
            settings)
    {
    }

    /// <summary>
    /// Creates a "horizontal deviation expression"
    /// </summary>
    public HorizontalDeviationExpression(Curve curveL, string nameL, CurveExpression rightExpression,
        string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}