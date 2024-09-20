using System.Text;
using Antlr4.Runtime;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.ExpressionsUtility.Internals;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// The class allows to define equivalences involving NetCal expressions
/// </summary>
public class Equivalence
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
    /// Left side of the equivalence
    /// </summary>
    public CurveExpression LeftSideExpression { get; }
    
    /// <summary>
    /// Right side of the equivalence
    /// </summary>
    public CurveExpression RightSideExpression { get; }

    private readonly Dictionary<string, IEnumerable<Predicate<CurveExpression>>> _hypothesis = new();

    private readonly Dictionary<Tuple<string, string>, IEnumerable<Func<CurveExpression, CurveExpression, bool>>>
        _hypothesisPair = new();

    private readonly
        Dictionary<Tuple<string, string, string>,
            IEnumerable<Func<CurveExpression, CurveExpression, CurveExpression, bool>>>
        _hypothesisTriple = new();

    private readonly Dictionary<string, IEnumerable<Predicate<RationalExpression>>> _rationalHypothesis = new();

    private readonly Dictionary<Tuple<string, string>, IEnumerable<Func<RationalExpression, RationalExpression, bool>>>
        _rationalHypothesisPair = new();

    private readonly
        Dictionary<Tuple<string, string, string>,
            IEnumerable<Func<RationalExpression, RationalExpression, RationalExpression, bool>>>
        _rationalHypothesisTriple = new();

    private int _matchedSide = -1; // -1: no matched side | 0: left side matched | 1: right side matched

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
        if (_hypothesis.TryGetValue(placeholder, out var hypothesisList))
            _hypothesis[placeholder] = hypothesisList.Append(h);
        else
            _hypothesis[placeholder] = [h];
    }

    public void AddHypothesis(string placeholder1, string placeholder2,
        Func<CurveExpression, CurveExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2);
        if (_hypothesisPair.TryGetValue(key, out var hypothesisList))
            _hypothesisPair[key] = hypothesisList.Append(h);
        else
            _hypothesisPair[key] = [h];
    }

    public void AddHypothesis(string placeholder1, string placeholder2, string placeholder3,
        Func<CurveExpression, CurveExpression, CurveExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2, placeholder3);
        if (_hypothesisTriple.TryGetValue(key, out var hypothesisList))
            _hypothesisTriple[key] = hypothesisList.Append(h);
        else
            _hypothesisTriple[key] = [h];
    }

    public void AddHypothesis(string placeholder, Predicate<RationalExpression> h)
    {
        if (_rationalHypothesis.TryGetValue(placeholder, out var hypothesisList))
            _rationalHypothesis[placeholder] = hypothesisList.Append(h);
        else
            _rationalHypothesis[placeholder] = [h];
    }

    public void AddHypothesis(string placeholder1, string placeholder2,
        Func<RationalExpression, RationalExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2);
        if (_rationalHypothesisPair.TryGetValue(key, out var hypothesisList))
            _rationalHypothesisPair[key] = hypothesisList.Append(h);
        else
            _rationalHypothesisPair[key] = [h];
    }

    public void AddHypothesis(string placeholder1, string placeholder2, string placeholder3,
        Func<RationalExpression, RationalExpression, RationalExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2, placeholder3);
        if (_rationalHypothesisTriple.TryGetValue(key, out var hypothesisList))
            _rationalHypothesisTriple[key] = hypothesisList.Append(h);
        else
            _rationalHypothesisTriple[key] = [h];
    }

    private bool MatchSideOfEquivalence(IGenericExpression<Curve> expression, CheckType checkType)
    {
        switch (checkType)
        {
            case CheckType.CheckLeftOnly:
                if (MatchSideOfEquivalence(LeftSideExpression, expression, true))
                {
                    _matchedSide = 0;
                    return true;
                }

                break;
            case CheckType.CheckRightOnly:
                if (MatchSideOfEquivalence(RightSideExpression, expression, true))
                {
                    _matchedSide = 1;
                    return true;
                }

                break;
            case CheckType.CheckBothSides:
                if (MatchSideOfEquivalence(LeftSideExpression, expression, true))
                {
                    _matchedSide = 0;
                    return true;
                }

                if (MatchSideOfEquivalence(RightSideExpression, expression, true))
                {
                    _matchedSide = 1;
                    return true;
                }

                break;
        }

        return false;
    }

    private bool MatchSideOfEquivalence<T>(IGenericExpression<T> pattern, IGenericExpression<T> expression,
        bool patternRoot)
    {
        switch (pattern)
        {
            case CurvePlaceholderExpression placeholder:
                return expression is IGenericExpression<Curve> &&
                       SetValueForPlaceholder(placeholder.Name, (CurveExpression)expression);
            case RationalPlaceholderExpression rationalPlaceholder:
                return expression is IGenericExpression<Rational> &&
                       SetValueForPlaceholder(rationalPlaceholder.Name, (RationalExpression)expression);
        }

        if (pattern.GetType() != expression.GetType()) return false;

        switch (pattern, expression)
        {
            case (ConcreteCurveExpression p, ConcreteCurveExpression e):
                return p.Name.Equals(e.Name) && p.Value.Equivalent(e.Value);
            case (IGenericUnaryExpression<Curve, T> p, IGenericUnaryExpression<Curve, T> e):
                return MatchSideOfEquivalence(p.Expression, e.Expression, false);
            case (IGenericUnaryExpression<Rational, T> p, IGenericUnaryExpression<Rational, T> e):
                return MatchSideOfEquivalence(p.Expression, e.Expression, false);
            case (IGenericBinaryExpression<Curve, Curve, T> p, IGenericBinaryExpression<Curve, Curve, T> e):
                return MatchSideOfEquivalence(p.LeftExpression, e.LeftExpression, false) &&
                       MatchSideOfEquivalence(p.RightExpression, e.RightExpression, false);
            case (IGenericBinaryExpression<Rational, Rational, T> p, IGenericBinaryExpression<Rational, Rational, T> e):
                return MatchSideOfEquivalence(p.LeftExpression, e.LeftExpression, false) &&
                       MatchSideOfEquivalence(p.RightExpression, e.RightExpression, false);
            case (IGenericBinaryExpression<Rational, Curve, T> p, IGenericBinaryExpression<Rational, Curve, T> e):
                return MatchSideOfEquivalence(p.LeftExpression, e.LeftExpression, false) &&
                       MatchSideOfEquivalence(p.RightExpression, e.RightExpression, false);
            case (IGenericBinaryExpression<Curve, Rational, T> p, IGenericBinaryExpression<Curve, Rational, T> e):
                return MatchSideOfEquivalence(p.LeftExpression, e.LeftExpression, false) &&
                       MatchSideOfEquivalence(p.RightExpression, e.RightExpression, false);
            case (CurveNAryExpression p, CurveNAryExpression e):
                return MatchSideOfEquivalenceNAry(p, e, patternRoot);
            case (RationalNAryExpression p, RationalNAryExpression e):
                return MatchSideOfEquivalenceNAry(p, e, patternRoot);
            case (RationalNumberExpression p, RationalNumberExpression e):
                return p.Value.Equals(e.Value) && p.Name.Equals(e.Name);
            default:
                throw new InvalidOperationException("Missing type " + pattern.GetType());
        }
    }
    
    private bool MatchSideOfEquivalenceNAry<T, TResult>(IGenericNAryExpression<T, TResult> pattern,
        IGenericNAryExpression<T, TResult> expression, bool patternRoot)
    {
        if (!patternRoot && pattern.Expressions.Count != expression.Expressions.Count) return false;
        if (patternRoot && pattern.Expressions.Count > expression.Expressions.Count) return false;
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
            var result = true;
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
                    temp = MatchSideOfEquivalence(ePattern, operand, false);
                    if (temp) // If o2 matches o1 --> move to next operand o1 (and don't consider anymore o2)
                    {
                        alreadyMatchedIndexes.Add(operandIndex);
                        break;
                    }
                }

                result = result && temp;
                if (!result) break;
            }

            if (result)
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
                                ExpressionReplacer<Curve, Curve>.NotMatchedExpressionsCurve = list;
                                break;
                            case List<IGenericExpression<Rational>> list:
                                ExpressionReplacer<Curve, Curve>.NotMatchedExpressionsRational = list;
                                break;
                        }

                        ExpressionReplacer<Curve, Curve>.NaryTypePartialMatch = expression.GetType();
                        ExpressionReplacer<Curve, Curve>.NaryNamePartialMatch = expression.Name;
                        ExpressionReplacer<Curve, Curve>.NarySettingsPartialMatch = expression.Settings;
                    }
                }

                return true;
            }

            _curvePlaceholders = placeholdersBefore;
            _rationalPlaceholders = rationalPlaceholdersBefore;
        }

        return false;
    }

    private bool SetValueForPlaceholder(string curveName, CurveExpression expression)
    {
        // If the placeholder has been already set, check if the corresponding expressions ('curveExpression') 
        // matches 'expression'
        if (_curvePlaceholders.TryGetValue(curveName, out var curveExpression))
            return MatchSideOfEquivalence(curveExpression, expression, false);
        _curvePlaceholders[curveName] = expression;
            
        // Verify every hypothesis involving curveName
        if(_hypothesis.TryGetValue(curveName, out var hyp))
            if (!hyp.All(value => value(expression)))
                return false;
            
        foreach (var (key, value) in _hypothesisPair)
        {
            if (!key.Item1.Equals(curveName) && !key.Item2.Equals(curveName)) continue;
            if (!_curvePlaceholders.TryGetValue(key.Item1, out var expression1)) continue;
            if (!_curvePlaceholders.TryGetValue(key.Item2, out var expression2)) continue;
            if (!value.All(hypothesis => hypothesis(expression1, expression2)))
                return false;
        }

        foreach (var (key, value) in _hypothesisTriple)
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
            return MatchSideOfEquivalence(rationalExpression, expression, false);
        _rationalPlaceholders[rationalName] = expression;
            
        // Verify every hypothesis involving rationalName
        if (!_rationalHypothesis[rationalName].All(value => value(expression)))
            return false;
            
        foreach (var (key, value) in _rationalHypothesisPair)
        {
            if (!key.Item1.Equals(rationalName) && !key.Item2.Equals(rationalName)) continue;
            if (!_rationalPlaceholders.TryGetValue(key.Item1, out var expression1)) continue;
            if (!_rationalPlaceholders.TryGetValue(key.Item2, out var expression2)) continue;
            if (!value.All(hypothesis => hypothesis(expression1, expression2)))
                return false;
        }

        foreach (var (key, value) in _rationalHypothesisTriple)
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
        foreach (var (key, value) in _hypothesis)
        {
            if (!_curvePlaceholders.TryGetValue(key, out var expression)) return false;
            if (!value.All(hypothesis => hypothesis(expression)))
                return false;
        }

        foreach (var (key, value) in _hypothesisPair)
        {
            if (!_curvePlaceholders.TryGetValue(key.Item1, out var expression1)) return false;
            if (!_curvePlaceholders.TryGetValue(key.Item2, out var expression2)) return false;
            if (!value.All(hypothesis => hypothesis(expression1, expression2)))
                return false;
        }

        foreach (var (key, value) in _hypothesisTriple)
        {
            if (!_curvePlaceholders.TryGetValue(key.Item1, out var expression1)) return false;
            if (!_curvePlaceholders.TryGetValue(key.Item2, out var expression2)) return false;
            if (!_curvePlaceholders.TryGetValue(key.Item3, out var expression3)) return false;
            if (!value.All(hypothesis => hypothesis(expression1, expression2, expression3)))
                return false;
        }

        foreach (var (key, value) in _rationalHypothesis)
        {
            if (!_rationalPlaceholders.TryGetValue(key, out var expression)) return false;
            if (!value.All(hypothesis => hypothesis(expression)))
                return false;
        }

        foreach (var (key, value) in _rationalHypothesisPair)
        {
            if (!_rationalPlaceholders.TryGetValue(key.Item1, out var expression1)) return false;
            if (!_rationalPlaceholders.TryGetValue(key.Item2, out var expression2)) return false;
            if (!value.All(hypothesis => hypothesis(expression1, expression2)))
                return false;
        }

        foreach (var (key, value) in _rationalHypothesisTriple)
        {
            if (!_rationalPlaceholders.TryGetValue(key.Item1, out var expression1)) return false;
            if (!_rationalPlaceholders.TryGetValue(key.Item2, out var expression2)) return false;
            if (!_rationalPlaceholders.TryGetValue(key.Item3, out var expression3)) return false;
            if (!value.All(hypothesis => hypothesis(expression1, expression2, expression3)))
                return false;
        }

        return true;
    }

    public IGenericExpression<Curve>? Apply(IGenericExpression<Curve> expression,
        CheckType checkType = CheckType.CheckLeftOnly)
    {
        _curvePlaceholders.Clear();
        _rationalPlaceholders.Clear();
        _matchedSide = -1;
        IGenericExpression<Curve>? newExpression = null;
        if (MatchSideOfEquivalence(expression, checkType) && VerifyHypothesis())
        {
            ExpressionReplacer<Curve, Curve>.IgnoreNotMatchedExpressions = true;
            // Find the side of the equivalence that matched with the expression and return it (replacing placeholders)
            newExpression = _matchedSide == 0 ? RightSideExpression : LeftSideExpression;
            foreach (var (key, value) in _curvePlaceholders)
                newExpression = newExpression.ReplaceByValue(Expressions.Placeholder(key), value);
            foreach (var (key, value) in _rationalPlaceholders)
                newExpression = newExpression.ReplaceByValue(Expressions.RationalPlaceholder(key), value);
            ExpressionReplacer<Curve, Curve>.IgnoreNotMatchedExpressions = false;
        }

        return newExpression;
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

    public static List<Equivalence> ReadEquivalences(string fileName)
    {
        List<Equivalence> equivalenceList = [];
        var equivalenceCatalog = File.ReadAllText(fileName);
        var inputStream = new AntlrInputStream(equivalenceCatalog);
        var lexer = new NetCalGLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new NetCalGParser(commonTokenStream);
        var equivalences = parser.equivalenceCatalog().equivalence();
        var visitor = new EquivalenceGrammarVisitor();
        if (equivalences.Length > 0)
            equivalenceList.AddRange(equivalences.Select(equivalence => (Equivalence)visitor.Visit(equivalence)));

        return equivalenceList;
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