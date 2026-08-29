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
        IGenericExpression<TOperandResult> operand,
        string expressionName = "", 
        ExpressionSettings? settings = null)
        : base(expressionName, settings)
    {
        Operand = operand;
    }

    /// <inheritdoc />
    public IGenericExpression<TOperandResult> Operand { get; init; }

    /// <inheritdoc cref="IGenericUnaryExpression{T,TResult}.Expression"/>
    [Obsolete("Renamed to Operand.")]
    public IGenericExpression<TOperandResult> Expression => Operand;
}