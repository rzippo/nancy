using Unipi.Nancy.Expressions.Equivalences;

namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// Rewrites an expression by walking every node and asking a <see cref="RewriteRule"/> to replace it.
/// </summary>
/// <remarks>
/// The traversal walks the node shapes through <see cref="IExpressionNode"/> and lets each node rebuild itself.
/// Replacement is a pure function of the expression, the pattern and the replacement, so calling it twice on the same inputs gives the same answer.
/// </remarks>
internal static class ExpressionRewriter
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
    /// <param name="equivalence">The equivalence to apply.</param>
    /// <param name="checkType">The direction in which the equivalence is applied.</param>
    /// <param name="replaceAll">Whether to apply the equivalence at every match or stop at the first one.</param>
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
    /// <param name="equivalence">The equivalence to apply.</param>
    /// <param name="checkType">The direction in which the equivalence is applied.</param>
    public static ExpressionRewriteResult ApplyEquivalenceByPosition(
        IExpression original,
        IReadOnlyList<PathStep> position,
        Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly)
    {
        var planter = new EquivalencePositionPlanter(equivalence, checkType);
        return TraverseByPosition(original, position, planter);
    }

    /// <summary>
    /// Replaces every placeholder in <paramref name="substitute"/> by what the match bound its name to.
    /// </summary>
    /// <remarks>
    /// The substitute is walked once, and each placeholder answers to its own name.
    /// </remarks>
    internal static IExpression Instantiate(IExpression substitute, IReadOnlyDictionary<string, IExpression> bindings)
        => Traverse(substitute, new PlaceholderInstantiationRule(bindings), replaceAll: true).Expression;

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
        if (rule.TryRewrite(expression, out var replacement, out var bindings))
        {
            state.Record(position, bindings);
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
            if (!planter.TryPlant(expression, out var replacement, out var bindings))
                return expression;
            if (ExpressionValueType.Of(replacement) != ExpressionValueType.Of(expression))
                throw ReplacementValueTypeDoesNotMatch(position, replacement, expression);
            state.Record(position, bindings);
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
    /// The unmatched operands are reattached to the replacement, which is what rewriting by an equivalence means.
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
        private Dictionary<string, IExpression>? _bindings;

        public TraversalState(bool replaceAll)
        {
            ReplaceAll = replaceAll;
        }

        private bool ReplaceAll { get; }

        private int Count { get; set; }

        public bool ShouldStop => !ReplaceAll && Count > 0;

        public void Record(ExpressionPosition position, IReadOnlyDictionary<string, IExpression>? bindings)
        {
            Count++;
            _positions.Add(position);
            if (bindings is not null)
                _bindings = new Dictionary<string, IExpression>(bindings);
        }

        public ExpressionRewriteResult ToResult(IExpression expression)
            => new()
            {
                Expression = expression,
                ReplacementCount = Count,
                Positions = _positions,
                Bindings = _bindings
            };
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
            out IReadOnlyDictionary<string, IExpression>? bindings);
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
            out IReadOnlyDictionary<string, IExpression>? bindings)
        {
            bindings = null;
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
    /// Puts a match's bindings in place of the placeholders that produced them.
    /// </summary>
    private sealed class PlaceholderInstantiationRule(IReadOnlyDictionary<string, IExpression> bindings) : RewriteRule
    {
        public override bool TryRewrite(
            IExpression expression,
            out IExpression replacement,
            out IReadOnlyDictionary<string, IExpression>? boundHere)
        {
            boundHere = null;
            if (expression is IPlaceholderExpression && bindings.TryGetValue(expression.Name, out var bound))
            {
                replacement = bound;
                return true;
            }

            replacement = null!;
            return false;
        }
    }

    /// <summary>
    /// An equivalence, whose placeholders bind, matched wherever its shape occurs.
    /// </summary>
    private sealed class EquivalenceRule(Equivalence equivalence, CheckType checkType) : RewriteRule
    {
        public override bool TryRewrite(
            IExpression expression,
            out IExpression replacement,
            out IReadOnlyDictionary<string, IExpression>? bindings)
        {
            var result = EquivalenceApplier.Apply(equivalence, expression, checkType);
            if (!result.IsMatch || result.NewExpression is null)
            {
                replacement = null!;
                bindings = null;
                return false;
            }

            replacement = CombineWithLeftover(expression, result.NewExpression, result.NotMatchedExpressions, ignoreNotMatchedExpressions: false);
            bindings = result.Bindings;
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
            out IReadOnlyDictionary<string, IExpression>? bindings);
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
            out IReadOnlyDictionary<string, IExpression>? bindings)
        {
            replacement = _replacement;
            bindings = null;
            return true;
        }
    }

    private sealed class EquivalencePositionPlanter(Equivalence equivalence, CheckType checkType) : PositionPlanter
    {
        public override bool TryPlant(
            IExpression target,
            out IExpression replacement,
            out IReadOnlyDictionary<string, IExpression>? bindings)
        {
            var result = EquivalenceApplier.Apply(equivalence, target, checkType);
            if (!result.IsMatch || result.NewExpression is null)
            {
                replacement = null!;
                bindings = null;
                return false;
            }

            replacement = CombineWithLeftover(target, result.NewExpression, result.NotMatchedExpressions, ignoreNotMatchedExpressions: false);
            bindings = result.Bindings;
            return true;
        }
    }

    #endregion Rules
}
