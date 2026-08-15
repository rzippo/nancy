using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class HasInfinity
{
    public static List<(Curve curve, bool hasPlusInfinity, bool hasMinusInfinity)> HasInfinityCases =
    [
        (
            curve: Curve.Zero(),
            hasPlusInfinity: false,
            hasMinusInfinity: false
        ),
        (
            curve: new RateLatencyServiceCurve(10, 10),
            hasPlusInfinity: false,
            hasMinusInfinity: false
        ),
        (
            curve: Curve.PlusInfinite(),
            hasPlusInfinity: true,
            hasMinusInfinity: false
        ),
        (
            curve: Curve.MinusInfinite(),
            hasPlusInfinity: false,
            hasMinusInfinity: true
        ),
        (
            // infinite only from the periodic part on
            curve: new DelayServiceCurve(3),
            hasPlusInfinity: true,
            hasMinusInfinity: false
        ),
        (
            curve: -new DelayServiceCurve(3),
            hasPlusInfinity: false,
            hasMinusInfinity: true
        ),
        (
            // both infinities, in the transient and in the periodic part respectively
            curve: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 0),
                    Point.PlusInfinite(1),
                    Segment.PlusInfinite(1, 2),
                    Point.MinusInfinite(2),
                    Segment.MinusInfinite(2, 3)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            hasPlusInfinity: true,
            hasMinusInfinity: true
        )
    ];

    public static IEnumerable<object[]> HasInfinityTestCases()
        => HasInfinityCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(HasInfinityTestCases))]
    public void HasInfinityValues(Curve curve, bool hasPlusInfinity, bool hasMinusInfinity)
    {
        Assert.Equal(hasPlusInfinity, curve.HasPlusInfinity);
        Assert.Equal(hasMinusInfinity, curve.HasMinusInfinity);

        // the curve is infinite exactly where its base sequence is,
        // since the periodic extension repeats those elements
        Assert.Equal(curve.BaseSequence.HasPlusInfinity, curve.HasPlusInfinity);
        Assert.Equal(curve.BaseSequence.HasMinusInfinity, curve.HasMinusInfinity);

        // the value is cached, so it must not change when queried again
        Assert.Equal(hasPlusInfinity, curve.HasPlusInfinity);
        Assert.Equal(hasMinusInfinity, curve.HasMinusInfinity);
    }
}
