﻿using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Composition
{
    public static List<(Curve f, Curve g, Curve expected)> KnownTuples =
    [
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    Point.Zero(1),
                    new Segment(1, 2, 0, 2)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            ),
            g: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    Point.Zero(1),
                    new Segment(1, 2, 0, 1),
                    new Point(2, 1),
                    Segment.Constant(2, 3, 1),
                    new Point(3, 1),
                    new Segment(3, 4, 1, 1)
                ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 3),
                    Point.Zero(3),
                    new Segment(3, 4, 0, 2)
                ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    Point.Zero(1),
                    new Segment(1, 4, 0, 3)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 9
            ),
            g: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 2),
                    new Point(1, 2),
                    Segment.Constant(1, 2, 2),
                    new Point(2, 2),
                    new Segment(2, 3, 2, 2)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 0.5m),
                    Point.Zero(0.5m),
                    new Segment(0.5m, 1, 0, 6),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 3),
                    new Point(2, 3),
                    new Segment(2, 3, 3, 6)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 6
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    Point.Zero(1),
                    new Segment(1, 4, 0, 3)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 9
            ),
            g: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 2),
                    new Point(1, 2),
                    Segment.Constant(1, 2, 2)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 0.5m),
                    Point.Zero(0.5m),
                    new Segment(0.5m, 1, 0, 6),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 3)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    Point.Zero(1),
                    new Segment(1, 4, 0, 3)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 4,
                pseudoPeriodHeight: 9
            ),
            g: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 2),
                    new Point(1, 2),
                    Segment.Constant(1, 2, 2)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 0.5m),
                    Point.Zero(0.5m),
                    new Segment(0.5m, 1, 0, 6),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 3)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 3),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 3)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            g: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 2),
                    new Point(1, 2),
                    Segment.Constant(1, 2, 2)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 0.5m, 0, 6),
                    new Point(0.5m, 3),
                    Segment.Constant(0.5m, 1.5m, 3)
                ]),
                pseudoPeriodStart: 0.5m,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 3),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 3)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            g: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 2),
                    new Point(1, 2),
                    Segment.Constant(1, 2, 2)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 0.5m, 0, 6),
                    new Point(0.5m, 3),
                    Segment.Constant(0.5m, 1.5m, 3)
                ]),
                pseudoPeriodStart: 0.5m,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        ),
        (
            f: new SigmaRhoArrivalCurve(4, 3),
            g: new SigmaRhoArrivalCurve(5, 4),
            expected: new SigmaRhoArrivalCurve(19, 12)
        ),
        (
            f: new SigmaRhoArrivalCurve(200, 100),
            g: new StairCurve(1, 1),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 300),
                    new Point(1, 300),
                    Segment.Constant(1, 2, 400),
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodHeight: 100,
                pseudoPeriodLength: 1
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 3, 200, 100)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 100
            ),
            g: new StairCurve(1, 1),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 300),
                    new Point(1, 300),
                    Segment.Constant(1, 2, 400),
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodHeight: 100,
                pseudoPeriodLength: 1
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 3, 200, 100)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 100
            ),
            g: new StairCurve(2, 1),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 400),
                    new Point(1, 400),
                    Segment.Constant(1, 2, 600),
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodHeight: 200,
                pseudoPeriodLength: 1
            )
        ),
        (
            f: new SigmaRhoArrivalCurve(200, 100),
            g: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 1, 1)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 300, 100),
                    new Point(1, 400),
                    new Segment(1, 2, 500, 100),
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 200
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 3, 200, 100)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 100
            ).DelayBy(100),
            g: new StairCurve(2, 1),
            expected: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    new Segment(0, 50, 0, 0),
                    new Point(50, 0),
                    new Segment(50, 51, 400, 0),
                    new Point(51, 400),
                    new Segment(51, 52, 600, 0)
                ]),
                pseudoPeriodStart: 51,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 200
            )
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0), 
                    new Segment(0, 12, 3, new Rational(1, 3)),
                    new Point(12, 7),
                    new Segment(12, 13, new Rational(1, 0), 0),
                    new Point(13, new Rational(1, 0)),
                    new Segment(13, 14, new Rational(1, 0), 0)
                ]),
                pseudoPeriodStart: 13,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            g: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    new Segment(0, 3, 0, 4),
                    new Point(3, 12),
                    new Segment(3, 4, 12, 0)
                ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 0),
                    new Segment(0, 3, 3, new Rational(4, 3)),
                    new Point(3, 7),
                    new Segment(3, 4, 7, 0)
                ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        )
    ];

    public static IEnumerable<object[]> GetTestCases()
        => KnownTuples.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void EquivalenceToExpected(Curve f, Curve g, Curve expected)
    {
        var result = Curve.Composition(f, g);
        var expectedNonDecreasing = f.IsNonDecreasing && g.IsNonDecreasing;
        Assert.Equal(expectedNonDecreasing, result.IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void EquivalenceToExpected_NoOpt(Curve f, Curve g, Curve expected)
    {
        var settings = ComputationSettings.Default() with { UseCompositionOptimizations = false };

        var result = Curve.Composition(f, g, settings);
        var expectedNonDecreasing = f.IsNonDecreasing && g.IsNonDecreasing;
        Assert.Equal(expectedNonDecreasing, result.IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));
    }
}