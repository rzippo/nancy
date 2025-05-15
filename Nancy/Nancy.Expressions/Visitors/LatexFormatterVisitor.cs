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
    ICurveExpressionVisitor<(StringBuilder LatexBuilder, bool NeedsParentheses)>, 
    IRationalExpressionVisitor<(StringBuilder LatexBuilder, bool NeedsParentheses)>
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

    /// <summary>
    /// If true, rational numbers are not shown using their value, but using their name.
    /// </summary>
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

    private (StringBuilder LatexBuilder, bool NeedsParentheses) GeneralizedAccept<TExpressionResult>(
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

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitUnaryCommand<T1, TResult>(
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
            var (innerLatex, _) = GeneralizedAccept(expression.Expression);

            sb.Append(latexCommand);
            sb.Append('{');
            sb.Append(innerLatex);
            sb.Append('}');

            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitUnaryPrefix<T1, TResult>(
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
            var (innerLatex, _) = GeneralizedAccept(expression.Expression);

            sb.Append(operation);
            sb.Append(@"\left( ");
            sb.Append(innerLatex);
            sb.Append(@" \right)");

            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitUnaryPostfix<T1, TResult>(
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
            var (innerLatex, innerNeedsParentheses) = GeneralizedAccept(expression.Expression);
            var needsParentheses = innerNeedsParentheses || forceParentheses; 
            if (needsParentheses)
            {
                sb.Append(@"\left( ");
                sb.Append(innerLatex);
                sb.Append(@" \right)");
                sb.Append(operation);
            }
            else
            {
                sb.Append(innerLatex);
                sb.Append(operation);
            }
            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitUnaryPostfix<T1, TResult>(
        IGenericUnaryExpression<T1, TResult> expression,
        string operation,
        Func<IGenericExpression<T1>, bool> parenthesesDeterminator
    )
        => VisitUnaryPostfix(expression, operation, parenthesesDeterminator(expression.Expression));

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitBinaryCommand<T1, T2, TResult>(
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

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitBinaryInfix<T1, T2, TResult>(
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

            var (leftLatexBuilder, leftNeedsParentheses) = GeneralizedAccept(expression.LeftExpression);
            if (leftNeedsParentheses)
            {
                sb.Append(@"\left( ");
                sb.Append(leftLatexBuilder);
                sb.Append(@" \right)");
            }
            else
                sb.Append(leftLatexBuilder);

            sb.Append(latexOperation);

            var (rightLatexBuilder, rightNeedsParentheses) = GeneralizedAccept(expression.RightExpression);
            if (rightNeedsParentheses)
            {
                sb.Append(@"\left( ");
                sb.Append(rightLatexBuilder);
                sb.Append(@" \right)");
            }
            else
                sb.Append(rightLatexBuilder);
            CurrentDepth--;

            return (sb, true);
        }
    }

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitBinaryPrefix<T1, T2, TResult>(
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
            var (leftLatex, _) = GeneralizedAccept(expression.LeftExpression);
            sb.Append(leftLatex);
            sb.Append(", ");
            var (rightLatex, _) = GeneralizedAccept(expression.RightExpression);
            sb.Append(rightLatex);
            sb.Append(@" \right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitNAryInfix<T, TResult>(
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
                var (latex, needsParentheses) = GeneralizedAccept(e);
                if (needsParentheses)
                {
                    sb.Append(@"\left( ");
                    sb.Append(latex);
                    sb.Append(@" \right)");
                }
                else
                    sb.Append(latex);
                c--;
                if (c > 0)
                    sb.Append(latexOperation);
            }

            CurrentDepth--;
            return (sb, true);
        }
    }

    private (StringBuilder LatexBuilder, bool NeedsParentheses) VisitNAryPrefix<T, TResult>(
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
                var (latex, _) = GeneralizedAccept(e);
                sb.Append(latex);
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

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ConcreteCurveExpression expression)
        => (FormatName(expression.Name), false);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalAdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalSubtractionExpression expression)
        => VisitBinaryInfix(expression, " - ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalProductExpression expression)
        => VisitNAryInfix(expression, " \\cdot ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalDivisionExpression expression)
        => VisitBinaryCommand(expression, "\\frac");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalLeastCommonMultipleExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{lcm}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalGreatestCommonDivisorExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{gcd}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalMinimumExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{min}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalMaximumExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{max}");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalNumberExpression numberExpression)
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

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(NegateExpression expression)
        => VisitUnaryPrefix(expression, "-");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ToNonNegativeExpression expression)
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
    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SubAdditiveClosureExpression expression)
        => VisitUnaryCommand(expression, @"\overline");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(
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

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ToUpperNonDecreasingExpression expression)
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

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ToLowerNonDecreasingExpression expression)
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
    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ToLeftContinuousExpression expression)
        => VisitUnaryPostfix(expression, "_{l}",
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <summary>
    /// Uses the notation from the PhD thesis of Damien Guidolin--Pina, where the operation is called "right projection".
    /// </summary>
    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ToRightContinuousExpression expression)
        => VisitUnaryPostfix(expression, "_{r}",
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(WithZeroOriginExpression expression)
        => VisitUnaryPostfix(expression, @"^{\circ}", true);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(LowerPseudoInverseExpression expression)
        => VisitUnaryPostfix(expression, @"^{\underline{-1}}", 
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(UpperPseudoInverseExpression expression)
        => VisitUnaryPostfix(expression, @"^{\overline{-1}}", 
            innerExpression => ContainsSubscriptOrSuperscript(innerExpression)
        );

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(AdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(SubtractionExpression expression)
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

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(MinimumExpression expression)
        => VisitNAryInfix(expression, @" \wedge ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(MaximumExpression expression)
        => VisitNAryInfix(expression, @" \vee ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ConvolutionExpression expression)
        => VisitNAryInfix(expression, @" \otimes ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(DeconvolutionExpression expression)
        => VisitBinaryInfix(expression, @" \oslash ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(MaxPlusConvolutionExpression expression)
        => VisitNAryInfix(expression, @" \overline{\otimes} ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(MaxPlusDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, @" \overline{\oslash} ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(CompositionExpression expression)
        => VisitBinaryInfix(expression, @" \circ ");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(DelayByExpression expression)
        => VisitBinaryPrefix(expression, " delayBy");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ForwardByExpression expression)
        => VisitBinaryPrefix(expression, " forwardBy");
    
    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(HorizontalShiftExpression expression)
        => VisitBinaryPrefix(expression, " hShift");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(VerticalShiftExpression expression)
    {
        switch (expression.RightExpression)
        {
            case NegateRationalExpression negate:
            {
                var inner = negate.Expression;
                var substitute = new VerticalShiftExpression(
                    (CurveExpression) expression.LeftExpression, 
                    (RationalExpression) inner);
                return VisitBinaryInfix(substitute, " - ");
            }

            case RationalNumberExpression rex when rex.Value.IsNegative:
            {
                var substitute = new VerticalShiftExpression(
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

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(NegateRationalExpression expression)
        => VisitUnaryPrefix(expression, "-");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(InvertRationalExpression expression)
        => VisitUnaryPostfix(expression, "^{-1}", expression.Expression is RationalNumberExpression);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(HorizontalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "hdev");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(VerticalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "vdev");

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ValueAtExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (curveLatex, curveNeedsParentheses) = expression.LeftExpression.Accept<(StringBuilder, bool)>(this);
            if (curveNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(curveLatex);
                sb.Append(@"\right)");
            }
            else
                sb.Append(curveLatex);
            var (timeLatex, _) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append(@"\left(");
            sb.Append(timeLatex);
            sb.Append(@"\right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(LeftLimitAtExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (curveLatex, curveNeedsParentheses) = expression.LeftExpression.Accept<(StringBuilder, bool)>(this);
            if (curveNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(curveLatex);
                sb.Append(@"\right)");
            }
            else
                sb.Append(curveLatex);
            var (timeLatex, timeNeedsParentheses) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append(@"\left(");
            if (timeNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(timeLatex);
                sb.Append("^-");
                sb.Append(@"\right)");
            }
            else
                sb.Append(timeLatex);
            sb.Append(@"\right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RightLimitAtExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (curveLatex, curveNeedsParentheses) = expression.LeftExpression.Accept<(StringBuilder, bool)>(this);
            if (curveNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(curveLatex);
                sb.Append(@"\right)");
            }
            else
                sb.Append(curveLatex);
            var (timeLatex, timeNeedsParentheses) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append(@"\left(");
            if (timeNeedsParentheses)
            {
                sb.Append(@"\left(");
                sb.Append(timeLatex);
                sb.Append("^+");
                sb.Append(@"\right)");
            }
            else
                sb.Append(timeLatex);
            sb.Append(@"\right)");
            CurrentDepth--;
            return (sb, false);
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(CurvePlaceholderExpression expression)
        => (new StringBuilder(expression.Name), false);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(RationalPlaceholderExpression expression)
        => (new StringBuilder(expression.Name), false);

    /// <inheritdoc />
    public virtual (StringBuilder LatexBuilder, bool NeedsParentheses) Visit(ScaleExpression expression)
        => VisitBinaryInfix(expression, @" \cdot ");

    /// <summary>
    /// Regular expression to detect strings which terminate with digits
    /// </summary>
    [GeneratedRegex("^(.*?)(\\d+)$")]
    private static partial Regex OperandNameRegex();
}