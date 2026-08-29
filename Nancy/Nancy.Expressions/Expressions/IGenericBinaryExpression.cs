namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Interface with members and functions which allow to define a binary (neither commutative nor associative) expression,
/// i.e. an expression whose root node corresponds to an operation that involves two operands.
/// </summary>
/// <typeparam name="T1">Left operand type</typeparam>
/// <typeparam name="T2">Right operand type</typeparam>
/// <typeparam name="TResult">Result type</typeparam>
public interface IGenericBinaryExpression<out T1, out T2, out TResult> : IGenericExpression<TResult>
{
    /// <summary>
    /// Left operand of the operator.
    /// </summary>
    public IGenericExpression<T1> LeftOperand { get; }

    /// <summary>
    /// Right operand of the operator.
    /// </summary>
    public IGenericExpression<T2> RightOperand { get; }

    /// <inheritdoc cref="LeftOperand"/>
    [Obsolete("Renamed to LeftOperand.")]
    public IGenericExpression<T1> LeftExpression => LeftOperand;

    /// <inheritdoc cref="RightOperand"/>
    [Obsolete("Renamed to RightOperand.")]
    public IGenericExpression<T2> RightExpression => RightOperand;
}