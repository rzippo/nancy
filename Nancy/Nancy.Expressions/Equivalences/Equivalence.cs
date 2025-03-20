using System.Text;
using Antlr4.Runtime;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.ExpressionsUtility.Internals;
using Unipi.Nancy.Expressions.Grammar;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// The class allows to define equivalences involving NetCal expressions
/// </summary>
public partial class Equivalence
{
    /// <summary>
    /// Left side of the equivalence
    /// </summary>
    public CurveExpression LeftSideExpression { get; init; }
    
    /// <summary>
    /// Right side of the equivalence
    /// </summary>
    public CurveExpression RightSideExpression { get; init; }

    internal readonly Dictionary<string, IEnumerable<Predicate<CurveExpression>>> Hypothesis = new();

    internal readonly Dictionary<Tuple<string, string>, IEnumerable<Func<CurveExpression, CurveExpression, bool>>>
        HypothesisPair = new();

    internal readonly
        Dictionary<Tuple<string, string, string>,
            IEnumerable<Func<CurveExpression, CurveExpression, CurveExpression, bool>>>
        HypothesisTriple = new();

    internal readonly Dictionary<string, IEnumerable<Predicate<RationalExpression>>> RationalHypothesis = new();

    internal readonly Dictionary<Tuple<string, string>, IEnumerable<Func<RationalExpression, RationalExpression, bool>>>
        RationalHypothesisPair = new();

    internal readonly
        Dictionary<Tuple<string, string, string>,
            IEnumerable<Func<RationalExpression, RationalExpression, RationalExpression, bool>>>
        RationalHypothesisTriple = new();

    /// <summary>
    /// Equivalence constructor
    /// </summary>
    /// <param name="leftSideExpression">The left side of the equivalence</param>
    /// <param name="rightSideExpression">The right side of the equivalence</param>
    /// <exception cref="Exception">Exception raised if left or right side don't contain placeholders</exception>
    public Equivalence(
        CurveExpression leftSideExpression,
        CurveExpression rightSideExpression)
    {
        if (!_endWithPlaceholder(leftSideExpression) || !_endWithPlaceholder(rightSideExpression))
            throw new InvalidOperationException(
                "Can't instantiate Equivalence: both sides must end with a placeholder in at least one branch!");

        LeftSideExpression = leftSideExpression;
        RightSideExpression = rightSideExpression;
    }

    public void AddHypothesis(string placeholder, Predicate<CurveExpression> h)
    {
        if (Hypothesis.TryGetValue(placeholder, out var hypothesisList))
            Hypothesis[placeholder] = hypothesisList.Append(h);
        else
            Hypothesis[placeholder] = [h];
    }

    public void AddHypothesis(string placeholder1, string placeholder2,
        Func<CurveExpression, CurveExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2);
        if (HypothesisPair.TryGetValue(key, out var hypothesisList))
            HypothesisPair[key] = hypothesisList.Append(h);
        else
            HypothesisPair[key] = [h];
    }

    public void AddHypothesis(string placeholder1, string placeholder2, string placeholder3,
        Func<CurveExpression, CurveExpression, CurveExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2, placeholder3);
        if (HypothesisTriple.TryGetValue(key, out var hypothesisList))
            HypothesisTriple[key] = hypothesisList.Append(h);
        else
            HypothesisTriple[key] = [h];
    }

    public void AddHypothesis(string placeholder, Predicate<RationalExpression> h)
    {
        if (RationalHypothesis.TryGetValue(placeholder, out var hypothesisList))
            RationalHypothesis[placeholder] = hypothesisList.Append(h);
        else
            RationalHypothesis[placeholder] = [h];
    }

    public void AddHypothesis(string placeholder1, string placeholder2,
        Func<RationalExpression, RationalExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2);
        if (RationalHypothesisPair.TryGetValue(key, out var hypothesisList))
            RationalHypothesisPair[key] = hypothesisList.Append(h);
        else
            RationalHypothesisPair[key] = [h];
    }

    public void AddHypothesis(string placeholder1, string placeholder2, string placeholder3,
        Func<RationalExpression, RationalExpression, RationalExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2, placeholder3);
        if (RationalHypothesisTriple.TryGetValue(key, out var hypothesisList))
            RationalHypothesisTriple[key] = hypothesisList.Append(h);
        else
            RationalHypothesisTriple[key] = [h];
    }

    public EquivalenceApplyResult Apply(
        IGenericExpression<Curve> expression,
        CheckType checkType = CheckType.CheckLeftOnly
    )
    {
        var applier = new OneTimeEquivalenceApplier{ Equivalence = this };
        return applier.Apply(expression, checkType);
    }

    private static bool _endWithPlaceholder<T>(IGenericExpression<T> expression)
    {
        return expression switch
        {
            CurvePlaceholderExpression => true,
            RationalPlaceholderExpression => true,
            ConcreteCurveExpression => false,
            RationalNumberExpression => false,
            IGenericUnaryExpression<Curve, T> e => _endWithPlaceholder(e.Expression),
            IGenericUnaryExpression<Rational, T> e => _endWithPlaceholder(e.Expression),
            IGenericBinaryExpression<Curve, Curve, T> e => _endWithPlaceholder(e.LeftExpression) ||
                                                           _endWithPlaceholder(e.RightExpression),
            IGenericBinaryExpression<Rational, Curve, T> e => _endWithPlaceholder(e.LeftExpression) ||
                                                              _endWithPlaceholder(e.RightExpression),
            IGenericBinaryExpression<Curve, Rational, T> e => _endWithPlaceholder(e.LeftExpression) ||
                                                              _endWithPlaceholder(e.RightExpression),
            IGenericBinaryExpression<Rational, Rational, T> e => _endWithPlaceholder(e.LeftExpression) ||
                                                                 _endWithPlaceholder(e.RightExpression),
            IGenericNAryExpression<T, T> e => e.Expressions.Any(_endWithPlaceholder),
            _ => throw new InvalidOperationException(expression.GetType() +
                                                     " case is missing in '_endWithPlaceholder' method")
        };
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new(LeftSideExpression.ToString());
        stringBuilder.Append(" = ");
        stringBuilder.Append(RightSideExpression);
        return stringBuilder.ToString();
    }
}

public enum CheckType
{
    CheckLeftOnly,
    CheckRightOnly,
    CheckBothSides
}

public record EquivalenceApplyResult
{
    public IGenericExpression<Curve>? NewExpression { get; init; }
    public required MatchPatternResult MatchPatternResult { get; init; }
}