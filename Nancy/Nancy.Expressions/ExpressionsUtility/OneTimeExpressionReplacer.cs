using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.ExpressionsUtility.Internals;

/// <summary>
/// Class which allows to manipulate DNC expressions.
/// It manages replacement by-value and by-position.
/// The features are also reused for applying equivalence by-value and by-position.
/// </summary>
/// <remarks>Each instance is safe to use only once.</remarks>
/// <typeparam name="TExpressionResult">Value type of <see cref="OriginalExpression"/> (Curve or Rational)</typeparam>
/// <typeparam name="TReplacedOperand">Value type of <see cref="NewExpressionToReplace"/> (Curve or Rational)</typeparam>
internal class OneTimeExpressionReplacer<TExpressionResult, TReplacedOperand>
{
    private IGenericExpression<Curve>? _tempCurveExpression;
    private IGenericExpression<Rational>? _tempRationalExpression;

    public bool AlreadyUsed { get; private set; } = false;
    
    public Equivalence? Equivalence { get; init; }
    public CheckType CheckType { get; init; }
    
    public IGenericExpression<TExpressionResult> OriginalExpression { get; init; }
    public IGenericExpression<TReplacedOperand> NewExpressionToReplace { get; private set; }

    public OneTimeExpressionReplacer(IGenericExpression<TExpressionResult> originalExpression,
        Equivalence equivalence, CheckType checkType = CheckType.CheckLeftOnly) : this(originalExpression,
        (IGenericExpression<TReplacedOperand>)Expressions.FromCurve(Curve.Zero()))
    {
        Equivalence = equivalence;
        CheckType = checkType;
    }

    /// <summary>
    /// Class which allows to manipulate DNC expressions. It manages replacement by-value and by-position. The features
    /// are also reused for applying equivalence by-value and by-position.
    /// </summary>
    /// <param name="originalExpression">The main DNC expression, a sub-expression of it needs to be replaced.</param>
    /// <param name="newExpressionToReplace">The new expression which must be "inserted" inside the main expression.</param>
    public OneTimeExpressionReplacer(
        IGenericExpression<TExpressionResult> originalExpression,
        IGenericExpression<TReplacedOperand> newExpressionToReplace)
    {
        OriginalExpression = originalExpression;
        NewExpressionToReplace = newExpressionToReplace;
    }

    /// <summary>
    /// Verifies if an expression is equivalent to another (called pattern) by comparing their structure
    /// </summary>
    /// <param name="pattern">The pattern expression</param>
    /// <param name="expression">The expression checked against the pattern</param>
    /// <param name="patternRoot">Indication of whether the match is at the root of the pattern or not</param>
    /// <returns>True if the expression matches the pattern, False otherwise</returns>
    public static MatchPatternResult MatchPattern<T>(
        IGenericExpression<T> pattern, 
        IGenericExpression<T> expression,
        bool patternRoot)
    {
        if (pattern.GetType() != expression.GetType()) 
            return new MatchPatternResult { IsMatch = false };

        switch (pattern, expression)
        {
            case (CurvePlaceholderExpression p, CurvePlaceholderExpression e):
                return new MatchPatternResult { IsMatch = p.Name.Equals(e.Name) };
            case (RationalPlaceholderExpression p, RationalPlaceholderExpression e):
                return new MatchPatternResult { IsMatch = p.Name.Equals(e.Name) };
            case (ConcreteCurveExpression p, ConcreteCurveExpression e):
                return new MatchPatternResult { IsMatch = p.Name.Equals(e.Name) && p.Value.Equivalent(e.Value) };
            case (IGenericUnaryExpression<Curve, T> p, IGenericUnaryExpression<Curve, T> e):
                return MatchPattern(p.Expression, e.Expression, false);
            case (IGenericUnaryExpression<Rational, T> p, IGenericUnaryExpression<Rational, T> e):
                return MatchPattern(p.Expression, e.Expression, false);
            case (IGenericBinaryExpression<Curve, Curve, T> p, IGenericBinaryExpression<Curve, Curve, T> e):
            {
                var leftMatch = MatchPattern(p.LeftExpression, e.LeftExpression, false);
                var rightMatch = MatchPattern(p.RightExpression, e.RightExpression, false);
                return new MatchPatternResult { IsMatch = leftMatch.IsMatch && rightMatch.IsMatch };
            }
            case (IGenericBinaryExpression<Rational, Rational, T> p, IGenericBinaryExpression<Rational, Rational, T> e):
            {
                var leftMatch = MatchPattern(p.LeftExpression, e.LeftExpression, false);
                var rightMatch = MatchPattern(p.RightExpression, e.RightExpression, false);
                return new MatchPatternResult { IsMatch = leftMatch.IsMatch && rightMatch.IsMatch };
            }
            case (IGenericBinaryExpression<Rational, Curve, T> p, IGenericBinaryExpression<Rational, Curve, T> e):
            {
                var leftMatch = MatchPattern(p.LeftExpression, e.LeftExpression, false);
                var rightMatch = MatchPattern(p.RightExpression, e.RightExpression, false);
                return new MatchPatternResult { IsMatch = leftMatch.IsMatch && rightMatch.IsMatch };
            }
            case (IGenericBinaryExpression<Curve, Rational, T> p, IGenericBinaryExpression<Curve, Rational, T> e):
            {
                var leftMatch = MatchPattern(p.LeftExpression, e.LeftExpression, false);
                var rightMatch = MatchPattern(p.RightExpression, e.RightExpression, false);
                return new MatchPatternResult { IsMatch = leftMatch.IsMatch && rightMatch.IsMatch };
            }
            case (CurveNAryExpression p, CurveNAryExpression e):
                return MatchPatternNAry(p, e, patternRoot);
            case (RationalNAryExpression p, RationalNAryExpression e):
                return MatchPatternNAry(p, e, patternRoot);
            case (RationalNumberExpression p, RationalNumberExpression e):
                return new MatchPatternResult { IsMatch = p.Value.Equals(e.Value) && p.Name.Equals(e.Name) };
            default:
                throw new InvalidOperationException("Missing type " + pattern.GetType());
        }
    }

    private static MatchPatternNAryResult MatchPatternNAry<T, TResult>(
        IGenericNAryExpression<T, TResult> pattern,
        IGenericNAryExpression<T, TResult> expression, 
        bool patternRoot
    )
    {
        var result = new MatchPatternNAryResult();
        
        if (!patternRoot && pattern.Expressions.Count != expression.Expressions.Count) 
            return result with { IsMatch = false };
        if (patternRoot && pattern.Expressions.Count > expression.Expressions.Count) 
            return result with { IsMatch = false };
        
        result.IsMatch = true;
        
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
                temp = MatchPattern(ePattern, operand, false).IsMatch;
                if (temp) // If o2 matches o1 --> move to next operand o1 (and don't consider anymore o2)
                {
                    alreadyMatchedIndexes.Add(i);
                    break;
                }
            }

            result.IsMatch &= temp;
            if (!result.IsMatch) 
                return result;
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
    
    private static IGenericExpression<T> getNewExpressionToReplace<T>(
        IGenericExpression<T> newExpressionToReplace,
        MatchPatternResult matchResult,
        bool ignoreNotMatchedExpressions = false
    )
    {
        if (ignoreNotMatchedExpressions) 
            return newExpressionToReplace;
        if (matchResult is not MatchPatternNAryResult matchNAryResult)
            return newExpressionToReplace;
        IGenericExpression<T> ret;
        switch (newExpressionToReplace)
        {
            case IGenericExpression<Curve> e:
                if (matchNAryResult.NotMatchedExpressionsCurve.Count == 0)
                    return newExpressionToReplace;
                // The pattern matched only some operands of n-ary expression, thus we need to keep the non-matched ones
                if (newExpressionToReplace.GetType() != matchNAryResult.NaryTypePartialMatch)
                {
                    var operandsList = matchNAryResult.NotMatchedExpressionsCurve;
                    operandsList.Add(e);
                    ret = Activator.CreateInstance(matchNAryResult.NaryTypePartialMatch!,
                        [operandsList, matchNAryResult.NaryNamePartialMatch, matchNAryResult.NarySettingsPartialMatch]) as IGenericExpression<T> ?? throw new InvalidOperationException();
                }
                else
                {
                    var exprToReturn = newExpressionToReplace as CurveNAryExpression;
                    exprToReturn = matchNAryResult.NotMatchedExpressionsCurve.Aggregate(exprToReturn,
                        (current, operand) => (CurveNAryExpression)current!.Append(operand));

                    ret = (exprToReturn as IGenericExpression<T>)!;
                }

                matchNAryResult.NotMatchedExpressionsCurve = [];
                return ret;
            case IGenericExpression<Rational> e:
                if (matchNAryResult.NotMatchedExpressionsRational.Count == 0)
                    return newExpressionToReplace;
                // The pattern matched only some operands of n-ary expression, thus we need to keep the non-matched ones
                if (newExpressionToReplace.GetType() != matchNAryResult.NaryTypePartialMatch)
                {
                    var operandsList = matchNAryResult.NotMatchedExpressionsRational;
                    operandsList.Add(e);
                    ret = Activator.CreateInstance(matchNAryResult.NaryTypePartialMatch!,
                        [operandsList, matchNAryResult.NaryNamePartialMatch, matchNAryResult.NarySettingsPartialMatch]) as IGenericExpression<T> ?? throw new
                        InvalidOperationException();
                }
                else
                {
                    var exprToReturn = newExpressionToReplace as RationalNAryExpression;
                    exprToReturn = matchNAryResult.NotMatchedExpressionsRational.Aggregate(exprToReturn,
                        (current, operand) => (RationalNAryExpression)current!.Append(operand));

                    ret = (exprToReturn as IGenericExpression<T>)!;
                }
                
                matchNAryResult.NotMatchedExpressionsRational = [];
                return ret;
        }

        return newExpressionToReplace;
    }

    public IGenericExpression<TExpressionResult> ReplaceByValue(
        IGenericExpression<TReplacedOperand> expressionPattern,
        bool ignoreNotMatchedExpressions = false
    )
    {
        if(AlreadyUsed)
            throw new InvalidOperationException("This replacer was already used.");
        AlreadyUsed = true;
        
        switch (OriginalExpression)
        {
            case IGenericExpression<Curve> e:
            {
                var replaceResult = ReplaceByValue(expressionPattern, e);
                if (replaceResult.Code == 1)
                {
                    var matchResult = replaceResult.MatchPatternResult;
                    return (IGenericExpression<TExpressionResult>)getNewExpressionToReplace(
                        NewExpressionToReplace, 
                        matchResult, 
                        ignoreNotMatchedExpressions
                    );
                }
                if (_tempCurveExpression != null)
                    return (IGenericExpression<TExpressionResult>)_tempCurveExpression;
                break;
            }
            case IGenericExpression<Rational> e:
            {
                var replaceResult = ReplaceByValue(expressionPattern, e);
                if (replaceResult.Code == 1)
                {
                    var matchResult = replaceResult.MatchPatternResult;
                    return (IGenericExpression<TExpressionResult>)getNewExpressionToReplace(
                        NewExpressionToReplace, 
                        matchResult, 
                        ignoreNotMatchedExpressions
                    );
                }
                if (_tempRationalExpression != null)
                    return (IGenericExpression<TExpressionResult>)_tempRationalExpression;
                break;
            }
        }

        return OriginalExpression;
    }

    private ReplaceResult ReplaceByValue<T>(
        IGenericExpression<TReplacedOperand> expressionPattern,
        IGenericExpression<T> expression) // Top-down match
    {
        var replaceResult = new ReplaceResult();
        switch (expressionPattern, expression, _equivalence: Equivalence)
        {
            case (IGenericExpression<Curve> p, IGenericExpression<Curve> e, null):
            {
                replaceResult.MatchPatternResult = MatchPattern(p, e, true);
                break;
            }
            case (IGenericExpression<Rational> p, IGenericExpression<Rational> e, null):
            {
                replaceResult.MatchPatternResult = MatchPattern(p, e, true);
                break;
            }
            case (IGenericExpression<Curve>, IGenericExpression<Curve> e, _):
            {
                var equivalenceApplyResult = Equivalence.Apply(e, CheckType);
                if (equivalenceApplyResult.NewExpression != null)
                {
                    NewExpressionToReplace = (IGenericExpression<TReplacedOperand>)equivalenceApplyResult.NewExpression;
                    replaceResult.MatchPatternResult = equivalenceApplyResult.MatchPatternResult;
                }
                else
                    replaceResult.MatchPatternResult.IsMatch = false;

                break;
            }
            default:
            {
                replaceResult.MatchPatternResult.IsMatch = false;
                break;
            }
        }

        if (replaceResult.MatchPatternResult.IsMatch)
        {
            replaceResult.Code = 1;
            return replaceResult;
        }

        switch (expression)
        {
            case IGenericUnaryExpression<Curve, T> c:
            {
                replaceResult.Code = ReplaceByValueUnaryExpression(expressionPattern, c).Code;
                return replaceResult;
            }
            case IGenericUnaryExpression<Rational, T> c:
            {
                replaceResult.Code = ReplaceByValueUnaryExpression(expressionPattern, c).Code;
                return replaceResult;
            }
            case IGenericBinaryExpression<Curve, Curve, T> c:
            {
                replaceResult.Code = ReplaceByValueBinaryExpression(expressionPattern, c).Code;
                return replaceResult;
            }
            case IGenericBinaryExpression<Curve, Rational, T> c:
            {
                replaceResult.Code = ReplaceByValueBinaryExpression(expressionPattern, c).Code;
                return replaceResult;
            }
            case IGenericBinaryExpression<Rational, Curve, T> c:
            {
                replaceResult.Code = ReplaceByValueBinaryExpression(expressionPattern, c).Code;
                return replaceResult;
            }
            case IGenericBinaryExpression<Rational, Rational, T> c:
            {
                replaceResult.Code = ReplaceByValueBinaryExpression(expressionPattern, c).Code;
                return replaceResult;
            }
            case CurveNAryExpression c:
            {
                List<CurveExpression> tempList = [];
                var matchInOperands = false;
                foreach (var e in c.Expressions)
                {
                    var innerReplaceResult = ReplaceByValue(expressionPattern, e); 
                    switch (innerReplaceResult.Code)
                    {
                        case 1:
                        {
                            matchInOperands = true;
                            var innerMatchResult = innerReplaceResult.MatchPatternResult;
                            tempList.Add((CurveExpression)getNewExpressionToReplace(NewExpressionToReplace, innerMatchResult));
                            break;
                        }
                        case 2:
                        {
                            matchInOperands = true;
                            tempList.Add((CurveExpression)_tempCurveExpression!);
                            break;
                        }
                        default:
                        {
                            tempList.Add((CurveExpression)e);
                            break;
                        }
                    }
                }

                if (matchInOperands)
                {
                    _tempCurveExpression =
                        Activator.CreateInstance(c.GetType(),
                            [tempList, c.Name, c.Settings]) as IGenericExpression<Curve>;
                    replaceResult.Code = 2;
                    return replaceResult;
                }
                else
                {
                    replaceResult.Code = 0;
                    return replaceResult;
                }
            }
            case RationalNAryExpression c:
            {
                List<RationalExpression> rationalTempList = [];
                var rationalMatchInOperands = false;
                foreach (var e in c.Expressions)
                {
                    var innerReplaceResult = ReplaceByValue(expressionPattern, e);
                    switch (innerReplaceResult.Code)
                    {
                        case 1:
                        {
                            var innerMatchResult = (MatchPatternNAryResult) innerReplaceResult.MatchPatternResult;
                            rationalMatchInOperands = true;
                            rationalTempList.Add((RationalExpression)getNewExpressionToReplace(NewExpressionToReplace, innerMatchResult));
                            break;
                        }
                        case 2:
                        {
                            rationalMatchInOperands = true;
                            rationalTempList.Add((RationalExpression)_tempRationalExpression!);
                            break;
                        }
                        default:
                        {
                            rationalTempList.Add((RationalExpression)e);
                            break;
                        }
                    }
                }

                if (rationalMatchInOperands)
                {
                    _tempRationalExpression =
                        Activator.CreateInstance(c.GetType(),
                            [rationalTempList, c.Name, c.Settings]) as IGenericExpression<Rational>;
                    replaceResult.Code = 2;
                    return replaceResult;
                }
                else
                {
                    replaceResult.Code = 0;
                    return replaceResult;
                }
            }
            default:
            {
                // No match
                replaceResult.Code = 0;
                return replaceResult; 
            }
        }
    }

    private ReplaceResult ReplaceByValueUnaryExpression<TArg, T>(
        IGenericExpression<TReplacedOperand> expressionPattern,
        IGenericUnaryExpression<TArg, T> unaryExpression)
    {
        var result = new ReplaceResult();
        var innerReplaceResult = ReplaceByValue(expressionPattern, unaryExpression.Expression);
        object? temp;
        if (typeof(TArg) == typeof(Curve))
            temp = _tempCurveExpression;
        else
            temp = _tempRationalExpression;
        switch (innerReplaceResult.Code)
        {
            case 1:
            {
                var innerMatchResult = innerReplaceResult.MatchPatternResult;
                if (typeof(T) == typeof(Curve))
                    _tempCurveExpression =
                        Activator.CreateInstance(unaryExpression.GetType(),
                            [
                                getNewExpressionToReplace(NewExpressionToReplace, innerMatchResult),
                                ((CurveExpression)unaryExpression).Name,
                                unaryExpression.Settings
                            ]) as
                            IGenericExpression<Curve>;
                else
                    _tempRationalExpression =
                        Activator.CreateInstance(unaryExpression.GetType(),
                            [
                                getNewExpressionToReplace(NewExpressionToReplace, innerMatchResult),
                                ((CurveExpression)unaryExpression).Name,
                                unaryExpression.Settings
                            ]) as
                            IGenericExpression<Rational>;

                result.Code = 2; 
                return result;
            }
            case 2:
            {
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

                result.Code = 2; 
                return result;
            }
            default:
            {
                result.Code = 0; 
                return result;
            }
        }
    }

    private ReplaceResult ReplaceByValueBinaryExpression<TLeft, TRight, T>(IGenericExpression<TReplacedOperand> expressionPattern,
        IGenericBinaryExpression<TLeft, TRight, T> binaryExpression)
    {
        var result = new ReplaceResult();
        var innerResultLeft = ReplaceByValue(expressionPattern, binaryExpression.LeftExpression);
        object? tempL;
        if (typeof(TLeft) == typeof(Curve))
        {
            tempL = innerResultLeft.Code switch
            {
                1 => (IGenericExpression<Curve>?)getNewExpressionToReplace(
                    NewExpressionToReplace, 
                    innerResultLeft.MatchPatternResult),
                2 => _tempCurveExpression,
                _ => binaryExpression.LeftExpression as IGenericExpression<Curve>
            };
        }
        else
        {
            tempL = innerResultLeft.Code switch
            {
                1 => (IGenericExpression<Rational>?)getNewExpressionToReplace(
                    NewExpressionToReplace,
                    innerResultLeft.MatchPatternResult),
                2 => _tempRationalExpression,
                _ => binaryExpression.LeftExpression as IGenericExpression<Rational>
            };
        }

        var innerResultRight = ReplaceByValue(expressionPattern, binaryExpression.RightExpression);
        object? tempR;
        if (typeof(TRight) == typeof(Curve))
        {
            tempR = innerResultRight.Code switch
            {
                1 => (IGenericExpression<Curve>?)getNewExpressionToReplace(
                    NewExpressionToReplace,
                    innerResultLeft.MatchPatternResult),
                2 => _tempCurveExpression,
                _ => binaryExpression.RightExpression as IGenericExpression<Curve>
            };
        }
        else
        {
            tempR = innerResultRight.Code switch
            {
                1 => (IGenericExpression<Rational>?)getNewExpressionToReplace(
                    NewExpressionToReplace,
                    innerResultLeft.MatchPatternResult),
                2 => _tempRationalExpression,
                _ => binaryExpression.RightExpression as IGenericExpression<Rational>
            };
        }

        if (innerResultLeft.Code == 0 && innerResultRight.Code == 0)
        {
            result.Code = 0; 
            return result;
        }

        if (typeof(T) == typeof(Curve))
        {
            if (binaryExpression is SubtractionExpression se)
            {
                _tempCurveExpression = Activator.CreateInstance(typeof(SubtractionExpression),
                        [tempL, tempR, se.NonNegative, se.Name, se.Settings])
                    as IGenericExpression<Curve>;
            }
            else
            {
                _tempCurveExpression = Activator.CreateInstance(binaryExpression.GetType(),
                        [tempL, tempR, ((CurveExpression)binaryExpression).Name, binaryExpression.Settings])
                    as IGenericExpression<Curve>;    
            }
        }
        else
            _tempRationalExpression = Activator.CreateInstance(binaryExpression.GetType(),
                    [tempL, tempR, ((CurveExpression)binaryExpression).Name, binaryExpression.Settings])
                as IGenericExpression<Rational>;
        
        result.Code = 2; 
        return result;
    }

    public IGenericExpression<TExpressionResult> ReplaceByPosition(
        IEnumerable<string> expressionPosition)
    {
        if(AlreadyUsed)
            throw new InvalidOperationException("This replacer was already used.");
        AlreadyUsed = true;
        
        var positionPath = expressionPosition.ToList();
        if (!ExpressionPosition.ValidateExpressionPosition(positionPath))
            throw new ArgumentException("Invalid position", nameof(expressionPosition));

        switch (OriginalExpression)
        {
            case IGenericExpression<Curve> e:
                if (ReplaceByPosition(positionPath.GetEnumerator(), e) == 1)
                    // replacement was at root, so the replacing expression is returned as it is
                    return (IGenericExpression<TExpressionResult>)NewExpressionToReplace;
                if (_tempCurveExpression != null)
                    // replacement was deeper than root
                    return (IGenericExpression<TExpressionResult>)_tempCurveExpression;
                break;
            case IGenericExpression<Rational> e:
                if (ReplaceByPosition(positionPath.GetEnumerator(), e) == 1)
                    // replacement was at root, so the replacing expression is returned as it is
                    return (IGenericExpression<TExpressionResult>)NewExpressionToReplace;
                if (_tempRationalExpression != null)
                    // replacement was deeper than root
                    return (IGenericExpression<TExpressionResult>)_tempRationalExpression;
                break;
        }

        return OriginalExpression;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="positionPath"></param>
    /// <param name="expression"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>
    /// Returns
    /// <list type="bullet">
    /// <item>1, if replacement happens at root</item>
    /// <item>2, if replacement happens deeper in the expression</item>
    /// <item>-1, if replacement did not happen</item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentException">If path is invalid</exception>
    private int ReplaceByPosition<T>(IEnumerator<string> positionPath, IGenericExpression<T> expression)
    {
        if (!positionPath.MoveNext())
        {
            if (Equivalence == null)
                return 1;
            if (expression is not CurveExpression curveExpression) return -1;
            var result = Equivalence.Apply(curveExpression, CheckType);
            if (result == null) return -1;
            NewExpressionToReplace = (IGenericExpression<TReplacedOperand>)result;
            return 1;
        }

        var current = positionPath.Current;
        switch (current)
        {
            case Positions.InnerOperand:
                return expression switch
                {
                    IGenericUnaryExpression<Curve, T> c => ReplaceByPositionUnaryExpression(positionPath, c),
                    IGenericUnaryExpression<Rational, T> c => ReplaceByPositionUnaryExpression(positionPath, c),
                    _ => throw new ArgumentException("Wrong position path!", nameof(positionPath))
                };
            case Positions.LeftOperand:
                return expression switch
                {
                    IGenericBinaryExpression<Curve, Curve, T> c => ReplaceByPositionLeftExpression(positionPath, c),
                    IGenericBinaryExpression<Rational, Curve, T> c => ReplaceByPositionLeftExpression(positionPath, c),
                    IGenericBinaryExpression<Curve, Rational, T> c => ReplaceByPositionLeftExpression(positionPath, c),
                    IGenericBinaryExpression<Rational, Rational, T> c => ReplaceByPositionLeftExpression(positionPath,
                        c),
                    _ => throw new ArgumentException("Wrong position path!", nameof(positionPath))
                };
            case Positions.RightOperand:
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
                                        tempList.Add((CurveExpression)NewExpressionToReplace);
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
                                        rationalTempList.Add((RationalExpression)NewExpressionToReplace);
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

    private int ReplaceByPositionUnaryExpression<TArg, T>(
        IEnumerator<string> positionPath,
        IGenericUnaryExpression<TArg, T> unaryExpression)
    {
        var result = ReplaceByPosition(positionPath, unaryExpression.Expression);
        switch (result)
        {
            case 1 when typeof(T) == typeof(Curve):
                _tempCurveExpression =
                    Activator.CreateInstance(unaryExpression.GetType(),
                            [NewExpressionToReplace, ((CurveExpression)unaryExpression).Name, unaryExpression.Settings])
                        as IGenericExpression<Curve>;
                break;
            case 1:
                _tempRationalExpression =
                    Activator.CreateInstance(unaryExpression.GetType(),
                            [NewExpressionToReplace, ((CurveExpression)unaryExpression).Name, unaryExpression.Settings])
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="positionPath"></param>
    /// <param name="binaryExpression"></param>
    /// <typeparam name="TLeft">Type of the left operand</typeparam>
    /// <typeparam name="TRight">Type of the right operand</typeparam>
    /// <typeparam name="TResult">Type of the result, and therefore of the expression</typeparam>
    /// <returns>
    /// Returns
    /// <list type="bullet">
    /// <item>1, if replacement happens at root</item>
    /// <item>2, if replacement happens deeper in the expression</item>
    /// <item>-1, if replacement did not happen</item>
    /// </list>
    /// </returns>
    private int ReplaceByPositionLeftExpression<TLeft, TRight, TResult>(
        IEnumerator<string> positionPath,
        IGenericBinaryExpression<TLeft, TRight, TResult> binaryExpression
    )
    {
        var result = ReplaceByPosition(positionPath, binaryExpression.LeftExpression);
        switch (result)
        {
            case 1 when typeof(TResult) == typeof(Curve):
            {
                var curveBinaryExpression = (CurveBinaryExpression<TReplacedOperand, TRight>)binaryExpression;
                _tempCurveExpression = curveBinaryExpression with
                {
                    LeftExpression = NewExpressionToReplace
                };
                break;
            }
            case 1 when typeof(TResult) == typeof(Rational):
            {
                var rationalBinaryExpression = (RationalBinaryExpression<TReplacedOperand, TRight>)binaryExpression;
                _tempRationalExpression = rationalBinaryExpression with
                {
                    LeftExpression = NewExpressionToReplace
                };
                break;
            }
            case 2 when typeof(TResult) == typeof(Curve):
            {
                var curveBinaryExpression = (CurveBinaryExpression<TReplacedOperand, TRight>)binaryExpression;
                _tempCurveExpression = curveBinaryExpression with
                {
                    LeftExpression = (typeof(TReplacedOperand) == typeof(Curve)) ?
                        (IGenericExpression<TReplacedOperand>) _tempCurveExpression! :
                        (IGenericExpression<TReplacedOperand>) _tempRationalExpression!
                };
                break;
            }
            case 2:
            {
                var rationalBinaryExpression = (RationalBinaryExpression<TReplacedOperand, TRight>)binaryExpression;
                _tempRationalExpression = rationalBinaryExpression with
                {
                    LeftExpression = (typeof(TReplacedOperand) == typeof(Curve)) ? 
                        (IGenericExpression<TReplacedOperand>) _tempCurveExpression! :
                        (IGenericExpression<TReplacedOperand>) _tempRationalExpression!
                };
                break;
            }
            default:
                return -1;
        }

        return 2;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="positionPath"></param>
    /// <param name="binaryExpression"></param>
    /// <typeparam name="TLeft">Type of the left operand</typeparam>
    /// <typeparam name="TRight">Type of the right operand</typeparam>
    /// <typeparam name="TResult">Type of the result, and therefore of the expression</typeparam>
    /// <returns>
    /// Returns
    /// <list type="bullet">
    /// <item>1, if replacement happens at root</item>
    /// <item>2, if replacement happens deeper in the expression</item>
    /// <item>-1, if replacement did not happen</item>
    /// </list>
    /// </returns>
    private int ReplaceByPositionRightExpression<TLeft, TRight, TResult>(
        IEnumerator<string> positionPath,
        IGenericBinaryExpression<TLeft, TRight, TResult> binaryExpression)
    {
        var result = ReplaceByPosition(positionPath, binaryExpression.RightExpression);
        switch (result)
        {
            case 1 when typeof(TResult) == typeof(Curve):
            {
                var curveBinaryExpression = (CurveBinaryExpression<TLeft, TReplacedOperand>)binaryExpression;
                _tempCurveExpression = curveBinaryExpression with
                {
                    RightExpression = NewExpressionToReplace
                };
                break;
            }
            case 1 when typeof(TResult) == typeof(Rational):
            {
                var rationalBinaryExpression = (RationalBinaryExpression<TLeft, TReplacedOperand>)binaryExpression;
                _tempRationalExpression = rationalBinaryExpression with
                {
                    RightExpression = NewExpressionToReplace
                };
                break;
            }
            case 2 when typeof(TResult) == typeof(Curve):
            {
                var curveBinaryExpression = (CurveBinaryExpression<TLeft, TReplacedOperand>)binaryExpression;
                _tempCurveExpression = curveBinaryExpression with
                {
                    RightExpression = (typeof(TReplacedOperand) == typeof(Curve)) ?
                        (IGenericExpression<TReplacedOperand>) _tempCurveExpression! :
                        (IGenericExpression<TReplacedOperand>) _tempRationalExpression!
                };
                break;
            }
            case 2 when typeof(TResult) == typeof(Rational):
            {
                var rationalBinaryExpression = (RationalBinaryExpression<TLeft, TReplacedOperand>)binaryExpression;
                _tempRationalExpression = rationalBinaryExpression with
                {
                    RightExpression = (typeof(TReplacedOperand) == typeof(Curve)) ?
                        (IGenericExpression<TReplacedOperand>) _tempCurveExpression! :
                        (IGenericExpression<TReplacedOperand>) _tempRationalExpression!
                };
                break;
            }
            default:
                return -1;
        }

        return 2;
    }
}


/// <summary>
/// to document 
/// </summary>
public record MatchPatternResult
{
    /// <summary>
    /// True if the match was successful.
    /// </summary>
    public bool IsMatch { get; set; } = false;
}


/// <summary>
/// to document 
/// </summary>
public record MatchPatternNAryResult : MatchPatternResult
{
    /// <summary>
    /// to document 
    /// </summary>
    public List<IGenericExpression<Curve>> NotMatchedExpressionsCurve { get; set; } = [];

    /// <summary>
    /// to document 
    /// </summary>
    public List<IGenericExpression<Rational>> NotMatchedExpressionsRational { get; set; } = [];

    /// <summary>
    /// to document 
    /// </summary>
    public Type? NaryTypePartialMatch { get; set; } = null;

    /// <summary>
    /// to document 
    /// </summary>
    public string? NaryNamePartialMatch { get; set; } = null;

    /// <summary>
    /// to document 
    /// </summary>
    public ExpressionSettings? NarySettingsPartialMatch { get; set; } = null;
}

/// <summary>
/// to document 
/// </summary>
public record ReplaceResult
{
    /// <summary>
    /// Result of the replacement.
    /// <list type="bullet">
    /// <item>0 = No Replacement</item>
    /// <item>1 = Replacement at the root</item>
    /// <item>2 = Replacement deeper in the expression</item>
    /// </list>
    /// </summary>
    public int Code { get; set; } = 0;

    /// <summary>
    /// to document 
    /// </summary>
    public MatchPatternResult MatchPatternResult { get; set; } = new MatchPatternResult();
}