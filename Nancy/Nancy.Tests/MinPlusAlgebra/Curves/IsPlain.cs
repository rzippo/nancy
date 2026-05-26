using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class IsPlain
{
    public static List<(Curve curve, bool isFinite, bool isPlain)> IsPlainCases =
    [
        (
            Curve.Zero(),
            isFinite: true,
            isPlain: true
        ),
        (
            new RateLatencyServiceCurve(10, 10),
            isFinite: true,
            isPlain: true
        ),
        (
            Curve.PlusInfinite(),
            isFinite: false,
            isPlain: true
        ),
        (   
            Curve.MinusInfinite(),
            isFinite: false,
            isPlain: true
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        Segment.Zero(0, 10),
                        Point.PlusInfinite(10),
                        Segment.PlusInfinite(10, 20)
                        
                    }
                ),
                pseudoPeriodStart: 10,
                pseudoPeriodLength: 10,
                pseudoPeriodHeight: 0
            ),
            isFinite: false,
            isPlain: true
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        Segment.Zero(0, 10),
                        Point.Zero(10),
                        Segment.PlusInfinite(10, 20)
                        
                    }
                ),
                pseudoPeriodStart: 11,
                pseudoPeriodLength: 9,
                pseudoPeriodHeight: 0
            ),
            isFinite: false,
            isPlain: true
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        Segment.Zero(0, 10),
                        Point.MinusInfinite(10),
                        Segment.MinusInfinite(10, 20)
                        
                    }
                ),
                pseudoPeriodStart: 10,
                pseudoPeriodLength: 10,
                pseudoPeriodHeight: 0
            ),
            isFinite: false,
            isPlain: true
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        Segment.Zero(0, 10),
                        Point.Zero(10),
                        Segment.MinusInfinite(10, 20)
                        
                    }
                ),
                pseudoPeriodStart: 11,
                pseudoPeriodLength: 9,
                pseudoPeriodHeight: 0
            ),
            isFinite: false,
            isPlain: true
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.PlusInfinite(0),
                        Segment.PlusInfinite(0, 5),
                        new Point(5, 0),
                        new Segment(5, 10, 0, 1)
                    }
                ),
                pseudoPeriodStart: 5,
                pseudoPeriodLength: 5,
                pseudoPeriodHeight: 0
            ),
            isFinite: false,
            isPlain: false
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        Segment.PlusInfinite(0, 10),
                        new Point(10, 0),
                        new Segment(10, 20, 0, 1)
                    }
                ),
                pseudoPeriodStart: 10,
                pseudoPeriodLength: 10,
                pseudoPeriodHeight: 10,
                isPartialCurve: true
            ),
            isFinite: false,
            isPlain: false
        )
    ];

    public static IEnumerable<object[]> GetIsPlainCases()
        => IsPlainCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetIsPlainCases))]
    public void IsPlainTest(Curve curve, bool isFinite, bool isPlain)
    {
        Assert.Equal(isFinite, curve.IsFinite);
        Assert.Equal(isPlain, curve.IsPlain);
    }
}
