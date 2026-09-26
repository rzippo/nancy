using Unipi.Nancy.Expressions.Equivalences;

namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// The bindings a match against one side of an equivalence accumulates, and the hypotheses that prune them.
/// </summary>
internal sealed class PatternMatchContext
{
    public PatternMatchContext(Equivalence equivalence)
    {
        Equivalence = equivalence;
    }

    private Equivalence Equivalence { get; }

    /// <summary>
    /// What each placeholder has bound to, by the placeholder's name.
    /// </summary>
    /// <remarks>
    /// A name denotes one thing, whatever kind of expression it stands for.
    /// </remarks>
    public Dictionary<string, IExpression> Bindings { get; } = new();

    public PatternMatchContext Clone()
    {
        var clone = new PatternMatchContext(Equivalence);
        foreach (var (key, value) in Bindings)
            clone.Bindings[key] = value;
        return clone;
    }

    public void Restore(PatternMatchContext snapshot)
    {
        Bindings.Clear();
        foreach (var (key, value) in snapshot.Bindings)
            Bindings[key] = value;
    }

    /// <summary>
    /// The binding of <paramref name="name"/>, if there is one and it is a <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// The bindings are one map of <see cref="IExpression"/>, so a caller that knows what it asked for says so here.
    /// </remarks>
    private bool TryGetBinding<T>(string name, out T value) where T : class, IExpression
    {
        if (Bindings.TryGetValue(name, out var bound) && bound is T typed)
        {
            value = typed;
            return true;
        }

        value = null!;
        return false;
    }

    public bool Bind(string name, IExpression expression)
    {
        if (Bindings.TryGetValue(name, out var existing))
            return ExpressionPatternMatcher.TryMatchSubstitution(existing, expression, false, out _);
        Bindings[name] = expression;
        return HypothesesHold();
    }

    /// <summary>
    /// True if every hypothesis whose placeholders are all bound holds.
    /// A hypothesis with an unbound placeholder is not evaluated yet.
    /// </summary>
    private bool HypothesesHold()
    {
        foreach (var (name, hypotheses) in Equivalence.Hypothesis)
            if (TryGetBinding<CurveExpression>(name, out var expression) && !hypotheses.All(h => h(expression)))
                return false;

        foreach (var (key, hypotheses) in Equivalence.HypothesisPair)
            if (TryGetBinding<CurveExpression>(key.Item1, out var first) && TryGetBinding<CurveExpression>(key.Item2, out var second)
                && !hypotheses.All(h => h(first, second)))
                return false;

        foreach (var (key, hypotheses) in Equivalence.HypothesisTriple)
            if (TryGetBinding<CurveExpression>(key.Item1, out var first) && TryGetBinding<CurveExpression>(key.Item2, out var second)
                && TryGetBinding<CurveExpression>(key.Item3, out var third) && !hypotheses.All(h => h(first, second, third)))
                return false;

        foreach (var (name, hypotheses) in Equivalence.RationalHypothesis)
            if (TryGetBinding<RationalExpression>(name, out var expression) && !hypotheses.All(h => h(expression)))
                return false;

        foreach (var (key, hypotheses) in Equivalence.RationalHypothesisPair)
            if (TryGetBinding<RationalExpression>(key.Item1, out var first) && TryGetBinding<RationalExpression>(key.Item2, out var second)
                && !hypotheses.All(h => h(first, second)))
                return false;

        foreach (var (key, hypotheses) in Equivalence.RationalHypothesisTriple)
            if (TryGetBinding<RationalExpression>(key.Item1, out var first) && TryGetBinding<RationalExpression>(key.Item2, out var second)
                && TryGetBinding<RationalExpression>(key.Item3, out var third) && !hypotheses.All(h => h(first, second, third)))
                return false;

        return true;
    }

    /// <summary>
    /// True if every hypothesis holds and every placeholder it names is bound.
    /// </summary>
    public bool AllHypothesesSatisfied()
    {
        foreach (var (name, _) in Equivalence.Hypothesis)
            if (!TryGetBinding<CurveExpression>(name, out _))
                return false;
        foreach (var (key, _) in Equivalence.HypothesisPair)
            if (!TryGetBinding<CurveExpression>(key.Item1, out _) || !TryGetBinding<CurveExpression>(key.Item2, out _))
                return false;
        foreach (var (key, _) in Equivalence.HypothesisTriple)
            if (!TryGetBinding<CurveExpression>(key.Item1, out _) || !TryGetBinding<CurveExpression>(key.Item2, out _) || !TryGetBinding<CurveExpression>(key.Item3, out _))
                return false;
        foreach (var (name, _) in Equivalence.RationalHypothesis)
            if (!TryGetBinding<RationalExpression>(name, out _))
                return false;
        foreach (var (key, _) in Equivalence.RationalHypothesisPair)
            if (!TryGetBinding<RationalExpression>(key.Item1, out _) || !TryGetBinding<RationalExpression>(key.Item2, out _))
                return false;
        foreach (var (key, _) in Equivalence.RationalHypothesisTriple)
            if (!TryGetBinding<RationalExpression>(key.Item1, out _) || !TryGetBinding<RationalExpression>(key.Item2, out _) || !TryGetBinding<RationalExpression>(key.Item3, out _))
                return false;

        return HypothesesHold();
    }
}
