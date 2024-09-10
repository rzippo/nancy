using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Rational"/>
/// object. 
/// </summary>
/// <typeparam name="T">The type of the value of the operand expression</typeparam>
public abstract class RationalUnaryExpression<T>(
    IGenericExpression<T> expression,
    string expressionName = "", 
    ExpressionSettings? settings = null)
    : RationalExpression(expressionName, settings), IGenericUnaryExpression<T, Rational>
{
    public IGenericExpression<T> Expression { get; } = expression;
}