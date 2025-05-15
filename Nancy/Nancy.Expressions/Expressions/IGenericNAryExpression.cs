using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Interface with members and functions which allow to define a n-ary expression, i.e. an expression whose root node
/// corresponds to an associative and commutative operation that involves n operands (n >= 2).
/// </summary>
/// <typeparam name="T1">Operands type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IGenericNAryExpression<out T1, out TResult> : IGenericExpression<TResult>
{
    /// <summary>
    /// The operands of the n-ary operator.
    /// </summary>
    public IReadOnlyCollection<IGenericExpression<T1>> Expressions { get; }
}