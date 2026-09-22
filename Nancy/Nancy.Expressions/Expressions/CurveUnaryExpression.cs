using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Curve"/>
/// object. 
/// </summary>
/// <typeparam name="T">The type of the value of the operand expression.</typeparam>
public abstract record CurveUnaryExpression<T> : CurveExpression, IGenericUnaryExpression<T, Curve>
{
    /// <summary>
    /// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Curve"/>
    /// object. 
    /// </summary>
    protected CurveUnaryExpression(
        IGenericExpression<T> operand,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : base(expressionName, settings)
    {
        Operand = operand;
    }

    /// <inheritdoc />
    public IGenericExpression<T> Operand { get; init; }

    /// <inheritdoc cref="IGenericUnaryExpression{T,TResult}.Expression"/>
    [Obsolete("Renamed to Operand.")]
    public IGenericExpression<T> Expression => Operand;

    /// <summary>
    /// Returns a copy of this node with <paramref name="operand"/> in place of its operand.
    /// </summary>
    /// <param name="operand">The new operand.</param>
    /// <remarks>
    /// The copy carries <see cref="CurveExpression.Name"/>, <see cref="CurveExpression.Generation"/> and <see cref="CurveExpression.Settings"/>, and leaves the computed-value caches behind.
    /// It is the concrete node's own type that is copied, so a node with state beyond its operands keeps that state.
    /// </remarks>
    public virtual IGenericExpression<Curve> WithOperand(IGenericExpression<T> operand)
        => this with { Operand = operand };
}