using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Intervals;

public class UpperEnvelopes
{
    public static IEnumerable<object[]> PointIntervalCases()
    {
        var testCases = new (Point[] points, Point expected)[]
        {
            (
                new[]
                {
                    new Point(5, 4),
                    new Point(5, 4),
                    new Point(5, 3),
                    new Point(5, 2),
                    new Point(5, 2),
                },
                new Point(5, 4)
            ),
            (
                new[]
                {
                    new Point(7, 3),
                    new Point(7, 3),
                    new Point(7, 3),
                    new Point(7, 3),
                    new Point(7, 3),
                },
                new Point(7, 3)
            )
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.points, testCase.expected };
    }

    [Theory]
    [MemberData(nameof(PointIntervalCases))]
    public void PointInterval(Point[] points, Point expected)
    {
        var time = points.First().Time;
        var interval = new Interval(time);
        interval.AddRange(points);

        var result = interval.UpperEnvelope();
        Assert.Single(result);
        Assert.IsType<Point>(result.Single());
        Assert.Equal(expected, result.Single());
    }

    public static IEnumerable<object[]> SegmentIntervalExpectedCases()
    {
        var testCases = new (Segment[] segments, Sequence expected)[]
        {
            //Same slope, varying start
            (
                new[]
                {
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -2, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -3, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -4, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -5, slope: -2),
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -2)
                    }
                )
            ),
            (
                new[]
                {
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -2, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -2, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -5, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -5, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -3, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -3, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -4, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -4, slope: -2),
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -2)
                    }
                )
            ),
            //Same start, varying slope
            (
                new[]
                {
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -5),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -4),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -3),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -1),
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -1)
                    }
                )
            ),
            (
                new[]
                {
                    new Segment(1, 2, -10, 1),
                    new Segment(1, 2, -5, -1)
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(1, 2, -5, -1)
                    }
                )
            ),
            (
                new[]
                {
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -5),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -3),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -1),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -4),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -5),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -3),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -1),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -4),
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -1, slope: -1)
                    }
                )
            ),
            //Scenario A
            (
                new[]
                {
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: 0, slope: -3),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -2, slope: -1),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -9, slope: new Rational(1, 2)),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -12, slope: 2)
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 0, endTime: 1, rightLimitAtStartTime: 0, slope: -3),
                        new Point(time: 1, value: -3),
                        new Segment(startTime: 1, endTime: 3, rightLimitAtStartTime: -3, slope: -1),
                    }
                )
            ),
            (
                new[]
                {
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: 0, slope: -3),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -2, slope: -1),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -9, slope: new Rational(1, 2)),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -12, slope: 2),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: 0, slope: -3),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -2, slope: -1),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -9, slope: new Rational(1, 2)),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -12, slope: 2)
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 0, endTime: 1, rightLimitAtStartTime: 0, slope: -3),
                        new Point(time: 1, value: -3),
                        new Segment(startTime: 1, endTime: 3, rightLimitAtStartTime: -3, slope: -1),
                    }
                )
            ),
            (
                new[]
                {
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -9, slope: new Rational(1, 2)),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: 0, slope: -3),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -12, slope: 2),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -2, slope: -1),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -2, slope: -1),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -2, slope: -1),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -2, slope: -1),
                    new Segment(startTime: 0, endTime: 3, rightLimitAtStartTime: -2, slope: -1),
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 0, endTime: 1, rightLimitAtStartTime: 0, slope: -3),
                        new Point(time: 1, value: -3),
                        new Segment(startTime: 1, endTime: 3, rightLimitAtStartTime: -3, slope: -1),
                    }
                )
            ),
            //Scenario B
            (
                new[]
                {
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: 0, slope: -3),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -9, slope: 2),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -12, slope: 3)
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 0, endTime: 1, rightLimitAtStartTime: 0, slope: -3),
                        new Point(time: 1, value: -3),
                        new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -3, slope: -2),
                        new Point(time: 2, value: -5),
                        new Segment(startTime: 2, endTime: 3, rightLimitAtStartTime: -5, slope: 2),
                        new Point(time: 3, value: -3),
                        new Segment(startTime: 3, endTime: 4, rightLimitAtStartTime: -3, slope: 3),
                    }
                )
            ),
            (
                new[]
                {
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: 0, slope: -3),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -9, slope: 2),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -12, slope: 3),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: 0, slope: -3),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -1, slope: -2),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -9, slope: 2),
                    new Segment(startTime: 0, endTime: 4, rightLimitAtStartTime: -12, slope: 3)
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(startTime: 0, endTime: 1, rightLimitAtStartTime: 0, slope: -3),
                        new Point(time: 1, value: -3),
                        new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: -3, slope: -2),
                        new Point(time: 2, value: -5),
                        new Segment(startTime: 2, endTime: 3, rightLimitAtStartTime: -5, slope: 2),
                        new Point(time: 3, value: -3),
                        new Segment(startTime: 3, endTime: 4, rightLimitAtStartTime: -3, slope: 3),
                    }
                )
            ),
            (
                new []
                {
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: new Rational(0, 1),
                        rightLimitAtStartTime: new Rational(0, 1),
                        slope: new Rational(0, 1)
                    ),
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: new Rational(0, 1),
                        rightLimitAtStartTime: new Rational(73, 19),
                        slope: new Rational(0, 1)
                    ),
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: new Rational(0, 1),
                        rightLimitAtStartTime: new Rational(73, 19),
                        slope: new Rational(0, 1)
                    ),
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: new Rational(0, 1),
                        rightLimitAtStartTime: new Rational(73, 19),
                        slope: new Rational(4, 5)
                    ),
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: new Rational(0, 1),
                        rightLimitAtStartTime: new Rational(73, 19),
                        slope: new Rational(4, 5)
                    ),
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: new Rational(0, 1),
                        rightLimitAtStartTime: new Rational(73, 19),
                        slope: new Rational(4, 5)
                    ),
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: new Rational(0, 1),
                        rightLimitAtStartTime: new Rational(-17, 19),
                        slope: new Rational(2, 1)
                    ),
                    new Segment(
                        startTime: new Rational(-18, 19),
                        endTime: new Rational(0, 1),
                        rightLimitAtStartTime: new Rational(-17, 19),
                        slope: new Rational(2, 1)
                    )
                },
                new Sequence(
                    new Element[]
                    {
                        new Segment(
                            startTime: new Rational(-18, 19),
                            endTime: 0,
                            rightLimitAtStartTime: new Rational(73, 19),
                            slope: new Rational(4, 5)
                        )
                    }
                )
            ),
            (
                new Segment[]
                {
                    new Segment(5, 6, 19, 0),
                    new Segment(5, 6, 20, 1),
                    new Segment(5, 6, 17, 5),
                },
                new Sequence(new List<Element>()
                {
                    new Segment(5, 5.75m, 20, 1),
                    new Point(5.75m, 20.75m),
                    new Segment(5.75m, 6, 20.75m, 5)
                })
            )
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.segments, testCase.expected };
    }

    [Theory]
    [MemberData(nameof(SegmentIntervalExpectedCases))]
    public void SegmentIntervalExpected(Segment[] segments, Sequence expected)
    {
        var start = segments.First().StartTime;
        var end = segments.First().EndTime;
        var interval = new Interval(start, end);
        interval.AddRange(segments);

        var result = interval.UpperEnvelope();
        var sequence = new Sequence(result).Optimize();
        Assert.Equal(expected, sequence);
    }

    public static List<(List<Segment> segments, Rational start, Rational end)> SegmentIntervals = new ()
    {
        (
            new List<Segment>{ 
                new Segment(1824,1834,201,0), 
                new Segment(1797,1869,197,new Rational(1, 9)), 
                new Segment(1789,1861,200,new Rational(1, 8)), 
                new Segment(1806,1834,197,new Rational(1, 7)), 
                new Segment(1827,1859,204,new Rational(1, 4)) 
            },
            1832,
            1834
        ),
        // (
        //     new List<Segment>{
        //         new Segment(0,2,21,0),
        //         new Segment(0,2,20,new Rational(1, 9)),
        //         new Segment(0,2,19,new Rational(1, 8)),
        //         new Segment(0,2,18,new Rational(1, 7)),
        //         new Segment(0,2,17,new Rational(1, 4))
        //     },
        //     0,
        //     2
        // )
    };

    public static IEnumerable<object[]> SegmentIntervalsTestCases()
    {
        foreach (var (segments, start, end) in SegmentIntervals)
            yield return new object[] { segments, start, end };
    }

    [Theory]
    [MemberData(nameof(SegmentIntervalsTestCases))]
    public void SegmentInterval(List<Segment> segments, Rational start, Rational end)
    {
        var interval = new Interval(start, end);
        interval.AddRange(segments);

        var result = interval.UpperEnvelope();
        var sequence = new Sequence(result);
    }

    /*
    public static IEnumerable<object[]> ConquerPhaseCases()
    {
        var testCases = new (List<Element> le_a, List<Element> le_b, List<Element> expected)[]
        {
            (
                le_a: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 3,
                        rightLimitAtStartTime: 0,
                        slope: 1
                    ),
                    new Point(
                        time: 3,
                        value: 3
                    ),
                    new Segment(
                        startTime: 3,
                        endTime: 11,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 4)
                    )
                },
                le_b: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 8,
                        rightLimitAtStartTime: 11,
                        slope: new Rational(-1, 4)
                    ),
                    new Point(
                        time: 8,
                        value: 9
                    ),
                    new Segment(
                        startTime: 8,
                        endTime: 11,
                        rightLimitAtStartTime: 9,
                        slope: -1
                    )
                },
                expected: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 3,
                        rightLimitAtStartTime: 0,
                        slope: 1
                    ),
                    new Point(
                        time: 3,
                        value: 3
                    ),
                    new Segment(
                        startTime: 3,
                        endTime: 11,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 4)
                    )
                }
            ),
            (
                le_a: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 3,
                        rightLimitAtStartTime: 0,
                        slope: 1
                    ),
                    new Point(
                        time: 3,
                        value: 3
                    ),
                    new Segment(
                        startTime: 3,
                        endTime: 11,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 4)
                    )
                },
                le_b: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 9,
                        rightLimitAtStartTime: new Rational(15, 2),
                        slope: new Rational(-1, 2)
                    ),
                    new Point(
                        time: 9,
                        value: 3
                    ),
                    new Segment(
                        startTime: 9,
                        endTime: 11,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(-3, 2)
                    )
                },
                expected: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 3,
                        rightLimitAtStartTime: 0,
                        slope: 1
                    ),
                    new Point(
                        time: 3,
                        value: 3
                    ),
                    new Segment(
                        startTime: 3,
                        endTime: 7,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 4)
                    ),
                    new Point(
                        time: 7,
                        value: 4
                    ),
                    new Segment(
                        startTime: 7,
                        endTime: 9,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(-1, 2)
                    ),
                    new Point(
                        time: 9,
                        value: 3
                    ),
                    new Segment(
                        startTime: 9,
                        endTime: 11,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(-3, 2)
                    )
                }
            ),
            (
                le_a: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 3,
                        rightLimitAtStartTime: 0,
                        slope: 1
                    ),
                    new Point(
                        time: 3,
                        value: 3
                    ),
                    new Segment(
                        startTime: 3,
                        endTime: 11,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 4)
                    )
                },
                le_b: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 7,
                        rightLimitAtStartTime: 6,
                        slope: new Rational(-2, 7)
                    ),
                    new Point(
                        time: 7,
                        value: 4
                    ),
                    new Segment(
                        startTime: 7,
                        endTime: 11,
                        rightLimitAtStartTime: 4,
                        slope: -1
                    )
                },
                expected: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 3,
                        rightLimitAtStartTime: 0,
                        slope: 1
                    ),
                    new Point(
                        time: 3,
                        value: 3
                    ),
                    new Segment(
                        startTime: 3,
                        endTime: 7,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 4)
                    ),
                    new Point(
                        time: 7,
                        value: 4
                    ),
                    new Segment(
                        startTime: 7,
                        endTime: 11,
                        rightLimitAtStartTime: 4,
                        slope: -1
                    )
                }
            ),
            (
                le_a: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 3,
                        rightLimitAtStartTime: 0,
                        slope: 1
                    ),
                    new Point(
                        time: 3,
                        value: 3
                    ),
                    new Segment(
                        startTime: 3,
                        endTime: 6,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 3)
                    ),
                    new Point(
                        time: 6,
                        value: 4
                    ),
                    new Segment(
                        startTime: 6,
                        endTime: 11,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(1, 5)
                    )
                },
                le_b: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 9,
                        rightLimitAtStartTime: 6,
                        slope: new Rational(-1, 3)
                    ),
                    new Point(
                        time: 9,
                        value: 3
                    ),
                    new Segment(
                        startTime: 9,
                        endTime: 11,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(-3, 2)
                    )
                },
                expected: new List<Element>
                {
                    new Segment(
                        startTime: 0,
                        endTime: 3,
                        rightLimitAtStartTime: 0,
                        slope: 1
                    ),
                    new Point(
                        time: 3,
                        value: 3
                    ),
                    new Segment(
                        startTime: 3,
                        endTime: 6,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(1, 3)
                    ),
                    new Point(
                        time: 6,
                        value: 4
                    ),
                    new Segment(
                        startTime: 6,
                        endTime: 9,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(-1, 3)
                    ),
                    new Point(
                        time: 9,
                        value: 3
                    ),
                    new Segment(
                        startTime: 9,
                        endTime: 11,
                        rightLimitAtStartTime: 3,
                        slope: new Rational(-3, 2)
                    )
                }
            ),
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.le_a, testCase.le_b, testCase.expected };
    }

    [Theory]
    [MemberData(nameof(ConquerPhaseCases))]
    public void ConquerPhase(List<Element> le_a, List<Element> le_b, List<Element> expected)
    {
        var envelopes = new List<List<Element>> { le_a, le_b };
        var result = Interval.UpperEnvelope(envelopes);

        Assert.Equal(expected, result);
    }
    */
}