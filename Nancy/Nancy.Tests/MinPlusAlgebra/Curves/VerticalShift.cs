using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

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

    /// <summary>$f(t) = t$.</summary>
    private static readonly Curve Identity = new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 1);

    /// <summary>
    /// Excepting the origin excepts it only: every later time is shifted, including the pseudo-period boundaries.
    /// </summary>
    public static List<(Curve operand, Rational shift, Rational time, Rational expected)> ExceptOriginValues =
    [
        (Identity, 2, 0, 0),
        (Identity, 2, new Rational(1, 2), new Rational(5, 2)),
        (Identity, 2, 1, 3),
        (Identity, 2, 2, 4),
        (Identity, 2, 10, 12),
    ];

    public static IEnumerable<object[]> ExceptOriginValuesTestCases()
        => ExceptOriginValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ExceptOriginValuesTestCases))]
    public void ExceptingTheOriginExceptsItOnly(Curve operand, Rational shift, Rational time, Rational expected)
    {
        Assert.Equal(expected, operand.VerticalShift(shift, exceptOrigin: true).ValueAt(time));
    }
}
