using System.Text;
using System.Text.RegularExpressions;
using Nancy.Expressions.Expressions;
using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Used for visiting an expression and create its representation using Latex code.
/// </summary>
/// <param name="depth">The maximum level of the expression tree (starting from the root) which must be fully expanded
/// in the representation (after this level, the expression name is used, if not empty)</param>
/// <param name="showRationalsAsName">If true, rational numbers are not shown using their value, but using their name
/// </param>
public partial class LatexFormatterVisitor(int depth = 20, bool showRationalsAsName = true)
    : ICurveExpressionVisitor, IRationalExpressionVisitor
{
    /// <summary>
    /// Latex code for the textual representation of the expression
    /// </summary>
    public StringBuilder Result { get; } = new();
    
    /// <summary>
    /// List of greek letters to substitute the expanded letters with their correspondent Latex symbol
    /// </summary>
    private static readonly List<string> GreekLetters =
    [
        "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta", "theta", "iota", "kappa", "lambda", "mu", "nu",
        "xi", "omicron", "pi", "rho", "sigma", "tau", "upsilon", "phi", "chi", "psi", "omega"
    ];
    
    // todo: explore having generic entry points that take a PreferredOperatorNotation enum or something similar
    
    private void VisitBinaryInfix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string latexOperation
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append("\\left(");
        expression.LeftExpression.Accept(this);
        Result.Append(latexOperation);
        expression.RightExpression.Accept(this);
        Result.Append("\\right)");
        depth++;
    }

    private void VisitBinaryPrefix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string latexOperation
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append(latexOperation);
        Result.Append("\\left(");
        expression.LeftExpression.Accept(this);
        Result.Append(", ");
        expression.RightExpression.Accept(this);
        Result.Append("\\right)");
        depth++;
    }
    
    private void VisitBinaryCommand<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string latexCommand
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append(latexCommand);
        Result.Append('{');
        expression.LeftExpression.Accept(this);
        Result.Append("}{");
        expression.RightExpression.Accept(this);
        Result.Append('}');
        depth++;
    }

    private void VisitNAryInfix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression, 
        string latexOperation
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append("\\left(");
        var c = expression.Expressions.Count;
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            c--;
            if (c > 0)
                Result.Append(latexOperation);
        }

        Result.Append("\\right)");
        depth++;
    }

    private void VisitNAryPrefix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression, 
        string latexOperation
    )
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append(latexOperation);
        Result.Append("\\left(");
        var c = expression.Expressions.Count;
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            c--;
            if (c > 0)
                Result.Append(", ");
        }

        Result.Append("\\right)");
        depth++;
    }

    /// <summary>
    /// Formats the name of an expression putting as subscript the ending digits and substituting a greek letter using
    /// the correspondent Latex command
    /// </summary>
    protected void FormatName(string name)
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
        if (GreekLetters.Any(s => s.Equals(nameLettersLower)))
            Result.Append("\\" + nameLettersLower);
        else
            Result.Append(nameLetters);

        Result.Append(nameNumber != null ? "_{{" + nameNumber + "}" : "");

        Result.Append(nameNumber != null ? "}" : "");
    }

    public virtual void Visit(ConcreteCurveExpression expression)
    {
        FormatName(expression.Name);
    }

    public virtual void Visit(RationalAdditionExpression expression)
        => VisitNAryInfix(expression, " + ");

    public virtual void Visit(RationalSubtractionExpression expression)
        => VisitBinaryCommand(expression, " - ");
    
    public virtual void Visit(RationalProductExpression expression)
        => VisitNAryInfix(expression, " \\cdot ");

    public virtual void Visit(RationalDivisionExpression expression)
        => VisitBinaryCommand(expression, "\\frac");

    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{lcm}");

    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => VisitNAryPrefix(expression, "\\operatorname{gcd}");

    public virtual void Visit(RationalNumberExpression numberExpression)
    {
        if (!numberExpression.Name.Equals("") && (showRationalsAsName || depth <= 0))
        {
            FormatName(numberExpression.Name);
            return;
        }
        
        if(numberExpression.Value.Denominator == 1)
            Result.Append($"{numberExpression.Value.Numerator}");
        else
            Result.Append($"\\frac{numberExpression.Value.Numerator}{numberExpression.Value.Denominator}");
    }

    public virtual void Visit(NegateExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append('-');
        expression.Expression.Accept(this);
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
        string resultToString = Result.ToString();
        // Usually ToNonNegative is used together with ToUpperNonDecreasing or ToLowerNonDecreasing
        // The following instructions are used to obtain the proper Latex formatting
        if (resultToString.EndsWith("{_\\uparrow}") || resultToString.EndsWith("{_\\downarrow}"))
        {
            Result.Remove(Result.Length - 1, 1);
            Result.Append("^{+}}");
        }
        else
            Result.Append("{^{+}}");

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
        Result.Append(" \\overline {");
        expression.Expression.Accept(this);
        Result.Append("} ");
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
        Result.Append(@" \overline{\overline{");
        expression.Expression.Accept(this);
        Result.Append("}} ");
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
        // Usually ToUpperNonDecreasing is used together with ToNonNegative 
        // The following instructions are used to obtain the proper Latex formatting
        if (Result.ToString().EndsWith("{^{+}}"))
        {
            Result.Remove(Result.Length - 1, 1);
            Result.Append("_\\uparrow}");
        }
        else
            Result.Append("{_\\uparrow}");

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
        // Usually ToLowerNonDecreasing is used together with ToNonNegative 
        // The following instructions are used to obtain the proper Latex formatting
        if (Result.ToString().EndsWith("{^{+}}"))
        {
            Result.Remove(Result.Length - 1, 1);
            Result.Append("_\\downarrow}");
        }
        else
            Result.Append("{_\\downarrow}");

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
        Result.Append("\\left(");
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
        Result.Append("{_\\downarrow^{-1}}");
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
        Result.Append("{_\\uparrow^{-1}}");
        depth++;
    }

    public virtual void Visit(AdditionExpression expression)
        => VisitNAryInfix(expression, "+");

    public virtual void Visit(SubtractionExpression expression)
        => VisitBinaryInfix(expression, "-");

    public virtual void Visit(MinimumExpression expression)
        => VisitNAryInfix(expression, " \\wedge ");

    public virtual void Visit(MaximumExpression expression)
        => VisitNAryInfix(expression, " \\vee ");

    public virtual void Visit(ConvolutionExpression expression)
        => VisitNAryInfix(expression, " \\otimes ");

    public virtual void Visit(DeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " \\oslash ");

    public virtual void Visit(MaxPlusConvolutionExpression expression)
        => VisitNAryInfix(expression, @" \overline{\otimes} ");

    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, @" \overline{\oslash} ");

    public virtual void Visit(CompositionExpression expression)
        => VisitBinaryInfix(expression, " \\circ ");

    public virtual void Visit(DelayByExpression expression)
        => VisitBinaryPrefix(expression, " delayBy");

    public virtual void Visit(AnticipateByExpression expression)
        => VisitBinaryPrefix(expression, " anticipateBy");

    public virtual void Visit(NegateRationalExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        Result.Append('-');
        expression.Expression.Accept(this);
        depth++;
    }

    public virtual void Visit(InvertRationalExpression expression)
    {
        if (depth <= 0 && !expression.Name.Equals(""))
        {
            FormatName(expression.Name);
            return;
        }

        depth--;
        var parenthesis = expression.Expression is RationalNumberExpression;
        if (parenthesis)
            Result.Append("\\left(");
        expression.Expression.Accept(this);
        if (parenthesis)
            Result.Append("\\right)");
        Result.Append("^{-1}");
        depth++;
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
        Result.Append("\\left(");
        expression.RightExpression.Accept(this);
        Result.Append(" \\cdot ");
        expression.LeftExpression.Accept(this);
        Result.Append("\\right)");
        depth++;
    }

    /// <summary>
    /// Regular expression to detect strings which terminate with digits
    /// </summary>
    [GeneratedRegex("^(.*?)(\\d+)$")]
    private static partial Regex MyRegex();
}