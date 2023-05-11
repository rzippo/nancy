using System;
using System.Collections.Generic;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Provides LINQ extensions methods for <see cref="Curve"/>, 
/// which are mostly shorthands to methods such as <see cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings?)"/>.
/// </summary>
public static class CurveExtensions
{
    /// <summary>
    /// True if all the curves in the set represent the same function. 
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool Equivalent(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.Equivalent(curves, settings);

    /// <inheritdoc cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve Addition(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.Addition(curves, settings);

    /// <inheritdoc cref="Curve.Addition(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve Addition(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.Addition(curves, settings);

    /// <inheritdoc cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve Minimum(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.Minimum(curves, settings);

    /// <inheritdoc cref="Curve.Minimum(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve Minimum(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.Minimum(curves, settings);

    /// <inheritdoc cref="Curve.Maximum(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve Maximum(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.Maximum(curves, settings);

    /// <inheritdoc cref="Curve.Maximum(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve Maximum(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.Maximum(curves, settings);

    /// <inheritdoc cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve Convolution(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.Convolution(curves, settings);

    /// <inheritdoc cref="Curve.Convolution(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve Convolution(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.Convolution(curves, settings);

    /// <inheritdoc cref="Curve.MaxPlusConvolution(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve MaxPlusConvolution(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.MaxPlusConvolution(curves, settings);

    /// <inheritdoc cref="Curve.MaxPlusConvolution(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve MaxPlusConvolution(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.MaxPlusConvolution(curves, settings);
}