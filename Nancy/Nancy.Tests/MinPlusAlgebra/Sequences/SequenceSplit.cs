using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceSplit
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
}