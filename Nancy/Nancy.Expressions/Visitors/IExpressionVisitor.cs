using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor interface of the Visitor design pattern. It allows to keep separated an algorithm ("visit") from the
/// complex data structure it is applied to, so that it is possible to implement new algorithms without modifying
/// the complex data structure itself.
/// </summary>
public interface IExpressionVisitor
{
    /// <summary>
    /// Visit method of a generic expression.
    /// Fallback method in case no more specific one is available, always raises a <exception cref="NotImplementedException"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value of the expression</typeparam>
    /// <exception cref="NotImplementedException">
    /// Always raised if the method is executed, it means that a more specific Visit method is missing.
    /// </exception>
    void Visit<T>(IGenericExpression<T> expression)
        => throw new NotImplementedException($"{this.GetType()} does not have a Visit method for type {expression.GetType()}");
}