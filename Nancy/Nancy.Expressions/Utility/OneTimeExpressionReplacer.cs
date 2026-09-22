using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// The outcome of a rewrite:
/// the new expression, how many replacements were made, where, and the bindings of an equivalence if one was applied.
/// </summary>
/// <param name="Expression">The expression the rewrite produced.</param>
/// <param name="ReplacementCount">How many sites were replaced.</param>
/// <param name="Positions">The positions of the replaced sites, in the order they were visited.</param>
/// <param name="CurveBindings">The curve bindings of the last equivalence applied, or <see langword="null"/>.</param>
/// <param name="RationalBindings">The rational bindings of the last equivalence applied, or <see langword="null"/>.</param>
internal sealed record ExpressionRewriteResult(
    IExpression Expression,
    int ReplacementCount,
    IReadOnlyList<ExpressionPosition> Positions,
    IReadOnlyDictionary<string, CurveExpression>? CurveBindings,
    IReadOnlyDictionary<string, RationalExpression>? RationalBindings)
{
    /// <summary>
    /// True if at least one site was replaced.
    /// </summary>
    public bool Matched => ReplacementCount > 0;
}

/// <summary>
/// Rewrites an expression by walking every node and asking a <see cref="RewriteRule"/> to replace it.
/// </summary>
/// <remarks>
/// The traversal names no value type: it walks the node shapes through <see cref="IExpressionNode"/> and lets each node rebuild itself.
/// Adding a value type therefore adds no arm here.
/// Replacement is a pure function of the expression, the pattern and the replacement, so calling it twice on the same inputs gives the same answer.
/// There is no instance state to reset.
/// </remarks>
internal static class OneTimeExpressionReplacer
{
    /// <summary>
    /// Replaces every occurrence of <paramref name="pattern"/> by <paramref name="replacement"/>.
    /// </summary>
    /// <param name="original">The expression to rewrite.</param>
    /// <param name="pattern">The sub-expression to look for.</param>
    /// <param name="replacement">The sub-expression to put in its place.</param>
    /// <param name="ignoreNotMatchedExpressions">Whether the unmatched operands of a partial n-ary match are dropped.</param>
    /// <param name="replaceAll">Whether to replace every match or stop at the first one.</param>
    public static ExpressionRewriteResult ReplaceByValue(
        IExpression original,
        IExpression pattern,
        IExpression replacement,
        bool ignoreNotMatchedExpressions = false,
        bool replaceAll = true)
    {
        var rule = new SubstitutionRule(pattern, replacement, ignoreNotMatchedExpressions);
        return Traverse(original, rule, replaceAll);
    }

    /// <summary>
    /// Applies an equivalence at every site where it matches.
    /// </summary>
    /// <param name="original">The expression to rewrite.</param>
    /// <param name="equivalence">The law to apply.</param>
    /// <param name="checkType">The direction in which the law is applied.</param>
    /// <param name="replaceAll">Whether to apply the law at every match or stop at the first one.</param>
    public static ExpressionRewriteResult ApplyEquivalence(
        IExpression original,
        Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly,
        bool replaceAll = true)
    {
        var rule = new EquivalenceRule(equivalence, checkType);
        return Traverse(original, rule, replaceAll);
    }

    /// <summary>
    /// Replaces the sub-expression at a position by <paramref name="replacement"/>.
    /// </summary>
    /// <param name="original">The expression to rewrite.</param>
    /// <param name="position">The steps from the root to the sub-expression.</param>
    /// <param name="replacement">The sub-expression to put there.</param>
    public static ExpressionRewriteResult ReplaceByPosition(
        IExpression original,
        IReadOnlyList<PathStep> position,
        IExpression replacement)
    {
        var planter = new PlainPositionPlanter(replacement);
        return TraverseByPosition(original, position, planter);
    }

    /// <summary>
    /// Applies an equivalence at a position, if it matches there.
    /// </summary>
    /// <param name="original">The expression to rewrite.</param>
    /// <param name="position">The steps from the root to the sub-expression.</param>
    /// <param name="equivalence">The law to apply.</param>
    /// <param name="checkType">The direction in which the law is applied.</param>
    public static ExpressionRewriteResult ApplyEquivalenceByPosition(
        IExpression original,
        IReadOnlyList<PathStep> position,
        Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly)
    {
        var planter = new EquivalencePositionPlanter(equivalence, checkType);
        return TraverseByPosition(original, position, planter);
    }

    #region By value

    private static ExpressionRewriteResult Traverse(IExpression original, RewriteRule rule, bool replaceAll)
    {
        var state = new TraversalState(replaceAll);
        var expression = Rewrite(original, rule, state, new ExpressionPosition());
        return state.ToResult(expression);
    }

    private static IExpression Rewrite(
        IExpression expression,
        RewriteRule rule,
        TraversalState state,
        ExpressionPosition position)
    {
        if (rule.TryRewrite(expression, out var replacement, out var curveBindings, out var rationalBindings))
        {
            state.Record(position, curveBindings, rationalBindings);
            return replacement;
        }

        if (state.ShouldStop || expression is not IExpressionNode node)
            return expression;

        var children = node.Children;
        var newChildren = new IExpression[children.Count];
        var changed = false;
        for (var i = 0; i < children.Count; i++)
        {
            if (state.ShouldStop)
            {
                for (var j = i; j < children.Count; j++)
                    newChildren[j] = children[j];
                break;
            }

            var child = Rewrite(children[i], rule, state, ChildPosition(position, node, i));
            newChildren[i] = child;
            if (!ReferenceEquals(child, children[i]))
                changed = true;
        }

        return changed ? node.Rebuild(newChildren) : expression;
    }

    #endregion By value

    #region By position

    private static ExpressionRewriteResult TraverseByPosition(
        IExpression original,
        IReadOnlyList<PathStep> position,
        PositionPlanter planter)
    {
        var state = new TraversalState(replaceAll: true);
        var expression = ReplaceAt(original, position, 0, new ExpressionPosition(), planter, state);
        return state.ToResult(expression);
    }

    private static IExpression ReplaceAt(
        IExpression expression,
        IReadOnlyList<PathStep> steps,
        int index,
        ExpressionPosition position,
        PositionPlanter planter,
        TraversalState state)
    {
        if (index == steps.Count)
        {
            if (!planter.TryPlant(expression, out var replacement, out var curveBindings, out var rationalBindings))
                return expression;
            if (ExpressionValueType.Of(replacement) != ExpressionValueType.Of(expression))
                throw ReplacementValueTypeDoesNotMatch(position, replacement, expression);
            state.Record(position, curveBindings, rationalBindings);
            return replacement;
        }

        if (expression is not IExpressionNode node)
            throw StepDoesNotFit(steps[index], expression);

        var step = steps[index];
        var childIndex = step.Kind switch
        {
            StepKind.InnerOperand when node.Arity == NodeArity.Unary => 0,
            StepKind.LeftOperand when node.Arity == NodeArity.Binary => 0,
            StepKind.RightOperand when node.Arity == NodeArity.Binary => 1,
            StepKind.IndexedOperand when node.Arity == NodeArity.NAry && step.Index < node.Children.Count => step.Index,
            StepKind.IndexedOperand when node.Arity == NodeArity.NAry =>
                throw OperandIndexOutOfRange(step, node.Children.Count, expression),
            _ => throw StepDoesNotFit(step, expression)
        };

        var children = node.Children;
        var newChild = ReplaceAt(
            children[childIndex],
            steps,
            index + 1,
            ChildPosition(position, node, childIndex),
            planter,
            state);
        if (ReferenceEquals(newChild, children[childIndex]))
            return expression;

        var newChildren = children.ToArray();
        newChildren[childIndex] = newChild;
        return node.Rebuild(newChildren);
    }

    #endregion By position

    #region Shared helpers

    private static ExpressionPosition ChildPosition(ExpressionPosition position, IExpressionNode node, int index) =>
        node.Arity switch
        {
            NodeArity.Unary => position.InnerOperand(),
            NodeArity.Binary => index == 0 ? position.LeftOperand() : position.RightOperand(),
            NodeArity.NAry => position.IndexedOperand(index),
            _ => position
        };

    /// <summary>
    /// Builds the expression a rule produces when a pattern covered part of an n-ary node's operands.
    /// The unmatched operands are reattached to the replacement, which is what rewriting by a law means.
    /// A replacement of the target's own operator is merged with them unless it carries a name, which makes it one operand.
    /// </summary>
    internal static IExpression CombineWithLeftover(
        IExpression target,
        IExpression replacement,
        IReadOnlyList<IExpression>? leftover,
        bool ignoreNotMatchedExpressions)
    {
        if (ignoreNotMatchedExpressions || leftover is null || leftover.Count == 0)
            return replacement;

        if (replacement.GetType() == target.GetType() && string.IsNullOrEmpty(replacement.Name))
        {
            var replacementNode = (IExpressionNode)replacement;
            return replacementNode.Rebuild([.. replacementNode.Children, .. leftover]);
        }

        var targetNode = (IExpressionNode)target;
        return targetNode.Rebuild([.. leftover, replacement]);
    }

    /// <summary>
    /// Builds the exception raised when a position step does not fit the node it lands on.
    /// </summary>
    private static ArgumentException StepDoesNotFit(PathStep step, IExpression expression)
        => new($"The position step \"{step}\" is not valid on the {DescribeNodeShape(expression)} node {expression.GetType().Name}.");

    /// <summary>
    /// Builds the exception raised when a replacement's value type differs from the expression it replaces.
    /// </summary>
    private static ArgumentException ReplacementValueTypeDoesNotMatch(ExpressionPosition position, IExpression replacement,
        IExpression target)
        => new($"The replacement at position \"{position}\" is a {ExpressionValueType.Of(replacement).Name} expression, where a {ExpressionValueType.Of(target).Name} expression is expected.");

    /// <summary>
    /// Builds the exception raised when an indexed position step is past the last operand of an n-ary node.
    /// </summary>
    private static ArgumentException OperandIndexOutOfRange(PathStep step, int operandCount, IExpression expression)
        => new($"The position step \"{step}\" is out of range on the {DescribeNodeShape(expression)} node {expression.GetType().Name}, which has {operandCount} operands.");

    /// <summary>
    /// Names the shape of an expression node: unary, binary, n-ary, or leaf.
    /// </summary>
    private static string DescribeNodeShape(IExpression expression) => expression is IExpressionNode node
        ? node.Arity switch
        {
            NodeArity.Unary => "unary",
            NodeArity.Binary => "binary",
            NodeArity.NAry => "n-ary",
            _ => "node"
        }
        : "leaf";

    #endregion Shared helpers

    #region State

    private sealed class TraversalState
    {
        private readonly List<ExpressionPosition> _positions = [];
        private Dictionary<string, CurveExpression>? _curveBindings;
        private Dictionary<string, RationalExpression>? _rationalBindings;

        public TraversalState(bool replaceAll)
        {
            ReplaceAll = replaceAll;
        }

        private bool ReplaceAll { get; }

        private int Count { get; set; }

        public bool ShouldStop => !ReplaceAll && Count > 0;

        public void Record(
            ExpressionPosition position,
            IReadOnlyDictionary<string, CurveExpression>? curveBindings,
            IReadOnlyDictionary<string, RationalExpression>? rationalBindings)
        {
            Count++;
            _positions.Add(position);
            if (curveBindings is not null)
                _curveBindings = new Dictionary<string, CurveExpression>(curveBindings);
            if (rationalBindings is not null)
                _rationalBindings = new Dictionary<string, RationalExpression>(rationalBindings);
        }

        public ExpressionRewriteResult ToResult(IExpression expression)
            => new(expression, Count, _positions, _curveBindings, _rationalBindings);
    }

    #endregion State

    #region Rules

    /// <summary>
    /// Decides whether and how to replace a node.
    /// </summary>
    private abstract class RewriteRule
    {
        public abstract bool TryRewrite(
            IExpression expression,
            out IExpression replacement,
            out IReadOnlyDictionary<string, CurveExpression>? curveBindings,
            out IReadOnlyDictionary<string, RationalExpression>? rationalBindings);
    }

    /// <summary>
    /// A substitution: a concrete pattern, matched by name at its leaves, put in place of what it matches.
    /// </summary>
    private sealed class SubstitutionRule : RewriteRule
    {
        private readonly IExpression _pattern;
        private readonly IExpression _replacement;
        private readonly bool _ignoreNotMatchedExpressions;

        public SubstitutionRule(IExpression pattern, IExpression replacement, bool ignoreNotMatchedExpressions)
        {
            _pattern = pattern;
            _replacement = replacement;
            _ignoreNotMatchedExpressions = ignoreNotMatchedExpressions;
        }

        public override bool TryRewrite(
            IExpression expression,
            out IExpression replacement,
            out IReadOnlyDictionary<string, CurveExpression>? curveBindings,
            out IReadOnlyDictionary<string, RationalExpression>? rationalBindings)
        {
            curveBindings = null;
            rationalBindings = null;
            if (!ExpressionPatternMatcher.TryMatchSubstitution(_pattern, expression, true, out var leftover))
            {
                replacement = null!;
                return false;
            }

            replacement = CombineWithLeftover(expression, _replacement, leftover, _ignoreNotMatchedExpressions);
            return true;
        }
    }

    /// <summary>
    /// An equivalence: a law whose placeholders bind, matched wherever its shape occurs.
    /// </summary>
    private sealed class EquivalenceRule(Equivalence equivalence, CheckType checkType) : RewriteRule
    {
        public override bool TryRewrite(
            IExpression expression,
            out IExpression replacement,
            out IReadOnlyDictionary<string, CurveExpression>? curveBindings,
            out IReadOnlyDictionary<string, RationalExpression>? rationalBindings)
        {
            var result = new OneTimeEquivalenceApplier { Equivalence = equivalence }.Apply(expression, checkType);
            if (!result.IsMatch || result.NewExpression is null)
            {
                replacement = null!;
                curveBindings = null;
                rationalBindings = null;
                return false;
            }

            replacement = CombineWithLeftover(expression, result.NewExpression, result.NotMatchedExpressions, ignoreNotMatchedExpressions: false);
            curveBindings = result.CurveBindings;
            rationalBindings = result.RationalBindings;
            return true;
        }
    }

    /// <summary>
    /// Plants an expression at the target of a position.
    /// </summary>
    private abstract class PositionPlanter
    {
        public abstract bool TryPlant(
            IExpression target,
            out IExpression replacement,
            out IReadOnlyDictionary<string, CurveExpression>? curveBindings,
            out IReadOnlyDictionary<string, RationalExpression>? rationalBindings);
    }

    private sealed class PlainPositionPlanter : PositionPlanter
    {
        private readonly IExpression _replacement;

        public PlainPositionPlanter(IExpression replacement)
        {
            _replacement = replacement;
        }

        public override bool TryPlant(
            IExpression target,
            out IExpression replacement,
            out IReadOnlyDictionary<string, CurveExpression>? curveBindings,
            out IReadOnlyDictionary<string, RationalExpression>? rationalBindings)
        {
            replacement = _replacement;
            curveBindings = null;
            rationalBindings = null;
            return true;
        }
    }

    private sealed class EquivalencePositionPlanter(Equivalence equivalence, CheckType checkType) : PositionPlanter
    {
        public override bool TryPlant(
            IExpression target,
            out IExpression replacement,
            out IReadOnlyDictionary<string, CurveExpression>? curveBindings,
            out IReadOnlyDictionary<string, RationalExpression>? rationalBindings)
        {
            var result = new OneTimeEquivalenceApplier { Equivalence = equivalence }.Apply(target, checkType);
            if (!result.IsMatch || result.NewExpression is null)
            {
                replacement = null!;
                curveBindings = null;
                rationalBindings = null;
                return false;
            }

            replacement = CombineWithLeftover(target, result.NewExpression, result.NotMatchedExpressions, ignoreNotMatchedExpressions: false);
            curveBindings = result.CurveBindings;
            rationalBindings = result.RationalBindings;
            return true;
        }
    }

    #endregion Rules
}

/// <summary>
/// Matches a pattern against an expression by walking the node shapes, without naming a value type.
/// </summary>
/// <remarks>
/// There are two operations rather than one rule.
/// Substitution identifies a leaf by its name, so a caller can say "replace what is called <c>f</c>".
/// A law ignores the name, so <c>f ⊗ g</c> matches a convolution whatever its operands are called.
/// Where a pattern covers only part of an n-ary node's operands, the match is chosen deterministically.
/// Pattern operands are assigned in order, and each takes the first still-unmatched operand it matches.
/// The assignment backtracks when a later operand has no candidate, and the first complete assignment in that order is the one returned.
/// </remarks>
internal static class ExpressionPatternMatcher
{
    private enum MatchKind
    {
        Substitution,
        Law
    }

    /// <summary>
    /// Matches a concrete pattern, whose leaves are identified by name.
    /// </summary>
    public static bool TryMatchSubstitution(
        IExpression pattern,
        IExpression expression,
        bool patternRoot,
        out List<IExpression>? leftover)
        => Match(pattern, expression, patternRoot, MatchKind.Substitution, null, out leftover);

    /// <summary>
    /// Matches a law, whose placeholders bind and whose names are ignored.
    /// </summary>
    public static bool TryMatchLaw(
        IExpression pattern,
        IExpression expression,
        bool patternRoot,
        LawMatchContext context,
        out List<IExpression>? leftover)
        => Match(pattern, expression, patternRoot, MatchKind.Law, context, out leftover);

    private static bool Match(
        IExpression pattern,
        IExpression expression,
        bool patternRoot,
        MatchKind kind,
        LawMatchContext? law,
        out List<IExpression>? leftover)
    {
        leftover = null;

        if (kind == MatchKind.Law && pattern is IPlaceholderExpression)
            return BindPlaceholder(pattern, expression, law!);

        if (pattern.GetType() != expression.GetType())
            return false;

        if (pattern is not IExpressionNode patternNode)
        {
            var valueMatches = pattern is IExpressionLeaf leaf
                ? leaf.ValueMatches(expression)
                : pattern.Equals(expression);
            return kind == MatchKind.Law
                ? valueMatches
                : pattern.Name == expression.Name && valueMatches;
        }

        var expressionNode = (IExpressionNode)expression;
        if (patternNode.Arity == NodeArity.NAry)
            return MatchNAry(patternNode, expressionNode, patternRoot, kind, law, out leftover);

        var patternChildren = patternNode.Children;
        var expressionChildren = expressionNode.Children;
        if (patternChildren.Count != expressionChildren.Count)
            return false;

        for (var i = 0; i < patternChildren.Count; i++)
        {
            if (!Match(patternChildren[i], expressionChildren[i], false, kind, law, out _))
                return false;
        }

        return true;
    }

    private static bool MatchNAry(
        IExpressionNode patternNode,
        IExpressionNode expressionNode,
        bool patternRoot,
        MatchKind kind,
        LawMatchContext? law,
        out List<IExpression>? leftover)
    {
        leftover = null;
        var patternOperands = patternNode.Children;
        var expressionOperands = expressionNode.Children;

        if (patternOperands.Count > expressionOperands.Count)
            return false;
        if (!patternRoot && patternOperands.Count != expressionOperands.Count)
            return false;

        var used = new bool[expressionOperands.Count];
        if (!AssignOperands(0, patternOperands, expressionOperands, used, kind, law))
            return false;

        if (patternRoot)
        {
            var unmatched = new List<IExpression>();
            for (var i = 0; i < expressionOperands.Count; i++)
                if (!used[i])
                    unmatched.Add(expressionOperands[i]);
            if (unmatched.Count > 0)
                leftover = unmatched;
        }

        return true;
    }

    private static bool AssignOperands(
        int patternIndex,
        IReadOnlyList<IExpression> patternOperands,
        IReadOnlyList<IExpression> expressionOperands,
        bool[] used,
        MatchKind kind,
        LawMatchContext? law)
    {
        if (patternIndex == patternOperands.Count)
            return true;

        for (var i = 0; i < expressionOperands.Count; i++)
        {
            if (used[i])
                continue;

            var snapshot = law?.Clone();
            if (Match(patternOperands[patternIndex], expressionOperands[i], false, kind, law, out _))
            {
                used[i] = true;
                if (AssignOperands(patternIndex + 1, patternOperands, expressionOperands, used, kind, law))
                    return true;
                used[i] = false;
            }

            if (snapshot is not null)
                law!.Restore(snapshot);
        }

        return false;
    }

    private static bool BindPlaceholder(IExpression pattern, IExpression expression, LawMatchContext law)
    {
        switch (pattern)
        {
            case CurvePlaceholderExpression:
                return expression is CurveExpression curve && law.BindCurve(pattern.Name, curve);
            case RationalPlaceholderExpression:
                return expression is RationalExpression rational && law.BindRational(pattern.Name, rational);
            default:
                return false;
        }
    }
}

/// <summary>
/// The bindings a law accumulates while matching, and the hypotheses that prune them.
/// </summary>
internal sealed class LawMatchContext
{
    public LawMatchContext(Equivalence equivalence)
    {
        Equivalence = equivalence;
    }

    private Equivalence Equivalence { get; }

    public Dictionary<string, CurveExpression> Curves { get; } = new();

    public Dictionary<string, RationalExpression> Rationals { get; } = new();

    public LawMatchContext Clone()
    {
        var clone = new LawMatchContext(Equivalence);
        foreach (var (key, value) in Curves)
            clone.Curves[key] = value;
        foreach (var (key, value) in Rationals)
            clone.Rationals[key] = value;
        return clone;
    }

    public void Restore(LawMatchContext snapshot)
    {
        Curves.Clear();
        foreach (var (key, value) in snapshot.Curves)
            Curves[key] = value;
        Rationals.Clear();
        foreach (var (key, value) in snapshot.Rationals)
            Rationals[key] = value;
    }

    public bool BindCurve(string name, CurveExpression expression)
    {
        if (Curves.TryGetValue(name, out var existing))
            return ExpressionPatternMatcher.TryMatchSubstitution(existing, expression, false, out _);
        Curves[name] = expression;
        return HypothesesHold();
    }

    public bool BindRational(string name, RationalExpression expression)
    {
        if (Rationals.TryGetValue(name, out var existing))
            return ExpressionPatternMatcher.TryMatchSubstitution(existing, expression, false, out _);
        Rationals[name] = expression;
        return HypothesesHold();
    }

    /// <summary>
    /// True if every hypothesis whose placeholders are all bound holds.
    /// A hypothesis with an unbound placeholder is not evaluated yet.
    /// </summary>
    private bool HypothesesHold()
    {
        foreach (var (name, hypotheses) in Equivalence.Hypothesis)
            if (Curves.TryGetValue(name, out var expression) && !hypotheses.All(h => h(expression)))
                return false;

        foreach (var (key, hypotheses) in Equivalence.HypothesisPair)
            if (Curves.TryGetValue(key.Item1, out var first) && Curves.TryGetValue(key.Item2, out var second)
                && !hypotheses.All(h => h(first, second)))
                return false;

        foreach (var (key, hypotheses) in Equivalence.HypothesisTriple)
            if (Curves.TryGetValue(key.Item1, out var first) && Curves.TryGetValue(key.Item2, out var second)
                && Curves.TryGetValue(key.Item3, out var third) && !hypotheses.All(h => h(first, second, third)))
                return false;

        foreach (var (name, hypotheses) in Equivalence.RationalHypothesis)
            if (Rationals.TryGetValue(name, out var expression) && !hypotheses.All(h => h(expression)))
                return false;

        foreach (var (key, hypotheses) in Equivalence.RationalHypothesisPair)
            if (Rationals.TryGetValue(key.Item1, out var first) && Rationals.TryGetValue(key.Item2, out var second)
                && !hypotheses.All(h => h(first, second)))
                return false;

        foreach (var (key, hypotheses) in Equivalence.RationalHypothesisTriple)
            if (Rationals.TryGetValue(key.Item1, out var first) && Rationals.TryGetValue(key.Item2, out var second)
                && Rationals.TryGetValue(key.Item3, out var third) && !hypotheses.All(h => h(first, second, third)))
                return false;

        return true;
    }

    /// <summary>
    /// True if every hypothesis holds and every placeholder it names is bound.
    /// </summary>
    public bool AllHypothesesSatisfied()
    {
        foreach (var (name, _) in Equivalence.Hypothesis)
            if (!Curves.ContainsKey(name))
                return false;
        foreach (var (key, _) in Equivalence.HypothesisPair)
            if (!Curves.ContainsKey(key.Item1) || !Curves.ContainsKey(key.Item2))
                return false;
        foreach (var (key, _) in Equivalence.HypothesisTriple)
            if (!Curves.ContainsKey(key.Item1) || !Curves.ContainsKey(key.Item2) || !Curves.ContainsKey(key.Item3))
                return false;
        foreach (var (name, _) in Equivalence.RationalHypothesis)
            if (!Rationals.ContainsKey(name))
                return false;
        foreach (var (key, _) in Equivalence.RationalHypothesisPair)
            if (!Rationals.ContainsKey(key.Item1) || !Rationals.ContainsKey(key.Item2))
                return false;
        foreach (var (key, _) in Equivalence.RationalHypothesisTriple)
            if (!Rationals.ContainsKey(key.Item1) || !Rationals.ContainsKey(key.Item2) || !Rationals.ContainsKey(key.Item3))
                return false;

        return HypothesesHold();
    }
}
