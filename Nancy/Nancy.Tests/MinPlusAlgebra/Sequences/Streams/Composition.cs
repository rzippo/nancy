using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

/// <summary>
/// Checks the forward composition sweep against inverse-image composition, including jumps, plateaus and boundary restrictions.
/// </summary>
public class Composition
{
    public static IEnumerable<(Sequence Outer, Sequence Inner)> CompositionPairs()
    {
        foreach (var elements in Properties.ElementCases())
        {
            var outer = new Sequence(elements);
            Sequence[] inners =
            [
                new Sequence([new Segment(0, 2, 0, 2)]),
                new Sequence([new Point(0, 0), Segment.Constant(0, 1, 0), new Point(1, 2), new Segment(1, 2, 2, 2)]),
                new Sequence([new Point(0, 1), Segment.Constant(0, 2, 1), new Point(2, 1)]),
                new Sequence([new Segment(0, 1, 0, 1), new Point(1, 1), Segment.Constant(1, 2, 1)]),
                new Sequence([new Point(2, 2)]),
                new Sequence([Segment.Constant(0, 1, 2)])
            ];
            foreach (var inner in inners) yield return (outer, inner);
        }
    }

    public static IEnumerable<object[]> Pairs() => CompositionPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Pairs))]
    public void TheSweepMatchesTheInverseImageReference(Sequence outer, Sequence inner)
    {
        Sequence expected;
        try { expected = MaterializedCompositionReference.Composition(outer, inner); }
        catch (ArgumentException)
        {
            Assert.Throws<ArgumentException>(() => outer.Elements.Composition(inner.Elements)
                .ToList());
            return;
        }
        var actual = outer.Elements.Composition(inner.Elements)
            .MergeAsEnumerable()
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, actual));
    }

    [Fact]
    public void IdentityCompositionConsumesOnlyAFinitePrefix()
    {
        var pulls = 0;
        IEnumerable<Element> Outer()
        {
            for (var i = 0; i < 10; i++)
            {
                pulls++; yield return new Point(i, i);
                pulls++; yield return new Segment(i, i + 1, i, 1);
            }
            throw new InvalidOperationException("The stream was read past the prefix the operation needs.");
        }
        Element[] inner = [new Point(0, 0), new Segment(0, 1_000_000, 0, 1)];
        var actual = Outer().Composition(inner)
            .Take(5)
            .ToList();
        Assert.Equal(5, actual.Count);
        Assert.InRange(pulls, 1, 8);
    }

    public static IEnumerable<object[]> InvalidInputs() => ValidationCases.InvalidInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void CompositionRejectsMalformedInnerOperands(Element[] elements)
    {
        Element[] outer = [new Point(0, 0), new Segment(0, 10, 0, 1), new Point(10, 10)];
        Assert.Throws<ArgumentException>(() => outer.Composition(ValidationCases.SingleUse(elements))
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void CompositionRejectsMalformedOuterOperands(Element[] elements)
    {
        Element[] inner = [new Point(0, 0), new Segment(0, 10, 0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Composition(inner)
            .ToList());
    }

    public static IEnumerable<(Sequence Outer, Sequence Inner)> UnsupportedPairs()
    {
        foreach (var pair in Sequences.Composition.RejectedPairs)
            yield return (pair.f, pair.g);

        var outer = new Sequence([new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2)]);
        Sequence[] invalidInnerValues =
        [
            new Sequence([new Segment(0, 1, 2, -1)]),
            new Sequence([new Point(0, 1), new Segment(0, 1, 0, 1)]),
            new Sequence([new Segment(0, 1, 0, 1), new Point(1, 0)]),
            // The value decreases below the preceding limit but stays ahead of the outer cursor's last lookup.
            new Sequence([new Segment(0, 1, 0, 1), new Point(1, new Rational(1, 2))]),
            new Sequence([Segment.Constant(0, 1, -1)]),
            new Sequence([Segment.PlusInfinite(0, 1)]),
            new Sequence([Segment.MinusInfinite(0, 1)]),
            new Sequence([Point.MinusInfinite(0)]),
            new Sequence([new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)])
        ];
        foreach (var inner in invalidInnerValues)
            yield return (outer, inner);

        var openOuter = new Sequence([new Segment(0, 2, 0, 1)]);
        Sequence[] uncoveredEndpoints =
        [
            new Sequence([new Point(0, 0)]),
            new Sequence([new Point(0, 2)]),
            new Sequence([Segment.Constant(0, 1, 0)]),
            new Sequence([Segment.Constant(0, 1, 2)]),
            new Sequence([new Point(0, 0), new Segment(0, 1, 0, 2)]),
            new Sequence([new Segment(0, 1, 0, 2), new Point(1, 2)]),
            new Sequence([Segment.Constant(0, 1, 0), new Point(1, 0), new Segment(1, 2, 0, 1)]),
            new Sequence([new Segment(0, 1, 1, 1), new Point(1, 2), Segment.Constant(1, 2, 2)])
        ];
        foreach (var inner in uncoveredEndpoints)
            yield return (openOuter, inner);

        yield return (
            new Sequence([new Point(1, 7)]),
            new Sequence([new Segment(0, 1, 1, 1)])
        );
        yield return (
            new Sequence([new Point(2, 7), new Segment(2, 3, 7, 1)]),
            new Sequence([new Segment(0, 1, 0, 3)])
        );
        yield return (
            new Sequence([new Point(2, 7)]),
            new Sequence([new Point(0, 1)])
        );
        yield return (
            new Sequence([new Segment(-2, 2, 7, 1)]),
            new Sequence([new Point(0, -1)])
        );
        yield return (
            new Sequence([new Point(Rational.PlusInfinity, 7)]),
            new Sequence([Point.PlusInfinite(0)])
        );
    }

    public static IEnumerable<object[]> UnsupportedCases() => UnsupportedPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UnsupportedCases))]
    public void StreamCompositionRejectsUnsupportedOperands(Sequence outer, Sequence inner)
    {
        Assert.Throws<ArgumentException>(() => outer.Elements.Composition(inner.Elements)
            .ToList());
    }

    [Fact]
    public void EmptyOuterOperandsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>()
            .Composition([new Point(0, 0)])
            .ToList());
    }

    [Fact]
    public void EmptyInnerOperandsAreRejected()
    {
        Element[] outer = [new Point(0, 0)];
        Assert.Throws<ArgumentException>(() => outer.Composition(Array.Empty<Element>())
            .ToList());
    }

    [Fact]
    public void MalformedOuterAdjacencyIsCheckedWhenTheNextElementIsConsumed()
    {
        Element[] outer = [new Point(0, 7), new Point(1, 9)];
        Element[] inner = [new Point(0, 0), Segment.Constant(0, 1, 0), new Point(1, 1)];
        using var result = outer.Composition(inner)
            .GetEnumerator();

        Assert.True(result.MoveNext());
        Assert.Equal(new Point(0, 7), result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal(Segment.Constant(0, 1, 7), result.Current);
        Assert.Throws<ArgumentException>(() => result.MoveNext());
    }

    [Fact]
    public void UncoveredLevelsAreRejectedWhenReached()
    {
        Element[] outer = [new Point(0, 7), Segment.Constant(0, 1, 9), new Point(1, 11)];
        Element[] inner = [new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)];
        using var result = outer.Composition(inner)
            .GetEnumerator();

        Assert.True(result.MoveNext());
        Assert.Equal(new Point(0, 7), result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal(Segment.Constant(0, 1, 9), result.Current);
        Assert.True(result.MoveNext());
        Assert.Equal(new Point(1, 11), result.Current);
        Assert.Throws<ArgumentException>(() => result.MoveNext());
    }

    [Fact]
    public void ACompletedCompositionLeavesUnusedOuterElementsUnread()
    {
        IEnumerable<Element> Outer()
        {
            yield return new Point(0, 0);
            yield return new Segment(0, 2, 0, 1);
            throw new InvalidOperationException("The remaining outer elements are not needed.");
        }

        Element[] inner = [Segment.Constant(0, 1, 1)];
        Assert.Equal(Segment.Constant(0, 1, 1), Assert.Single(Outer().Composition(inner)));
    }

    [Fact]
    public void EarlyTerminationDisposesBothOperands()
    {
        var outer = new ObservedElements(ValidationCases.ValidInput());
        var inner = new ObservedElements(ValidationCases.ValidInput());
        var result = outer.Composition(inner);
        Assert.False(outer.Started);
        Assert.False(inner.Started);

        Assert.Equal(new Point(0, 0), Assert.Single(result.Take(1)));
        Assert.True(outer.Disposed);
        Assert.True(inner.Disposed);
    }

    [Fact]
    public void ValidationFailureDisposesBothOperands()
    {
        var outer = new ObservedElements(ValidationCases.ValidInput());
        var inner = new ObservedElements([new Point(0, 0), Segment.Constant(0, 1, -1)]);

        Assert.Throws<ArgumentException>(() => outer.Composition(inner)
            .ToList());
        Assert.True(outer.Disposed);
        Assert.True(inner.Disposed);
    }

    [Fact]
    public void CompositionConsumesEachOperandOnceAndLeavesUnreadTailsAlone()
    {
        Assert.NotEmpty(ValidationCases.SingleUse(ValidationCases.ValidInput())
            .Composition(ValidationCases.SingleUse(ValidationCases.ValidInput()))
            .ToList());
        Element[] invalid = [new Point(0, 0), new Point(-1, 1)];
        Assert.Single(invalid.Composition(invalid)
            .Take(1));
    }

    private sealed class ObservedElements(IEnumerable<Element> elements) : IEnumerable<Element>
    {
        public bool Started { get; private set; }
        public bool Disposed { get; private set; }

        public IEnumerator<Element> GetEnumerator()
        {
            Assert.False(Started);
            Started = true;
            try
            {
                foreach (var element in elements)
                    yield return element;
            }
            finally
            {
                Disposed = true;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
