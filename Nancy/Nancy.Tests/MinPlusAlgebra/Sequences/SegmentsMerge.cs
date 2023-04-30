using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SegmentsMerge
{
    [Fact]
    public void AlignedSequenceMerge()
    {
        Element[] segments = new Element[]
        {
            new Segment(
                startTime: 0,
                endTime: 5,
                rightLimitAtStartTime: 0,
                slope: 5
            ),
            new Point(time: 5, value: 25), 
            new Segment(
                startTime: 5,
                endTime: 10,
                rightLimitAtStartTime: 25,
                slope: 5
            ),
            new Point(time: 10, value: 50), 
            new Segment(
                startTime: 10,
                endTime: 15,
                rightLimitAtStartTime: 50,
                slope: 5
            )
        };

        var mergedSegments = segments.Merge();
        var mergedSegment = (Segment) mergedSegments.Single();

        Assert.Equal(0, mergedSegment.StartTime);
        Assert.Equal(15, mergedSegment.EndTime);
        Assert.Equal(5, mergedSegment.Slope);
    }

    [Fact]
    public void UnalignedSequenceMerge()
    {
        Element[] segments = new Element[]
        {
            new Segment(
                startTime: 0,
                endTime: 5,
                rightLimitAtStartTime: 0,
                slope: 5
            ),
            new Point(time: 5, value: 25),
            new Segment(
                startTime: 5,
                endTime: 10,
                rightLimitAtStartTime: 25,
                slope: 5
            ),
            new Point(time: 10, value: 40),
            new Segment(
                startTime: 10,
                endTime: 15,
                rightLimitAtStartTime: 50,
                slope: 5
            )
        };

        var mergedSegments = segments.Merge();
        Assert.True(mergedSegments.Count() > 1);
        Assert.Equal(0, mergedSegments.First().StartTime);
        Assert.Equal(15, mergedSegments.Last().EndTime);

        Sequence sequence = new Sequence(mergedSegments);
        Assert.Equal(40, sequence.ValueAt(10));
        Assert.Equal(50, sequence.RightLimitAt(10));
    }

    [Fact]
    public void InfiniteMerge()
    {
        Segment leftInfinite = Segment.PlusInfinite(0, 5);
        Point pointInfinite = Point.PlusInfinite(5);
        Segment rightInfinite = Segment.PlusInfinite(5, 10);

        Assert.True(SequenceExtensions.CanMergeTriplet(leftInfinite, pointInfinite, rightInfinite));

        Segment merge = SequenceExtensions.MergeTriplet(leftInfinite, pointInfinite, rightInfinite);
        Assert.True(merge.IsInfinite);
        Assert.Equal(0, merge.StartTime);
        Assert.Equal(10, merge.EndTime);
    }

    [Fact]
    public void AlignedSequenceMergeEnumerable()
    {
        Element[] segments = new Element[]
        {
            new Segment(
                startTime: 0,
                endTime: 5,
                rightLimitAtStartTime: 0,
                slope: 5
            ),
            new Point(time: 5, value: 25), 
            new Segment(
                startTime: 5,
                endTime: 10,
                rightLimitAtStartTime: 25,
                slope: 5
            ),
            new Point(time: 10, value: 50), 
            new Segment(
                startTime: 10,
                endTime: 15,
                rightLimitAtStartTime: 50,
                slope: 5
            )
        };

        var mergedSegments = segments.Merge();
        var mergedSegment = (Segment) mergedSegments.Single();

        Assert.Equal(0, mergedSegment.StartTime);
        Assert.Equal(15, mergedSegment.EndTime);
        Assert.Equal(5, mergedSegment.Slope);
    }

    [Fact]
    public void UnalignedSequenceMergeEnumerable()
    {
        Element[] segments = new Element[]
        {
            new Segment(
                startTime: 0,
                endTime: 5,
                rightLimitAtStartTime: 0,
                slope: 5
            ),
            new Point(time: 5, value: 25),
            new Segment(
                startTime: 5,
                endTime: 10,
                rightLimitAtStartTime: 25,
                slope: 5
            ),
            new Point(time: 10, value: 40),
            new Segment(
                startTime: 10,
                endTime: 15,
                rightLimitAtStartTime: 50,
                slope: 5
            )
        };

        var mergedSegments = segments.Merge().ToList();
        Assert.True(mergedSegments.Count > 1);
        Assert.Equal(0, mergedSegments.First().StartTime);
        Assert.Equal(15, mergedSegments.Last().EndTime);

        Sequence sequence = new Sequence(mergedSegments);
        Assert.Equal(40, sequence.ValueAt(10));
        Assert.Equal(50, sequence.RightLimitAt(10));
    }

    public static IEnumerable<object[]> MergeTestCases()
    {
        var testCases = new[]
        {
            new {
                Left = new Segment(
                    startTime: 0,
                    endTime: 5,
                    rightLimitAtStartTime: 0,
                    slope: 5
                ),
                Center = new Point(time: 5, value: 25),
                Right = new Segment(
                    startTime: 5,
                    endTime: 10,
                    rightLimitAtStartTime: 25,
                    slope: 5
                ),
                Expected = true
            },
            new {
                Left = new Segment(1, 2, -3, -1),
                Center = new Point(2, -4),
                Right = new Segment(2, 3, -4, -1),
                Expected = true
            }
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase.Left, testCase.Center, testCase.Right, testCase.Expected};
        }
    }

    [Theory]
    [MemberData(nameof(MergeTestCases))]
    public void CanMerge(Segment left, Point center, Segment right, bool expected)
    {
        var canMerge = SequenceExtensions.CanMergeTriplet(left, center, right);
        Assert.Equal(canMerge, expected);
    }
}