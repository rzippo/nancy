using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class with extension methods involving <see cref="CurveExpression"/> type.
/// </summary>
public static class CurveExpressionExtensions
{
    /// <summary>
    /// Creates an expression composed of the addition between the expressions passed as arguments.
    /// </summary>
    public static AdditionExpression Sum(this IEnumerable<CurveExpression> curveExpressions)
        => new AdditionExpression(curveExpressions.ToList());

    /// <summary>
    /// Creates an expression composed of the addition between the expressions passed as arguments.
    /// </summary>
    public static AdditionExpression Sum(this IReadOnlyCollection<CurveExpression> curveExpressions)
        => new AdditionExpression(curveExpressions);
}