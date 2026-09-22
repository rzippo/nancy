using Unipi.Nancy.Expressions.Nodes;
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

    /// <summary>
    /// Returns a copy of this node with <paramref name="operand"/> in place of its operand.
    /// </summary>
    /// <param name="operand">The new operand.</param>
    /// <remarks>
    /// The copy carries <see cref="RationalExpression.Name"/>, <see cref="RationalExpression.Generation"/> and <see cref="RationalExpression.Settings"/>, and leaves the computed-value caches behind.
    /// It is the concrete node's own type that is copied, so a node with state beyond its operands keeps that state.
    /// </remarks>
    public virtual IGenericExpression<Rational> WithOperand(IGenericExpression<TOperandResult> operand)
        => this with { Operand = operand };
}