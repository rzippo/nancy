using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// What an evaluator carries down a computation: the settings given where the computation was asked for, and the operands whose values it has read.
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
    /// The operands whose values have been read.
    /// </summary>
    private readonly List<IExpression> _operandsRead = [];

    /// <summary>
    /// Computes <paramref name="operand"/> under the same settings, and records it as read.
    /// </summary>
    public T Read<T>(IGenericExpression<T> operand)
    {
        _operandsRead.Add(operand);
        return operand.Compute(Settings);
    }

    /// <summary>
    /// Clears the values of the operands read, as <see cref="CacheSettings.ClearOperandsWhenComputed"/> describes, if <paramref name="cacheSettings"/> asks for it.
    /// </summary>
    /// <param name="cacheSettings">The cache settings of the node whose computation read the operands.</param>
    public void ReleaseOperands(CacheSettings? cacheSettings)
    {
        if (cacheSettings is not { ClearOperandsWhenComputed: true })
            return;

        foreach (var operand in _operandsRead)
        {
            if (!string.IsNullOrEmpty(operand.Name) || operand is IExpressionLeaf)
                continue;
            (operand as IValueCacheOwner)?.DropValueIfLargerThan(cacheSettings.CheapCacheElementThreshold);
        }
    }
}
