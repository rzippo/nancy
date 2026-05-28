using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Tests equivalence of different methods to sample element of a Sequence
/// </summary>
public class GetElement
{
    public static IEnumerable<object[]> GetTestCases()
    {
        // taken from SequenceMax tests
        var pairs = new List<(Sequence a, Sequence b)>
        {
            (
                a: new SigmaRhoArrivalCurve(sigma:100, rho: 5).Cut(0, 20),
                b: new RateLatencyServiceCurve(rate: 20, latency: 10).Cut(0, 20)
            ),
            (
                a: new SigmaRhoArrivalCurve(sigma:100, rho: 5).Cut(0, 20),
                b: new RateLatencyServiceCurve(rate: 20, latency: 10).Cut(5, 30)
            ),
            (
                a: new Curve(
                    baseSequence: new Sequence(
                        elements: new Element[]
                        {
                            new Segment(
                                startTime: 4,
                                endTime: 8,
                                rightLimitAtStartTime: 4,
                                slope: 2
                            )
                        }
                    ),
                    pseudoPeriodStart: 4,
                    pseudoPeriodLength: 4,
                    pseudoPeriodHeight: 6,
                    isPartialCurve: true
                )
                .Cut(0, 20),
                b: new Curve(
                    baseSequence: new Sequence(
                        elements: new Element[]
                        {
                            new Segment(
                                startTime: 5,
                                endTime: 6,
                                rightLimitAtStartTime: 5,
                                slope: 2
                            ),
                            new Point(
                                time: 6,
                                value: 7
                            ),
                            new Segment(
                                startTime: 6,
                                endTime: 7,
                                rightLimitAtStartTime: 6,
                                slope: 2
                            )
                        }
                    ),
                    pseudoPeriodStart: 6,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1,
                    isPartialCurve: true
                )
                .Cut(0, 20)
            ),
            (
                a: new FlowControlCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)).Cut(0, 300),
                b: new FlowControlCurve(new Rational(3*7*13), 5000, new Rational(3*7*13)).Cut(0, 300)
            )
        };

        var testCases = pairs.Select(p => Sequence.Maximum(p.a, p.b));

        foreach (var s in testCases)
            yield return new object[] { s };
    }


    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void ElementAt(Sequence s)
    {

        var breakPointTimes = s.EnumerateBreakpoints()
            .Select(x => x.center.Time)
            .ToList();
        var times = breakPointTimes.Zip(breakPointTimes.Skip(1))
            .SelectMany(tuple => new[]
            {
                tuple.First,
                (tuple.First + tuple.Second) / 2,
                tuple.Second
            })
            .OrderBy(t => t)
            .OrderedDistinct()
            .Where(s.IsDefinedAt)
            .ToList();

        var index = 0;
        foreach (var time in times)
        {
            var viaBinarySearch = s.GetElementAt(time);
            var (viaLinearSearch, i) = s.GetElementAt_Linear(time, index);
            Assert.Equal(viaBinarySearch, viaLinearSearch);
            index = i;
        }
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void ElementBefore(Sequence s)
    {
       var breakPointTimes = s.EnumerateBreakpoints()
            .Select(x => x.center.Time)
            .ToList();
        var times = breakPointTimes.Zip(breakPointTimes.Skip(1))
            .SelectMany(tuple => new[]
            {
                tuple.First,
                (tuple.First + tuple.Second) / 2,
                tuple.Second
            })
            .OrderBy(t => t)
            .OrderedDistinct()
            .Where(s.IsDefinedBefore)
            .ToList();

        var index = 0;
        foreach (var time in times.Skip(1))
        {
            var viaBinarySearch = s.GetSegmentBefore(time);
            var (viaLinearSearch, i) = s.GetSegmentBefore_Linear(time, index);
            Assert.Equal(viaBinarySearch, viaLinearSearch);
            index = i;
        }
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void ElementAfter(Sequence s)
    {
       var breakPointTimes = s.EnumerateBreakpoints()
            .Select(x => x.center.Time)
            .ToList();
        var times = breakPointTimes.Zip(breakPointTimes.Skip(1))
            .SelectMany(tuple => new[]
            {
                tuple.First,
                (tuple.First + tuple.Second) / 2,
                tuple.Second
            })
            .OrderBy(t => t)
            .OrderedDistinct()
            .Where(s.IsDefinedAfter)
            .ToList();

        var index = 0;
        foreach (var time in times.Skip(1))
        {
            var viaBinarySearch = s.GetSegmentAfter(time);
            var (viaLinearSearch, i) = s.GetSegmentAfter_Linear(time, index);
            Assert.Equal(viaBinarySearch, viaLinearSearch);
            index = i;
        }
    }

    [Fact]
    public void GetElementAt_Linear_OutOfSupport_Throws()
    {
        var s = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 10, 0, 1)
        });
        Assert.Throws<ArgumentException>(() => s.GetElementAt_Linear(20));
    }

    [Fact]
    public void GetElementAt_Linear_BadStartingIndex_Throws()
    {
        // time=0 is covered by Point(0,0) at index 0.
        // startingIndex=1 skips it, and no later element covers time 0.
        var s = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 10, 0, 1)
        });
        Assert.Throws<ArgumentException>(() => s.GetElementAt_Linear(0, startingIndex: 1));
    }

    [Fact]
    public void GetSegmentBefore_Linear_OutOfSupport_Throws()
    {
        var s = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 10, 0, 1)
        });
        Assert.Throws<ArgumentException>(() => s.GetSegmentBefore_Linear(20));
    }

    [Fact]
    public void GetSegmentBefore_Linear_AtPoint_Throws()
    {
        // time=0 matches Point(0,0) first via EndTime >= 0,
        // cast to Segment fails → InvalidCastException caught.
        var s = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 10, 0, 1)
        });
        Assert.Throws<ArgumentException>(() => s.GetSegmentBefore_Linear(0));
    }

    [Fact]
    public void GetSegmentAfter_Linear_OutOfSupport_Throws()
    {
        var s = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 10, 0, 1)
        });
        Assert.Throws<ArgumentException>(() => s.GetSegmentAfter_Linear(20));
    }

    [Fact]
    public void GetSegmentAfter_Linear_BadStartingIndex_Throws()
    {
        // time=2 is covered by Segment(0,5) at index 1.
        // startingIndex=2 skips it, and no later segment has StartTime <= 2.
        var s = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 5, 0, 0),
            new Point(5, 0),
            new Segment(5, 10, 0, 0)
        });
        Assert.Throws<ArgumentException>(() => s.GetSegmentAfter_Linear(2, startingIndex: 2));
    }
}