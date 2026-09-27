using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// What an evaluator carries down a computation: the settings given where the computation was asked for.
/// </summary>
/// <param name="settings">The settings given to <see cref="IGenericExpression{TExpressionResult}.Compute"/>, if any.</param>
internal sealed class EvaluationContext(ExpressionSettings? settings)
{
    /// <summary>
    /// The settings given to <see cref="IGenericExpression{TExpressionResult}.Compute"/>, passed on unchanged to every operand.
    /// </summary>
    public ExpressionSettings? Settings { get; } = settings;

    /// <summary>
    /// The computation settings for a node whose own settings are <paramref name="own"/>.
    /// </summary>
    public ComputationSettings? ComputationSettingsOf(ExpressionSettings? own)
        => ExpressionSettings.Resolve(Settings, own)?.ComputationSettings;

    /// <summary>
    /// Computes <paramref name="operand"/> under the same settings.
    /// </summary>
    public T Read<T>(IGenericExpression<T> operand)
        => operand.Compute(Settings);
}
