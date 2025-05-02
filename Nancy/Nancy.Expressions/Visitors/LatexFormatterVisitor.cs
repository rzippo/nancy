using System.Text;
using System.Text.RegularExpressions;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

// todo: document notation used by the library

/// <summary>
/// Used for visiting an expression and create its representation using Latex code.
/// </summary>
public partial class LatexFormatterVisitor : 
    ICurveExpressionVisitor<(StringBuilder UnicodeBuilder, bool NeedsParentheses)>, 
    IRationalExpressionVisitor<(StringBuilder UnicodeBuilder, bool NeedsParentheses)>
{
    /// <summary>
    /// The current depth of visit.
    /// 0 means root node, and is incremented during each child visit.
    /// </summary>
    public int CurrentDepth { get; private set; }

    /// <summary>
    /// Max depth at which children should be fully expanded.
    /// After this depth is reached, any node that has a name is represented through that name, instead of being expanded further.
    /// </summary>
    /// <remarks>
    /// If set to 0, any node that has a name is not expanded.
    /// </remarks>
    public int MaxDepth { get; init; }

    public bool ShowRationalsAsName { get; init; }

    /// <summary>
    /// Used for visiting an expression and create its representation using Latex code.
    /// </summary>
    /// <param name="depth">The maximum level of the expression tree (starting from the root) which must be fully expanded
    /// in the representation (after this level, the expression name is used, if not empty)</param>
    /// <param name="showRationalsAsName">If true, rational numbers are not shown using their value, but using their name
    /// </param>
    public LatexFormatterVisitor(int depth = 20, bool showRationalsAsName = true)
    {
        MaxDepth = depth;
        CurrentDepth = 0;
        ShowRationalsAsName = showRationalsAsName;
    }

    /// <summary>
    /// List of greek letters to substitute the expanded letters with their correspondent Latex symbol
    /// </summary>
    private static readonly List<string> GreekLetters =
    [
        "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta", "theta", "iota", "kappa", "lambda", "mu", "nu",
        "xi", "omicron", "pi", "rho", "sigma", "tau", "upsilon", "phi", "chi", "psi", "omega"
    ];

    // todo: explore having generic entry points that take a PreferredOperatorNotation enum or something similar

    /// <summary>
    /// Returns true if the latex produced by the given expression contains, in right-most part, a subscript or superscript.
    /// Used, e.g., to force parentheses in post-fix unary operators that add their own subscript or superscript.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static bool ContainsSubscriptOrSuperscript<TResult>(IGenericExpression<TResult> expression)
    {
        // todo: improve with other cases
        if (expression is (
                LowerPseudoInverseExpression or 
                UpperPseudoInverseExpression or
                ToLowerNonDecreasingExpression or
                ToUpperNonDecreasingExpression or
                ToNonNegativeExpression or
                ToLeftContinuousExpression or
                ToRightContinuousExpression
        ))
            return true;
        else
            return false;
    }

    #region Default formatters

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) GeneralizedAccept<TExpressionResult>(
        IGenericExpression<TExpressionResult> expression
    )
    {
        if (expression is IGenericExpression<Curve> curveExpression)
            return curveExpression.Accept<(StringBuilder, bool)>(this);
        else if (expression is IGenericExpression<Rational> rationalExpression)
            return rationalExpression.Accept<(StringBuilder, bool)>(this);
        else
            throw new NotImplementedException();
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitUnaryCommand<T1, TResult>(
        IGenericUnaryExpression<T1, TResult> expression,
        string latexCommand
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (innerUnicode, _) = GeneralizedAccept(expression.Expression);

            sb.Append(latexCommand);
            sb.Append('{');
            sb.Append(innerUnicode);
            sb.Append('}');

            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitUnaryPrefix<T1, TResult>(
        IGenericUnaryExpression<T1, TResult> expression,
        string operation
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (innerUnicode, _) = GeneralizedAccept(expression.Expression);

            sb.Append(operation);
            sb.Append(@"\left( ");
            sb.Append(innerUnicode);
            sb.Append(@" \right)");

            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitUnaryPostfix<T1, TResult>(
        IGenericUnaryExpression<T1, TResult> expression,
        string operation,
        bool forceParentheses = true
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (innerUnicode, innerNeedsParentheses) = GeneralizedAccept(expression.Expression);
            var needsParentheses = innerNeedsParentheses || forceParentheses; 
            if (needsParentheses)
            {
                sb.Append(@"\left( ");
                sb.Append(innerUnicode);
                sb.Append(@" \right)");
                sb.Append(operation);
            }
            else
            {
                sb.Append(innerUnicode);
                sb.Append(operation);
            }
            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitUnaryPostfix<T1, TResult>(
        IGenericUnaryExpression<T1, TResult> expression,
        string operation,
        Func<IGenericExpression<T1>, bool> parenthesesDeterminator
    )
        => VisitUnaryPostfix(expression, operation, parenthesesDeterminator(expression.Expression));

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitBinaryCommand<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string latexCommand
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(latexCommand);
            sb.Append("{");
            var (leftLatex, _) = GeneralizedAccept(expression.LeftExpression);
            sb.Append(leftLatex);
            sb.Append("}{");
            var (rightLatex, _) = GeneralizedAccept(expression.RightExpression);
            sb.Append(rightLatex);
            sb.Append('}');
            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitBinaryInfix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string latexOperation
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();

            var (leftUnicodeBuilder, leftNeedsParentheses) = GeneralizedAccept(expression.LeftExpression);
            if (leftNeedsParentheses)
            {
                sb.Append(@"\left( ");
                sb.Append(leftUnicodeBuilder);
                sb.Append(@" \right)");
            }
            else
                sb.Append(leftUnicodeBuilder);

            sb.Append(latexOperation);

            var (rightUnicodeBuilder, rightNeedsParentheses) = GeneralizedAccept(expression.RightExpression);
            if (rightNeedsParentheses)
            {
                sb.Append(@"\left( ");
                sb.Append(rightUnicodeBuilder);
                sb.Append(@" \right)");
            }
            else
                sb.Append(rightUnicodeBuilder);
            CurrentDepth--;

            return (sb, true);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitBinaryPrefix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string latexOperation
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(latexOperation);
            sb.Append(@"\left( ");
            var (leftUnicode, _) = GeneralizedAccept(expression.LeftExpression);
            sb.Append(leftUnicode);
            sb.Append(", ");
            var (rightUnicode, _) = GeneralizedAccept(expression.RightExpression);
            sb.Append(rightUnicode);
            sb.Append(@" \right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitNAryInfix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression, 
        string latexOperation
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();

            var c = expression.Expressions.Count;
            foreach (var e in expression.Expressions)
            {
                var (unicode, needsParentheses) = GeneralizedAccept(e);
                if (needsParentheses)
                {
                    sb.Append(@"\left( ");
                    sb.Append(unicode);
                    sb.Append(@" \right)");
                }
                else
                    sb.Append(unicode);
                c--;
                if (c > 0)
                    sb.Append(latexOperation);
            }

            CurrentDepth--;
            return (sb, true);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitNAryPrefix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression, 
        string latexOperation
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(latexOperation);
            sb.Append(@"\left( ");
            var c = expression.Expressions.Count;
            foreach (var e in expression.Expressions)
            {
                var (unicode, _) = GeneralizedAccept(e);
                sb.Append(unicode);
                c--;
                if (c > 0)
                    sb.Append(", ");
            }
            sb.Append(@" \right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    /// <summary>
    /// Formats the name of an expression putting as subscript the ending digits and substituting a greek letter using
    /// the correspondent Latex command
    /// </summary>
    private StringBuilder FormatName(string name)
    {
        var match = OperandNameRegex().Match(name);
        string nameLetters;
        string? nameNumber = null;
        if (match.Success)
        {
            nameLetters = match.Groups[1].Value;
            nameNumber = match.Groups[2].Value;
        }
        else
            nameLetters = name;

        var nameLettersLower = nameLetters.ToLower();

        var sb = new StringBuilder();
        if (GreekLetters.Any(s => s.Equals(nameLettersLower)))
            sb.Append("\\" + nameLettersLower);
        else
            sb.Append(nameLetters);

        sb.Append(nameNumber != null ? "_{{" + nameNumber + "}" : "");

        sb.Append(nameNumber != null ? "}" : "");
        return sb;
    }

    #endregion Default formatters

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ConcreteCurveExpression expression)
        => (FormatName(expression.Name), false);

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalAdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalSubtractionExpression expression)
        => VisitBinaryInfix(expression, " - ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalProductExpression expression)
        => VisitNAryInfix(expression, " \\cdot ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalDivisionExpression expression)
        => VisitBinaryCommand(expression, "\\frac");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalLeastCommonMultipleExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{lcm}");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalGreatestCommonDivisorExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{gcd}");

    public (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalMinimumExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{min}");

    public (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalMaximumExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{max}");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalNumberExpression numberExpression)
    {
        if (!numberExpression.Name.Equals("") && (ShowRationalsAsName || CurrentDepth >= MaxDepth))
            return (FormatName(numberExpression.Name), false);
        else
        {
            var sb = new StringBuilder();
            if (numberExpression.Value.Denominator == 1)
                sb.Append($"{numberExpression.Value.Numerator}");
            else
                sb.Append($"\\frac{numberExpression.Value.Numerator}{numberExpression.Value.Denominator}");
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(NegateExpression expression)
        => VisitUnaryPrefix(expression, "-");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToNonNegativeExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var squareParenthesis = expression.Expression is not (ConcreteCurveExpression
                or ToUpperNonDecreasingExpression
                or ToLowerNonDecreasingExpression);
            if (squareParenthesis) sb.Append(@"\left[ ");
            var (latex, _) = expression.Expression.Accept<(StringBuilder, bool)>(this);
            sb.Append(latex);
            if (squareParenthesis) sb.Append(@" \right]");
            string resultToString = sb.ToString();
            // Usually ToNonNegative is used together with ToUpperNonDecreasing or ToLowerNonDecreasing
            // The following instructions are used to obtain the proper Latex formatting
            if (resultToString.EndsWith(@"_{\uparrow}") || resultToString.EndsWith(@"_{\downarrow}"))
            {
                sb.Remove(sb.Length - 1, 1);
                sb.Append("^{+}}");
            }
            else
                sb.Append("^{+}");

            CurrentDepth--;
            return (sb, false);
        }
    }

    // todo: review notation for subAdditive and superAdditive closures
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SubAdditiveClosureExpression expression)
        => VisitUnaryCommand(expression, @"\overline");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(
        SuperAdditiveClosureExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            var (temp, _) = VisitUnaryPrefix(expression, @"\overline{\overline");
            temp.Append('}');
            return (temp, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToUpperNonDecreasingExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            switch (expression.Expression)
            {
                case ConcreteCurveExpression concreteCurveOperand:
                {
                    var formattedName = FormatName(concreteCurveOperand.Name).ToString();
                    var needsSquareParentheses = formattedName.Contains('_');
                    if (needsSquareParentheses)
                    {
                        sb.Append(@"\left[ ");
                        sb.Append(formattedName);
                        sb.Append(@" \right]");
                    }
                    else
                        sb.Append(formattedName);
                    sb.Append(@"_{\uparrow}");
                    break;
                }

                // non-negative and non-decreasing closures are formatted together
                case ToNonNegativeExpression toNonNegativeOperand:
                {
                    var innerOperand = toNonNegativeOperand.Expression;
                    if (innerOperand is ConcreteCurveExpression concreteCurveOperand)
                    {
                        // no need for square parentheses if name is plain
                        var formattedName = FormatName(concreteCurveOperand.Name).ToString();
                        var needsSquareParentheses = formattedName.Contains('_');
                        if (needsSquareParentheses)
                        {
                            sb.Append(@"\left[ ");
                            sb.Append(formattedName);
                            sb.Append(@" \right]");
                        }
                        else
                            sb.Append(formattedName);
                    }
                    else
                    {
                        var (innerLatex, _) = innerOperand.Accept<(StringBuilder, bool)>(this);
                        sb.Append(@"\left[ ");
                        sb.Append(innerLatex);
                        sb.Append(@" \right]");
                    }
                    // add both non-negative and upper non-decreasing closures
                    sb.Append(@"^{+}_{\uparrow}");
                    break;
                }

                // curve subtractions may imply non-negative closure
                case SubtractionExpression subtractionOperand:
                {
                    var (innerLatex, _) = VisitBinaryInfix(subtractionOperand, " - ");
                    sb.Append(@"\left[ ");
                    sb.Append(innerLatex);
                    sb.Append(@" \right]");
                    if (subtractionOperand.NonNegative)
                        sb.Append(@"^{+}_{\uparrow}");
                    else
                        sb.Append(@"_{\uparrow}");
                    break;
                }

                default:
                {
                    var (innerLatex, _) = expression.Expression.Accept<(StringBuilder, bool)>(this);
                    sb.Append(@"\left[ ");
                    sb.Append(innerLatex);
                    sb.Append(@" \right]");
                    sb.Append(@"_{\uparrow}");
                    break;
                }
            }

            CurrentDepth--;
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToLowerNonDecreasingExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            switch (expression.Expression)
            {
                case ConcreteCurveExpression concreteCurveOperand:
                {
                    var formattedName = FormatName(concreteCurveOperand.Name).ToString();
                    var needsSquareParentheses = formattedName.Contains('_');
                    if (needsSquareParentheses)
                    {
                        sb.Append(@"\left[ ");
                        sb.Append(formattedName);
                        sb.Append(@" \right]");
                    }
                    else
                        sb.Append(formattedName);
                    sb.Append(@"_{\downarrow}");
                    break;
                }

                // non-negative and non-decreasing closures are formatted together
                case ToNonNegativeExpression toNonNegativeOperand:
                {
                    var innerOperand = toNonNegativeOperand.Expression;
                    if (innerOperand is ConcreteCurveExpression concreteCurveOperand)
                    {
                        // no need for square parentheses if name is plain
                        var formattedName = FormatName(concreteCurveOperand.Name).ToString();
                        var needsSquareParentheses = formattedName.Contains('_');
                        if (needsSquareParentheses)
                        {
                            sb.Append(@"\left[ ");
                            sb.Append(formattedName);
                            sb.Append(@" \right]");
                        }
                        else
                            sb.Append(formattedName);
                    }
                    else
                    {
                        var (innerLatex, _) = innerOperand.Accept<(StringBuilder, bool)>(this);
                        sb.Append(@"\left[ ");
                        sb.Append(innerLatex);
                        sb.Append(@" \right]");
                    }
                    // add both non-negative and upper non-decreasing closures
                    sb.Append(@"^{+}_{\downarrow}");
                    break;
                }

                // curve subtractions may imply non-negative closure
                case SubtractionExpression subtractionOperand:
                {
                    var (innerLatex, _) = VisitBinaryInfix(subtractionOperand, " - ");
                    sb.Append(@"\left[ ");
                    sb.Append(innerLatex);
                    sb.Append(@" \right]");
                    if (subtractionOperand.NonNegative)
                        sb.Append(@"^{+}_{\downarrow}");
                    else
                        sb.Append(@"_{\downarrow}");
                    break;
                }

                default:
                {
                    var (innerLatex, _) = expression.Expression.Accept<(StringBuilder, bool)>(this);
                    sb.Append(@"\left[ ");
                    sb.Append(innerLatex);
                    sb.Append(@" \right]");
                    sb.Append(@"_{\downarrow}");
                    break;
                }
            }

            CurrentDepth--;
            return (sb, false);
        }
    }

    /// <summary>
    /// Uses the notation from the PhD thesis of Damien Guidolin--Pina, where the operation is called "left projection".
    /// </summary>
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToLeftContinuousExpression expression)
        => VisitUnaryPostfix(expression, "_{l}",
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <summary>
    /// Uses the notation from the PhD thesis of Damien Guidolin--Pina, where the operation is called "right projection".
    /// </summary>
    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToRightContinuousExpression expression)
        => VisitUnaryPostfix(expression, "_{r}",
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(WithZeroOriginExpression expression)
        => VisitUnaryPostfix(expression, @"^{\circ}", true);

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(LowerPseudoInverseExpression expression)
        => VisitUnaryPostfix(expression, @"^{\underline{-1}}", 
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(UpperPseudoInverseExpression expression)
        => VisitUnaryPostfix(expression, @"^{\overline{-1}}", 
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(AdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SubtractionExpression expression)
    {
        if (expression.NonNegative)
        {
            var asNegative = expression with { NonNegative = false };
            var toNonNegative = new ToNonNegativeExpression(asNegative);
            return Visit(toNonNegative);
        }
        else
        {
            return VisitBinaryInfix(expression, " - ");
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(MinimumExpression expression)
        => VisitNAryInfix(expression, @" \wedge ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(MaximumExpression expression)
        => VisitNAryInfix(expression, @" \vee ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ConvolutionExpression expression)
        => VisitNAryInfix(expression, @" \otimes ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(DeconvolutionExpression expression)
        => VisitBinaryInfix(expression, @" \oslash ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(MaxPlusConvolutionExpression expression)
        => VisitNAryInfix(expression, @" \overline{\otimes} ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(MaxPlusDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, @" \overline{\oslash} ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(CompositionExpression expression)
        => VisitBinaryInfix(expression, @" \circ ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(DelayByExpression expression)
        => VisitBinaryPrefix(expression, " delayBy");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ForwardByExpression expression)
        => VisitBinaryPrefix(expression, " forwardBy");

    public (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ShiftExpression expression)
    {
        switch (expression.RightExpression)
        {
            case NegateRationalExpression negate:
            {
                var inner = negate.Expression;
                var substitute = new ShiftExpression(
                    (CurveExpression) expression.LeftExpression, 
                    (RationalExpression) inner);
                return VisitBinaryInfix(substitute, " - ");
            }

            case RationalNumberExpression rex when rex.Value.IsNegative:
            {
                var substitute = new ShiftExpression(
                    (CurveExpression) expression.LeftExpression, 
                    new RationalNumberExpression(-rex.Value));
                return VisitBinaryInfix(substitute, " - ");
            }

            default:
            {
                return VisitBinaryInfix(expression, " + ");
            }
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(NegateRationalExpression expression)
        => VisitUnaryPrefix(expression, "-");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(InvertRationalExpression expression)
        => VisitUnaryPostfix(expression, "^{-1}", expression.Expression is RationalNumberExpression);

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(HorizontalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "hdev");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(VerticalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "vdev");

    public (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ValueAtExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (curveUnicode, curveNeedsParentheses) = expression.LeftExpression.Accept<(StringBuilder, bool)>(this);
            if (curveNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(curveUnicode);
                sb.Append(@"\right)");
            }
            else
                sb.Append(curveUnicode);
            var (timeUnicode, _) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append(@"\left(");
            sb.Append(timeUnicode);
            sb.Append(@"\right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    public (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(LeftLimitAtExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (curveUnicode, curveNeedsParentheses) = expression.LeftExpression.Accept<(StringBuilder, bool)>(this);
            if (curveNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(curveUnicode);
                sb.Append(@"\right)");
            }
            else
                sb.Append(curveUnicode);
            var (timeUnicode, timeNeedsParentheses) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append(@"\left(");
            if (timeNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(timeUnicode);
                sb.Append("^-");
                sb.Append(@"\right)");
            }
            else
                sb.Append(timeUnicode);
            sb.Append(@"\right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    public (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RightLimitAtExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (curveUnicode, curveNeedsParentheses) = expression.LeftExpression.Accept<(StringBuilder, bool)>(this);
            if (curveNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(curveUnicode);
                sb.Append(@"\right)");
            }
            else
                sb.Append(curveUnicode);
            var (timeUnicode, timeNeedsParentheses) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append(@"\left(");
            if (timeNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(timeUnicode);
                sb.Append("^+");
                sb.Append(@"\right)");
            }
            else
                sb.Append(timeUnicode);
            sb.Append(@"\right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(CurvePlaceholderExpression expression)
        => (new StringBuilder(expression.Name), false);

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalPlaceholderExpression expression)
        => (new StringBuilder(expression.Name), false);

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ScaleExpression expression)
        => VisitBinaryInfix(expression, @" \cdot ");

    /// <summary>
    /// Regular expression to detect strings which terminate with digits
    /// </summary>
    [GeneratedRegex("^(.*?)(\\d+)$")]
    private static partial Regex OperandNameRegex();
}