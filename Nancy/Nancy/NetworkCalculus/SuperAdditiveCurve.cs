using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using NLog;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Used to represent curves that are known to be super-additive, and with $f(0) = 0$ (see <see cref="Curve.IsRegularSuperAdditive"/>).
/// </summary>
/// <remarks>
/// $f(0) = 0$ is what the optimizations written against this type rely on, as they do on the sub-additive side.
/// The constructor does not test it separately, since <see cref="Curve.IsSuperAdditive"/> implies it for a curve finite at the origin:
/// it tests $f = f \overline{\otimes} f$, which at $t = 0$ forces $2 f(0) = f(0)$.
/// The one curve it admits without it is the everywhere $+\infty$ one, which is super-additive and does not pass through the origin.
/// </remarks>
[JsonConverter(typeof(SuperAdditiveCurveSystemJsonConverter))]
public class SuperAdditiveCurve : Curve
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    /// <summary>
    /// Type identification constant for JSON (de)serialization. 
    /// </summary>
    /// <exclude />
    public new const string TypeCode = "superAdditiveCurve";

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
    /// Skipping it asserts the property, and with it the $f(0) = 0$ that the optimizations of this type rely on.
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
    /// Skipping it asserts the property, and with it the $f(0) = 0$ that the optimizations of this type rely on.
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

    /// <inheritdoc cref="Curve.Addition(Curve, ComputationSettings)"/>
    /// <remarks>
    /// The sum of two super-additive curves is super-additive, so the type is kept when both operands have it.
    /// </remarks>
    public override Curve Addition(Curve b, ComputationSettings? settings = null)
    {
        var sum = base.Addition(b, settings);
        if (b is SuperAdditiveCurve)
            return new SuperAdditiveCurve(sum, false);
        else
            return sum;
    }

    /// <inheritdoc cref="Curve.Addition(Curve, ComputationSettings)"/>
    public SuperAdditiveCurve Addition(SuperAdditiveCurve b, ComputationSettings? settings = null)
    {
        return new SuperAdditiveCurve(base.Addition(b, settings), false);
    }
}