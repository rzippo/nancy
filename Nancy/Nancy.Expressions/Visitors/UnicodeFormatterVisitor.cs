using System.Text;
using System.Text.RegularExpressions;
using Nancy.Expressions.Expressions;
using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Used for visiting an expression and create its representation using the Unicode character set.
/// </summary>
/// <param name="depth">The maximum level of the expression tree (starting from the root) which must be fully expanded
/// in the representation (after this level, the expression name is used, if not empty)</param>
/// <param name="showRationalsAsName">If true, rational numbers are not shown using their value, but using their name
/// </param>
public partial class UnicodeFormatterVisitor(int depth = 20, bool showRationalsAsName = true)
    : ICurveExpressionVisitor, IRationalExpressionVisitor
{
    /// <summary>
    /// Textual representation of the expression
    /// </summary>
    public StringBuilder Result { get; } = new();

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

    private void VisitBinaryInfix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string unicodeOperation
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append('(');
        expression.LeftExpression.Accept(this);
        Result.Append(unicodeOperation);
        expression.RightExpression.Accept(this);
        Result.Append(')');
        depth++;
    }

    private void VisitBinaryPrefix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string unicodeOperation
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append(unicodeOperation);
        Result.Append('(');
        expression.LeftExpression.Accept(this);
        Result.Append(", ");
        expression.RightExpression.Accept(this);
        Result.Append(')');
        depth++;
    }

    private void VisitNAryInfix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression, 
        string unicodeOperation
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append('(');
        var c = expression.Expressions.Count;
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            c--;
            if (c > 0)
                Result.Append(unicodeOperation);
        }

        Result.Append(')');
        depth++;
    }
    
    private void VisitNAryPrefix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression, 
        string unicodeOperation
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append(unicodeOperation);
        Result.Append('(');
        var c = expression.Expressions.Count;
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            c--;
            if (c > 0)
                Result.Append(", ");
        }

        Result.Append(')');
        depth++;
    }

    /// <summary>
    /// Formats the name of an expression substituting a greek letter using the correspondent symbol
    /// </summary>
    private void FormatName(string name)
    {
        var match = MyRegex().Match(name);
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
        Result.Append(GreekLetters.GetValueOrDefault(nameLettersLower, nameLetters));

        if (nameNumber == null) return;
        Result.Append(nameNumber);
    }

    public virtual void Visit(ConcreteCurveExpression expression)
        => FormatName(expression.Name);

    public virtual void Visit(RationalAdditionExpression expression)
        => VisitNAryInfix(expression, " + ");
    
    public virtual void Visit(RationalSubtractionExpression expression)
        => VisitBinaryInfix(expression, "-");

    public virtual void Visit(RationalProductExpression expression)
        => VisitNAryInfix(expression, " * ");

    public virtual void Visit(RationalDivisionExpression expression)
        => VisitBinaryInfix(expression, "/");

    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => VisitNAryPrefix(expression, "lcm");

    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => VisitNAryPrefix(expression, "gcd");

    public virtual void Visit(RationalNumberExpression numberExpression)
    {
        if (!numberExpression.Name.Equals("") && (showRationalsAsName || depth <= 0))
        {
            FormatName(numberExpression.Name);
            return;
        }

        Result.Append(numberExpression.Value);
    }

    public virtual void Visit(NegateExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append("-(");
        expression.Expression.Accept(this);
        Result.Append(')');
        depth++;
    }

    public virtual void Visit(ToNonNegativeExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        var squareParenthesis = expression.Expression is not (ConcreteCurveExpression
            or ToUpperNonDecreasingExpression
            or ToLowerNonDecreasingExpression);
        if (squareParenthesis) Result.Append('[');
        expression.Expression.Accept(this);
        if (squareParenthesis) Result.Append(']');
        Result.Append('⁺');
        depth++;
    }

    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append("subadditiveClosure{");
        expression.Expression.Accept(this);
        Result.Append("}");
        depth++;
    }

    public virtual void Visit(SuperAdditiveClosureExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append("superadditiveClosure{");
        expression.Expression.Accept(this);
        Result.Append("}");
        depth++;
    }

    public virtual void Visit(ToUpperNonDecreasingExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        var squareParenthesis = expression.Expression is not (ConcreteCurveExpression or ToNonNegativeExpression);
        if (squareParenthesis) Result.Append('[');
        expression.Expression.Accept(this);
        if (squareParenthesis) Result.Append(']');
        Result.Append('↑');
        depth++;
    }

    public virtual void Visit(ToLowerNonDecreasingExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        var squareParenthesis = expression.Expression is not (ConcreteCurveExpression or ToNonNegativeExpression);
        if (squareParenthesis) Result.Append('[');
        expression.Expression.Accept(this);
        if (squareParenthesis) Result.Append(']');
        Result.Append('↓');
        depth++;
    }

    public virtual void Visit(ToLeftContinuousExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append("toLeftContinuous ");
        expression.Expression.Accept(this);
        depth++;
    }

    public virtual void Visit(ToRightContinuousExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append("toRightContinuous ");
        expression.Expression.Accept(this);
        depth++;
    }

    public virtual void Visit(WithZeroOriginExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append('(');
        expression.Expression.Accept(this);
        Result.Append("°)");
        depth++;
    }

    public virtual void Visit(LowerPseudoInverseExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        expression.Expression.Accept(this);
        Result.Append('↓');
        Result.Append("\u207B" + "\u00B9");
        depth++;
    }

    public virtual void Visit(UpperPseudoInverseExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        expression.Expression.Accept(this);
        Result.Append('↑');
        Result.Append("\u207B\u00B9");
        depth++;
    }

    public virtual void Visit(AdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    public virtual void Visit(SubtractionExpression expression)
        => VisitBinaryInfix(expression, " - ");

    public virtual void Visit(MinimumExpression expression)
        => VisitNAryInfix(expression, " \u2227 ");

    public virtual void Visit(MaximumExpression expression)
        => VisitNAryInfix(expression, " \u2228 ");

    public virtual void Visit(ConvolutionExpression expression)
        => VisitNAryInfix(expression, " \u2297 ");

    public virtual void Visit(DeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " \u2298 ");

    public virtual void Visit(MaxPlusConvolutionExpression expression)
        => VisitNAryInfix(expression, " \u0305⊗ ");

    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " \u0305⊘ ");

    public virtual void Visit(CompositionExpression expression)
        => VisitBinaryInfix(expression, " \u2218 ");

    public virtual void Visit(DelayByExpression expression)
        => VisitBinaryPrefix(expression, "delayBy");

    public virtual void Visit(AnticipateByExpression expression)
        => VisitBinaryPrefix(expression, "anticipateBy");

    public virtual void Visit(NegateRationalExpression expression)
    {
        Result.Append('-');
        expression.Expression.Accept(this);
    }

    public virtual void Visit(InvertRationalExpression expression)
    {
        var parenthesis = expression.Expression is RationalNumberExpression;
        if (parenthesis)
            Result.Append('(');
        expression.Expression.Accept(this);
        if (parenthesis)
            Result.Append(')');
        Result.Append("\u207B" + "\u00B9");
    }

    public virtual void Visit(HorizontalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "hdev");

    public virtual void Visit(VerticalDeviationExpression expression)
        => VisitBinaryPrefix(expression, "vdev");

    public virtual void Visit(CurvePlaceholderExpression expression)
        => Result.Append(expression.Name);

    public virtual void Visit(RationalPlaceholderExpression expression)
        => Result.Append(expression.Name);

    public virtual void Visit(ScaleExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append('(');
        expression.RightExpression.Accept(this);
        Result.Append('*');
        expression.LeftExpression.Accept(this);
        Result.Append(')');
        depth++;
    }
    
    /// <summary>
    /// Regular expression to detect strings which terminate with digits
    /// </summary>
    [GeneratedRegex("^(.*?)(\\d+)$")]
    private static partial Regex MyRegex();
}