using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

/// <summary>
/// Checks streamed envelopes at crossings, support boundaries and infinities, and their incremental consumption.
/// </summary>
public class Envelopes
{
    public static IEnumerable<(string Name, Sequence Left, Sequence Right)> OperandCases()
    {
        foreach (var data in Addition.FiniteOperandCases().Concat(Addition.InfiniteOperandCases()))
            yield return data;
        yield return ("crossing inside segments", new Sequence([new Segment(0, 4, 0, 1)]), new Sequence([new Segment(0, 4, 4, -1)]));
        yield return ("crossing at a breakpoint", new Sequence([new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2), new Segment(2, 4, 2, 1)]), new Sequence([Segment.Constant(0, 4, 2)]));
        yield return ("opposite infinities", Sequence.PlusInfinite(0, 4), Sequence.MinusInfinite(0, 4));
        yield return ("equal infinities", Sequence.PlusInfinite(0, 4), Sequence.PlusInfinite(0, 4));
        yield return ("negative values", new Sequence([new Segment(0, 4, -4, 1)]), new Sequence([Segment.Constant(0, 4, -2)]));
    }

    public static IEnumerable<object[]> Operands() => OperandCases().ToXUnitTestCases();

    public static IEnumerable<(string Name, Sequence Left, Sequence Right)> SupportPairCases()
    {
        var sequences = new List<Sequence>();
        foreach (var start in new[] { 0, 1 })
        foreach (var leftClosed in new[] { false, true })
        foreach (var rightClosed in new[] { false, true })
        foreach (var slope in new[] { -1, 2 })
        {
            var elements = new List<Element>();
            if (leftClosed) elements.Add(new Point(start, 3));
            elements.Add(new Segment(start, 2, -1, slope));
            elements.Add(new Point(2, 4));
            elements.Add(new Segment(2, 3, 2, -slope));
            if (rightClosed) elements.Add(new Point(3, -2));
            sequences.Add(new Sequence(elements));
        }
        foreach (var a in sequences)
        foreach (var b in sequences)
            yield return ("support and jump combinations", a, b);
    }

    public static IEnumerable<object[]> SupportPairs() => SupportPairCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Operands))]
    [MemberData(nameof(SupportPairs))]
    public void EnvelopesMatchTheBucketReferences(string name, Sequence a, Sequence b)
    {
        Assert.NotEmpty(name);
        foreach (var (left, right) in new[] { (a, b), (b, a) })
        {
            Check(MaterializedPointwiseReference.Minimum(left, right), left.Elements.Minimum(right.Elements));
            Check(MaterializedPointwiseReference.Maximum(left, right), left.Elements.Maximum(right.Elements));
        }
    }

    private static void Check(Sequence expected, IEnumerable<Element> stream)
    {
        var actual = stream.ToList();
        Assert.True(actual.AreUninterruptedSequence());
        Assert.True(Sequence.Equivalent(expected, actual.MergeAsEnumerable()
            .ToSequence()));
    }

    [Fact]
    public void MinimumCrossingSegmentsIncludeTheirIntersection()
    {
        Element[] a = [new Segment(0, 4, 0, 1)];
        Element[] b = [new Segment(0, 4, 4, -1)];
        var actual = (a.Minimum(b))
            .ToList();
        Assert.Equal(3, actual.Count);
        Assert.Equal(new Point(2, 2), actual[1]);
        Assert.Equal(0, ((Segment) actual[0]).RightLimitAtStartTime);
        Assert.Equal(0, ((Segment) actual[2]).LeftLimitAtEndTime);
    }

    [Fact]
    public void MaximumCrossingSegmentsIncludeTheirIntersection()
    {
        Element[] a = [new Segment(0, 4, 0, 1)];
        Element[] b = [new Segment(0, 4, 4, -1)];
        var actual = (a.Maximum(b))
            .ToList();
        Assert.Equal(3, actual.Count);
        Assert.Equal(new Point(2, 2), actual[1]);
        Assert.Equal(4, ((Segment) actual[0]).RightLimitAtStartTime);
        Assert.Equal(4, ((Segment) actual[2]).LeftLimitAtEndTime);
    }

    [Fact]
    public void MinimumEmptyAndDisjointSupportsAreRejected()
    {
        Element[] a = [new Point(0, 1), Segment.Constant(0, 1, 1)];
        Element[] b = [new Point(1, 2), Segment.Constant(1, 2, 2)];
        Assert.Throws<ArgumentException>(() => (a.Minimum(b))
            .ToList());
        Assert.Throws<ArgumentException>(() => (b.Minimum(a))
            .ToList());
        Assert.Throws<ArgumentException>(() => (a.Minimum([]))
            .ToList());
        Assert.Throws<ArgumentException>(() => (Enumerable.Empty<Element>().Minimum(a))
            .ToList());
    }

    [Fact]
    public void MaximumEmptyAndDisjointSupportsAreRejected()
    {
        Element[] a = [new Point(0, 1), Segment.Constant(0, 1, 1)];
        Element[] b = [new Point(1, 2), Segment.Constant(1, 2, 2)];
        Assert.Throws<ArgumentException>(() => (a.Maximum(b))
            .ToList());
        Assert.Throws<ArgumentException>(() => (b.Maximum(a))
            .ToList());
        Assert.Throws<ArgumentException>(() => (a.Maximum([]))
            .ToList());
        Assert.Throws<ArgumentException>(() => (Enumerable.Empty<Element>().Maximum(a))
            .ToList());
    }

    [Fact]
    public void MinimumEarlyComparisonDisposesBothOperandsAtTheConsumedPrefix()
    {
        var disposed = 0;
        var pulled = 0;
        IEnumerable<Element> Source(int value)
        {
            try
            {
                pulled++;
                yield return new Point(0, value);
                throw new InvalidOperationException("The tail must not be read.");
            }
            finally { disposed++; }
        }
        var left = Source(1);
        var right = Source(2);
        var envelope = left.Minimum(right);
        Assert.Equal(0, pulled);
        Assert.False(Sequence.Equivalent(new[] { new Point(0, 3) }, envelope));
        Assert.Equal(2, pulled);
        Assert.Equal(2, disposed);
    }

    [Fact]
    public void MaximumEarlyComparisonDisposesBothOperandsAtTheConsumedPrefix()
    {
        var disposed = 0;
        var pulled = 0;
        IEnumerable<Element> Source(int value)
        {
            try
            {
                pulled++;
                yield return new Point(0, value);
                throw new InvalidOperationException("The tail must not be read.");
            }
            finally { disposed++; }
        }
        var left = Source(1);
        var right = Source(2);
        var envelope = left.Maximum(right);
        Assert.Equal(0, pulled);
        Assert.False(Sequence.Equivalent(new[] { new Point(0, 3) }, envelope));
        Assert.Equal(2, pulled);
        Assert.Equal(2, disposed);
    }

    [Fact]
    public void MinimumInfiniteOperandsProduceAFinitePrefix()
    {
        var pulls = 0;
        IEnumerable<Element> Source(int value)
        {
            for (var t = 0; t < 10; t++)
            {
                pulls++;
                yield return new Point(t, value);
                pulls++;
                yield return Segment.Constant(t, t + 1, value);
            }
            throw new InvalidOperationException("The stream was read past the prefix the operation needs.");
        }
        var left = Source(1);
        var right = Source(2);
        var prefix = (left.Minimum(right))
            .Take(5)
            .ToList();
        Assert.Equal(5, prefix.Count);
        Assert.InRange(pulls, 2, 12);
        Assert.All(prefix, e => Assert.Equal(1, e is Point p ? p.Value : ((Segment) e).RightLimitAtStartTime));
    }

    [Fact]
    public void MaximumInfiniteOperandsProduceAFinitePrefix()
    {
        var pulls = 0;
        IEnumerable<Element> Source(int value)
        {
            for (var t = 0; t < 10; t++)
            {
                pulls++;
                yield return new Point(t, value);
                pulls++;
                yield return Segment.Constant(t, t + 1, value);
            }
            throw new InvalidOperationException("The stream was read past the prefix the operation needs.");
        }
        var left = Source(1);
        var right = Source(2);
        var prefix = (left.Maximum(right))
            .Take(5)
            .ToList();
        Assert.Equal(5, prefix.Count);
        Assert.InRange(pulls, 2, 12);
        Assert.All(prefix, e => Assert.Equal(2, e is Point p ? p.Value : ((Segment) e).RightLimitAtStartTime));
    }

    public static IEnumerable<object[]> InvalidInputs() => ValidationCases.InvalidInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void MinimumRejectsMalformedOperands(Element[] elements)
    {
        Element[] other = [new Point(0, 1), Segment.Constant(0, 10, 1), new Point(10, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Minimum(other)
            .ToList());
        Assert.Throws<ArgumentException>(() => other.Minimum(ValidationCases.SingleUse(elements))
            .ToList());
    }

    [Fact]
    public void MinimumEnumeratesEachOperandOnce()
    {
        Assert.NotEmpty(ValidationCases.SingleUse(ValidationCases.ValidInput())
            .Minimum(ValidationCases.SingleUse(ValidationCases.ValidInput()))
            .ToList());
    }

    [Fact]
    public void MinimumLeavesAnInvalidUnreadTailAlone()
    {
        Element[] elements = [new Point(0, 0), new Point(-1, 1)];
        Assert.Single(ValidationCases.SingleUse(elements).Minimum(ValidationCases.ValidInput())
            .Take(1));
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void MaximumRejectsMalformedOperands(Element[] elements)
    {
        Element[] other = [new Point(0, 1), Segment.Constant(0, 10, 1), new Point(10, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Maximum(other)
            .ToList());
        Assert.Throws<ArgumentException>(() => other.Maximum(ValidationCases.SingleUse(elements))
            .ToList());
    }

    [Fact]
    public void MaximumEnumeratesEachOperandOnce()
    {
        Assert.NotEmpty(ValidationCases.SingleUse(ValidationCases.ValidInput())
            .Maximum(ValidationCases.SingleUse(ValidationCases.ValidInput()))
            .ToList());
    }

    [Fact]
    public void MaximumLeavesAnInvalidUnreadTailAlone()
    {
        Element[] elements = [new Point(0, 0), new Point(-1, 1)];
        Assert.Single(ValidationCases.SingleUse(elements).Maximum(ValidationCases.ValidInput())
            .Take(1));
    }
}
