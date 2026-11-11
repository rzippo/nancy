using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Split
{
    //Start closed, end open
    private static Sequence a = new Sequence(new Element[]
    {
        Point.Origin(),
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30)
    });

    //Start open, end open
    private static Sequence b = new Sequence(new Element[]
    {
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30)
    });

    //Start open, end closed
    private static Sequence c = new Sequence(new Element[]
    {
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30),
        Point.Zero(30)
    });

    //Start closed, end closed
    private static Sequence d = new Sequence(new Element[]
    {
        Point.Origin(),
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30),
        Point.Zero(30)
    });

    public static IEnumerable<object[]> GetSuccessTestCases()
    {
        var testCases = new (Sequence sequence, Rational splitTime)[]
        {
            //Within bounds
            (sequence: a, splitTime: 15),
            (sequence: a, splitTime: 10),

            //At endpoints
            (sequence: a, splitTime: 0),
            (sequence: c, splitTime: 30),
            (sequence: d, splitTime: 0),
            (sequence: d, splitTime: 30)
        };

        foreach (var testCase in testCases)
        {
            yield return new object[]
            {
                testCase.sequence,
                testCase.splitTime
            };
        }
    }

    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    public void EnforceSplitAt(Sequence sequence, Rational splitTime)
    {
        Sequence withSplitEnforced = sequence.EnforceSplitAt(splitTime);

        Assert.Equal(sequence.DefinedFrom, withSplitEnforced.DefinedFrom);
        Assert.Equal(sequence.DefinedUntil, withSplitEnforced.DefinedUntil);
        Assert.Equal(sequence.IsLeftClosed, withSplitEnforced.IsLeftClosed);
        Assert.Equal(sequence.IsRightClosed, withSplitEnforced.IsRightClosed);
        Assert.True(Sequence.Equivalent(sequence, withSplitEnforced));

        Assert.True(withSplitEnforced.GetElementAt(splitTime) is Point);
    }

    public static IEnumerable<object[]> GetExceptionTestCases()
    {
        var testCases = new (Sequence sequence, Rational splitTime)[]
        {
            //Out of bounds
            (sequence: a, splitTime: 35),

            //At endpoints
            (sequence: a, splitTime: 30),
            (sequence: b, splitTime: 0),
            (sequence: b, splitTime: 30),
            (sequence: c, splitTime: 0)
        };

        foreach (var testCase in testCases)
        {
            yield return new object[]
            {
                testCase.sequence,
                testCase.splitTime
            };
        }
    }

    [Theory]
    [MemberData(nameof(GetExceptionTestCases))]
    public void EnforceSplitAt_Exception(Sequence sequence, Rational splitTime)
    {
        Assert.Throws<ArgumentException>(() => sequence.EnforceSplitAt(splitTime));
    }

    /// <summary>
    /// The split segment in the middle, first, last and alone, with the elements the split must give.
    /// </summary>
    public static IEnumerable<object[]> SegmentPositions()
    {
        yield return [a, new Rational(15), new Element[]
        {
            Point.Origin(), Segment.Zero(0, 10), Point.Zero(10), Segment.Zero(10, 15), Point.Zero(15), Segment.Zero(15, 20),
            Point.Zero(20), Segment.Zero(20, 30)
        }];
        yield return [b, new Rational(5), new Element[]
        {
            Segment.Zero(0, 5), Point.Zero(5), Segment.Zero(5, 10), Point.Zero(10), Segment.Zero(10, 20), Point.Zero(20),
            Segment.Zero(20, 30)
        }];
        yield return [a, new Rational(25), new Element[]
        {
            Point.Origin(), Segment.Zero(0, 10), Point.Zero(10), Segment.Zero(10, 20), Point.Zero(20), Segment.Zero(20, 25),
            Point.Zero(25), Segment.Zero(25, 30)
        }];
        yield return [new Sequence(new Element[] { new Segment(0, 2, 1, 1) }), new Rational(1), new Element[]
        {
            new Segment(0, 1, 1, 1), new Point(1, 2), new Segment(1, 2, 2, 1)
        }];
    }

    [Theory]
    [MemberData(nameof(SegmentPositions))]
    public void EnforceSplitAtGivesTheSplitElements(Sequence sequence, Rational splitTime, Element[] expected)
    {
        Assert.Equal(expected, sequence.EnforceSplitAt(splitTime).Elements);
    }

    [Fact]
    public void EnforceSplitAtAnExistingPointIsTheSequence()
    {
        Assert.Same(a, a.EnforceSplitAt(10));
    }

}
