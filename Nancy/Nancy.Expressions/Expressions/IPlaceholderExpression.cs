namespace Unipi.Nancy.Expressions;

/// <summary>
/// A placeholder expression, standing for whatever an equivalence binds it to.
/// </summary>
/// <remarks>
/// A matcher tells a pattern variable from a concrete leaf by this marker.
/// </remarks>
internal interface IPlaceholderExpression : IExpression
{
    /// <summary>
    /// True if <paramref name="candidate"/> is something this placeholder may stand for.
    /// </summary>
    bool Accepts(IExpression candidate);
}
