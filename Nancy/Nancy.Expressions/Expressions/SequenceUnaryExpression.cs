using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes unary expressions (root operation has only one operand) whose value is a <see cref="Sequence"/>
/// object.
/// </summary>
/// <typeparam name="T">The type of the value of the operand expression.</typeparam>
public abstract record SequenceUnaryExpression<T> : SequenceExpression, IGenericUnaryExpression<T, Sequence>
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
}
