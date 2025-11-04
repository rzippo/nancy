using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ToLowerNonDecreasing
{
    private readonly ITestOutputHelper output;

    public ToLowerNonDecreasing(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<(Curve operand, Curve expected)> KnownPairs =
    [
        (
            operand: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    new Segment(2, 3, 2, -1),
                    new Point(3, 1),
                    new Segment(3, 6, 1, 1),
                    new Point(6, 4),
                    new Segment(6, 7, 4, 1)
                }),
                pseudoPeriodStart: 6,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 3, 1),
                    new Point(3, 1),
                    new Segment(3, 6, 1, 1),
                    new Point(6, 4),
                    new Segment(6, 7, 4, 1)
                }),
                pseudoPeriodStart: 6,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            )
        ),
        (
            // this is essentially the same test, but the baseSequence ends before the intersection
            operand: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    new Segment(2, 3, 2, -1),
                    new Point(3, 1),
                    new Segment(3, 3.1m, 1, 1),
                    new Point(3.1m, 1.1m),
                    new Segment(3.1m, 3.2m, 1.1m, 1)
                }),
                pseudoPeriodStart: 3.1m,
                pseudoPeriodLength: 0.1m,
                pseudoPeriodHeight: 0.1m
            ),
            expected: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 3, 1),
                    new Point(3, 1),
                    new Segment(3, 6, 1, 1),
                    new Point(6, 4),
                    new Segment(6, 7, 4, 1)
                }),
                pseudoPeriodStart: 6,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    new Segment(2, 3, 2, -1),
                    new Point(3, 1),
                    new Segment(3, 5, 1, 1),
                    new Point(5, 3),
                    new Segment(5, 6, 3, -1),
                    new Point(6, 2),
                    new Segment(6, 8, 2, 1)
                }),
                pseudoPeriodStart: 5,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 3, 1),
                    new Point(3, 1),
                    new Segment(3, 4, 1, 1)
                }),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 1
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    new Segment(2, 4, 1, 1),
                    new Point(4, 3),
                    new Segment(4, 5, 3, 1)
                }),
                pseudoPeriodStart: 4,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 2, 1),
                    new Point(2, 1),
                    new Segment(2, 4, 1, 1),
                    new Point(4, 3),
                    new Segment(4, 5, 3, 1),
                }),
                pseudoPeriodStart: 4,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    new Segment(2, 2.1m, 1, 1),
                    new Point(2.1m, 1.1m),
                    new Segment(2.1m, 2.2m, 1.1m, 1)
                }),
                pseudoPeriodStart: 2.1m,
                pseudoPeriodLength: 0.1m,
                pseudoPeriodHeight: 0.1m
            ),
            expected: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 2, 1),
                    new Point(2, 1),
                    new Segment(2, 2.1m, 1, 1),
                    new Point(2.1m, 1.1m),
                    new Segment(2.1m, 2.2m, 1.1m, 1)
                }),
                pseudoPeriodStart: 2.1m,
                pseudoPeriodLength: 0.1m,
                pseudoPeriodHeight: 0.1m
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    new Segment(2, 4, 1, 1)
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 2, 1)
                }),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 0), new Segment(0, new Rational(11, 30), 0, 0), new Point(new Rational(11, 30), 0),
                    new Segment(new Rational(11, 30), new Rational(13, 30), 2, -5),
                    new Point(new Rational(13, 30), new Rational(5, 3)),
                    new Segment(new Rational(13, 30), new Rational(143, 300), new Rational(5, 3), 10)
                }),
                pseudoPeriodStart: new Rational(13, 30),
                pseudoPeriodLength: new Rational(13, 300),
                pseudoPeriodHeight: new Rational(13, 30)
            ),
            expected: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 0), new Segment(0, new Rational(11, 30), 0, 0), new Point(new Rational(11, 30), 0),
                    new Segment(new Rational(11, 30), new Rational(13, 30), new Rational(5, 3), 0),
                    new Point(new Rational(13, 30), new Rational(5, 3)),
                    new Segment(new Rational(13, 30), new Rational(143, 300), new Rational(5, 3), 10)
                }),
                pseudoPeriodStart: new Rational(13, 30),
                pseudoPeriodLength: new Rational(13, 300),
                pseudoPeriodHeight: new Rational(13, 30)
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 0), new Segment(0, new Rational(1, 2), new Rational(7, 5), -1),
                    new Point(new Rational(1, 2), new Rational(9, 10)),
                    new Segment(new Rational(1, 2), new Rational(23, 30), new Rational(9, 10), new Rational(-1, 2)),
                    new Point(new Rational(23, 30), new Rational(23, 30)),
                    new Segment(new Rational(23, 30), new Rational(53, 30), new Rational(23, 30), 1)
                }),
                pseudoPeriodStart: new Rational(23, 30),
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 0), new Segment(0, new Rational(23, 30), new Rational(23, 30), 0),
                    new Point(new Rational(23, 30), new Rational(23, 30)),
                    new Segment(new Rational(23, 30), new Rational(53, 30), new Rational(23, 30), 1)
                }),
                pseudoPeriodStart: new Rational(23, 30),
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 300),
                    new Point(1, 100),
                    Segment.Constant(1, 2, 400),
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodHeight: 100,
                pseudoPeriodLength: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 100),
                    new Point(1, 100),
                    Segment.Constant(1, 2, 200),
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodHeight: 100,
                pseudoPeriodLength: 1
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence([
                    new Point(0, -2),
                    new Segment(0, 1, -2, -1),
                    new Point(1, -3),
                    new Segment(1, 3, -3, 1)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 2
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    new Point(0, -3),
                    new Segment(0, 1, -3, 0),
                    new Point(1, -3),
                    new Segment(1, 2, -3, 1)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 2, 0, 1),
                    new Point(2, 2),
                    new Segment(2, 3, 2, -1),
                    new Point(3, 1),
                    new Segment(3, 5, 1, 1),
                    new Point(5, 3),
                    new Segment(5, 6, 3, -1),
                    new Point(6, 2),
                    new Segment(6, 8, 2, 1)
                }),
                pseudoPeriodStart: 5,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 1
            ).VerticalShift(-3, false),
            expected: new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 3, 1),
                    new Point(3, 1),
                    new Segment(3, 4, 1, 1)
                }),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 1
            ).VerticalShift(-3, false)
        )
    ];

    public static IEnumerable<object[]> GetDecreasingTestCases()
        => KnownPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetDecreasingTestCases))]
    public void ToLowerNonDecreasingTest(Curve operand, Curve expected)
    {
        output.WriteLine($"var operand = {operand.ToCodeString()}");
        Assert.False(operand.IsNonDecreasing);

        var result = operand.ToLowerNonDecreasing();
        output.WriteLine($"var result = {result.ToCodeString()}");
        output.WriteLine($"var expected = {expected.ToCodeString()}");

        Assert.True(result.IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));

        var (dominance, lower, upper) = Curve.Dominance(operand, result);
        Assert.True(dominance);
        Assert.True(Curve.Equivalent(lower, result));
    }
}