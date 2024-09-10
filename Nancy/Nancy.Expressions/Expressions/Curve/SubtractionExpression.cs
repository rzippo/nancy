using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the subtraction
/// </summary>
public class SubtractionExpression(
    CurveExpression leftExpression,
    CurveExpression rightExpression,
    string expressionName = "",
    ExpressionSettings? settings = null)
    : CurveBinaryExpression<Curve, Curve>(leftExpression, rightExpression, expressionName, settings)
{
    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    public SubtractionExpression(Curve curveL, string nameL, Curve curveR, string nameR,
        string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), new ConcreteCurveExpression(curveR, nameR), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression
    /// </summary>
    public SubtractionExpression(Curve curveL, string nameL, CurveExpression rightExpression,
        string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}