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

    public bool Bind(string name, IExpression expression)
    {
        if (Bindings.TryGetValue(name, out var existing))
            return ExpressionPatternMatcher.TryMatchSubstitution(existing, expression, false, out _);
        Bindings[name] = expression;
        return HypothesesHold();
    }

    /// <summary>
    /// True if every hypothesis whose placeholders are all bound holds.
    /// A hypothesis naming a placeholder that is not bound yet cannot be decided, and is not evaluated.
    /// </summary>
    private bool HypothesesHold()
    {
        foreach (var hypothesis in Equivalence.Hypotheses)
            if (TryGetBindings(hypothesis.Placeholders, out var bound) && !hypothesis.Holds(bound))
                return false;

        return true;
    }

    /// <summary>
    /// True if every hypothesis holds and every placeholder it names is bound.
    /// </summary>
    public bool AllHypothesesSatisfied()
    {
        foreach (var hypothesis in Equivalence.Hypotheses)
            if (!TryGetBindings(hypothesis.Placeholders, out var bound) || !hypothesis.Holds(bound))
                return false;

        return true;
    }

    /// <summary>
    /// What <paramref name="names"/> are bound to, in the same order, if all of them are bound.
    /// </summary>
    /// <remarks>
    /// The value type is not consulted here: a hypothesis written for one value type is false of a binding of another, which the hypothesis itself decides.
    /// </remarks>
    private bool TryGetBindings(IReadOnlyList<string> names, out IReadOnlyList<IExpression> bound)
    {
        var operands = new IExpression[names.Count];
        for (var i = 0; i < names.Count; i++)
            if (!Bindings.TryGetValue(names[i], out operands[i]!))
            {
                bound = [];
                return false;
            }

        bound = operands;
        return true;
    }
}
