using System.Text;
using System.Text.RegularExpressions;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Precedence level of an MPPG expression, used to decide where parentheses are needed.
/// </summary>
/// <remarks>
/// The MPPG syntax has three levels, and all its infix operators are left-associative.
/// An operand is parenthesized when its level is lower than the level required by the position it appears in.
/// </remarks>
public enum MppgPrecedence
{
    /// <summary>
    /// Level of the sum operators, i.e. <c>+</c>, <c>-</c>, <c>/\</c> and <c>\/</c>.
    /// </summary>
    Sum = 0,

    /// <summary>
    /// Level of the product operators, i.e. <c>*</c>, <c>*^</c>, <c>/</c>, <c>/^</c>, <c>comp</c>, <c>div</c> and <c>mod</c>.
    /// </summary>
    Product = 1,

    /// <summary>
    /// Level of the operands that never need parentheses, i.e. names, literals, call forms and sampling.
    /// </summary>
    Atom = 2
}

/// <summary>
/// Used for visiting an expression and create its representation as MPPG source text.
/// </summary>
/// <remarks>
/// The result is a single MPPG expression, without any statement around it, valid under syntax version 1.3 —
/// or 1.4 if the visited expression uses a monotonicity closure ($\mathrm{UND}$, $\mathrm{LND}$, $\mathrm{UNI}$,
/// $\mathrm{LNI}$), which this formatter always renders with their explicit, <c>closure</c>-suffixed 1.4 spelling.
/// Parsing and evaluating it yields a value equivalent to computing the visited expression.
/// The visit neither computes the expression nor caches any computed value.
/// </remarks>
public partial class MppgFormatterVisitor :
    ICurveExpressionVisitor<(StringBuilder MppgBuilder, MppgPrecedence Precedence)>,
    IRationalExpressionVisitor<(StringBuilder MppgBuilder, MppgPrecedence Precedence)>
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
    /// A node whose name is not a valid MPPG name is expanded regardless of the depth, since its name would not parse.
    /// </remarks>
    public int MaxDepth { get; init; }

    /// <summary>
    /// If true, rational numbers are not shown using their value, but using their name.
    /// </summary>
    public bool ShowRationalsAsName { get; init; }

    /// <summary>
    /// Used for visiting an expression and create its representation as MPPG source text.
    /// </summary>
    /// <param name="depth">The maximum level of the expression tree (starting from the root) which must be fully expanded
    /// in the representation (after this level, the expression name is used, if not empty)</param>
    /// <param name="showRationalsAsName">If true, rational numbers are not shown using their value, but using their name
    /// </param>
    public MppgFormatterVisitor(int depth = 20, bool showRationalsAsName = false)
    {
        MaxDepth = depth;
        CurrentDepth = 0;
        ShowRationalsAsName = showRationalsAsName;
    }

    #region Names

    /// <summary>
    /// Names that MPPG lexes as keywords, as of syntax version 1.4, and can therefore not be used as variable names.
    /// </summary>
    /// <remarks>
    /// This formatter targets syntax version 1.3 for everything except the four closures, which it renders
    /// with their explicit, <c>closure</c>-suffixed 1.4 spelling (<c>upnondecclosure</c>, <c>lownondecclosure</c>,
    /// <c>nnupnondecclosure</c>, <c>nnlownondecclosure</c>, <c>upnonincclosure</c>, <c>lownonincclosure</c>).
    /// Because a rendered expression can therefore always require a 1.4 parser, all twelve of the closure
    /// tokens 1.4 reserves — including the six this formatter never itself emits (the short aliases
    /// <c>upnondec</c>/<c>lownondec</c>/<c>nnupnondec</c>/<c>nnlownondec</c>/<c>upnoninc</c>/<c>lownoninc</c>) —
    /// must be listed here too, so a curve or rational happening to carry one of those names as its own name
    /// is never emitted as a bare identifier.
    /// </remarks>
    private static readonly HashSet<string> ReservedNames =
    [
        "abs", "affine", "assert", "bg", "bucket", "ceil", "comp", "delay", "div", "epsilon", "floor", "gcd", "grid",
        "gui", "hDev", "hShift", "hdev", "hshift", "inv", "lcm", "low_inv", "lowclosure", "lownondec",
        "lownondecclosure", "lownoninc", "lownonincclosure", "main", "mod", "nnlowclosure", "nnlownondec",
        "nnlownondecclosure", "nnupclosure", "nnupnondec", "nnupnondecclosure", "out", "period", "plot", "plotTikz",
        "pow", "printExpression", "ratency", "stair", "star", "step", "subaddclosure", "superaddclosure", "title",
        "uaf", "up_inv", "upclosure", "upnondec", "upnondecclosure", "upnoninc", "upnonincclosure", "upp", "vDev",
        "vShift", "vdev", "vshift", "xlab", "xlim", "ylab", "ylim", "zDev", "zdev", "zero"
    ];

    /// <summary>
    /// True if <paramref name="name"/> can be used as a variable name in MPPG.
    /// </summary>
    /// <remarks>
    /// A name is usable if it matches the lexer rule for variable names and is not a keyword of syntax version 1.4.
    /// Note that the rendered expression only parses in a context where the names it uses are declared.
    /// </remarks>
    public static bool IsValidMppgName(string name)
        => MppgNameRegex().IsMatch(name) && !ReservedNames.Contains(name);

    /// <summary>
    /// Regular expression matching the MPPG lexer rule for variable names.
    /// </summary>
    [GeneratedRegex("^[a-zA-Z_][a-zA-Z_0-9]*$")]
    private static partial Regex MppgNameRegex();

    #endregion Names

    #region Default formatters

    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) GeneralizedAccept<TExpressionResult>(
        IGenericExpression<TExpressionResult> expression
    )
    {
        if (expression is IGenericExpression<Curve> curveExpression)
            return curveExpression.Accept<(StringBuilder, MppgPrecedence)>(this);
        else if (expression is IGenericExpression<Rational> rationalExpression)
            return rationalExpression.Accept<(StringBuilder, MppgPrecedence)>(this);
        else
            throw new NotImplementedException();
    }

    /// <summary>
    /// Visits <paramref name="expression"/> and parenthesizes the result if it binds less tightly than <paramref name="required"/>.
    /// </summary>
    private StringBuilder Render<TExpressionResult>(
        IGenericExpression<TExpressionResult> expression,
        MppgPrecedence required
    )
    {
        var (mppg, precedence) = GeneralizedAccept(expression);
        if (precedence >= required)
            return mppg;
        else
            return new StringBuilder().Append('(').Append(mppg).Append(')');
    }

    /// <summary>
    /// The level an operand must have to appear after the first one in a left-associative chain.
    /// </summary>
    private static MppgPrecedence Tighter(MppgPrecedence precedence)
        => precedence + 1;

    /// <summary>
    /// Visits <paramref name="expression"/> as the operand of an infix operation, parenthesizing it if it is one itself.
    /// </summary>
    /// <remarks>
    /// The formatting style spells the grouping out, rather than leaving it to the reader to apply the precedence rules,
    /// so a compound operand is parenthesized even where precedence would not require it: $f + x * y$ is written as <c>f + (x * y)</c>.
    /// </remarks>
    private StringBuilder RenderOperand<TExpressionResult>(
        IGenericExpression<TExpressionResult> expression,
        MppgPrecedence required
    )
    {
        var (mppg, precedence) = GeneralizedAccept(expression);

        // a rational literal is one value rather than a compound operand, so it is written as it is,
        // and parenthesized only where it would re-associate, as in the right operand of a division
        if (expression is IGenericExpression<Rational> rational && IsTightLiteral(rational, CurrentDepth))
            return precedence >= required ? mppg : Parenthesized(mppg);

        return precedence == MppgPrecedence.Atom ? mppg : Parenthesized(mppg);
    }

    /// <summary>
    /// Wraps what has been built so far, which is an infix operation, so that it can appear as an operand of the next one.
    /// </summary>
    private static StringBuilder Parenthesized(StringBuilder mppg)
        => new StringBuilder().Append('(').Append(mppg).Append(')');

    /// <summary>
    /// True if the expression must be represented through its name, which happens past <see cref="MaxDepth"/>.
    /// </summary>
    private bool TryFormatAsName(
        IExpression expression,
        out (StringBuilder MppgBuilder, MppgPrecedence Precedence) result
    )
    {
        if (CurrentDepth >= MaxDepth && IsValidMppgName(expression.Name))
        {
            result = (new StringBuilder(expression.Name), MppgPrecedence.Atom);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) VisitUnaryCall<T1, TResult>(
        IGenericUnaryExpression<T1, TResult> expression,
        string mppgOperation
    )
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(mppgOperation);
            sb.Append('(');
            sb.Append(Render(expression.Expression, MppgPrecedence.Sum));
            sb.Append(')');
            CurrentDepth--;
            return (sb, MppgPrecedence.Atom);
        }
    }

    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) VisitBinaryCall<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string mppgOperation
    )
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(mppgOperation);
            sb.Append('(');
            sb.Append(Render(expression.LeftExpression, MppgPrecedence.Sum));
            sb.Append(", ");
            sb.Append(Render(expression.RightExpression, MppgPrecedence.Sum));
            sb.Append(')');
            CurrentDepth--;
            return (sb, MppgPrecedence.Atom);
        }
    }

    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) VisitBinaryInfix<T1, T2, TResult>(
        IGenericBinaryExpression<T1, T2, TResult> expression,
        string mppgOperation,
        MppgPrecedence precedence
    )
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(RenderOperand(expression.LeftExpression, precedence));
            sb.Append(mppgOperation);
            sb.Append(RenderOperand(expression.RightExpression, Tighter(precedence)));
            CurrentDepth--;
            return (sb, precedence);
        }
    }

    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) VisitNAryInfix<T, TResult>(
        IGenericNAryExpression<T, TResult> expression,
        string mppgOperation,
        MppgPrecedence precedence
    )
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            // the operation is n-ary here and binary in MPPG, and the style spells out the left-associative grouping,
            // so the operands are chained as $((a + b) + c) + d$ rather than written flat
            var sb = new StringBuilder();
            var rendered = 0;
            foreach (var e in expression.Operands)
            {
                if (rendered == 0)
                    sb.Append(RenderOperand(e, precedence));
                else if (rendered == 1)
                    sb.Append(mppgOperation).Append(RenderOperand(e, Tighter(precedence)));
                else
                    sb = Parenthesized(sb).Append(mppgOperation).Append(RenderOperand(e, Tighter(precedence)));
                rendered++;
            }
            CurrentDepth--;
            return (sb, precedence);
        }
    }

    /// <summary>
    /// Renders an n-ary operation through nested calls of its binary MPPG form, which is safe since the operations rendered this way are associative.
    /// </summary>
    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) VisitNAryNestedCall<T, TResult>(
        IGenericNAryExpression<T, TResult> expression,
        string mppgOperation
    )
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            var operands = expression.Operands
                .Select(e => Render(e, MppgPrecedence.Sum))
                .ToList();
            var sb = operands[^1];
            for (var i = operands.Count - 2; i >= 0; i--)
            {
                sb = new StringBuilder()
                    .Append(mppgOperation)
                    .Append('(')
                    .Append(operands[i])
                    .Append(", ")
                    .Append(sb)
                    .Append(')');
            }
            CurrentDepth--;
            return (sb, MppgPrecedence.Atom);
        }
    }

    /// <summary>
    /// Renders the negation of a rational expression, avoiding the <c>--</c> that a negative literal would produce.
    /// </summary>
    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) RenderNegated(
        IGenericExpression<Rational> expression
    )
    {
        if (expression is RationalNumberExpression number && !ShowRationalsAsName)
            return (
                new StringBuilder((-number.Value).ToMppgString()),
                RationalLiteralPrecedence(-number.Value)
            );
        else if (expression is RationalDivisionExpression division
            && !ShowRationalsAsName
            && IsTightLiteral(division, CurrentDepth)
            && !StartsWithMinus(division))
            return (
                new StringBuilder().Append('-').Append(RenderTightLiteral(division).MppgBuilder),
                MppgPrecedence.Product
            );
        else
            return (
                new StringBuilder().Append("-(").Append(Render(expression, MppgPrecedence.Sum)).Append(')'),
                MppgPrecedence.Sum
            );
    }

    /// <summary>
    /// The precedence of a rational literal, which is that of a product when it is rendered as a fraction.
    /// </summary>
    private static MppgPrecedence RationalLiteralPrecedence(Rational value)
        => value.IsInfinite || value.Denominator == 1 ? MppgPrecedence.Atom : MppgPrecedence.Product;

    /// <summary>
    /// True if the expression is a rational literal, which renders without spaces around its <c>/</c> and with no space after its <c>-</c>.
    /// </summary>
    /// <remarks>
    /// A literal is a number, a negation of a literal, or a division of two literals whose tight rendering re-parses to the same value.
    /// A subtree whose leaf would render as a name, rather than as its value, is not a literal.
    /// </remarks>
    private bool IsTightLiteral(IGenericExpression<Rational> expression, int depth)
    {
        if (ShowRationalsAsName)
            return false;
        switch (expression)
        {
            case RationalNumberExpression number:
                return !(depth >= MaxDepth && IsValidMppgName(number.Name));
            case NegateRationalExpression negate:
                if (depth >= MaxDepth && IsValidMppgName(negate.Name))
                    return false;
                return negate.Expression switch
                {
                    RationalNumberExpression => true,
                    RationalDivisionExpression division => IsTightLiteral(division, depth + 1) && !StartsWithMinus(division),
                    _ => false
                };
            case RationalDivisionExpression division:
                if (depth >= MaxDepth && IsValidMppgName(division.Name))
                    return false;
                return IsTightLiteral(division.LeftExpression, depth + 1)
                    && IsTightLiteral(division.RightExpression, depth + 1)
                    && !TightFormHasDivision(division.RightExpression);
            default:
                return false;
        }
    }

    /// <summary>
    /// Renders a rational literal tightly, without the spaces the binary operators get.
    /// </summary>
    /// <remarks>
    /// The caller guarantees through <see cref="IsTightLiteral"/> that the rendering re-parses to the same value.
    /// </remarks>
    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) RenderTightLiteral(
        IGenericExpression<Rational> expression
    ) => expression switch
    {
        RationalNumberExpression number => (
            new StringBuilder(number.Value.ToMppgString()),
            RationalLiteralPrecedence(number.Value)
        ),
        NegateRationalExpression negate => negate.Expression switch
        {
            RationalNumberExpression number => (
                new StringBuilder((-number.Value).ToMppgString()),
                RationalLiteralPrecedence(-number.Value)
            ),
            _ => (
                new StringBuilder().Append('-').Append(RenderTightLiteral(negate.Expression).MppgBuilder),
                MppgPrecedence.Product
            )
        },
        RationalDivisionExpression division => (
            new StringBuilder()
                .Append(RenderTightLiteral(division.LeftExpression).MppgBuilder)
                .Append('/')
                .Append(RenderTightLiteral(division.RightExpression).MppgBuilder),
            MppgPrecedence.Product
        ),
        _ => throw new InvalidOperationException("The expression is not a rational literal.")
    };

    /// <summary>
    /// True if the tight rendering of the expression contains a <c>/</c>, which as the right operand of a tight division would re-associate.
    /// </summary>
    private static bool TightFormHasDivision(IGenericExpression<Rational> expression)
        => expression switch
        {
            RationalNumberExpression number => !number.Value.IsInfinite && number.Value.Denominator != 1,
            NegateRationalExpression negate => TightFormHasDivision(negate.Expression),
            RationalDivisionExpression => true,
            _ => false
        };

    /// <summary>
    /// True if the tight rendering of the expression starts with <c>-</c>, which a glued negation would double.
    /// </summary>
    private static bool StartsWithMinus(IGenericExpression<Rational> expression)
        => expression switch
        {
            RationalNumberExpression number => number.Value.IsNegative,
            NegateRationalExpression negate => !StartsWithMinus(negate.Expression),
            RationalDivisionExpression division => StartsWithMinus(division.LeftExpression),
            _ => false
        };

    /// <summary>
    /// Renders the sampling forms, which MPPG only accepts on the name of a function variable.
    /// </summary>
    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) VisitSampling<TResult>(
        IGenericBinaryExpression<Curve, Rational, TResult> expression,
        string timeSuffix
    )
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            var curveName = expression.LeftExpression.Name;
            if (!IsValidMppgName(curveName))
                throw new MppgFormattingException(
                    expression.GetType(),
                    expression.Name,
                    "MPPG samples a function only through the name of a variable, and the sampled expression has no name usable as one"
                );

            CurrentDepth++;
            var sb = new StringBuilder();
            sb.Append(curveName);
            sb.Append('(');
            sb.Append(Render(expression.RightExpression, MppgPrecedence.Sum));
            sb.Append(timeSuffix);
            sb.Append(')');
            CurrentDepth--;
            return (sb, MppgPrecedence.Atom);
        }
    }

    /// <summary>
    /// Reports an operation for which the MPPG syntax has no notation.
    /// </summary>
    private static (StringBuilder MppgBuilder, MppgPrecedence Precedence) Unsupported(
        IExpression expression,
        string reason
    )
        => throw new MppgFormattingException(expression.GetType(), expression.Name, reason);

    #endregion Default formatters

    #region Curve expressions

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ConcreteCurveExpression expression)
    {
        if (IsValidMppgName(expression.Name))
            return (new StringBuilder(expression.Name), MppgPrecedence.Atom);
        else
            return (new StringBuilder(expression.Value.ToMppgString()), MppgPrecedence.Atom);
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(NegateExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder()
                .Append('-')
                .Append(Render(expression.Expression, MppgPrecedence.Atom));
            CurrentDepth--;
            return (sb, MppgPrecedence.Atom);
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ToNonNegativeExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else if (expression.Expression is ToUpperNonDecreasingExpression upper)
            return VisitFusedNonNegativeClosure(upper.Expression, "nnupnondecclosure");
        else if (expression.Expression is ToLowerNonDecreasingExpression lower)
            return VisitFusedNonNegativeClosure(lower.Expression, "nnlownondecclosure");
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder()
                .Append(Render(expression.Expression, MppgPrecedence.Product))
                .Append(" \\/ 0");
            CurrentDepth--;
            return (sb, MppgPrecedence.Sum);
        }
    }

    /// <summary>
    /// Renders the non-negative non-decreasing closures, which MPPG spells as a single operator.
    /// </summary>
    /// <remarks>
    /// The two operations commute, so the fused form is used for either nesting order.
    /// </remarks>
    private (StringBuilder MppgBuilder, MppgPrecedence Precedence) VisitFusedNonNegativeClosure(
        IGenericExpression<Curve> innerExpression,
        string mppgOperation
    )
    {
        CurrentDepth++;
        var sb = new StringBuilder()
            .Append(mppgOperation)
            .Append('(')
            .Append(Render(innerExpression, MppgPrecedence.Sum))
            .Append(')');
        CurrentDepth--;
        return (sb, MppgPrecedence.Atom);
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(SubAdditiveClosureExpression expression)
        => VisitUnaryCall(expression, "subaddclosure");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(SuperAdditiveClosureExpression expression)
        => VisitUnaryCall(expression, "superaddclosure");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ToUpperNonDecreasingExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else if (expression.Expression is ToNonNegativeExpression nonNegative)
            return VisitFusedNonNegativeClosure(nonNegative.Expression, "nnupnondecclosure");
        else
            return VisitUnaryCall(expression, "upnondecclosure");
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ToLowerNonDecreasingExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else if (expression.Expression is ToNonNegativeExpression nonNegative)
            return VisitFusedNonNegativeClosure(nonNegative.Expression, "nnlownondecclosure");
        else
            return VisitUnaryCall(expression, "lownondecclosure");
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ToUpperNonIncreasingExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
            return VisitUnaryCall(expression, "upnonincclosure");
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ToLowerNonIncreasingExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
            return VisitUnaryCall(expression, "lownonincclosure");
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ToLeftContinuousExpression expression)
        => VisitUnaryCall(expression, "left-ext");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ToRightContinuousExpression expression)
        => VisitUnaryCall(expression, "right-ext");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(WithZeroOriginExpression expression)
        => Unsupported(expression, "MPPG has no notation for setting the value at the origin to zero");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(WithOriginAtExpression expression)
        => Unsupported(expression, "MPPG has no notation for setting the value at the origin");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(LowerPseudoInverseExpression expression)
        => VisitUnaryCall(expression, "low_inv");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(UpperPseudoInverseExpression expression)
        => VisitUnaryCall(expression, "up_inv");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(AdditionExpression expression)
        => VisitNAryInfix(expression, " + ", MppgPrecedence.Sum);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(SubtractionExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else if (expression.NonNegative)
        {
            CurrentDepth++;
            var sb = new StringBuilder()
                .Append('(')
                .Append(Render(expression.LeftExpression, MppgPrecedence.Sum))
                .Append(" - ")
                .Append(Render(expression.RightExpression, MppgPrecedence.Product))
                .Append(") \\/ 0");
            CurrentDepth--;
            return (sb, MppgPrecedence.Sum);
        }
        else
            return VisitBinaryInfix(expression, " - ", MppgPrecedence.Sum);
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(MinimumExpression expression)
        => VisitNAryInfix(expression, " /\\ ", MppgPrecedence.Sum);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(MaximumExpression expression)
        => VisitNAryInfix(expression, " \\/ ", MppgPrecedence.Sum);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ConvolutionExpression expression)
        => VisitNAryInfix(expression, " * ", MppgPrecedence.Product);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(DeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " / ", MppgPrecedence.Product);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(MaxPlusConvolutionExpression expression)
        => VisitNAryInfix(expression, " *^ ", MppgPrecedence.Product);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(MaxPlusDeconvolutionExpression expression)
        => VisitBinaryInfix(expression, " /^ ", MppgPrecedence.Product);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(CompositionExpression expression)
        => VisitBinaryInfix(expression, " comp ", MppgPrecedence.Product);

    /// <inheritdoc />
    /// <remarks>
    /// Rendered through <c>hShift</c>, which delays the curve for the non-negative shifts that <see cref="Curve.DelayBy"/> accepts.
    /// </remarks>
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(DelayByExpression expression)
        => VisitBinaryCall(expression, "hShift");

    /// <inheritdoc />
    /// <remarks>
    /// Rendered through <c>hShift</c> of the negated time, which brings the curve forward for the non-negative times that <see cref="Curve.ForwardBy"/> accepts.
    /// </remarks>
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ForwardByExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            var (negatedTime, _) = RenderNegated(expression.RightExpression);
            var sb = new StringBuilder()
                .Append("hShift(")
                .Append(Render(expression.LeftExpression, MppgPrecedence.Sum))
                .Append(", ")
                .Append(negatedTime)
                .Append(')');
            CurrentDepth--;
            return (sb, MppgPrecedence.Atom);
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(HorizontalShiftExpression expression)
        => VisitBinaryCall(expression, "hShift");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(VerticalShiftExpression expression)
        => VisitBinaryCall(expression, "vShift");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ScaleExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            // the scalar of a scaling is restricted to the enclosed forms, which exclude the infix ones
            var sb = new StringBuilder()
                .Append(Render(expression.RightExpression, MppgPrecedence.Atom))
                .Append(" * ")
                .Append(Render(expression.LeftExpression, MppgPrecedence.Atom));
            CurrentDepth--;
            return (sb, MppgPrecedence.Product);
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(CurvePlaceholderExpression expression)
        => Unsupported(expression, "a placeholder stands for no value, and MPPG has no notation for it");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(FloorExpression expression)
        => VisitUnaryCall(expression, "floor");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(CeilExpression expression)
        => VisitUnaryCall(expression, "ceil");

    #endregion Curve expressions

    #region Rational expressions

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalNumberExpression numberExpression)
    {
        if ((ShowRationalsAsName || CurrentDepth >= MaxDepth) && IsValidMppgName(numberExpression.Name))
            return (new StringBuilder(numberExpression.Name), MppgPrecedence.Atom);
        else
            return (
                new StringBuilder(numberExpression.Value.ToMppgString()),
                RationalLiteralPrecedence(numberExpression.Value)
            );
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalPlaceholderExpression expression)
        => Unsupported(expression, "a placeholder stands for no value, and MPPG has no notation for it");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalAdditionExpression expression)
        => VisitNAryInfix(expression, " + ", MppgPrecedence.Sum);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalSubtractionExpression expression)
        => VisitBinaryInfix(expression, " - ", MppgPrecedence.Sum);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalProductExpression expression)
        => VisitNAryInfix(expression, " * ", MppgPrecedence.Product);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalDivisionExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else if (IsTightLiteral(expression, CurrentDepth))
            return RenderTightLiteral(expression);
        else
            return VisitBinaryInfix(expression, " / ", MppgPrecedence.Product);
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalModuloExpression expression)
        => VisitBinaryInfix(expression, " mod ", MppgPrecedence.Product);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalPowerExpression expression)
        => VisitBinaryCall(expression, "pow");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalAbsoluteValueExpression expression)
        => VisitUnaryCall(expression, "abs");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalFloorExpression expression)
        => VisitUnaryCall(expression, "floor");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalCeilExpression expression)
        => VisitUnaryCall(expression, "ceil");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalMinimumExpression expression)
        => VisitNAryInfix(expression, " /\\ ", MppgPrecedence.Sum);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalMaximumExpression expression)
        => VisitNAryInfix(expression, " \\/ ", MppgPrecedence.Sum);

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalGreatestCommonDivisorExpression expression)
        => VisitNAryNestedCall(expression, "gcd");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RationalLeastCommonMultipleExpression expression)
        => VisitNAryNestedCall(expression, "lcm");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(NegateRationalExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            var negated = RenderNegated(expression.Expression);
            CurrentDepth--;
            return negated;
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(InvertRationalExpression expression)
    {
        if (TryFormatAsName(expression, out var named))
            return named;
        else
        {
            CurrentDepth++;
            var sb = new StringBuilder()
                .Append("1/")
                .Append(Render(expression.Expression, MppgPrecedence.Atom));
            CurrentDepth--;
            return (sb, MppgPrecedence.Product);
        }
    }

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ValueAtExpression expression)
        => VisitSampling(expression, "");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(LeftLimitAtExpression expression)
        => VisitSampling(expression, "~-");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(RightLimitAtExpression expression)
        => VisitSampling(expression, "~+");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(HorizontalDeviationExpression expression)
        => VisitBinaryCall(expression, "hDev");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(VerticalDeviationExpression expression)
        => VisitBinaryCall(expression, "vDev");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(ZDeviationExpression expression)
        => VisitBinaryCall(expression, "zDev");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(SupValueExpression expression)
        => Unsupported(expression, "MPPG has no notation for the supremum of a curve");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(InfValueExpression expression)
        => Unsupported(expression, "MPPG has no notation for the infimum of a curve");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(MaxValueExpression expression)
        => Unsupported(expression, "MPPG has no notation for the maximum of a curve");

    /// <inheritdoc />
    public virtual (StringBuilder MppgBuilder, MppgPrecedence Precedence) Visit(MinValueExpression expression)
        => Unsupported(expression, "MPPG has no notation for the minimum of a curve");

    #endregion Rational expressions
}
