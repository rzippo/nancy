using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class PeriodStartInfimum
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PeriodStartInfimum(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve curve, Rational expected)> PeriodStartInfimumKnownCases =
    [
        (
            new RateLatencyServiceCurve(4, 3),
            3
        ),
        (
            new SigmaRhoArrivalCurve(4, 3),
            0
        ),
        (
            new SigmaRhoArrivalCurve(4, 0),
            0
        ),
        (
            new DelayServiceCurve(5),
            5
        ),
        (
            Curve.Minimum(new ConstantCurve(3), new DelayServiceCurve(5)),
            5
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    new Point(1, 0),
                    new Segment(1, 2, 0, 1),
                    new Point(2, 1),
                    Segment.Constant(2, 3, 2),
                    new Point(3, 2),
                    Segment.Constant(3, 4, 2)
                ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            2
        ),
        (
            new Curve(new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 1),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 3)
                ]),
                1,
                1,
                2
            ),
            0
        )
    ];

    public static IEnumerable<object[]> PeriodStartInfimumTestCases()
        => PeriodStartInfimumKnownCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PeriodStartInfimumTestCases))]
    public void EquivalenceToExpected(Curve curve, Rational expected)
    {
        _testOutputHelper.WriteLine($"var curve = {curve.ToCodeString()};");
        _testOutputHelper.WriteLine($"var expected = {expected.ToCodeString()};");
        var t_l = curve.PseudoPeriodStartInfimum;
        _testOutputHelper.WriteLine($"var t_l = {t_l.ToCodeString()};");
        Assert.Equal(t_l, expected);
    }
}