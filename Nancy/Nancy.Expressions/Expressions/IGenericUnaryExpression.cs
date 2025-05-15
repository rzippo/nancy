namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Interface with members and functions which allow to define a unary expression, i.e. an expression whose root node
/// corresponds to an operation that involves only one operand.
/// </summary>
/// <typeparam name="T">Operand type</typeparam>
/// <typeparam name="TResult">Result type</typeparam>
public interface IGenericUnaryExpression<out T, out TResult> : IGenericExpression<TResult> 
{
    /// <summary>
    /// The single operand of the operator.
    /// </summary>
    public IGenericExpression<T> Expression { get; }
}