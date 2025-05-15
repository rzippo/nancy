using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Rational"/>
/// object. 
/// </summary>
/// <typeparam name="TOperandResult">The type of the value of the operand expression.</typeparam>
public abstract record RationalUnaryExpression<TOperandResult> : RationalExpression, IGenericUnaryExpression<TOperandResult, Rational>
{
    /// <summary>
    /// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Rational"/>
    /// object. 
    /// </summary>
    protected RationalUnaryExpression(
        IGenericExpression<TOperandResult> expression,
        string expressionName = "", 
        ExpressionSettings? settings = null)
        : base(expressionName, settings)
    {
        Expression = expression;
    }

    /// <inheritdoc />
    public IGenericExpression<TOperandResult> Expression { get; init; }
}