namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// Matches a pattern against an expression by walking the node shapes, without naming a value type.
/// </summary>
/// <remarks>
/// There are two kinds of match.
/// Substitution identifies a leaf by its name, so a caller can say "replace what is called $f$".
/// A side of an equivalence ignores the name, so $f \otimes g$ matches a convolution whatever its operands are called.
/// Where a pattern covers only part of an n-ary node's operands, the match is chosen deterministically.
/// Pattern operands are assigned in order, and each takes the first still-unmatched operand it matches.
/// The assignment backtracks when a later operand has no candidate, and the first complete assignment in that order is the one returned.
/// </remarks>
internal static class ExpressionPatternMatcher
{
    private enum MatchKind
    {
        /// <summary>A concrete expression, whose leaves are identified by name.</summary>
        Substitution,

        /// <summary>One side of an equivalence: placeholders bind, and names are ignored.</summary>
        Pattern
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
    /// Matches one side of an equivalence, whose placeholders bind and whose names are ignored.
    /// </summary>
    public static bool TryMatchPattern(
        IExpression pattern,
        IExpression expression,
        bool patternRoot,
        PatternMatchContext context,
        out List<IExpression>? leftover)
        => Match(pattern, expression, patternRoot, MatchKind.Pattern, context, out leftover);

    private static bool Match(
        IExpression pattern,
        IExpression expression,
        bool patternRoot,
        MatchKind kind,
        PatternMatchContext? context,
        out List<IExpression>? leftover)
    {
        leftover = null;

        if (kind == MatchKind.Pattern && pattern is IPlaceholderExpression)
            return BindPlaceholder(pattern, expression, context!);

        if (pattern.GetType() != expression.GetType())
            return false;

        if (pattern is not IExpressionNode patternNode)
        {
            var valueMatches = pattern is IExpressionLeaf leaf
                ? leaf.ValueMatches(expression)
                : pattern.Equals(expression);
            return kind == MatchKind.Pattern
                ? valueMatches
                : pattern.Name == expression.Name && valueMatches;
        }

        var expressionNode = (IExpressionNode)expression;
        if (!ParametersAgree(patternNode, expressionNode))
            return false;

        if (patternNode.Arity == NodeArity.NAry)
            return MatchNAry(patternNode, expressionNode, patternRoot, kind, context, out leftover);

        var patternChildren = patternNode.Children;
        var expressionChildren = expressionNode.Children;
        if (patternChildren.Count != expressionChildren.Count)
            return false;

        for (var i = 0; i < patternChildren.Count; i++)
        {
            if (!Match(patternChildren[i], expressionChildren[i], false, kind, context, out _))
                return false;
        }

        return true;
    }

    private static bool MatchNAry(
        IExpressionNode patternNode,
        IExpressionNode expressionNode,
        bool patternRoot,
        MatchKind kind,
        PatternMatchContext? context,
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
        if (!AssignOperands(0, patternOperands, expressionOperands, used, kind, context))
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
        PatternMatchContext? context)
    {
        if (patternIndex == patternOperands.Count)
            return true;

        for (var i = 0; i < expressionOperands.Count; i++)
        {
            if (used[i])
                continue;

            var snapshot = context?.Clone();
            if (Match(patternOperands[patternIndex], expressionOperands[i], false, kind, context, out _))
            {
                used[i] = true;
                if (AssignOperands(patternIndex + 1, patternOperands, expressionOperands, used, kind, context))
                    return true;
                used[i] = false;
            }

            if (snapshot is not null)
                context!.Restore(snapshot);
        }

        return false;
    }

    /// <summary>
    /// True if the two nodes of the same operator hold the same parameters besides their operands, such as a cut's interval or a subtraction's non-negative flag.
    /// </summary>
    /// <remarks>
    /// The candidate is rebuilt around the pattern's operands, so the equality that follows compares nothing but the parameters, and ignores the name as equality does.
    /// Comparing on the pattern's side keeps the cost to the size of the pattern, which is small, rather than of the candidate's subtree.
    /// A parameter is not an expression, so it cannot be a placeholder: an equivalence stated over a cut applies to cuts over that interval only.
    /// </remarks>
    private static bool ParametersAgree(IExpressionNode patternNode, IExpressionNode expressionNode)
        => expressionNode.Rebuild(patternNode.Children).Equals(patternNode);

    private static bool BindPlaceholder(IExpression pattern, IExpression expression, PatternMatchContext context)
        => pattern is IPlaceholderExpression placeholder
           && placeholder.Accepts(expression)
           && context.Bind(pattern.Name, expression);
}
