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
}
