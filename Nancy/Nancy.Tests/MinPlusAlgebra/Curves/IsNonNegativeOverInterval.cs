using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class IsNonNegativeOverInterval
{
    public static List<(Curve curve, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, bool expected)> SinglePointCases = new()
    {
        (
            curve: new Curve(
                new Sequence(new List<Element> { new Point(0, -1), new Segment(0, 1, -1, 0) }),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0),
            start: 1,
            end: 1,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: false
        ),
        (
            curve: new Curve(
                new Sequence(new List<Element> { new Point(0, -1), new Segment(0, 1, -1, 0) }),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0),
            start: 0,
            end: 0,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: false
        ),
        (
            curve: new Curve(
                new Sequence(new List<Element> { new Point(0, 5), new Segment(0, 1, 5, 0) }),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0),
            start: 3,
            end: 3,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: true
        ),
        (
            curve: new Curve(
                new Sequence(new List<Element> { new Point(0, 10), new Segment(0, 1, 10, -1) }),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: -1),
            start: 11,
            end: 11,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: false
        ),
        (
            curve: new Curve(
                new Sequence(new List<Element> { new Point(0, 10), new Segment(0, 1, 10, -1) }),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: -1),
            start: 10,
            end: 10,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: true
        ),
    };

    public static IEnumerable<object[]> SinglePointTestCases()
        => SinglePointCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(SinglePointTestCases))]
    public void SinglePointIntervalIsEvaluatedAtThePoint(
        Curve curve,
        Rational start,
        Rational? end,
        bool isStartIncluded,
        bool isEndIncluded,
        bool expected)
    {
        Assert.Equal(expected, curve.IsNonNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    public static List<(Curve curve, Rational start, bool expected)> UnboundedCases = new()
    {
        (
            curve: new Curve(
                new Sequence(new List<Element> { new Point(0, 10), new Segment(0, 1, 10, -1) }),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: -1),
            start: 1,
            expected: false
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, 3),
                    new Segment(2, 4, 3, new Rational(-1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: -1),
            start: 1,
            expected: false
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, 3),
                    new Segment(2, 4, 3, new Rational(-1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: -1),
            start: 2,
            expected: false
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, 3),
                    new Segment(2, 4, 3, new Rational(-1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: -1),
            start: 3,
            expected: false
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, 2),
                    new Segment(2, 4, 2, new Rational(-1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 0),
            start: 1,
            expected: true
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, 2),
                    new Segment(2, 4, 2, new Rational(-1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 0),
            start: 2,
            expected: true
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, 2),
                    new Segment(2, 4, 2, new Rational(-1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 0),
            start: 3,
            expected: true
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, -1),
                    new Segment(2, 4, -1, new Rational(1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1),
            start: 1,
            expected: false
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, -1),
                    new Segment(2, 4, -1, new Rational(1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1),
            start: 2,
            expected: false
        ),
        (
            curve: new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 5),
                    new Segment(0, 2, 5, 0),
                    new Point(2, -1),
                    new Segment(2, 4, -1, new Rational(1, 1))
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1),
            start: 3,
            expected: true
        ),
    };

    public static IEnumerable<object[]> UnboundedTestCases()
        => UnboundedCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UnboundedTestCases))]
    public void UnboundedIntervalAccountsForPseudoPeriodicBehavior(Curve curve, Rational start, bool expected)
    {
        Assert.Equal(expected, curve.IsNonNegativeOverInterval(start));
    }
}
