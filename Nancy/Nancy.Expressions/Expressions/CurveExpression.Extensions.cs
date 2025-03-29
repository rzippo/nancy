using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class with extension methods involving <see cref="CurveExpression"/> type.
/// </summary>
public static class CurveExpressionExtensions
{
    /// <inheritdoc cref="Expressions.FromCurve"/>
    public static ConcreteCurveExpression ToExpression(this Curve c, [CallerArgumentExpression("c")] string name = "")
        => Expressions.FromCurve(c, name);
    
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