using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Used to represent curves that are known to be super-additive.
/// </summary>
public class SuperAdditiveCurve : Curve
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="baseSequence">Describes the curve from 0 to <see cref="Curve.PseudoPeriodStart"/> + <see cref="Curve.PseudoPeriodLength"/></param>
    /// <param name="pseudoPeriodStart">Instant after which the curve is pseudo-periodic</param>
    /// <param name="pseudoPeriodLength">Length of each pseudo-period</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period</param>
    /// <param name="doTest">
    /// If true, the super-additive property is tested.
    /// This test can be computationally expensive.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// If <paramref name="doTest"/> is true and the super-additive property was not successfully verified.
    /// </exception>
    public SuperAdditiveCurve(Sequence baseSequence, Rational pseudoPeriodStart, Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight, bool doTest = true)
        : base(baseSequence, pseudoPeriodStart, pseudoPeriodLength, pseudoPeriodHeight)
    {
        if (doTest && !base.IsSuperAdditive)
            throw new InvalidOperationException("The curve constructed is not actually is super-additive");
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="other">The <see cref="Curve"/> object to copy from.</param>
    /// <param name="doTest">
    /// If true, the super-additive property is tested.
    /// This test can be computationally expensive.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// If <paramref name="doTest"/> is true and the super-additive property was not successfully verified.
    /// </exception>
    public SuperAdditiveCurve(Curve other, bool doTest = true)
        : base(other)
    {
        if (doTest && !base.IsSuperAdditive)
            throw new InvalidOperationException("The curve constructed is not actually is super-additive");
    }

    /// <summary>
    /// True if the curve is super-additive.
    /// </summary>
    /// <remarks>
    /// For a <see cref="SuperAdditiveCurve"/> this will always return true, without performing any checks.
    /// </remarks>
    public override bool IsSuperAdditive => true;

    /// <summary>
    /// Forced check for super-additive property.
    /// </summary>
    /// <remarks>
    /// Can be computationally expensive the first time it is invoked, the result is cached afterwards.
    /// </remarks>
    public bool IsSuperAdditiveCheck()
    {
        return base.IsSuperAdditive;
    }

    /// <inheritdoc />
    public override SuperAdditiveCurve SuperAdditiveClosure(ComputationSettings? settings = null)
    {
        return this;
    }
}