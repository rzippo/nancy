namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// Reports the value type an expression is written over.
/// </summary>
internal static class ExpressionValueType
{
    /// <summary>
    /// The type argument of the <see cref="IGenericExpression{T}"/> interface the expression's runtime type implements.
    /// </summary>
    /// <param name="expression">The expression whose value type is asked.</param>
    /// <exception cref="ArgumentException">If the expression implements no <see cref="IGenericExpression{T}"/> interface.</exception>
    internal static Type Of(IExpression expression)
    {
        var genericExpression = expression.GetType().GetInterfaces()
            .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGenericExpression<>));
        if (genericExpression is null)
            throw new ArgumentException(
                $"The expression {expression.GetType().Name} implements no IGenericExpression<T> interface.");

        return genericExpression.GetGenericArguments()[0];
    }
}
