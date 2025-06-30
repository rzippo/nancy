using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Ceil
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Ceil(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve Curve, Curve Ceil)> KnownPairs =
    [
        (
            new ConstantCurve(1),
            new ConstantCurve(1)
        ),
        (
            new ConstantCurve(1.25m),
            new ConstantCurve(2)
        ),
        (
            new SigmaRhoArrivalCurve(1, 1),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 2),
                    new Point(1, 2),
                    Segment.Constant(1, 2, 3),
                ]),
                1,
                1,
                1
            )
        ),
        (
            new SigmaRhoArrivalCurve(1, 0.3m),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, new Rational(10, 3), 2),
                    new Point(new Rational(10, 3), 2),
                    Segment.Constant(new Rational(10, 3), new Rational(20, 3), 3),
                ]),
                new Rational(10, 3),
                new Rational(10, 3),
                1
            )
        ),
        (
            -new SigmaRhoArrivalCurve(1, 1),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, -1),
                    new Point(1, -2),
                    Segment.Constant(1, 2, -2),
                ]),
                1,
                1,
                -1
            )
        ),
        (
            new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 0.75m),
                    new Point(1, 1.5m),
                    Segment.Constant(1, 2, 1.5m),
                    new Point(2, 0.5m),
                    Segment.Constant(2, 3, 0.5m),
                    new Point(3, 0.5m),
                    new Segment(3, 4, 0.5m, 2),
                    new Point(4, 2.5m),
                    Segment.Constant(4, 5, 1.25m),
                ]),
                4,
                1,
                0.25m
            ),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 1),
                    new Point(1, 2),
                    Segment.Constant(1, 2, 2),
                    new Point(2, 1),
                    Segment.Constant(2, 3.25m, 1),
                    new Point(3.25m, 1),
                    Segment.Constant(3.25m, 3.75m, 2),
                    new Point(3.75m, 2),
                    Segment.Constant(3.75m, 4, 3),
                    new Point(4, 3),
                    Segment.Constant(4, 5, 2),
                    new Point(5, 3),
                    Segment.Constant(5, 6, 2),
                    new Point(6, 3),
                    Segment.Constant(6, 7, 2),
                    new Point(7, 4),
                    Segment.Constant(7, 8, 2),
                ]),
                4,
                4,
                1
            )
        )
    ];
    
    public static IEnumerable<object[]> CeilTestCases()
        => KnownPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CeilTestCases))]
    public void CeilTest(Curve curve, Curve expected)
    {
        var ceil = curve.Ceil();
        
        _testOutputHelper.WriteLine($"var curve = {curve.ToCodeString()};");
        _testOutputHelper.WriteLine($"var ceil = {ceil.ToCodeString()};");
        _testOutputHelper.WriteLine($"var expected = {expected.ToCodeString()};");
        
        Assert.True(Curve.Equivalent(expected, ceil));
    }
}