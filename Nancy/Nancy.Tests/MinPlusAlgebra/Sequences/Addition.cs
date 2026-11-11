using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Addition
{
    private static readonly ComputationSettings SerialSettings = new() { UseParallelism = false };

    private static readonly ComputationSettings ParallelSettings = new()
    {
        UseParallelism = false, UseParallelAddition = true,
        MaxDegreeOfParallelism = 4, AdditionParallelizationThreshold = 0
    };

    public static IEnumerable<object[]> ParallelOperands()
    {
        foreach (var pair in Streams.Addition.FiniteOperandCases())
            yield return new object[] { pair.Left, pair.Right };
        foreach (var pair in Streams.Addition.InfiniteOperandCases())
            yield return new object[] { pair.Left, pair.Right };
    }

    [Theory]
    [MemberData(nameof(ParallelOperands))]
    public void ParallelAdditionMatchesSerial(Sequence left, Sequence right)
    {
        var serial = left.Addition(right, SerialSettings);
        var parallel = left.Addition(right, ParallelSettings);
        Assert.Equal(serial.Elements, parallel.Elements);
    }

    /// <summary>
    /// Sixteen unit steps give the boundary sampling interior times, so the opposite infinities from time 16 meet in the last of several chunks, inside a worker.
    /// </summary>
    [Fact]
    public void ParallelAdditionThrowsTheSerialExceptionFromAWorkerChunk()
    {
        static Sequence StepsThen(Point infinitePoint, Segment infiniteSegment)
        {
            var elements = new List<Element>();
            for (int i = 0; i < 16; i++)
            {
                elements.Add(new Point(i, i));
                elements.Add(new Segment(i, i + 1, i, 0));
            }
            elements.Add(infinitePoint);
            elements.Add(infiniteSegment);
            return new Sequence(elements);
        }

        var left = StepsThen(Point.PlusInfinite(16), Segment.PlusInfinite(16, 20));
        var right = StepsThen(Point.MinusInfinite(16), Segment.MinusInfinite(16, 20));

        var serial = Assert.Throws<UndeterminedResultException>(() => left.Addition(right, SerialSettings));
        var parallel = Assert.Throws<UndeterminedResultException>(() => left.Addition(right, ParallelSettings));
        Assert.Equal(serial.Message, parallel.Message);
    }

    [Fact]
    public void Add()
    {
        Sequence f1 = TestFunctions.SequenceA;
        Sequence f2 = TestFunctions.SequenceB;

        Sequence sum = f1.Addition(f2);

        Assert.Equal(0, sum.DefinedFrom);
        Assert.Equal(6, sum.DefinedUntil);

        //Assert.Equal(3, sum.Optimize().Count);
        Assert.True(sum.GetSegmentBefore(3).Slope > 0);
        Assert.True(sum.GetSegmentAfter(3).Slope < 0);
        Assert.Equal(-2, sum.GetSegmentAfter(5).Slope);
        Assert.Equal(26, sum.LeftLimitAt(6));
    }

    public static IEnumerable<object[]> GetSigmaRhoRateLatencyTests()
    {
        var testcases = new List<(Sequence a, Sequence b, Sequence expected)>
        {
            (
                a: new SigmaRhoArrivalCurve(sigma: 100, rho: 5).Extend(20),
                b: new RateLatencyServiceCurve(rate: 20, latency: 10).Extend(20),
                expected: new Sequence(new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 10, 100, 5),
                    new Point(10, 150),
                    new Segment(10, 20, 150, 25)
                })
            ),
            (
                a: new RateLatencyServiceCurve(rate: 20, latency: 10).Extend(20),
                b: new SigmaRhoArrivalCurve(sigma: 100, rho: 5).Extend(20),
                expected: new Sequence(new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 10, 100, 5),
                    new Point(10, 150),
                    new Segment(10, 20, 150, 25)
                })
            )
        };

        foreach (var (a, b, expected) in testcases)
        {
            yield return new object[] { a, b, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetSigmaRhoRateLatencyTests))]
    public void SigmaRhoRateLatency(Sequence a, Sequence b, Sequence expected)
    {
        var diff = a + b;
        Assert.True(Sequence.Equivalent(expected, diff));
    }

    /// <summary>
    /// $t$ over $[0, 1]$, and $t + 5$ over $[1, 2]$, $]1, 2]$, and $t$ over $[0, 1[$.
    /// </summary>
    private static readonly Sequence TOn01 = new([ Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1) ]);
    private static readonly Sequence TPlus5On12 = new([ new Point(1, 6), new Segment(1, 2, 6, 1), new Point(2, 7) ]);
    private static readonly Sequence TPlus5OnOpen12 = new([ new Segment(1, 2, 6, 1), new Point(2, 7) ]);
    private static readonly Sequence TOn01Open = new([ Point.Origin(), new Segment(0, 1, 0, 1) ]);

    /// <summary>
    /// Supports that share a single time, which both include, give the sum at that time alone.
    /// </summary>
    [Fact]
    public void SupportsSharingASingleTimeGiveTheSumThere()
    {
        var sum = TOn01 + TPlus5On12;
        Assert.True(Sequence.Equivalent(new Sequence([ new Point(1, 7) ]), sum), $"{sum}");
    }

    /// <summary>
    /// Supports that touch at a time one of them excludes share no time, and the sum throws.
    /// </summary>
    [Fact]
    public void SupportsTouchingAtAnExcludedTimeAreRejected()
    {
        Assert.Throws<ArgumentException>(() => TOn01 + TPlus5OnOpen12);
        Assert.Throws<ArgumentException>(() => TOn01Open + TPlus5On12);
    }
}
