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
