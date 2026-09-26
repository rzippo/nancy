namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// A condition an equivalence places on what its placeholders bind to.
/// </summary>
/// <remarks>
/// The value type is tested inside <see cref="Holds"/>, by the closure <see cref="Equivalence.AddHypothesis{T}(string,System.Predicate{T})"/> builds.
/// </remarks>
/// <param name="Placeholders">The names this condition speaks about, in the order <see cref="Holds"/> expects them.</param>
/// <param name="Holds">
/// Whether the condition holds of the expressions those names are bound to.
/// It is false where one of them is not of the value type the condition was written for.
/// </param>
internal sealed record Hypothesis(
    IReadOnlyList<string> Placeholders,
    Func<IReadOnlyList<IExpression>, bool> Holds);
