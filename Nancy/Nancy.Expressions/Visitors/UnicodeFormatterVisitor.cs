using System.Text;
using System.Text.RegularExpressions;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Used for visiting an expression and create its representation using the Unicode character set.
/// </summary>
public partial class UnicodeFormatterVisitor : 
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
    /// Used for visiting an expression and create its representation using the Unicode character set.
    /// </summary>
    /// <param name="depth">The maximum level of the expression tree (starting from the root) which must be fully expanded
    /// in the representation (after this level, the expression name is used, if not empty)</param>
    /// <param name="showRationalsAsName">If true, rational numbers are not shown using their value, but using their name
    /// </param>
    public UnicodeFormatterVisitor(int depth = 20, bool showRationalsAsName = true)
    {
        MaxDepth = depth;
        CurrentDepth = 0;
        ShowRationalsAsName = showRationalsAsName;
    }

    /// <summary>
    /// Dictionary of greek letters to substitute the expanded letters with their correspondent Unicode symbol
    /// </summary>
    private static readonly Dictionary<string, string> GreekLetters = new()
    {
        { "alpha", "\u03B1" },
        { "beta", "\u03B2" },
        { "gamma", "\u03B3" },
        { "delta", "\u03B4" },
        { "epsilon", "\u03B5" },
        { "zeta", "\u03B6" },
        { "eta", "\u03B7" },
        { "theta", "\u03B8" },
        { "iota", "\u03B9" },
        { "kappa", "\u03BA" },
        { "lambda", "\u03BB" },
        { "mu", "\u03BC" },
        { "nu", "\u03BD" },
        { "xi", "\u03BE" },
        { "omicron", "\u03BF" },
        { "pi", "\u03C0" },
        { "rho", "\u03C1" },
        { "sigma", "\u03C3" },
        { "tau", "\u03C4" },
        { "upsilon", "\u03C5" },
        { "phi", "\u03C6" },
        { "chi", "\u03C7" },
        { "psi", "\u03C8" },
        { "omega", "\u03C9" }
    };

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

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitUnaryPrefix<T1, TResult>(
        IGenericUnaryExpression<T1, TResult> expression,
        string unicodeOperation
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (innerUnicode, _) = GeneralizedAccept(expression.Expression);

            sb.Append(unicodeOperation);
            sb.Append('(');
            sb.Append(innerUnicode);
            sb.Append(')');

            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitUnaryPostfix<T1, TResult>(
        IGenericUnaryExpression<T1, TResult> expression,
        string unicodeOperation,
        bool forceParentheses = true
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (innerUnicode, needsParentheses) = GeneralizedAccept(expression.Expression);
            if (forceParentheses || needsParentheses)
            {
                sb.Append('(');
                sb.Append(innerUnicode);
                sb.Append(')');
                sb.Append(unicodeOperation);
            }
            else
            {
                sb.Append(innerUnicode);
                sb.Append(unicodeOperation);
            }
            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitBinaryInfix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string unicodeOperation
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
                sb.Append('(');
                sb.Append(leftUnicodeBuilder);
                sb.Append(')');
            }
            else
                sb.Append(leftUnicodeBuilder);

            sb.Append(unicodeOperation);

            var (rightUnicodeBuilder, rightNeedsParentheses) = GeneralizedAccept(expression.RightExpression);
            if (rightNeedsParentheses)
            {
                sb.Append('(');
                sb.Append(rightUnicodeBuilder);
                sb.Append(')');
            }
            else
                sb.Append(rightUnicodeBuilder);
            CurrentDepth--;

            return (sb, true);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitBinaryPrefix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string unicodeOperation
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(unicodeOperation);
            sb.Append('(');
            var (leftUnicode, _) = GeneralizedAccept(expression.LeftExpression);
            sb.Append(leftUnicode);
            sb.Append(", ");
            var (rightUnicode, _) = GeneralizedAccept(expression.RightExpression);
            sb.Append(rightUnicode);
            sb.Append(')');
            CurrentDepth--;
            return (sb, false);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitNAryInfix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression, 
        string unicodeOperation
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
                    sb.Append('(');
                    sb.Append(unicode);
                    sb.Append(')');
                }
                else
                    sb.Append(unicode);
                c--;
                if (c > 0)
                    sb.Append(unicodeOperation);
            }

            CurrentDepth--;
            return (sb, true);
        }
    }

    private (StringBuilder UnicodeBuilder, bool NeedsParentheses) VisitNAryPrefix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression, 
        string unicodeOperation
    )
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(unicodeOperation);
            sb.Append('(');
            var c = expression.Expressions.Count;
            foreach (var e in expression.Expressions)
            {
                var (unicode, _) = GeneralizedAccept(e);
                sb.Append(unicode);
                c--;
                if (c > 0)
                    sb.Append(", ");
            }
            sb.Append(')');
            CurrentDepth--;
            return (sb, false);
        }
    }

    /// <summary>
    /// Formats the name of an expression substituting a greek letter using the correspondent symbol
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
        sb.Append(GreekLetters.GetValueOrDefault(nameLettersLower, nameLetters));

        if (nameNumber != null)
            sb.Append(nameNumber);
        return sb;
    }

    #endregion Default formatters

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ConcreteCurveExpression expression)
    {
        var formattedName = FormatName(expression.Name).ToString();
        var needsParentheses = formattedName.Contains(' ');
        return (new StringBuilder(formattedName), needsParentheses);
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalAdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalSubtractionExpression expression)
        => VisitBinaryInfix(expression, " - ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalProductExpression expression)
        => VisitNAryInfix(expression, " * ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalDivisionExpression expression)
        => VisitBinaryInfix(expression, "/");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalLeastCommonMultipleExpression expression)
        => VisitNAryPrefix(expression, "lcm");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalGreatestCommonDivisorExpression expression)
        => VisitNAryPrefix(expression, "gcd");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalMinimumExpression expression)
        => VisitNAryPrefix(expression, "min");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalMaximumExpression expression)
        => VisitNAryPrefix(expression, "max");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalNumberExpression numberExpression)
    {
        if (!numberExpression.Name.Equals("") && (ShowRationalsAsName || CurrentDepth >= MaxDepth))
            return (FormatName(numberExpression.Name), false);
        else
            return (
                new StringBuilder(numberExpression.Value.ToString()),numberExpression.Value.IsNegative || numberExpression.Value.Denominator != 1
            );
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
            var needsSquareParentheses = expression.Expression is not ConcreteCurveExpression cce || !FormatName(cce.Name).ToString().Contains('_');
            if (needsSquareParentheses) sb.Append('[');
            var (unicode, _) = expression.Expression.Accept<(StringBuilder, bool)>(this);
            sb.Append(unicode);
            if (needsSquareParentheses) sb.Append(']');
            sb.Append('⁺');
            CurrentDepth--;
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SubAdditiveClosureExpression expression)
        => VisitUnaryPrefix(expression, "subadditiveClosure");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(SuperAdditiveClosureExpression expression)
        => VisitUnaryPrefix(expression, "superadditiveClosure");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToUpperNonDecreasingExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (unicode, needsParentheses) = expression.Expression.Accept<(StringBuilder, bool)>(this);
            var parenthesesExceptions = expression.Expression is (ConcreteCurveExpression or ToNonNegativeExpression);
            var squareParentheses = needsParentheses && !parenthesesExceptions;
            if (squareParentheses) 
                sb.Append('[');
            sb.Append(unicode);
            if (squareParentheses) 
                sb.Append(']');
            sb.Append('↑');
            CurrentDepth--;
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToLowerNonDecreasingExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (unicode, needsParentheses) = expression.Expression.Accept<(StringBuilder, bool)>(this);
            var parenthesesExceptions = expression.Expression is (ConcreteCurveExpression or ToNonNegativeExpression);
            var squareParentheses = needsParentheses && !parenthesesExceptions;
            if (squareParentheses)
                sb.Append('[');
            sb.Append(unicode);
            if (squareParentheses)
                sb.Append(']');
            sb.Append('↓');
            CurrentDepth--;
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToLeftContinuousExpression expression)
        => VisitUnaryPrefix(expression, "toLeftContinuous");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ToRightContinuousExpression expression)
        => VisitUnaryPrefix(expression, "toRightContinuous");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(WithZeroOriginExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (unicode, needsParentheses) = expression.Expression.Accept<(StringBuilder, bool)>(this);
            if (needsParentheses)
            {
                sb.Append('(');
                sb.Append(unicode);
                sb.Append(")°");
            }
            else
            {
                sb.Append(unicode);
                sb.Append('°');
            }
            CurrentDepth--;
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(LowerPseudoInverseExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (unicode, innerNeedsParentheses) = expression.Expression.Accept<(StringBuilder, bool)>(this);
            var operationNeedsParentheses = expression.Expression is (
                LowerPseudoInverseExpression or 
                UpperPseudoInverseExpression or
                ToLowerNonDecreasingExpression or
                ToUpperNonDecreasingExpression or
                ToNonNegativeExpression
            );
            var needsParentheses = innerNeedsParentheses || operationNeedsParentheses;
            if (needsParentheses)
            {
                sb.Append('(');
                sb.Append(unicode);
                sb.Append(')');
                sb.Append('↓');
                sb.Append("\u207B" + "\u00B9");
            }
            else
            {
                sb.Append(unicode);
                sb.Append('↓');
                sb.Append("\u207B" + "\u00B9");
            }
            CurrentDepth--;
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(UpperPseudoInverseExpression expression)
    {
        if (CurrentDepth >= MaxDepth && !expression.Name.Equals(""))
            return (FormatName(expression.Name), false);
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            var (unicode, innerNeedsParentheses) = expression.Expression.Accept<(StringBuilder, bool)>(this);
            var operationNeedsParentheses = expression.Expression is (
                LowerPseudoInverseExpression or 
                UpperPseudoInverseExpression or
                ToLowerNonDecreasingExpression or
                ToUpperNonDecreasingExpression or
                ToNonNegativeExpression
                );
            var needsParentheses = innerNeedsParentheses || operationNeedsParentheses;
            if (needsParentheses)
            {
                sb.Append('(');
                sb.Append(unicode);
                sb.Append(')');
                sb.Append('↑');
                sb.Append("\u207B\u00B9");
            }
            else
            {
                sb.Append(unicode);
                sb.Append('↑');
                sb.Append("\u207B\u00B9");
            }
            CurrentDepth--;
            return (sb, false);
        }
    }

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
        => VisitNAryInfix(expression, " \u2227 ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(MaximumExpression expression)
        => VisitNAryInfix(expression, " \u2228 ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ConvolutionExpression expression)
        => VisitNAryInfix(expression, " \u2297 ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(DeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " \u2298 ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(MaxPlusConvolutionExpression expression)
        => VisitNAryInfix(expression, " \u0305⊗ ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(MaxPlusDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " \u0305⊘ ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(CompositionExpression expression)
        => VisitBinaryInfix(expression, " \u2218 ");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(DelayByExpression expression)
        => VisitBinaryPrefix(expression, "delayBy");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ForwardByExpression expression)
        => VisitBinaryPrefix(expression, "forwardBy");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(VerticalShiftExpression expression)
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

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(NegateRationalExpression expression)
        => VisitUnaryPrefix(expression, "-");

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(InvertRationalExpression expression)
        => VisitUnaryPostfix(expression, "\u207B" + "\u00B9", expression.Expression is RationalNumberExpression);

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
                sb.Append('(');
                sb.Append(curveUnicode);
                sb.Append(')');
            }
            else
                sb.Append(curveUnicode);
            var (timeUnicode, _) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append('(');
            sb.Append(timeUnicode);
            sb.Append(')');
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
                sb.Append('(');
                sb.Append(curveUnicode);
                sb.Append(')');
            }
            else
                sb.Append(curveUnicode);
            var (timeUnicode, timeNeedsParentheses) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append('(');
            if (timeNeedsParentheses)
            {
                sb.Append('(');
                sb.Append(timeUnicode);
                sb.Append("^-");
                sb.Append(')');
            }
            else
                sb.Append(timeUnicode);
            sb.Append(')');
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
                sb.Append('(');
                sb.Append(curveUnicode);
                sb.Append(')');
            }
            else
                sb.Append(curveUnicode);
            var (timeUnicode, timeNeedsParentheses) = expression.RightExpression.Accept<(StringBuilder, bool)>(this);
            sb.Append('(');
            if (timeNeedsParentheses)
            {
                sb.Append('(');
                sb.Append(timeUnicode);
                sb.Append("^+");
                sb.Append(')');
            }
            else
                sb.Append(timeUnicode);
            sb.Append(')');
            CurrentDepth--;
            return (sb, false);
        }
    }

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(CurvePlaceholderExpression expression)
        => (new StringBuilder(expression.Name), false);

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(RationalPlaceholderExpression expression)
        => (new StringBuilder(expression.Name), false);

    public virtual (StringBuilder UnicodeBuilder, bool NeedsParentheses) Visit(ScaleExpression expression)
        => VisitBinaryInfix(expression, "·");

    /// <summary>
    /// Regular expression to detect strings which terminate with digits
    /// </summary>
    [GeneratedRegex("^(.*?)(\\d+)$")]
    private static partial Regex OperandNameRegex();
}