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

    /// <summary>
    /// The settings a node computes under, given those passed to <see cref="IGenericExpression{TExpressionResult}.Compute"/> and its own.
    /// </summary>
    /// <remarks>
    /// Each group is resolved on its own: a group <paramref name="argument"/> sets wins, and one it leaves <see langword="null"/> falls back to <paramref name="own"/>.
    /// </remarks>
    internal static ExpressionSettings? Resolve(ExpressionSettings? argument, ExpressionSettings? own)
    {
        if (argument is null)
            return own;
        if (own is null)
            return argument;
        return new ExpressionSettings
        {
            ComputationSettings = argument.ComputationSettings ?? own.ComputationSettings,
            CacheSettings = argument.CacheSettings ?? own.CacheSettings
        };
    }
}