using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// An operand that is $-\infty$ at the origin reaches every time of the (min,+) convolution from there, so the result is $-\infty$ throughout; likewise $+\infty$ for the (max,+) convolution, nancy#183.
/// </summary>
public class ConvolutionInfiniteAtOrigin
{
    /// <summary>The given infinity on [0, 1[ and 1 on [1, 2[, repeated with height 2.</summary>
    private static Curve InfiniteThenOne(Rational infinity) => new(new Sequence([
        new Point(0, infinity), Segment.Constant(0, 1, infinity), new Point(1, 1), Segment.Constant(1, 2, 1)
    ]), 0, 2, 2);

    /// <summary>The given infinity at the origin only, then $t$.</summary>
    private static Curve InfiniteAtOriginOnly(Rational infinity) => new(new Sequence([
        new Point(0, infinity), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 1)
    ]), 1, 1, 1);

    public static IEnumerable<object[]> FiniteOperands()
    {
        yield return [new RateLatencyServiceCurve(1, 1)];
        yield return [new ConstantCurve(5)];
    }

    [Theory]
    [MemberData(nameof(FiniteOperands))]
    public void ConvolutionWithAnOperandMinusInfiniteAtTheOriginIsMinusInfinite(Curve g)
    {
        foreach (var f in new[] { InfiniteThenOne(Rational.MinusInfinity), InfiniteAtOriginOnly(Rational.MinusInfinity) })
        {
            Assert.True(Curve.Convolution(f, g).Equivalent(Curve.MinusInfinite()));
            Assert.True(Curve.Convolution(g, f).Equivalent(Curve.MinusInfinite()));
        }
    }

    [Theory]
    [MemberData(nameof(FiniteOperands))]
    public void MaxPlusConvolutionWithAnOperandPlusInfiniteAtTheOriginIsPlusInfinite(Curve g)
    {
        foreach (var f in new[] { InfiniteThenOne(Rational.PlusInfinity), InfiniteAtOriginOnly(Rational.PlusInfinity) })
        {
            Assert.True(Curve.MaxPlusConvolution(f, g).Equivalent(Curve.PlusInfinite()));
            Assert.True(Curve.MaxPlusConvolution(g, f).Equivalent(Curve.PlusInfinite()));
        }
    }
}
