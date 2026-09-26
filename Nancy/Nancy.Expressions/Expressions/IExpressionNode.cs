namespace Unipi.Nancy.Expressions;

/// <summary>
/// The shape of an expression node, as far as its operands are concerned.
/// </summary>
internal enum NodeArity
{
    /// <summary>
    /// A node with a single operand.
    /// </summary>
    Unary,

    /// <summary>
    /// A node with two operands whose order matters.
    /// </summary>
    Binary,

    /// <summary>
    /// A node with two or more operands, combined by an associative and commutative operation.
    /// </summary>
    NAry
}

/// <summary>
/// A non-leaf expression node, which exposes its operands and can rebuild itself around new ones.
/// </summary>
/// <remarks>
/// The operand types are the node's own, so a traversal can walk an expression tree without naming any value type.
/// The members are implemented explicitly on the base types, so they add nothing to the public surface of a node.
/// </remarks>
internal interface IExpressionNode : IExpression
{
    /// <summary>
    /// The shape of this node.
    /// </summary>
    NodeArity Arity { get; }

    /// <summary>
    /// This node's operands, in order.
    /// </summary>
    IReadOnlyList<IExpression> Children { get; }

    /// <summary>
    /// Returns a copy of this node with <paramref name="children"/> in place of its operands.
    /// </summary>
    /// <param name="children">The new operands, one per child, in the same order.</param>
    IExpression Rebuild(IReadOnlyList<IExpression> children);
}

/// <summary>
/// A placeholder expression, standing for whatever a law binds it to.
/// </summary>
/// <remarks>
/// The marker exists so a matcher can tell a pattern variable from a concrete leaf without naming any value type.
/// </remarks>
internal interface IPlaceholderExpression : IExpression
{
    /// <summary>
    /// True if <paramref name="candidate"/> is something this placeholder may stand for.
    /// </summary>
    /// <remarks>
    /// The placeholder answers, rather than the matcher asking what value type it is:
    /// a placeholder for a new value type arrives with its own answer and adds no arm anywhere else.
    /// </remarks>
    bool Accepts(IExpression candidate);
}

/// <summary>
/// A concrete leaf expression, whose value is compared by the value's own equivalence rather than by record equality.
/// </summary>
/// <remarks>
/// The comparison lives on the leaf so a matcher can use it without naming the value type.
/// </remarks>
internal interface IExpressionLeaf : IExpression
{
    /// <summary>
    /// True if this leaf's value matches the value of <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The expression to compare against.</param>
    bool ValueMatches(IExpression other);
}
