using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class with extension methods involving <see cref="CurveExpression"/> type.
/// </summary>
public static class RationalExpressionExtensions
{
    /// <inheritdoc cref="Expressions.FromRational"/>
    public static RationalNumberExpression ToExpression(this Rational r, [CallerArgumentExpression("r")] string name = "")
        => Expressions.FromRational(r, name);
    
    /// <summary>
    /// Creates an expression composed of the addition between the rational expressions passed as arguments.
    /// </summary>
    public static RationalAdditionExpression Sum(this IEnumerable<RationalExpression> rationalExpressions)
        => new RationalAdditionExpression(rationalExpressions.ToList());

    /// <summary>
    /// Creates an expression composed of the addition between the rational expressions passed as arguments.
    /// </summary>
    public static RationalAdditionExpression Sum(this IReadOnlyCollection<RationalExpression> rationalExpressions)
        => new RationalAdditionExpression(rationalExpressions);
}