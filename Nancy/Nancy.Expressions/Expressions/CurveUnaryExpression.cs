using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Curve"/>
/// object. 
/// </summary>
/// <typeparam name="T">The type of the value of the operand expression</typeparam>
public abstract class CurveUnaryExpression<T>(
    IGenericExpression<T> expression,
    string expressionName = "", 
    ExpressionSettings? settings = null)
    : CurveExpression(expressionName, settings), IGenericUnaryExpression<T, Curve>
{
    public IGenericExpression<T> Expression { get; set; } = expression;
}