using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.ExpressionsUtility.Internals;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// Class which allows to apply an equivalence to an expression.
/// </summary>
/// <remarks>Each instance is safe to use only once.</remarks>
public class OneTimeEquivalenceApplier
{
    /// <summary>
    /// Dictionary of placeholders found in the equivalence, along with the correspondent curve expression found inside
    /// the expression under analysis (for the application of the equivalence)
    /// </summary>
    private Dictionary<string, CurveExpression> _curvePlaceholders = new();

    /// <summary>
    /// Dictionary of placeholders found in the equivalence, along with the correspondent rational expression found
    /// inside the expression under analysis (for the application of the equivalence)
    /// </summary>
    private Dictionary<string, RationalExpression> _rationalPlaceholders = new();

    /// <summary>
    /// <list type="bullet">
    /// <item>-1: no matched side</item> 
    /// <item>0: left side matched</item> 
    /// <item>1: right side matched</item>
    /// </list>
    /// </summary>
    private int _matchedSide = -1;

    /// <summary>
    /// The equivalence to apply.
    /// </summary>
    public required Equivalence Equivalence { get; init; }

    /// <summary>
    /// If true, this object was already used to apply an equivalence and should not be re-used.
    /// </summary>
    public bool AlreadyUsed { get; private set; } = false;

    /// <summary>
    /// Check and apply the equivalence.
    /// </summary>
    /// <param name="expression">The expression on which the equivalence is to be applied.</param>
    /// <param name="checkType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this equivalence applier was already used.
    /// </exception>
    public EquivalenceApplyResult Apply(
        IGenericExpression<Curve> expression,
        CheckType checkType = CheckType.CheckLeftOnly
    )
    {
        if (AlreadyUsed)
            throw new InvalidOperationException("This equivalence applier was already used.");
        AlreadyUsed = true;

        _curvePlaceholders.Clear();
        _rationalPlaceholders.Clear();
        _matchedSide = -1;

        IGenericExpression<Curve>? newExpression = null;
        var matchResult = MatchSideOfEquivalence(expression, checkType);
        if (matchResult.IsMatch && VerifyHypothesis())
        {
            // Find the side of the equivalence that matched with the expression and return it (replacing placeholders)
            newExpression = _matchedSide == 0 ? Equivalence.RightSideExpression : Equivalence.LeftSideExpression;
            foreach (var (key, value) in _curvePlaceholders)
                newExpression = newExpression.ReplaceByValue(
                    Expressions.Placeholder(key),
                    value,
                    true
                );
            foreach (var (key, value) in _rationalPlaceholders)
                newExpression = newExpression.ReplaceByValue(
                    Expressions.RationalPlaceholder(key),
                    value,
                    true
                );
        }

        return new EquivalenceApplyResult
        {
            NewExpression = newExpression,
            MatchPatternResult = matchResult,
        };
    }

    private MatchPatternResult MatchSideOfEquivalence(IGenericExpression<Curve> expression, CheckType checkType)
    {
        switch (checkType)
        {
            case CheckType.CheckLeftOnly:
                {
                    var matchResult = MatchSideOfEquivalence(Equivalence.LeftSideExpression, expression, true);
                    if (matchResult.IsMatch)
                    {
                        _matchedSide = 0;
                        return matchResult;
                    }
                    break;
                }
            case CheckType.CheckRightOnly:
                {
                    var matchResult = MatchSideOfEquivalence(Equivalence.RightSideExpression, expression, true);
                    if (matchResult.IsMatch)
                    {
                        _matchedSide = 1;
                        return matchResult;
                    }
                    break;
                }
            case CheckType.CheckBothSides:
                {
                    var leftMatchResult = MatchSideOfEquivalence(Equivalence.LeftSideExpression, expression, true);
                    if (leftMatchResult.IsMatch)
                    {
                        _matchedSide = 0;
                        return leftMatchResult;
                    }

                    var rightMatchResult = MatchSideOfEquivalence(Equivalence.RightSideExpression, expression, true);
                    if (rightMatchResult.IsMatch)
                    {
                        _matchedSide = 1;
                        return rightMatchResult;
                    }

                    break;
                }
        }

        return new MatchPatternResult { IsMatch = false };
    }

    private MatchPatternResult MatchSideOfEquivalence<T>(
        IGenericExpression<T> pattern,
        IGenericExpression<T> expression,
        bool patternRoot)
    {
        switch (pattern)
        {
            case CurvePlaceholderExpression placeholder:
                return new MatchPatternResult
                {
                    IsMatch = expression is IGenericExpression<Curve> &&
                    SetValueForPlaceholder(placeholder.Name, (CurveExpression)expression)
                };
            case RationalPlaceholderExpression rationalPlaceholder:
                return new MatchPatternResult
                {
                    IsMatch = expression is IGenericExpression<Rational> &&
                    SetValueForPlaceholder(rationalPlaceholder.Name, (RationalExpression)expression)
                };
        }

        if (pattern.GetType() != expression.GetType())
            return new MatchPatternResult { IsMatch = false };

        switch (pattern, expression)
        {
            case (ConcreteCurveExpression p, ConcreteCurveExpression e):
                return new MatchPatternResult
                {
                    IsMatch = p.Name.Equals(e.Name) && p.Value.Equivalent(e.Value)
                };
            case (IGenericUnaryExpression<Curve, T> p, IGenericUnaryExpression<Curve, T> e):
                return MatchSideOfEquivalence(p.Expression, e.Expression, false);
            case (IGenericUnaryExpression<Rational, T> p, IGenericUnaryExpression<Rational, T> e):
                return MatchSideOfEquivalence(p.Expression, e.Expression, false);
            case (IGenericBinaryExpression<Curve, Curve, T> p, IGenericBinaryExpression<Curve, Curve, T> e):
                {
                    var leftMatchResult = MatchSideOfEquivalence(p.LeftExpression, e.LeftExpression, false);
                    var rightMatchResult = MatchSideOfEquivalence(p.RightExpression, e.RightExpression, false);
                    return new MatchPatternResult { IsMatch = leftMatchResult.IsMatch && rightMatchResult.IsMatch };
                }
            case (IGenericBinaryExpression<Rational, Rational, T> p, IGenericBinaryExpression<Rational, Rational, T> e):
                {
                    var leftMatchResult = MatchSideOfEquivalence(p.LeftExpression, e.LeftExpression, false);
                    var rightMatchResult = MatchSideOfEquivalence(p.RightExpression, e.RightExpression, false);
                    return new MatchPatternResult { IsMatch = leftMatchResult.IsMatch && rightMatchResult.IsMatch };
                }
            case (IGenericBinaryExpression<Rational, Curve, T> p, IGenericBinaryExpression<Rational, Curve, T> e):
                {
                    var leftMatchResult = MatchSideOfEquivalence(p.LeftExpression, e.LeftExpression, false);
                    var rightMatchResult = MatchSideOfEquivalence(p.RightExpression, e.RightExpression, false);
                    return new MatchPatternResult { IsMatch = leftMatchResult.IsMatch && rightMatchResult.IsMatch };
                }
            case (IGenericBinaryExpression<Curve, Rational, T> p, IGenericBinaryExpression<Curve, Rational, T> e):
                {
                    var leftMatchResult = MatchSideOfEquivalence(p.LeftExpression, e.LeftExpression, false);
                    var rightMatchResult = MatchSideOfEquivalence(p.RightExpression, e.RightExpression, false);
                    return new MatchPatternResult { IsMatch = leftMatchResult.IsMatch && rightMatchResult.IsMatch };
                }
            case (CurveNAryExpression p, CurveNAryExpression e):
                return MatchSideOfEquivalenceNAry(p, e, patternRoot);
            case (RationalNAryExpression p, RationalNAryExpression e):
                return MatchSideOfEquivalenceNAry(p, e, patternRoot);
            case (RationalNumberExpression p, RationalNumberExpression e):
                return new MatchPatternResult
                {
                    IsMatch = p.Value.Equals(e.Value) && p.Name.Equals(e.Name)
                };
            default:
                throw new InvalidOperationException("Missing type " + pattern.GetType());
        }
    }

    private MatchPatternNAryResult MatchSideOfEquivalenceNAry<T, TResult>(
        IGenericNAryExpression<T, TResult> pattern,
        IGenericNAryExpression<T, TResult> expression,
        bool patternRoot)
    {
        var result = new MatchPatternNAryResult();

        if (!patternRoot && pattern.Expressions.Count != expression.Expressions.Count)
            return result with { IsMatch = false };
        if (patternRoot && pattern.Expressions.Count > expression.Expressions.Count)
            return result with { IsMatch = false };
        var array = Enumerable.Range(0, expression.Expressions.Count);
        var indexesComb = array.GetCombinations(pattern.Expressions.Count);
        var operands = expression.Expressions.ToArray();
        foreach (var indexes in indexesComb)
        {
            var indexesList = indexes.ToList();
            var placeholdersBefore = _curvePlaceholders.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            var rationalPlaceholdersBefore = _rationalPlaceholders.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            result.IsMatch = true;
            List<int> alreadyMatchedIndexes = [];
            // For each operand o1 of pattern Expression
            foreach (var ePattern in pattern.Expressions)
            {
                var temp = false;
                // For each operand o2 of real expression
                foreach (var operandIndex in indexesList)
                {
                    if (alreadyMatchedIndexes.Contains(operandIndex)) continue;
                    var operand = operands[operandIndex];
                    temp = MatchSideOfEquivalence(ePattern, operand, false).IsMatch;
                    if (temp) // If o2 matches o1 --> move to next operand o1 (and don't consider anymore o2)
                    {
                        alreadyMatchedIndexes.Add(operandIndex);
                        break;
                    }
                }

                result.IsMatch &= temp;
                if (!result.IsMatch)
                    break;
            }

            if (result.IsMatch)
            {
                if (patternRoot)
                {
                    List<IGenericExpression<T>> notMatchedExpressions = [];
                    notMatchedExpressions.AddRange(operands.Where((t, i) => !alreadyMatchedIndexes.Contains(i)));

                    if (notMatchedExpressions.Count > 0)
                    {
                        // Save the operands which didn't match the pattern
                        switch (notMatchedExpressions)
                        {
                            case List<IGenericExpression<Curve>> list:
                                result.NotMatchedExpressionsCurve = list;
                                break;
                            case List<IGenericExpression<Rational>> list:
                                result.NotMatchedExpressionsRational = list;
                                break;
                        }

                        result.NaryTypePartialMatch = expression.GetType();
                        result.NaryNamePartialMatch = expression.Name;
                        result.NarySettingsPartialMatch = expression.Settings;
                    }
                }

                return result;
            }

            _curvePlaceholders = placeholdersBefore;
            _rationalPlaceholders = rationalPlaceholdersBefore;
        }

        result.IsMatch = false;
        return result;
    }

    private bool SetValueForPlaceholder(string curveName, CurveExpression expression)
    {
        // If the placeholder has been already set, check if the corresponding expressions ('curveExpression') 
        // matches 'expression'
        if (_curvePlaceholders.TryGetValue(curveName, out var curveExpression))
            return MatchSideOfEquivalence(curveExpression, expression, false).IsMatch;
        _curvePlaceholders[curveName] = expression;

        // Verify every hypothesis involving curveName
        if (Equivalence.Hypothesis.TryGetValue(curveName, out var hyp))
            if (!hyp.All(value => value(expression)))
                return false;

        foreach (var (key, value) in Equivalence.HypothesisPair)
        {
            if (!key.Item1.Equals(curveName) && !key.Item2.Equals(curveName)) continue;
            if (!_curvePlaceholders.TryGetValue(key.Item1, out var expression1)) continue;
            if (!_curvePlaceholders.TryGetValue(key.Item2, out var expression2)) continue;
            if (!value.All(hypothesis => hypothesis(expression1, expression2)))
                return false;
        }

        foreach (var (key, value) in Equivalence.HypothesisTriple)
        {
            if (!key.Item1.Equals(curveName) && !key.Item2.Equals(curveName) &&
                !key.Item3.Equals(curveName)) continue;
            if (!_curvePlaceholders.TryGetValue(key.Item1, out var expression1)) continue;
            if (!_curvePlaceholders.TryGetValue(key.Item2, out var expression2)) continue;
            if (!_curvePlaceholders.TryGetValue(key.Item3, out var expression3)) continue;
            if (!value.All(hypothesis => hypothesis(expression1, expression2, expression3)))
                return false;
        }

        return true;
    }

    private bool SetValueForPlaceholder(string rationalName, RationalExpression expression)
    {
        // If the placeholder has been already set, check if the corresponding expressions ('rationalExpression') 
        // matches 'expression'
        if (_rationalPlaceholders.TryGetValue(rationalName, out var rationalExpression))
            return MatchSideOfEquivalence(rationalExpression, expression, false).IsMatch;
        _rationalPlaceholders[rationalName] = expression;

        // Verify every hypothesis involving rationalName
        if (Equivalence.RationalHypothesis.TryGetValue(rationalName, out var hypotheses) && 
            !hypotheses.All(value => value(expression)))
            return false;

        foreach (var (key, value) in Equivalence.RationalHypothesisPair)
        {
            if (!key.Item1.Equals(rationalName) && !key.Item2.Equals(rationalName)) continue;
            if (!_rationalPlaceholders.TryGetValue(key.Item1, out var expression1)) continue;
            if (!_rationalPlaceholders.TryGetValue(key.Item2, out var expression2)) continue;
            if (!value.All(hypothesis => hypothesis(expression1, expression2)))
                return false;
        }

        foreach (var (key, value) in Equivalence.RationalHypothesisTriple)
        {
            if (!key.Item1.Equals(rationalName) && !key.Item2.Equals(rationalName) &&
                !key.Item3.Equals(rationalName)) continue;
            if (!_rationalPlaceholders.TryGetValue(key.Item1, out var expression1)) continue;
            if (!_rationalPlaceholders.TryGetValue(key.Item2, out var expression2)) continue;
            if (!_rationalPlaceholders.TryGetValue(key.Item3, out var expression3)) continue;
            if (!value.All(hypothesis => hypothesis(expression1, expression2, expression3)))
                return false;
        }

        return true;
    }

    private bool VerifyHypothesis()
    {
        foreach (var (key, value) in Equivalence.Hypothesis)
        {
            if (!_curvePlaceholders.TryGetValue(key, out var expression)) return false;
            if (!value.All(hypothesis => hypothesis(expression)))
                return false;
        }

        foreach (var (key, value) in Equivalence.HypothesisPair)
        {
            if (!_curvePlaceholders.TryGetValue(key.Item1, out var expression1)) return false;
            if (!_curvePlaceholders.TryGetValue(key.Item2, out var expression2)) return false;
            if (!value.All(hypothesis => hypothesis(expression1, expression2)))
                return false;
        }

        foreach (var (key, value) in Equivalence.HypothesisTriple)
        {
            if (!_curvePlaceholders.TryGetValue(key.Item1, out var expression1)) return false;
            if (!_curvePlaceholders.TryGetValue(key.Item2, out var expression2)) return false;
            if (!_curvePlaceholders.TryGetValue(key.Item3, out var expression3)) return false;
            if (!value.All(hypothesis => hypothesis(expression1, expression2, expression3)))
                return false;
        }

        foreach (var (key, value) in Equivalence.RationalHypothesis)
        {
            if (!_rationalPlaceholders.TryGetValue(key, out var expression)) return false;
            if (!value.All(hypothesis => hypothesis(expression)))
                return false;
        }

        foreach (var (key, value) in Equivalence.RationalHypothesisPair)
        {
            if (!_rationalPlaceholders.TryGetValue(key.Item1, out var expression1)) return false;
            if (!_rationalPlaceholders.TryGetValue(key.Item2, out var expression2)) return false;
            if (!value.All(hypothesis => hypothesis(expression1, expression2)))
                return false;
        }

        foreach (var (key, value) in Equivalence.RationalHypothesisTriple)
        {
            if (!_rationalPlaceholders.TryGetValue(key.Item1, out var expression1)) return false;
            if (!_rationalPlaceholders.TryGetValue(key.Item2, out var expression2)) return false;
            if (!_rationalPlaceholders.TryGetValue(key.Item3, out var expression3)) return false;
            if (!value.All(hypothesis => hypothesis(expression1, expression2, expression3)))
                return false;
        }

        return true;
    }
}