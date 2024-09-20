using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.ExpressionsUtility.Internals;

/// <summary>
/// Class which allows to manipulate DNC expressions. It manages replacement by-value and by-position. The features
/// are also reused for applying equivalence by-value and by-position.
/// </summary>
/// <param name="originalExpression">The main DNC expression, a sub-expression of it needs to be replaced.</param>
/// <param name="newExpressionToReplace">The new expression which must be "inserted" inside the main expression.</param>
/// <typeparam name="T1">Value type of <paramref name="originalExpression"/> (Curve or Rational)</typeparam>
/// <typeparam name="T2">Value type of <paramref name="newExpressionToReplace"/> (Curve or Rational)</typeparam>
public class ExpressionReplacer<T1, T2>(
    IGenericExpression<T1> originalExpression,
    IGenericExpression<T2> newExpressionToReplace)
{
    private IGenericExpression<Curve>? _tempCurveExpression;
    private IGenericExpression<Rational>? _tempRationalExpression;
    private readonly Equivalence? _equivalence;
    private readonly CheckType _checkType;

    public ExpressionReplacer(IGenericExpression<T1> originalExpression,
        Equivalence equivalence, CheckType checkType = CheckType.CheckLeftOnly) : this(originalExpression,
        (IGenericExpression<T2>)Expressions.FromCurve(Curve.Zero()))
    {
        _equivalence = equivalence;
        _checkType = checkType;
    }

    /// <summary>
    /// Verifies if an expression is equivalent to another (called pattern) by comparing their structure
    /// </summary>
    /// <param name="pattern">The pattern expression</param>
    /// <param name="expression">The expression checked against the pattern</param>
    /// <param name="patternRoot">Indication of whether the match is at the root of the pattern or not</param>
    /// <returns>True if the expression matches the pattern, False otherwise</returns>
    public static bool MatchPattern<T>(IGenericExpression<T> pattern, IGenericExpression<T> expression,
        bool patternRoot)
    {
        if (pattern.GetType() != expression.GetType()) return false;

        switch (pattern, expression)
        {
            case (CurvePlaceholderExpression p, CurvePlaceholderExpression e):
                return p.Name.Equals(e.Name);
            case (RationalPlaceholderExpression p, RationalPlaceholderExpression e):
                return p.Name.Equals(e.Name);
            case (ConcreteCurveExpression p, ConcreteCurveExpression e):
                return p.Name.Equals(e.Name) && p.Value.Equivalent(e.Value);
            case (IGenericUnaryExpression<Curve, T> p, IGenericUnaryExpression<Curve, T> e):
                return MatchPattern(p.Expression, e.Expression, false);
            case (IGenericUnaryExpression<Rational, T> p, IGenericUnaryExpression<Rational, T> e):
                return MatchPattern(p.Expression, e.Expression, false);
            case (IGenericBinaryExpression<Curve, Curve, T> p, IGenericBinaryExpression<Curve, Curve, T> e):
                return MatchPattern(p.LeftExpression, e.LeftExpression, false) &&
                       MatchPattern(p.RightExpression, e.RightExpression, false);
            case (IGenericBinaryExpression<Rational, Rational, T> p, IGenericBinaryExpression<Rational, Rational, T> e):
                return MatchPattern(p.LeftExpression, e.LeftExpression, false) &&
                       MatchPattern(p.RightExpression, e.RightExpression, false);
            case (IGenericBinaryExpression<Rational, Curve, T> p, IGenericBinaryExpression<Rational, Curve, T> e):
                return MatchPattern(p.LeftExpression, e.LeftExpression, false) &&
                       MatchPattern(p.RightExpression, e.RightExpression, false);
            case (IGenericBinaryExpression<Curve, Rational, T> p, IGenericBinaryExpression<Curve, Rational, T> e):
                return MatchPattern(p.LeftExpression, e.LeftExpression, false) &&
                       MatchPattern(p.RightExpression, e.RightExpression, false);
            case (CurveNAryExpression p, CurveNAryExpression e):
                return MatchPatternNAry(p, e, patternRoot);
            case (RationalNAryExpression p, RationalNAryExpression e):
                return MatchPatternNAry(p, e, patternRoot);
            case (RationalNumberExpression p, RationalNumberExpression e):
                return p.Value.Equals(e.Value) && p.Name.Equals(e.Name);
            default:
                throw new InvalidOperationException("Missing type " + pattern.GetType());
        }
    }

    private static bool MatchPatternNAry<T, TResult>(IGenericNAryExpression<T, TResult> pattern,
        IGenericNAryExpression<T, TResult> expression, bool patternRoot)
    {
        if (!patternRoot && pattern.Expressions.Count != expression.Expressions.Count) return false;
        if (patternRoot && pattern.Expressions.Count > expression.Expressions.Count) return false;
        var result = true;
        List<int> alreadyMatchedIndexes = [];
        var operands = expression.Expressions.ToArray();
        // For each operand o1 of pattern Expression
        foreach (var ePattern in pattern.Expressions)
        {
            var temp = false;
            // For each operand o2 of real expression
            for (int i = 0; i < operands.Length; i++)
            {
                if (alreadyMatchedIndexes.Contains(i)) continue;
                var operand = operands[i];
                temp = MatchPattern(ePattern, operand, false);
                if (temp) // If o2 matches o1 --> move to next operand o1 (and don't consider anymore o2)
                {
                    alreadyMatchedIndexes.Add(i);
                    break;
                }
            }

            result = result && temp;
            if (!result) return false;
        }

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
                        NotMatchedExpressionsCurve = list;
                        break;
                    case List<IGenericExpression<Rational>> list:
                        NotMatchedExpressionsRational = list;
                        break;
                }

                NaryTypePartialMatch = expression.GetType();
                NaryNamePartialMatch = expression.Name;
                NarySettingsPartialMatch = expression.Settings;
            }
        }

        return true;
    }

    internal static List<IGenericExpression<Curve>>? NotMatchedExpressionsCurve { get; set; }
    internal static List<IGenericExpression<Rational>>? NotMatchedExpressionsRational { get; set; }
    internal static Type? NaryTypePartialMatch { get; set; }
    internal static string? NaryNamePartialMatch { get; set; }
    internal static ExpressionSettings? NarySettingsPartialMatch { get; set; }

    internal static bool IgnoreNotMatchedExpressions = false;
    
    private static IGenericExpression<T> getNewExpressionToReplace<T>(IGenericExpression<T> newExpressionToReplace)
    {
        if (IgnoreNotMatchedExpressions) return newExpressionToReplace;
        IGenericExpression<T> ret;
        switch (newExpressionToReplace)
        {
            case IGenericExpression<Curve> e:
                if (NotMatchedExpressionsCurve == null)
                    return newExpressionToReplace;
                // The pattern matched only some operands of n-ary expression, thus we need to keep the non-matched ones
                if (newExpressionToReplace.GetType() != NaryTypePartialMatch)
                {
                    var operandsList = NotMatchedExpressionsCurve;
                    operandsList.Add(e);
                    ret = Activator.CreateInstance(NaryTypePartialMatch!,
                        [operandsList, NaryNamePartialMatch, NarySettingsPartialMatch]) as IGenericExpression<T> ?? throw new InvalidOperationException();
                }
                else
                {
                    var exprToReturn = newExpressionToReplace as CurveNAryExpression;
                    exprToReturn = NotMatchedExpressionsCurve.Aggregate(exprToReturn,
                        (current, operand) => (CurveNAryExpression)current!.Append(operand));

                    ret = (exprToReturn as IGenericExpression<T>)!;
                }

                NotMatchedExpressionsCurve = null;
                return ret;
            case IGenericExpression<Rational> e:
                if (NotMatchedExpressionsRational == null)
                    return newExpressionToReplace;
                // The pattern matched only some operands of n-ary expression, thus we need to keep the non-matched ones
                if (newExpressionToReplace.GetType() != NaryTypePartialMatch)
                {
                    var operandsList = NotMatchedExpressionsRational;
                    operandsList.Add(e);
                    ret = Activator.CreateInstance(NaryTypePartialMatch!,
                        [operandsList, NaryNamePartialMatch, NarySettingsPartialMatch]) as IGenericExpression<T> ?? throw new
                        InvalidOperationException();
                }
                else
                {
                    var exprToReturn = newExpressionToReplace as RationalNAryExpression;
                    exprToReturn = NotMatchedExpressionsRational.Aggregate(exprToReturn,
                        (current, operand) => (RationalNAryExpression)current!.Append(operand));

                    ret = (exprToReturn as IGenericExpression<T>)!;
                }
                
                NotMatchedExpressionsRational = null;
                return ret;
        }

        return newExpressionToReplace;
    }

    public IGenericExpression<T1> ReplaceByValue(IGenericExpression<T2> expressionPattern)
    {
        switch (originalExpression)
        {
            case IGenericExpression<Curve> e:
                if (ReplaceByValue(expressionPattern, e) == 1)
                    return (IGenericExpression<T1>)getNewExpressionToReplace(newExpressionToReplace);
                if (_tempCurveExpression != null)
                    return (IGenericExpression<T1>)_tempCurveExpression;
                break;
            case IGenericExpression<Rational> e:
                if (ReplaceByValue(expressionPattern, e) == 1)
                    return (IGenericExpression<T1>)getNewExpressionToReplace(newExpressionToReplace);
                if (_tempRationalExpression != null)
                    return (IGenericExpression<T1>)_tempRationalExpression;
                break;
        }

        return originalExpression;
    }

    // Return value: 0 = No Replacement | 1 = Replacement at the root | 2 = Replacement deeper in the expression
    private int ReplaceByValue<T>(IGenericExpression<T2> expressionPattern,
        IGenericExpression<T> expression) // Top-down match
    {
        bool match;
        switch (expressionPattern, expression, _equivalence)
        {
            case (IGenericExpression<Curve> p, IGenericExpression<Curve> e, null):
                match = MatchPattern(p, e, true);
                break;
            case (IGenericExpression<Rational> p, IGenericExpression<Rational> e, null):
                match = MatchPattern(p, e, true);
                break;
            case (IGenericExpression<Curve>, IGenericExpression<Curve> e, _):
                var result = _equivalence.Apply(e, _checkType);
                if (result != null)
                {
                    newExpressionToReplace = (IGenericExpression<T2>)result;
                    match = true;
                }
                else
                    match = false;

                break;
            default:
                match = false;
                break;
        }

        if (match)
            return 1;

        switch (expression)
        {
            case IGenericUnaryExpression<Curve, T> c:
                return ReplaceByValueUnaryExpression(expressionPattern, c);
            case IGenericUnaryExpression<Rational, T> c:
                return ReplaceByValueUnaryExpression(expressionPattern, c);
            case IGenericBinaryExpression<Curve, Curve, T> c:
                return ReplaceByValueBinaryExpression(expressionPattern, c);
            case IGenericBinaryExpression<Curve, Rational, T> c:
                return ReplaceByValueBinaryExpression(expressionPattern, c);
            case IGenericBinaryExpression<Rational, Curve, T> c:
                return ReplaceByValueBinaryExpression(expressionPattern, c);
            case IGenericBinaryExpression<Rational, Rational, T> c:
                return ReplaceByValueBinaryExpression(expressionPattern, c);
            case CurveNAryExpression c:
                List<CurveExpression> tempList = [];
                var matchInOperands = false;
                foreach (var e in c.Expressions)
                {
                    switch (ReplaceByValue(expressionPattern, e))
                    {
                        case 1:
                            matchInOperands = true;
                            tempList.Add((CurveExpression)getNewExpressionToReplace(newExpressionToReplace));
                            break;
                        case 2:
                            matchInOperands = true;
                            tempList.Add((CurveExpression)_tempCurveExpression!);
                            break;
                        default:
                            tempList.Add((CurveExpression)e);
                            break;
                    }
                }

                if (matchInOperands)
                {
                    _tempCurveExpression =
                        Activator.CreateInstance(c.GetType(),
                            [tempList, c.Name, c.Settings]) as IGenericExpression<Curve>;
                    return 2;
                }

                return 0;
            case RationalNAryExpression c:
                List<RationalExpression> rationalTempList = [];
                var rationalMatchInOperands = false;
                foreach (var e in c.Expressions)
                {
                    switch (ReplaceByValue(expressionPattern, e))
                    {
                        case 1:
                            rationalMatchInOperands = true;
                            rationalTempList.Add((RationalExpression)getNewExpressionToReplace(newExpressionToReplace));
                            break;
                        case 2:
                            rationalMatchInOperands = true;
                            rationalTempList.Add((RationalExpression)_tempRationalExpression!);
                            break;
                        default:
                            rationalTempList.Add((RationalExpression)e);
                            break;
                    }
                }

                if (rationalMatchInOperands)
                {
                    _tempRationalExpression =
                        Activator.CreateInstance(c.GetType(),
                            [rationalTempList, c.Name, c.Settings]) as IGenericExpression<Rational>;
                    return 2;
                }

                return 0;
            default:
                return 0; // No match
        }
    }

    private int ReplaceByValueUnaryExpression<TArg, T>(IGenericExpression<T2> expressionPattern,
        IGenericUnaryExpression<TArg, T> unaryExpression)
    {
        var result = ReplaceByValue(expressionPattern, unaryExpression.Expression);
        object? temp;
        if (typeof(TArg) == typeof(Curve))
            temp = _tempCurveExpression;
        else
            temp = _tempRationalExpression;
        switch (result)
        {
            case 1:
                if (typeof(T) == typeof(Curve))
                    _tempCurveExpression =
                        Activator.CreateInstance(unaryExpression.GetType(),
                            [
                                getNewExpressionToReplace(newExpressionToReplace), ((CurveExpression)unaryExpression).Name,
                                unaryExpression.Settings
                            ]) as
                            IGenericExpression<Curve>;
                else
                    _tempRationalExpression =
                        Activator.CreateInstance(unaryExpression.GetType(),
                            [
                                getNewExpressionToReplace(newExpressionToReplace), ((CurveExpression)unaryExpression).Name,
                                unaryExpression.Settings
                            ]) as
                            IGenericExpression<Rational>;

                return 2;
            case 2:
                if (typeof(T) == typeof(Curve))
                    _tempCurveExpression =
                        Activator.CreateInstance(unaryExpression.GetType(),
                                [temp, ((CurveExpression)unaryExpression).Name, unaryExpression.Settings]) as
                            IGenericExpression<Curve>;
                else
                    _tempRationalExpression =
                        Activator.CreateInstance(unaryExpression.GetType(),
                                [temp, ((CurveExpression)unaryExpression).Name, unaryExpression.Settings]) as
                            IGenericExpression<Rational>;

                return 2;
            default:
                return 0;
        }
    }

    private int ReplaceByValueBinaryExpression<TLeft, TRight, T>(IGenericExpression<T2> expressionPattern,
        IGenericBinaryExpression<TLeft, TRight, T> binaryExpression)
    {
        var resultL = ReplaceByValue(expressionPattern, binaryExpression.LeftExpression);
        object? tempL;
        if (typeof(TLeft) == typeof(Curve))
        {
            tempL = resultL switch
            {
                1 => (IGenericExpression<Curve>?)getNewExpressionToReplace(newExpressionToReplace),
                2 => _tempCurveExpression,
                _ => binaryExpression.LeftExpression as IGenericExpression<Curve>
            };
        }
        else
        {
            tempL = resultL switch
            {
                1 => (IGenericExpression<Rational>?)getNewExpressionToReplace(newExpressionToReplace),
                2 => _tempRationalExpression,
                _ => binaryExpression.LeftExpression as IGenericExpression<Rational>
            };
        }

        var resultR = ReplaceByValue(expressionPattern, binaryExpression.RightExpression);
        object? tempR;
        if (typeof(TRight) == typeof(Curve))
        {
            tempR = resultR switch
            {
                1 => (IGenericExpression<Curve>?)getNewExpressionToReplace(newExpressionToReplace),
                2 => _tempCurveExpression,
                _ => binaryExpression.RightExpression as IGenericExpression<Curve>
            };
        }
        else
        {
            tempR = resultR switch
            {
                1 => (IGenericExpression<Rational>?)getNewExpressionToReplace(newExpressionToReplace),
                2 => _tempRationalExpression,
                _ => binaryExpression.RightExpression as IGenericExpression<Rational>
            };
        }

        if (resultL == 0 && resultR == 0) return 0;
        if (typeof(T) == typeof(Curve))
            _tempCurveExpression = Activator.CreateInstance(binaryExpression.GetType(),
                    [tempL, tempR, ((CurveExpression)binaryExpression).Name, binaryExpression.Settings])
                as IGenericExpression<Curve>;
        else
            _tempRationalExpression = Activator.CreateInstance(binaryExpression.GetType(),
                    [tempL, tempR, ((CurveExpression)binaryExpression).Name, binaryExpression.Settings])
                as IGenericExpression<Rational>;
        return 2;
    }

    public IGenericExpression<T1> ReplaceByPosition(IEnumerable<string> expressionPosition)
    {
        var positionPath = expressionPosition.ToList();
        if (!ExpressionPosition.ValidateExpressionPosition(positionPath))
            throw new ArgumentException("Invalid position", nameof(expressionPosition));

        switch (originalExpression)
        {
            case IGenericExpression<Curve> e:
                if (ReplaceByPosition(positionPath.GetEnumerator(), e) == 1)
                    return (IGenericExpression<T1>)newExpressionToReplace;
                if (_tempCurveExpression != null)
                    return (IGenericExpression<T1>)_tempCurveExpression;
                break;
            case IGenericExpression<Rational> e:
                if (ReplaceByPosition(positionPath.GetEnumerator(), e) == 1)
                    return (IGenericExpression<T1>)newExpressionToReplace;
                if (_tempRationalExpression != null)
                    return (IGenericExpression<T1>)_tempRationalExpression;
                break;
        }

        return originalExpression;
    }

    // Return value: 1 = Replacement at root | 2 = Replacement deeper in the expression | Exception if position path wrong
    private int ReplaceByPosition<T>(IEnumerator<string> positionPath, IGenericExpression<T> expression)
    {
        if (!positionPath.MoveNext())
        {
            if (_equivalence == null)
                return 1;
            if (expression is not CurveExpression curveExpression) return -1;
            var result = _equivalence.Apply(curveExpression, _checkType);
            if (result == null) return -1;
            newExpressionToReplace = (IGenericExpression<T2>)result;
            return 1;
        }

        var current = positionPath.Current;
        switch (current)
        {
            case "Operand":
                return expression switch
                {
                    IGenericUnaryExpression<Curve, T> c => ReplaceByPositionUnaryExpression(positionPath, c),
                    IGenericUnaryExpression<Rational, T> c => ReplaceByPositionUnaryExpression(positionPath, c),
                    _ => throw new ArgumentException("Wrong position path!", nameof(positionPath))
                };
            case "LeftOperand":
                return expression switch
                {
                    IGenericBinaryExpression<Curve, Curve, T> c => ReplaceByPositionLeftExpression(positionPath, c),
                    IGenericBinaryExpression<Rational, Curve, T> c => ReplaceByPositionLeftExpression(positionPath, c),
                    IGenericBinaryExpression<Curve, Rational, T> c => ReplaceByPositionLeftExpression(positionPath, c),
                    IGenericBinaryExpression<Rational, Rational, T> c => ReplaceByPositionLeftExpression(positionPath,
                        c),
                    _ => throw new ArgumentException("Wrong position path!", nameof(positionPath))
                };
            case "RightOperand":
                return expression switch
                {
                    IGenericBinaryExpression<Curve, Curve, T> c => ReplaceByPositionRightExpression(positionPath, c),
                    IGenericBinaryExpression<Rational, Curve, T> c => ReplaceByPositionRightExpression(positionPath, c),
                    IGenericBinaryExpression<Curve, Rational, T> c => ReplaceByPositionRightExpression(positionPath, c),
                    IGenericBinaryExpression<Rational, Rational, T> c => ReplaceByPositionRightExpression(positionPath,
                        c),
                    _ => throw new ArgumentException("Wrong position path!", nameof(positionPath))
                };
            default:
                var number = int.Parse(current);
                switch (expression)
                {
                    case CurveNAryExpression c:
                        if (number >= c.Expressions.Count)
                            throw new ArgumentException("Wrong position path! Out of range for the number of operands!",
                                nameof(positionPath));
                        List<CurveExpression> tempList = [];
                        var i = 0;
                        foreach (var e in c.Expressions)
                        {
                            if (i == number)
                            {
                                switch (ReplaceByPosition(positionPath, e))
                                {
                                    case 1:
                                        tempList.Add((CurveExpression)newExpressionToReplace);
                                        break;
                                    case 2:
                                        tempList.Add((CurveExpression)_tempCurveExpression!);
                                        break;
                                }
                            }
                            else
                                tempList.Add((CurveExpression)e);

                            i++;
                        }

                        _tempCurveExpression =
                            Activator.CreateInstance(c.GetType(),
                                [tempList, c.Name, c.Settings]) as IGenericExpression<Curve>;
                        return 2;
                    case RationalNAryExpression c:
                        if (number >= c.Expressions.Count)
                            throw new ArgumentException("Wrong position path! Out of range for the number of operands!",
                                nameof(positionPath));
                        List<RationalExpression> rationalTempList = [];
                        var j = 0;
                        foreach (var e in c.Expressions)
                        {
                            if (j == number)
                            {
                                switch (ReplaceByPosition(positionPath, e))
                                {
                                    case 1:
                                        rationalTempList.Add((RationalExpression)newExpressionToReplace);
                                        break;
                                    case 2:
                                        rationalTempList.Add((RationalExpression)_tempRationalExpression!);
                                        break;
                                }
                            }
                            else
                                rationalTempList.Add((RationalExpression)e);

                            j++;
                        }

                        _tempRationalExpression =
                            Activator.CreateInstance(c.GetType(),
                                [rationalTempList, c.Name, c.Settings]) as IGenericExpression<Rational>;
                        return 2;
                    default:
                        throw new ArgumentException("Wrong position path!", nameof(positionPath));
                }
        }
    }

    private int ReplaceByPositionUnaryExpression<TArg, T>(IEnumerator<string> positionPath,
        IGenericUnaryExpression<TArg, T> unaryExpression)
    {
        var result = ReplaceByPosition(positionPath, unaryExpression.Expression);
        switch (result)
        {
            case 1 when typeof(T) == typeof(Curve):
                _tempCurveExpression =
                    Activator.CreateInstance(unaryExpression.GetType(),
                            [newExpressionToReplace, ((CurveExpression)unaryExpression).Name, unaryExpression.Settings])
                        as IGenericExpression<Curve>;
                break;
            case 1:
                _tempRationalExpression =
                    Activator.CreateInstance(unaryExpression.GetType(),
                            [newExpressionToReplace, ((CurveExpression)unaryExpression).Name, unaryExpression.Settings])
                        as IGenericExpression<Rational>;
                break;
            case 2 when typeof(T) == typeof(Curve):
                _tempCurveExpression = Activator.CreateInstance(unaryExpression.GetType(),
                        [_tempCurveExpression, ((CurveExpression)unaryExpression).Name, unaryExpression.Settings])
                    as IGenericExpression<Curve>;
                break;
            case 2:
                _tempRationalExpression =
                    Activator.CreateInstance(unaryExpression.GetType(),
                        [
                            _tempRationalExpression, ((CurveExpression)unaryExpression).Name, unaryExpression.Settings
                        ])
                        as IGenericExpression<Rational>;
                break;
            case -1:
                return -1;
        }

        return 2;
    }

    private int ReplaceByPositionLeftExpression<TLeft, TRight, T>(IEnumerator<string> positionPath,
        IGenericBinaryExpression<TLeft, TRight, T> binaryExpression)
    {
        var result = ReplaceByPosition(positionPath, binaryExpression.LeftExpression);
        switch (result)
        {
            case 1 when typeof(T) == typeof(Curve):
                _tempCurveExpression =
                    Activator.CreateInstance(binaryExpression.GetType(),
                        [
                            newExpressionToReplace, binaryExpression.RightExpression,
                            ((CurveExpression)binaryExpression).Name, binaryExpression.Settings
                        ])
                        as IGenericExpression<Curve>;
                break;
            case 1:
                _tempRationalExpression =
                    Activator.CreateInstance(binaryExpression.GetType(),
                        [
                            newExpressionToReplace, binaryExpression.RightExpression,
                            ((CurveExpression)binaryExpression).Name, binaryExpression.Settings
                        ])
                        as IGenericExpression<Rational>;
                break;
            case 2 when typeof(T) == typeof(Curve):
                _tempCurveExpression = Activator.CreateInstance(binaryExpression.GetType(),
                    [
                        _tempCurveExpression, binaryExpression.RightExpression,
                        ((CurveExpression)binaryExpression).Name, binaryExpression.Settings
                    ])
                    as IGenericExpression<Curve>;
                break;
            case 2:
                _tempRationalExpression =
                    Activator.CreateInstance(binaryExpression.GetType(),
                        [
                            _tempRationalExpression, binaryExpression.RightExpression,
                            ((CurveExpression)binaryExpression).Name, binaryExpression.Settings
                        ])
                        as IGenericExpression<Rational>;
                break;
            case -1:
                return -1;
        }

        return 2;
    }

    private int ReplaceByPositionRightExpression<TLeft, TRight, T>(IEnumerator<string> positionPath,
        IGenericBinaryExpression<TLeft, TRight, T> binaryExpression)
    {
        var result = ReplaceByPosition(positionPath, binaryExpression.RightExpression);
        if (result == 1)
        {
            if (typeof(T) == typeof(Curve))
                _tempCurveExpression =
                    Activator.CreateInstance(binaryExpression.GetType(),
                        [
                            binaryExpression.LeftExpression, newExpressionToReplace,
                            ((CurveExpression)binaryExpression).Name, binaryExpression.Settings
                        ])
                        as IGenericExpression<Curve>;
            else
                _tempRationalExpression =
                    Activator.CreateInstance(binaryExpression.GetType(),
                        [
                            binaryExpression.LeftExpression, newExpressionToReplace,
                            ((CurveExpression)binaryExpression).Name, binaryExpression.Settings
                        ])
                        as IGenericExpression<Rational>;
        }
        else if (result == 2)
        {
            if (typeof(T) == typeof(Curve))
                _tempCurveExpression = Activator.CreateInstance(binaryExpression.GetType(),
                    [
                        binaryExpression.LeftExpression, _tempCurveExpression, ((CurveExpression)binaryExpression).Name,
                        binaryExpression.Settings
                    ])
                    as IGenericExpression<Curve>;
            else
                _tempRationalExpression =
                    Activator.CreateInstance(binaryExpression.GetType(),
                        [
                            binaryExpression.LeftExpression, _tempRationalExpression,
                            ((CurveExpression)binaryExpression).Name, binaryExpression.Settings
                        ])
                        as IGenericExpression<Rational>;
        }
        else if (result == -1) return -1;

        return 2;
    }
}