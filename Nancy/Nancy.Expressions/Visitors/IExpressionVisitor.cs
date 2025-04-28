using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor interface of the Visitor design pattern. It allows to keep separated an algorithm ("visit") from the
/// complex data structure it is applied to, so that it is possible to implement new algorithms without modifying
/// the complex data structure itself.
/// </summary>
/// <typeparam name="TExpressionResult">
/// Type of the value of the expression
/// </typeparam>
public interface IExpressionVisitor<in TExpressionResult>
{
    /// <summary>
    /// Visit method of a generic expression.
    /// Fallback method in case no more specific one is available, always raises a <exception cref="NotImplementedException"/>.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Always raised if the method is executed, it means that a more specific Visit method is missing.
    /// </exception>
    void Visit(IGenericExpression<TExpressionResult> expression)
        => throw new NotImplementedException($"{this.GetType()} does not have a Visit method for type {expression.GetType()}");
}

/// <summary>
/// Visitor interface of the Visitor design pattern. It allows to keep separated an algorithm ("visit") from the
/// complex data structure it is applied to, so that it is possible to implement new algorithms without modifying
/// the complex data structure itself.
/// </summary>
/// <typeparam name="TExpressionResult">Type of the value of the expression.</typeparam>
/// <typeparam name="TResult">Type of the value produce by the visit.</typeparam>
public interface IExpressionVisitor<in TExpressionResult, out TResult>
{
    /// <summary>
    /// Visit method of a generic expression.
    /// Fallback method in case no more specific one is available, always raises a <exception cref="NotImplementedException"/>.
    /// </summary>
    /// <typeparam name="TExpressionResult">Type of the value of the expression</typeparam>
    /// <exception cref="NotImplementedException">
    /// Always raised if the method is executed, it means that a more specific Visit method is missing.
    /// </exception>
    TResult Visit(IGenericExpression<TExpressionResult> expression)
        => throw new NotImplementedException($"{this.GetType()} does not have a Visit method for type {expression.GetType()}");
}