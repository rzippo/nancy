using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class with the settings necessary for the expressions creation and evaluation.
/// </summary>
public record ExpressionSettings
{
    /// <summary>
    /// Settings for the computation of an expression.
    /// </summary>
    public ComputationSettings? ComputationSettings;
}