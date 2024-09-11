using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the operation to anticipate a curve by a certain time
/// </summary>
public class AnticipateByExpression(
    CurveExpression leftExpression,
    RationalExpression rightExpression,
    string expressionName = "", ExpressionSettings? settings = null)
    : CurveBinaryExpression<Curve, Rational>(leftExpression, rightExpression, expressionName, settings)
{
    /// <summary>
    /// Creates the "anticipate-by" expression
    /// </summary>
    public AnticipateByExpression(Curve curveL, string nameL, Rational time, string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), new RationalNumberExpression(time), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "anticipate-by" expression
    /// </summary>
    public AnticipateByExpression(Curve curveL, string nameL, RationalExpression rightExpression,
        string expressionName = "", ExpressionSettings? settings = null) :
        this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}