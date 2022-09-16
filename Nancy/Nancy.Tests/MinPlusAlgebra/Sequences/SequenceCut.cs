using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceCut
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
        
    //Long. Start closed, end closed
    private static Sequence e = new Sequence(new Element[]
    {
        Point.Origin(),
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30),
        Point.Zero(30),
        Segment.Zero(30, 40),
        Point.Zero(40),
        Segment.Zero(40, 50),
        Point.Zero(50)
    });
        
    public static IEnumerable<object[]> GetSuccessTestCases()
    {
        var testCases =
            new (Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartInclusive, bool isEndInclusive)
                []
                {
                    // Within extremes
                    (sequence: a, cutStart: 5, cutEnd: 15, isStartInclusive: false, isEndInclusive: true),
                    (sequence: a, cutStart: 5, cutEnd: 15, isStartInclusive: true, isEndInclusive: true),
                    (sequence: a, cutStart: 5, cutEnd: 15, isStartInclusive: false, isEndInclusive: false),
                    (sequence: a, cutStart: 5, cutEnd: 15, isStartInclusive: true, isEndInclusive: false),
                        
                    // At extremes
                    (sequence: a, cutStart: 0, cutEnd: 30, isStartInclusive: true, isEndInclusive: false),
                    (sequence: b, cutStart: 0, cutEnd: 30, isStartInclusive: false, isEndInclusive: false),
                    (sequence: c, cutStart: 0, cutEnd: 30, isStartInclusive: false, isEndInclusive: true),
                    (sequence: d, cutStart: 0, cutEnd: 30, isStartInclusive: true, isEndInclusive: true),
                        
                    // At segments extremes
                    (sequence: a, cutStart: 10, cutEnd: 20, isStartInclusive: true, isEndInclusive: false),
                    (sequence: a, cutStart: 10, cutEnd: 20, isStartInclusive: false, isEndInclusive: false),
                    (sequence: a, cutStart: 10, cutEnd: 20, isStartInclusive: false, isEndInclusive: true),
                    (sequence: a, cutStart: 10, cutEnd: 20, isStartInclusive: true, isEndInclusive: true),
                    (sequence: e, cutStart: 10, cutEnd: 40, isStartInclusive: true, isEndInclusive: false),
                    (sequence: e, cutStart: 10, cutEnd: 40, isStartInclusive: false, isEndInclusive: false),
                    (sequence: e, cutStart: 10, cutEnd: 40, isStartInclusive: false, isEndInclusive: true),
                    (sequence: e, cutStart: 10, cutEnd: 40, isStartInclusive: true, isEndInclusive: true),
                        
                    // Within segments extremes
                    (sequence: a, cutStart: 15, cutEnd: 25, isStartInclusive: true, isEndInclusive: false),
                    (sequence: a, cutStart: 15, cutEnd: 25, isStartInclusive: false, isEndInclusive: false),
                    (sequence: a, cutStart: 15, cutEnd: 25, isStartInclusive: false, isEndInclusive: true),
                    (sequence: a, cutStart: 15, cutEnd: 25, isStartInclusive: true, isEndInclusive: true),
                    (sequence: e, cutStart: 15, cutEnd: 45, isStartInclusive: true, isEndInclusive: false),
                    (sequence: e, cutStart: 15, cutEnd: 45, isStartInclusive: false, isEndInclusive: false),
                    (sequence: e, cutStart: 15, cutEnd: 45, isStartInclusive: false, isEndInclusive: true),
                    (sequence: e, cutStart: 15, cutEnd: 45, isStartInclusive: true, isEndInclusive: true),
                    
                    // Matching, both inclusive
                    (sequence: a, cutStart: 20, cutEnd: 20, isStartInclusive: true, isEndInclusive: true)
                };

        foreach (var testCase in testCases)
        {
            yield return new object[]
            {
                testCase.sequence,
                testCase.cutStart,
                testCase.cutEnd,
                testCase.isStartInclusive,
                testCase.isEndInclusive
            };
        }
    }

    public static IEnumerable<object[]> FromOtherTests()
    {
        var testCases =
            new (Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartInclusive, bool isEndInclusive)
                []
                {
                    (
                        sequence: new Sequence(new Element[]
                        {
                            Point.Zero(2),
                            Segment.Zero(2, 3),
                            Point.Zero(3),
                            Segment.Zero(3, 6),
                            Point.Zero(6),
                            Segment.Zero(6, 7),
                            Point.Zero(7),
                            Segment.Zero(7, 8),
                            Point.Zero(8),
                        }), 
                        cutStart: 2, 
                        cutEnd: new Rational(20, 7), 
                        isStartInclusive: true, 
                        isEndInclusive: false
                    ),
                    (
                        sequence: new Sequence(new Element[]
                        {
                            Segment.Zero(-100, -10),
                            Point.Zero(-10),
                            Segment.Zero(-10, 190),
                            Point.Zero(190),
                            Segment.Zero(190, 200), 
                        }), 
                        cutStart: 0, 
                        cutEnd: 100, 
                        isStartInclusive: true, 
                        isEndInclusive: false
                    )
                        
                };

        foreach (var testCase in testCases)
        {
            yield return new object[]
            {
                testCase.sequence,
                testCase.cutStart,
                testCase.cutEnd,
                testCase.isStartInclusive,
                testCase.isEndInclusive
            };
        }
    }
        
    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    [MemberData(nameof(FromOtherTests))]
    public void Cut(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartInclusive, bool isEndInclusive)
    {
        Sequence cut = sequence.Cut(cutStart, cutEnd, isStartInclusive, isEndInclusive);
            
        Assert.Equal(cutStart, cut.DefinedFrom);
        Assert.Equal(cutEnd, cut.DefinedUntil);
        Assert.Equal(isStartInclusive, cut.IsLeftClosed);
        Assert.Equal(isEndInclusive, cut.IsRightClosed);
    }
        
    public static IEnumerable<object[]> GetThrowingTestCases()
    {
        var testCases =
            new (Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartInclusive, bool isEndInclusive)
                []
                {
                    // Out of extremes
                    (sequence: a, cutStart: 5, cutEnd: 35, isStartInclusive: false, isEndInclusive: true),
                    (sequence: a, cutStart: 5, cutEnd: 35, isStartInclusive: true, isEndInclusive: true),
                    (sequence: a, cutStart: 5, cutEnd: 35, isStartInclusive: false, isEndInclusive: false),
                    (sequence: a, cutStart: 5, cutEnd: 35, isStartInclusive: true, isEndInclusive: false),
                        
                    // At extremes
                    (sequence: a, cutStart: 0, cutEnd: 30, isStartInclusive: true, isEndInclusive: true),
                    (sequence: b, cutStart: 0, cutEnd: 30, isStartInclusive: true, isEndInclusive: true),
                    (sequence: c, cutStart: 0, cutEnd: 30, isStartInclusive: true, isEndInclusive: true),
                    
                    // Matching, either non-inclusive
                    (sequence: a, cutStart: 20, cutEnd: 20, isStartInclusive: false, isEndInclusive: true),
                    (sequence: a, cutStart: 20, cutEnd: 20, isStartInclusive: true, isEndInclusive: false),
                    (sequence: a, cutStart: 20, cutEnd: 20, isStartInclusive: false, isEndInclusive: false)
                };

        foreach (var testCase in testCases)
        {
            yield return new object[]
            {
                testCase.sequence,
                testCase.cutStart,
                testCase.cutEnd,
                testCase.isStartInclusive,
                testCase.isEndInclusive
            };
        }
    }
        
    [Theory]
    [MemberData(nameof(GetThrowingTestCases))]
    public void CutChecks(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartInclusive, bool isEndInclusive)
    {
        Assert.Throws<ArgumentException>(() => sequence.Cut(cutStart, cutEnd, isStartInclusive, isEndInclusive));
    }
}