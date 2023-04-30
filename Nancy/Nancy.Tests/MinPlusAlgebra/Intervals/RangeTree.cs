using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Intervals;

public class RangeTree
{
    [Fact]
    public void IntervalsComputation1()
    {
        Element[] elements = 
        {
            Segment.Zero(0, 1), 
            Segment.Zero(5, 9),
            Segment.Zero(11, 15),
            Segment.Zero(3, 11),
            Segment.Zero(13, 15),
            Segment.Zero(0, 5),
            Point.Zero(7),
            Segment.Zero(9, 15)
        };

        var intervals = Interval.ComputeIntervals(elements);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(15, intervals.Count);

        int[] expectedSegmentCounts = { 2, 1, 1, 1, 2, 1, 2, 3, 2, 1, 2, 1, 2, 2, 3 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation1_1000()
    {
        Element[] elements = 
        {
            Segment.Zero(0, 1), 
            Segment.Zero(5, 9),
            Segment.Zero(11, 15),
            Segment.Zero(3, 11),
            Segment.Zero(13, 15),
            Segment.Zero(0, 5),
            Point.Zero(7),
            Segment.Zero(9, 15)
        };

        IEnumerable<Element> replicated = new Element[]{};
        for (int i = 0; i < 1000; i++)
            replicated = replicated.Concat(elements);
        elements = replicated.ToArray();

        var settings = new ComputationSettings
        {
            ParallelComputeIntervalsThreshold = 100
        };

        var intervals = Interval.ComputeIntervals(elements, settings);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(15, intervals.Count);

        int[] expectedSegmentCounts = { 2, 1, 1, 1, 2, 1, 2, 3, 2, 1, 2, 1, 2, 2, 3 };
        for (int i = 0; i < expectedSegmentCounts.Length; i++)
            expectedSegmentCounts[i] *= 1000;

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
        Element[] elements =
        {
            Segment.Zero(0, 10),
            Segment.Zero(0, 10),
            Segment.Zero(10, 60),
            Point.Zero(10),
            Segment.Zero(10, 60),
            Segment.Zero(10, 50),
            Segment.Zero(10, 60),
            Segment.Zero(60, 100)
        };

        var intervals = Interval.ComputeIntervals(elements);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(6, intervals.Count);

        int[] expectedSegmentCounts = { 2, 1, 4, 3, 3, 1 };
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
        Element[] elements =
        {
            Point.Origin(),
            Segment.Zero(0, 10),
            Point.Zero(10),
            Segment.Zero(10, 15),

            Point.Origin(),
            Segment.Zero(0, 10),
            Point.Zero(10),
            Segment.Zero(10, 15),
        };

        var intervals = Interval.ComputeIntervals(elements);

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
        Element[] elements =
        {
            Point.Origin(),
            Point.Zero(10),
            Point.Origin(),
            Point.Origin(),
            Segment.Zero(0, 10),
            Segment.Zero(10, 15),

            Segment.Zero(0, 10),
            Point.Zero(10),
            Segment.Zero(10, 15),
        };

        var intervals = Interval.ComputeIntervals(elements);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(4, intervals.Count);
        int[] expectedSegmentCounts = { 3, 2, 2, 2 };
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

        var elements = sequenceA.Elements.Concat(sequenceB.Elements);
        var intervals = Interval.ComputeIntervals(elements.ToList());

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
    public void IntervalsComputation1_Negative()
    {
        Element[] elements =
        {
            Segment.Zero(0, 1),
            Segment.Zero(5, 9),
            Segment.Zero(11, 15),
            Segment.Zero(3, 11),
            Segment.Zero(13, 15),
            Segment.Zero(0, 5),
            Point.Zero(7),
            Segment.Zero(9, 15)
        };

        //Make the test on negative times by shifting all back
        elements = elements.
            Select(e => e.Delay(-20))
            .ToArray();

        var intervals = Interval.ComputeIntervals(elements);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(15, intervals.Count);

        int[] expectedSegmentCounts = { 2, 1, 1, 1, 2, 1, 2, 3, 2, 1, 2, 1, 2, 2, 3 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation2_Negative()
    {
        Element[] elements =
        {
            Segment.Zero(0, 10),
            Segment.Zero(0, 10),
            Segment.Zero(10, 60),
            Point.Zero(10),
            Segment.Zero(10, 60),
            Segment.Zero(10, 50),
            Segment.Zero(10, 60),
            Segment.Zero(60, 100)
        };

        //Make the test on negative times by shifting all back
        elements = elements.
            Select(e => e.Delay(-200))
            .ToArray();

        var intervals = Interval.ComputeIntervals(elements);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(6, intervals.Count);

        int[] expectedSegmentCounts = { 2, 1, 4, 3, 3, 1 };
        for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
        {
            Assert.Equal(
                expectedSegmentCounts[intervalIndex],
                intervals[intervalIndex].Count);
        }
    }

    [Fact]
    public void IntervalsComputation3_Negative()
    {
        Element[] elements =
        {
            Point.Origin(),
            Segment.Zero(0, 10),
            Point.Zero(10),
            Segment.Zero(10, 15),

            Point.Origin(),
            Segment.Zero(0, 10),
            Point.Zero(10),
            Segment.Zero(10, 15),
        };

        //Make the test on negative times by shifting all back
        elements = elements.
            Select(e => e.Delay(-20))
            .ToArray();

        var intervals = Interval.ComputeIntervals(elements);

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
    public void IntervalsComputation4_Negative()
    {
        Element[] elements =
        {
            Point.Origin(),
            Point.Zero(10),
            Point.Origin(),
            Point.Origin(),
            Segment.Zero(0, 10),
            Segment.Zero(10, 15),

            Segment.Zero(0, 10),
            Point.Zero(10),
            Segment.Zero(10, 15),
        };

        //Make the test on negative times by shifting all back
        elements = elements.
            Select(e => e.Delay(-20))
            .ToArray();

        var intervals = Interval.ComputeIntervals(elements);

        Assert.True(intervals.AreInTimeOrder());
        Assert.Equal(4, intervals.Count);
        int[] expectedSegmentCounts = { 3, 2, 2, 2 };
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

        var elements = sequenceA.Elements.Concat(sequenceB.Elements);
        var intervals = Interval.ComputeIntervals(elements.ToList());

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

        var elements = sequenceA.Elements.Concat(sequenceB.Elements);
        var intervals = Interval.ComputeIntervals(elements.ToList());

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

        var elements = sequenceA.Elements.Concat(sequenceB.Elements);
        var intervals = Interval.ComputeIntervals(elements.ToList());

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

        var elements = sequenceA.Elements.Concat(sequenceB.Elements);
        var intervals = Interval.ComputeIntervals(elements.ToList());

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
}