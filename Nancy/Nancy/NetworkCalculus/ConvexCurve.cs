using System;
using System.Collections.Generic;
using System.Linq;

using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus
{
    /// <summary>
    /// This class should be used to represent convex curves with $f(0) = 0$ (see <see cref="Curve.IsRegularConvex"/>), 
    /// and exploit these properties to optimize computations.
    /// </summary>
    /// <remarks>
    /// $f(0) = 0$ is required for the curve to be <see cref="Curve.IsRegularSuperAdditive"/>, hence derive from <see cref="SuperAdditiveCurve" />, 
    /// but is not required for convexity to be stable on maximum, addition and convolution.
    /// To keep the type system simple for the common cases of NC, we opted not to support non-regular convex functions with their own type.
    /// </remarks>
    public class ConvexCurve : SuperAdditiveCurve
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ConvexCurve(Sequence baseSequence, Rational pseudoPeriodStart, Rational pseudoPeriodLength,
            Rational pseudoPeriodHeight)
            : base(baseSequence, pseudoPeriodStart, pseudoPeriodLength, pseudoPeriodHeight, false)
        {
            if (!IsRegularConvex)
                throw new InvalidOperationException("The curve constructed is not actually convex with f(0) = 0");
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public ConvexCurve(Curve other)
            : base(other, false) 
        {
            if (!IsRegularConvex)
                throw new InvalidOperationException("The curve constructed is not actually convex with f(0) = 0");
        }

        /// <inheritdoc cref="Curve.Addition(Curve)"/>
        public override Curve Addition(Curve b)
        {
            var sum = base.Addition(b);
            if (b is ConvexCurve)
                return new ConvexCurve(sum);
            else
                return sum;
        }

        /// <inheritdoc cref="Curve.Addition(Curve)"/>
        public ConvexCurve Addition(ConvexCurve b)
        {
            return new ConvexCurve(base.Addition(b));
        }

        /// <inheritdoc cref="Curve.Maximum(Curve, ComputationSettings?)"/>
        public override Curve Maximum(Curve curve, ComputationSettings? settings = null)
        {
            var min = base.Maximum(curve, settings);
            if (curve is ConvexCurve)
                return new ConvexCurve(min);
            else
                return min;
        }

        /// <inheritdoc cref="Curve.Maximum(Curve, ComputationSettings?)"/>
        public ConvexCurve Maximum(ConvexCurve curve, ComputationSettings? settings = null)
        {
            return new ConvexCurve(base.Maximum(curve, settings));
        }

        /// <inheritdoc cref="Curve.Convolution(Curve, ComputationSettings?)"/>
        public override Curve Convolution(Curve curve, ComputationSettings? settings = null)
        {
            var conv = base.Convolution(curve, settings);
            if (curve is ConvexCurve)
                return new ConvexCurve(conv);
            else
                return conv;
        }

        /// <inheritdoc cref="Curve.Convolution(Curve, ComputationSettings?)"/>
        public ConvexCurve Convolution(ConvexCurve curve, ComputationSettings? settings = null)
            => new ConvexCurve(base.Convolution(curve, settings));
    }

    /// <summary>
    /// Provides LINQ extension methods for <see cref="ConvexCurve"/>
    /// </summary>
    public static class ConvexCurveExtensions
    {
        /// <inheritdoc cref="Curve.Maximum(IEnumerable{Curve}, ComputationSettings?)"/>
        public static ConvexCurve Maximum(this IEnumerable<ConvexCurve> curves, ComputationSettings? settings = null)
            => curves.Aggregate((a, b) => a.Maximum(b, settings));

        /// <inheritdoc cref="Curve.Maximum(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
        public static ConvexCurve Maximum(this IReadOnlyCollection<ConvexCurve> curves, ComputationSettings? settings = null)
            => new ConvexCurve(Curve.Maximum(curves, settings));

        /// <inheritdoc cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings?)"/>
        public static ConvexCurve Convolution(this IEnumerable<ConvexCurve> curves, ComputationSettings? settings = null)
            => curves.Aggregate((a, b) => a.Convolution(b, settings));

        /// <inheritdoc cref="Curve.Convolution(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
        public static ConvexCurve Convolution(this IReadOnlyCollection<ConvexCurve> curves, ComputationSettings? settings = null)
            => new ConvexCurve(Convolution(curves, settings));
    }
}
