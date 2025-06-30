using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Floor
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Floor(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve Curve, Curve Floor)> KnownPairs =
    [
        (
            new ConstantCurve(1),
            new ConstantCurve(1)
        ),
        (
            new ConstantCurve(1.25m),
            new ConstantCurve(1)
        ),
        (
            new SigmaRhoArrivalCurve(1, 1),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 1),
                    new Point(1, 2),
                    Segment.Constant(1, 2, 2),
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
                    Segment.Constant(0, new Rational(10, 3), 1),
                    new Point(new Rational(10, 3), 2),
                    Segment.Constant(new Rational(10, 3), new Rational(20, 3), 2),
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
                    Segment.Constant(0, 1, -2),
                    new Point(1, -2),
                    Segment.Constant(1, 2, -3),
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
                    Segment.Constant(0, 1, 0),
                    new Point(1, 1),
                    Segment.Constant(1, 2, 1),
                    new Point(2, 0),
                    Segment.Constant(2, 3.25m, 0),
                    new Point(3.25m, 1),
                    Segment.Constant(3.25m, 3.75m, 1),
                    new Point(3.75m, 2),
                    Segment.Constant(3.75m, 4, 2),
                    new Point(4, 2),
                    Segment.Constant(4, 5, 1),
                    new Point(5, 2),
                    Segment.Constant(5, 6, 1),
                    new Point(6, 3),
                    Segment.Constant(6, 7, 1),
                    new Point(7, 3),
                    Segment.Constant(7, 8, 2),
                ]),
                4,
                4,
                1
            )
        )
    ];
    
    public static IEnumerable<object[]> FloorTestCases()
        => KnownPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(FloorTestCases))]
    public void FloorTest(Curve curve, Curve expected)
    {
        var floor = curve.Floor();
        
        _testOutputHelper.WriteLine($"var curve = {curve.ToCodeString()};");
        _testOutputHelper.WriteLine($"var floor = {floor.ToCodeString()};");
        _testOutputHelper.WriteLine($"var expected = {expected.ToCodeString()};");
        
        Assert.True(Curve.Equivalent(expected, floor));
    }

    public static IEnumerable<object[]> IsIntegerLowerBoundTestCases()
        => KnownPairs.Select(p => p.Curve).ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(IsIntegerLowerBoundTestCases))]
    public void IsIntegerLowerBound(Curve curve)
    {
        var floor = curve.Floor();
        
        Assert.True(floor <= curve);
        foreach (var element in floor.CutAsEnumerable(0, floor.SecondPseudoPeriodEnd))
        {
            if(element is Point p)
                Assert.True(p.Value.IsInteger);
            else if (element is Segment s)
                Assert.True(s is
                {
                    IsConstant: true, 
                    RightLimitAtStartTime.IsInteger: true
                });
        }
    }
}