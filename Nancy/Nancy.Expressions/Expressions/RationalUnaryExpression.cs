using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Rational"/>
/// object. 
/// </summary>
/// <typeparam name="T">The type of the value of the operand expression</typeparam>
public abstract record RationalUnaryExpression<T> : RationalExpression, IGenericUnaryExpression<T, Rational>
{
    /// <summary>
    /// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Rational"/>
    /// object. 
    /// </summary>
    /// <typeparam name="T">The type of the value of the operand expression</typeparam>
    protected RationalUnaryExpression(
        IGenericExpression<T> expression,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : base(expressionName, settings)
    {
        Expression = expression;
    }

    public IGenericExpression<T> Expression { get; init; }
}