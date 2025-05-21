using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class VerticalShift
{
    private readonly ITestOutputHelper _testOutputHelper;

    public VerticalShift(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve curve, Rational shift, bool exceptOrigin, Curve expected)> KnownCases =
    [
        (
            new StairCurve(1, 1),
            2, 
            true,
            new Curve(new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 3),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 4)
                ]),
                1, 1, 1
            )
        ),
        (
            new StairCurve(1, 1),
            2, 
            false,
            new Curve(new Sequence([
                    new Point(0, 2),
                    Segment.Constant(0, 1, 3)
                ]),
                0, 1, 1
            )
        )
    ];

    public static IEnumerable<object[]> EquivalenceTestCases =
        KnownCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(EquivalenceTestCases))]
    public void VerticalShiftEquivalence(Curve curve, Rational shift, bool exceptOrigin, Curve expected)
    {
        var shifted = curve.VerticalShift(shift, exceptOrigin);
        _testOutputHelper.WriteLine(curve.ToString());
        _testOutputHelper.WriteLine(expected.ToString());
        _testOutputHelper.WriteLine(shifted.ToString());
        Assert.True(Curve.Equivalent(expected, shifted));
    }
}