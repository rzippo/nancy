using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class with the settings necessary for the expressions creation and evaluation.
/// </summary>
/// <remarks>
/// Affects only how an expression is computed or cached, never what it computes to, and is excluded from expression equality for that reason.
/// </remarks>
public record ExpressionSettings
{
    /// <summary>
    /// Settings for the computation of an expression.
    /// </summary>
    public ComputationSettings? ComputationSettings;

    /// <summary>
    /// Settings for how an expression's own caches are managed.
    /// </summary>
    public CacheSettings? CacheSettings;
}