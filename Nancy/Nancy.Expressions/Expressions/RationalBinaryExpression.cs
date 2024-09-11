using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes binary (nor commutative or associative) expressions whose value is a <see cref="Rational"/>
/// object.
/// </summary>
/// <typeparam name="T1">The type of the value of the left operand</typeparam>
/// <typeparam name="T2">The type of the value of the right operand</typeparam>
public abstract class RationalBinaryExpression<T1, T2>(
    IGenericExpression<T1> leftExpression,
    IGenericExpression<T2> rightExpression,
    string expressionName = "", 
    ExpressionSettings? settings = null)
    : RationalExpression(expressionName, settings), IGenericBinaryExpression<T1, T2, Rational>
{
    public IGenericExpression<T1> LeftExpression { get; } = leftExpression;
    public IGenericExpression<T2> RightExpression { get; } = rightExpression;
}