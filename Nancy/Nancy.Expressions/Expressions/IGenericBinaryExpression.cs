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
    /// Left operand expression
    /// </summary>
    public IGenericExpression<T1> LeftExpression { get; }

    /// <summary>
    /// Right operand expression
    /// </summary>
    public IGenericExpression<T2> RightExpression { get; }
}