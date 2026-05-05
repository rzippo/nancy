using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class AttainsValue
{
    public static List<(Curve curve, Rational value, bool expected)> KnownPairs =
    [
        (Curve.Zero(), 0, true),
        (Curve.Zero(), 1, false),
        (Curve.PlusInfinite(), Rational.PlusInfinity, true),
        (Curve.PlusInfinite(), 0, false),
        (Curve.MinusInfinite(), Rational.MinusInfinity, true),
        (Curve.MinusInfinite(), Rational.PlusInfinity, false),
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 1)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            1,
            true
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 1)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            new Rational(17, 6),
            true
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 1)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            -1,
            false
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    Segment.Constant(0, 1, 2)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 3
            ),
            3,
            true
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    Segment.Constant(0, 1, 2)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 3
            ),
            4,
            false
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    Segment.Constant(0, 1, 2)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 3
            ),
            5,
            true
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    new Segment(0, 1, 0, -1)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: -1
            ),
            new Rational(-1, 2),
            true
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    new Segment(0, 1, 0, -1)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: -1
            ),
            -2,
            true
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    new Segment(0, 1, 0, -1)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: -1
            ),
            1,
            false
        )
    ];

    public static IEnumerable<object[]> KnownPairsTestCases()
        => KnownPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownPairsTestCases))]
    public void KnownPairsTest(Curve curve, Rational value, bool expected)
    {
        Assert.Equal(expected, curve.AttainsValue(value));
    }
}
