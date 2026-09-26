using System.Text;
using Antlr4.Runtime;
using Unipi.Nancy.Expressions.Utility;
using Unipi.Nancy.Expressions.Grammar;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// The class allows to define equivalences involving NetCal expressions
/// </summary>
public partial class Equivalence
{
    /// <summary>
    /// Left side of the equivalence
    /// </summary>
    public CurveExpression LeftSideExpression { get; init; }
    
    /// <summary>
    /// Right side of the equivalence
    /// </summary>
    public CurveExpression RightSideExpression { get; init; }

    /// <summary>
    /// Hypotheses on a single curve operand.
    /// </summary>
    internal readonly Dictionary<string, IEnumerable<Predicate<CurveExpression>>> Hypothesis = new();

    /// <summary>
    /// Hypotheses on a two curve operands.
    /// </summary>
    internal readonly Dictionary<Tuple<string, string>, IEnumerable<Func<CurveExpression, CurveExpression, bool>>>
        HypothesisPair = new();

    /// <summary>
    /// Hypotheses on a three curve operands.
    /// </summary>
    internal readonly
        Dictionary<Tuple<string, string, string>,
            IEnumerable<Func<CurveExpression, CurveExpression, CurveExpression, bool>>>
        HypothesisTriple = new();

    /// <summary>
    /// Hypotheses on a single rational operand.
    /// </summary>
    internal readonly Dictionary<string, IEnumerable<Predicate<RationalExpression>>> RationalHypothesis = new();

    /// <summary>
    /// Hypotheses on two rational operands.
    /// </summary>
    internal readonly Dictionary<Tuple<string, string>, IEnumerable<Func<RationalExpression, RationalExpression, bool>>>
        RationalHypothesisPair = new();

    /// <summary>
    /// Hypotheses on three rational operands.
    /// </summary>
    internal readonly
        Dictionary<Tuple<string, string, string>,
            IEnumerable<Func<RationalExpression, RationalExpression, RationalExpression, bool>>>
        RationalHypothesisTriple = new();

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

    /// <summary>
    /// Add an hypothesis on a single curve operand.
    /// </summary>
    public void AddHypothesis(string placeholder, Predicate<CurveExpression> h)
    {
        if (Hypothesis.TryGetValue(placeholder, out var hypothesisList))
            Hypothesis[placeholder] = hypothesisList.Append(h);
        else
            Hypothesis[placeholder] = [h];
    }

    /// <summary>
    /// Add an hypothesis on a two curve operands.
    /// </summary>
    public void AddHypothesis(string placeholder1, string placeholder2,
        Func<CurveExpression, CurveExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2);
        if (HypothesisPair.TryGetValue(key, out var hypothesisList))
            HypothesisPair[key] = hypothesisList.Append(h);
        else
            HypothesisPair[key] = [h];
    }

    /// <summary>
    /// Add an hypothesis on a three curve operands.
    /// </summary>
    public void AddHypothesis(string placeholder1, string placeholder2, string placeholder3,
        Func<CurveExpression, CurveExpression, CurveExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2, placeholder3);
        if (HypothesisTriple.TryGetValue(key, out var hypothesisList))
            HypothesisTriple[key] = hypothesisList.Append(h);
        else
            HypothesisTriple[key] = [h];
    }

    /// <summary>
    /// Add an hypothesis on a single rational operand.
    /// </summary>
    public void AddHypothesis(string placeholder, Predicate<RationalExpression> h)
    {
        if (RationalHypothesis.TryGetValue(placeholder, out var hypothesisList))
            RationalHypothesis[placeholder] = hypothesisList.Append(h);
        else
            RationalHypothesis[placeholder] = [h];
    }

    /// <summary>
    /// Add an hypothesis on two rational operands.
    /// </summary>
    public void AddHypothesis(string placeholder1, string placeholder2,
        Func<RationalExpression, RationalExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2);
        if (RationalHypothesisPair.TryGetValue(key, out var hypothesisList))
            RationalHypothesisPair[key] = hypothesisList.Append(h);
        else
            RationalHypothesisPair[key] = [h];
    }

    /// <summary>
    /// Add an hypothesis on three rational operands.
    /// </summary>
    public void AddHypothesis(string placeholder1, string placeholder2, string placeholder3,
        Func<RationalExpression, RationalExpression, RationalExpression, bool> h)
    {
        var key = Tuple.Create(placeholder1, placeholder2, placeholder3);
        if (RationalHypothesisTriple.TryGetValue(key, out var hypothesisList))
            RationalHypothesisTriple[key] = hypothesisList.Append(h);
        else
            RationalHypothesisTriple[key] = [h];
    }

    /// <summary>
    /// Check and apply the equivalence.
    /// </summary>
    /// <param name="expression">The expression on which the equivalence is to be applied.</param>
    /// <param name="checkType">The type-checking mode to use.</param>
    /// <returns>The result.</returns>
    public EquivalenceApplyResult Apply(
        IGenericExpression<Curve> expression,
        CheckType checkType = CheckType.CheckLeftOnly
    )
    {
        var applier = new OneTimeEquivalenceApplier { Equivalence = this };
        return applier.Apply(expression, checkType);
    }

    private static bool _endWithPlaceholder(IExpression expression)
        => expression is CurvePlaceholderExpression or RationalPlaceholderExpression
           || expression is IExpressionNode node && node.Children.Any(_endWithPlaceholder);

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder stringBuilder = new(LeftSideExpression.ToString());
        stringBuilder.Append(" = ");
        stringBuilder.Append(RightSideExpression);
        return stringBuilder.ToString();
    }
}

/// <summary>
/// todo: document 
/// </summary>
public enum CheckType
{
    /// <summary>
    /// todo: document 
    /// </summary>
    CheckLeftOnly,
    /// <summary>
    /// todo: document 
    /// </summary>
    CheckRightOnly,
    /// <summary>
    /// todo: document 
    /// </summary>
    CheckBothSides
}

/// <summary>
/// The result of applying an equivalence.
/// </summary>
public record EquivalenceApplyResult
{
    /// <summary>
    /// The expression the equivalence produced, or <see langword="null"/> if it did not match.
    /// </summary>
    public IGenericExpression<Curve>? NewExpression { get; init; }

    /// <summary>
    /// True if the equivalence matched.
    /// </summary>
    public bool IsMatch { get; init; }

    /// <summary>
    /// What the law's placeholders bound to, by name.
    /// </summary>
    public IReadOnlyDictionary<string, IExpression> Bindings { get; init; }
        = new Dictionary<string, IExpression>();

    /// <summary>
    /// The unmatched operands of a partial n-ary match, to be reattached to the new expression.
    /// </summary>
    public IReadOnlyList<IExpression>? NotMatchedExpressions { get; init; }
}