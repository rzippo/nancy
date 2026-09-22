using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes binary (nor commutative or associative) expressions whose value is a <see cref="Sequence"/>
/// object.
/// </summary>
/// <typeparam name="T1">The type of the value of the left operand</typeparam>
/// <typeparam name="T2">The type of the value of the right operand</typeparam>
public abstract record SequenceBinaryExpression<T1, T2> : SequenceExpression, IGenericBinaryExpression<T1, T2, Sequence>
{
    /// <inheritdoc/>
    protected SequenceBinaryExpression(
        IGenericExpression<T1> leftExpression,
        IGenericExpression<T2> rightExpression,
        string expressionName = "",
        ExpressionSettings? Settings = null) : base(expressionName, Settings)
    {
        LeftOperand = leftExpression;
        RightOperand = rightExpression;
    }

    /// <inheritdoc />
    public IGenericExpression<T1> LeftOperand { get; init; }

    /// <inheritdoc />
    public IGenericExpression<T2> RightOperand { get; init; }

    /// <summary>
    /// Returns a copy of this node with the given operands in place of its own.
    /// </summary>
    /// <param name="leftOperand">The new left operand.</param>
    /// <param name="rightOperand">The new right operand.</param>
    /// <remarks>
    /// The copy carries <see cref="SequenceExpression.Name"/>, <see cref="SequenceExpression.Generation"/> and <see cref="SequenceExpression.Settings"/>, and leaves the computed-value caches behind.
    /// It is the concrete node's own type that is copied, so a node with state beyond its operands keeps that state.
    /// </remarks>
    public virtual IGenericExpression<Sequence> WithOperands(IGenericExpression<T1> leftOperand, IGenericExpression<T2> rightOperand)
        => this with { LeftOperand = leftOperand, RightOperand = rightOperand };
}
