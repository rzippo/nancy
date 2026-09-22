using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Sequence"/>
/// object.
/// </summary>
/// <typeparam name="T">The type of the value of the operand expression.</typeparam>
public abstract record SequenceUnaryExpression<T> : SequenceExpression, IGenericUnaryExpression<T, Sequence>, IExpressionNode
{
    /// <summary>
    /// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Sequence"/>
    /// object.
    /// </summary>
    protected SequenceUnaryExpression(
        IGenericExpression<T> expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expressionName, settings)
    {
        Operand = expression;
    }

    /// <inheritdoc />
    public IGenericExpression<T> Operand { get; init; }

    /// <summary>
    /// Returns a copy of this node with <paramref name="operand"/> in place of its operand.
    /// </summary>
    /// <param name="operand">The new operand.</param>
    /// <remarks>
    /// The copy carries <see cref="SequenceExpression.Name"/>, <see cref="SequenceExpression.Generation"/> and <see cref="SequenceExpression.Settings"/>, and leaves the computed-value caches behind.
    /// It is the concrete node's own type that is copied, so a node with state beyond its operands keeps that state.
    /// </remarks>
    public virtual IGenericExpression<Sequence> WithOperand(IGenericExpression<T> operand)
        => this with { Operand = operand };

    /// <inheritdoc />
    NodeArity IExpressionNode.Arity => NodeArity.Unary;

    /// <inheritdoc />
    IReadOnlyList<IExpression> IExpressionNode.Children => [Operand];

    /// <inheritdoc />
    IExpression IExpressionNode.Rebuild(IReadOnlyList<IExpression> children)
        => WithOperand((IGenericExpression<T>)children[0]);
}
