using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class VerticalShift
{
    public static List<Curve> Curves =
    [
        new ConstantCurve(5),
        new RateLatencyServiceCurve(rate: 2, latency: 3),
        new FlowControlCurve(latency: 3, rate: 2, height: 5),
        new SigmaRhoArrivalCurve(sigma: 3, rho: 2),
        new DelayServiceCurve(3),
        new StepCurve(value: 5, stepTime: 3)
    ];

    public static IEnumerable<object[]> GetCurves()
        => Curves.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetCurves))]
    public void CutAndShiftCommute(Curve curve)
    {
        Rational shift = 5;

        var shiftedThenCut = (curve + shift).Cut(0, 10);
        var cutThenShifted = curve.Cut(0, 10) + shift;

        Assert.True(Sequence.Equivalent(shiftedThenCut, cutThenShifted));
    }

    [Fact]
    public void DefaultShiftsTheElementAtTheOrigin()
    {
        var sequence = new Sequence([new Point(0, 4), new Segment(0, 2, 4, 1)]);

        var shifted = sequence.VerticalShift(5);

        Assert.Equal(9, shifted.ValueAt(0));
        Assert.Equal(9, shifted.RightLimitAt(0));
    }

    [Fact]
    public void ExceptOriginLeavesTheElementAtTheOrigin()
    {
        var sequence = new Sequence([new Point(0, 4), new Segment(0, 2, 4, 1)]);

        var shifted = sequence.VerticalShift(5, exceptOrigin: true);

        Assert.Equal(4, shifted.ValueAt(0));
        Assert.Equal(9, shifted.RightLimitAt(0));
    }

    [Fact]
    public void ConcatAtTheOriginIsContinuous()
    {
        // the second operand is displaced to start where the first ends, which here is t = 0,
        // so the whole of it must be lifted, the element at the origin included
        var a = new Sequence([new Point(-2, 1), new Segment(-2, 0, 1, 1)]);
        var b = new Sequence([new Point(0, 7), new Segment(0, 2, 7, 1)]);

        var concat = Sequence.Concat(a, b);

        Assert.Equal(a.LeftLimitAt(0), concat.ValueAt(0));
        Assert.Equal(concat.ValueAt(0), concat.RightLimitAt(0));
    }
}
