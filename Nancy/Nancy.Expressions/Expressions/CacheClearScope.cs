namespace Unipi.Nancy.Expressions;

/// <summary>
/// How far <see cref="CurveExpression.ClearValueCache"/>/<see cref="RationalExpression.ClearValueCache"/>
/// recurses when clearing a node's cached <c>Value</c>.
/// </summary>
public enum CacheClearScope
{
    /// <summary>
    /// Clear only the node <c>ClearValueCache</c> is called on; leave every descendant's cache alone.
    /// </summary>
    SelfOnly,

    /// <summary>
    /// Clear the node and every descendant, unconditionally.
    /// </summary>
    Subtree,

    /// <summary>
    /// Clear the node and every descendant, stopping at any child that carries an explicit bound <see cref="IExpression.Name"/>.
    /// </summary>
    /// <remarks>
    /// A named child may still be referenced elsewhere, and clearing through it costs whoever else holds it a recompute.
    /// </remarks>
    SubtreeUntilNamed
}
