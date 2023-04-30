using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Intervals;

public class SequencePairMerge
{
    [Fact]
    public void IntervalsComputation1()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(1),
            Segment.Zero(1, 3),

            Point.Zero(3),
            Segment.Zero(3, 4)
        };
        Sequence sequenceB = new Sequence(elementsB);


        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(8, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 2, 2, 2, 2, 2, 2 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation2()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(1),
            Segment.Zero(1, 3),

            Point.Zero(3),
            Segment.Zero(3, 5)
        };
        Sequence sequenceB = new Sequence(elementsB);


        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(10, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation3()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };
        Sequence sequenceB = new Sequence(elementsB);


        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(4, intervals.Count);

        int[] expectedSegmentCounts = { 2, 2, 2, 2 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation4()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4),

            Point.Zero(4),
            Segment.Zero(4, 5),

            Point.Zero(5),
            Segment.Zero(5, 7),

            Point.Zero(7),
            Segment.Zero(7, 8),
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(1),
            Segment.Zero(1, 3),

            Point.Zero(3),
            Segment.Zero(3, 5),

            Point.Zero(5),
            Segment.Zero(5, 6)
        };
        Sequence sequenceB = new Sequence(elementsB);


        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(16, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void NegativeTimes()
    {
        Element[] elementsA =
        {
            Point.Zero(-3),
            Segment.Zero(-3, -1),

            Point.Zero(-1),
            Segment.Zero(-1, 1),

            Point.Zero(1)
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Segment.Zero(-2, 0),

            Point.Zero(0),
            Segment.Zero(0, 2)
        };
        Sequence sequenceB = new Sequence(elementsB);

        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(10, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 1, 2, 2, 2, 2, 2, 2, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation1_Negated()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };

        elementsA = elementsA
            .Select(e => e.Delay(-5))
            .ToArray();

        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(1),
            Segment.Zero(1, 3),

            Point.Zero(3),
            Segment.Zero(3, 4)
        };

        elementsB = elementsB
            .Select(e => e.Delay(-5))
            .ToArray();

        Sequence sequenceB = new Sequence(elementsB);


        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(8, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 2, 2, 2, 2, 2, 2 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation2_Negated()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };

        elementsA = elementsA
            .Select(e => e.Delay(-6))
            .ToArray();

        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(1),
            Segment.Zero(1, 3),

            Point.Zero(3),
            Segment.Zero(3, 5)
        };

        elementsB = elementsB
            .Select(e => e.Delay(-6))
            .ToArray();

        Sequence sequenceB = new Sequence(elementsB);


        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(10, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation3_Negated()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };

        elementsA = elementsA
            .Select(e => e.Delay(-5))
            .ToArray();

        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };

        elementsB = elementsB
            .Select(e => e.Delay(-5))
            .ToArray();

        Sequence sequenceB = new Sequence(elementsB);


        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(4, intervals.Count);

        int[] expectedSegmentCounts = { 2, 2, 2, 2 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation4_Negated()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4),

            Point.Zero(4),
            Segment.Zero(4, 5),

            Point.Zero(5),
            Segment.Zero(5, 7),

            Point.Zero(7),
            Segment.Zero(7, 8),
        };

        elementsA = elementsA
            .Select(e => e.Delay(-9))
            .ToArray();

        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(1),
            Segment.Zero(1, 3),

            Point.Zero(3),
            Segment.Zero(3, 5),

            Point.Zero(5),
            Segment.Zero(5, 6)
        };

        elementsB = elementsB
            .Select(e => e.Delay(-9))
            .ToArray();

        Sequence sequenceB = new Sequence(elementsB);


        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(16, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void NonOverlappingSequences1()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(5),
            Segment.Zero(5, 7),

            Point.Zero(7),
            Segment.Zero(7, 9)
        };
        Sequence sequenceB = new Sequence(elementsB);

        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(8, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 1, 1, /* 0,*/ 1, 1, 1, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void NonOverlappingSequences2()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4)
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Segment.Zero(5, 7),

            Point.Zero(7),
            Segment.Zero(7, 9)
        };
        Sequence sequenceB = new Sequence(elementsB);

        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(7, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 1, 1, /* 0,*/ 1, 1, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void NonOverlappingSequences3()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4),

            Point.Zero(4)
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Point.Zero(5),
            Segment.Zero(5, 7),

            Point.Zero(7),
            Segment.Zero(7, 9)
        };
        Sequence sequenceB = new Sequence(elementsB);

        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(9, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 1, 1, 1, /* 0,*/ 1, 1, 1, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void NonOverlappingSequences4()
    {
        Element[] elementsA =
        {
            Point.Zero(0),
            Segment.Zero(0, 2),

            Point.Zero(2),
            Segment.Zero(2, 4),

            Point.Zero(4)
        };
        Sequence sequenceA = new Sequence(elementsA);

        Element[] elementsB =
        {
            Segment.Zero(5, 7),

            Point.Zero(7),
            Segment.Zero(7, 9)
        };
        Sequence sequenceB = new Sequence(elementsB);

        var intervals = Interval.ComputeIntervals(sequenceA, sequenceB);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(8, intervals.Count);

        int[] expectedSegmentCounts = { 1, 1, 1, 1, 1, /* 0,*/ 1, 1, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    public static IEnumerable<object[]> ElementsEnumerationCases()
    {
        var testCases = new (Sequence a, Sequence b, List<Element> expected)[]
        {
            (
                a: new Sequence(
                    new Element[]
                    {
                        Point.Zero(0),
                        Segment.Zero(0,1),
                        Point.Zero(1),
                        Segment.Zero(1,3),
                        Point.Zero(3),
                        Segment.Zero(3,4),
                        Point.Zero(4),
                        Segment.Zero(4,5)
                    }    
                ),
                b: new Sequence(
                    new Element[]
                    {
                        Point.Zero(1),
                        Segment.Zero(1,2),
                        Point.Zero(2),
                        Segment.Zero(2,3),
                        Point.Zero(3),
                        Segment.Zero(3,5),
                        Point.Zero(5),
                        Segment.Zero(5,6)
                    }    
                ),
                expected: new List<Element>
                {
                    Point.Zero(0),
                    Segment.Zero(0,1),
                    Point.Zero(1),
                    Point.Zero(1),
                    Segment.Zero(1,2),
                    Segment.Zero(1,2),
                    Point.Zero(2),
                    Point.Zero(2),
                    Segment.Zero(2,3),
                    Segment.Zero(2,3),
                    Point.Zero(3),
                    Point.Zero(3),
                    Segment.Zero(3,4),
                    Segment.Zero(3,4),
                    Point.Zero(4),
                    Point.Zero(4),
                    Segment.Zero(4,5),
                    Segment.Zero(4,5),
                    Point.Zero(5),
                    Segment.Zero(5,6)
                }
            ),
            (
                a: new Sequence(
                    new Element[]
                    {
                        Point.Zero(1),
                        Segment.Zero(1,2),
                        Point.Zero(2),
                        Segment.Zero(2,3),
                        Point.Zero(3),
                        Segment.Zero(3,5),
                        Point.Zero(5),
                        Segment.Zero(5,6)
                    }    
                ),
                b: new Sequence(
                    new Element[]
                    {
                        Point.Zero(0),
                        Segment.Zero(0,1),
                        Point.Zero(1),
                        Segment.Zero(1,3),
                        Point.Zero(3),
                        Segment.Zero(3,4),
                        Point.Zero(4),
                        Segment.Zero(4,5)
                    }    
                ),
                expected: new List<Element>
                {
                    Point.Zero(0),
                    Segment.Zero(0,1),
                    Point.Zero(1),
                    Point.Zero(1),
                    Segment.Zero(1,2),
                    Segment.Zero(1,2),
                    Point.Zero(2),
                    Point.Zero(2),
                    Segment.Zero(2,3),
                    Segment.Zero(2,3),
                    Point.Zero(3),
                    Point.Zero(3),
                    Segment.Zero(3,4),
                    Segment.Zero(3,4),
                    Point.Zero(4),
                    Point.Zero(4),
                    Segment.Zero(4,5),
                    Segment.Zero(4,5),
                    Point.Zero(5),
                    Segment.Zero(5,6)
                }
            ),
            (
                a: new Sequence(
                    new Element[]
                    {
                        Point.Zero(0),
                        Segment.Zero(0,2),
                        Point.Zero(2),
                        Segment.Zero(2,4),
                        Point.Zero(4),
                        Segment.Zero(4,6)
                    }    
                ),
                b: new Sequence(
                    new Element[]
                    {
                        Point.Zero(1),
                        Segment.Zero(1,3),
                        Point.Zero(3),
                        Segment.Zero(3,5)
                    }    
                ),
                expected: new List<Element>
                {
                    Point.Zero(0),
                    Segment.Zero(0,1),
                    Point.Zero(1),
                    Point.Zero(1),
                    Segment.Zero(1,2),
                    Segment.Zero(1,2),
                    Point.Zero(2),
                    Point.Zero(2),
                    Segment.Zero(2,3),
                    Segment.Zero(2,3),
                    Point.Zero(3),
                    Point.Zero(3),
                    Segment.Zero(3,4),
                    Segment.Zero(3,4),
                    Point.Zero(4),
                    Point.Zero(4),
                    Segment.Zero(4,5),
                    Segment.Zero(4,5),
                    Point.Zero(5),
                    Segment.Zero(5,6)
                }
            ),
            (
                a: new Sequence(
                    new Element[]
                    {
                        Point.Zero(1),
                        Segment.Zero(1,3),
                        Point.Zero(3),
                        Segment.Zero(3,5)
                    }    
                ),
                b: new Sequence(
                    new Element[]
                    {
                        Point.Zero(0),
                        Segment.Zero(0,2),
                        Point.Zero(2),
                        Segment.Zero(2,4),
                        Point.Zero(4),
                        Segment.Zero(4,6)
                    }    
                ),
                expected: new List<Element>
                {
                    Point.Zero(0),
                    Segment.Zero(0,1),
                    Point.Zero(1),
                    Point.Zero(1),
                    Segment.Zero(1,2),
                    Segment.Zero(1,2),
                    Point.Zero(2),
                    Point.Zero(2),
                    Segment.Zero(2,3),
                    Segment.Zero(2,3),
                    Point.Zero(3),
                    Point.Zero(3),
                    Segment.Zero(3,4),
                    Segment.Zero(3,4),
                    Point.Zero(4),
                    Point.Zero(4),
                    Segment.Zero(4,5),
                    Segment.Zero(4,5),
                    Point.Zero(5),
                    Segment.Zero(5,6)
                }
            ),
            (
                a: new Sequence(
                    new Element[]
                    {
                        Point.Zero(0),
                        Segment.Zero(0,2),
                        Point.Zero(2),
                        Segment.Zero(2,4)
                    }    
                ),
                b: new Sequence(
                    new Element[]
                    {
                        Point.Zero(1),
                        Segment.Zero(1,3),
                        Point.Zero(3),
                        Segment.Zero(3,4)
                    }    
                ),
                expected: new List<Element>
                {
                    Point.Zero(0),
                    Segment.Zero(0,1),
                    Point.Zero(1),
                    Point.Zero(1),
                    Segment.Zero(1,2),
                    Segment.Zero(1,2),
                    Point.Zero(2),
                    Point.Zero(2),
                    Segment.Zero(2,3),
                    Segment.Zero(2,3),
                    Point.Zero(3),
                    Point.Zero(3),
                    Segment.Zero(3,4),
                    Segment.Zero(3,4)
                }
            )
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.a, testCase.b, testCase.expected };
    }

    [Theory]
    [MemberData(nameof(ElementsEnumerationCases))]
    public void ElementsEnumeration(Sequence a, Sequence b, List<Element> expected)
    {
        var result = Interval.ElementsIterator(a, b).ToList();
        Assert.Equal(expected, result);
    }
}