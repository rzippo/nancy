using System;
using System.Collections.Generic;
using System.Linq;

using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus
{
    /// <summary>
    /// This class should be used to represent concave curves with $f(0) = 0$ (see <see cref="Curve.IsRegularConcave"/>), 
    /// and exploit these properties to optimize computations.
    /// </summary>
    /// <remarks>
    /// $f(0) = 0$ is required for the curve to be <see cref="Curve.IsRegularSubAdditive"/>, hence derive from <see cref="SubAdditiveCurve"/>, 
    /// and the convolution to become the minimum, 
    /// but is not required for concavity to be stable on minimum, addition and convolution.
    /// To keep the type system simple for the common cases of NC, we opted not to support non-regular concave functions with their own type.
    /// </remarks>
    public class ConcaveCurve : SubAdditiveCurve
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ConcaveCurve(Sequence baseSequence, Rational pseudoPeriodStart, Rational pseudoPeriodLength,
            Rational pseudoPeriodHeight)
            : base(baseSequence, pseudoPeriodStart, pseudoPeriodLength, pseudoPeriodHeight, false)
        {
            if (!IsRegularConcave)
                throw new InvalidOperationException("The curve constructed is not actually concave with f(0) = 0");
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public ConcaveCurve(Curve other)
            : base(other, false) 
        {
            if (!IsRegularConcave)
                throw new InvalidOperationException("The curve constructed is not actually concave with f(0) = 0");
        }

        /// <inheritdoc cref="Curve.Addition(Curve)"/>
        public override Curve Addition(Curve b)
        {
            var sum = base.Addition(b);
            if (b is ConcaveCurve)
                return new ConcaveCurve(sum);
            else
                return sum;
        }

        /// <inheritdoc cref="Curve.Addition(Curve)"/>
        public ConcaveCurve Addition(ConcaveCurve b)
        {
            return new ConcaveCurve(base.Addition(b));
        }

        /// <inheritdoc cref="Curve.Minimum(Curve, ComputationSettings?)"/>
        public override Curve Minimum(Curve curve, ComputationSettings? settings = null)
        {
            var min = base.Minimum(curve, settings);
            if (curve is ConcaveCurve)
                return new ConcaveCurve(min);
            else
                return min;
        }

        /// <inheritdoc cref="Curve.Minimum(Curve, ComputationSettings?)"/>
        public ConcaveCurve Minimum(ConcaveCurve curve, ComputationSettings? settings = null)
        {
            return new ConcaveCurve(base.Minimum(curve, settings));
        }

        /// <inheritdoc cref="Curve.Convolution(Curve, ComputationSettings?)"/>
        public override Curve Convolution(Curve curve, ComputationSettings? settings = null)
        {
            if (curve is ConcaveCurve cs)
                return Minimum(cs, settings);
            else
                return base.Convolution(curve, settings);
        }

        /// <summary>
        /// Computes the convolution of the two curves.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="settings"></param>
        /// <returns>The curve resulting from the convolution.</returns>
        /// <remarks>Optimized as the minimum of the two convex curves.</remarks>
        public ConcaveCurve Convolution(ConcaveCurve curve, ComputationSettings? settings = null)
            => Minimum(curve, settings);
    }

    /// <summary>
    /// Provides LINQ extension methods for <see cref="ConcaveCurve"/>
    /// </summary>
    public static class ConcaveCurveExtensions
    {
        /// <inheritdoc cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings?)"/>
        public static ConcaveCurve Minimum(this IEnumerable<ConcaveCurve> curves, ComputationSettings? settings = null)
            => curves.Aggregate((a, b) => a.Minimum(b));

        /// <inheritdoc cref="Curve.Minimum(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
        public static ConcaveCurve Minimum(this IReadOnlyCollection<ConcaveCurve> curves, ComputationSettings? settings = null)
            => new ConcaveCurve(Curve.Minimum(curves, settings));

        /// <inheritdoc cref="Curve.Convolution(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
        public static ConcaveCurve Convolution(this IEnumerable<ConcaveCurve> curves, ComputationSettings? settings = null)
            => Minimum(curves, settings);

        /// <inheritdoc cref="Curve.Convolution(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
        public static ConcaveCurve Convolution(this IReadOnlyCollection<ConcaveCurve> curves, ComputationSettings? settings = null)
            => Minimum(curves, settings);
    }
}
