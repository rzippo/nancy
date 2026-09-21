using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class with extension methods involving <see cref="SequenceExpression"/> type.
/// </summary>
public static class SequenceExpressionExtensions
{
    /// <inheritdoc cref="Expressions.FromSequence"/>
    public static ConcreteSequenceExpression ToExpression(this Sequence c, [CallerArgumentExpression("c")] string name = "")
        => Expressions.FromSequence(c, name);

    /// <summary>
    /// Creates an expression composed of the addition between the expressions passed as arguments.
    /// </summary>
    public static SequenceAdditionExpression Sum(this IEnumerable<SequenceExpression> sequenceExpressions)
        => new SequenceAdditionExpression(sequenceExpressions.Cast<IGenericExpression<Sequence>>().ToList());

    /// <summary>
    /// Creates an expression composed of the addition between the expressions passed as arguments.
    /// </summary>
    public static SequenceAdditionExpression Sum(this IReadOnlyCollection<SequenceExpression> sequenceExpressions)
        => new SequenceAdditionExpression(sequenceExpressions.Cast<IGenericExpression<Sequence>>().ToList());

    /// <summary>
    /// Creates an expression that cuts this curve expression over the given interval, crossing from a curve expression to a sequence one.
    /// </summary>
    /// <remarks>
    /// This is the only bridge between the two trees:
    /// a curve is defined over $[0, +\infty[$ and a sequence over a bounded interval, so a cut is what turns one into the other.
    /// </remarks>
    public static SequenceExpression Cut(
        this CurveExpression expression,
        Interval interval,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new CurveCutExpression(expression, interval, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts this curve expression over a neighbourhood of $[a, b]$, crossing from a curve expression to a sequence one.
    /// </summary>
    /// <remarks>
    /// See <see cref="Curve.CutToNeighbourhood"/> for what the result carries that <see cref="Cut(CurveExpression, Interval, string, ExpressionSettings)"/> does not.
    /// </remarks>
    public static SequenceExpression CutToNeighbourhood(
        this CurveExpression expression,
        Rational cutStart,
        Rational cutEnd,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new CurveCutToNeighbourhoodExpression(expression, cutStart, cutEnd, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts this sequence expression over the given interval.
    /// </summary>
    public static SequenceExpression Cut(
        this SequenceExpression expression,
        Interval interval,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceCutExpression(expression, interval, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts this sequence expression over a neighbourhood of $[a, b]$.
    /// </summary>
    public static SequenceExpression CutToNeighbourhood(
        this SequenceExpression expression,
        Rational cutStart,
        Rational cutEnd,
        string expressionName = "",
        ExpressionSettings? settings = null)
        => new SequenceCutToNeighbourhoodExpression(expression, cutStart, cutEnd, expressionName, settings);
}
