using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

/// <summary>
/// Checks streamed addition and subtraction against interval-bucket references and verifies incremental consumption.
/// </summary>
public class Addition
{
    private static Sequence S(params Element[] elements) => new(elements);

    private static readonly Sequence Ramp0To4 = S(new Point(0, 0), new Segment(0, 4, 0, 1), new Point(4, 4));
    private static readonly Sequence Steps0To6 = S(
        new Point(0, 1), Segment.Constant(0, 2, 1), new Point(2, 3), Segment.Constant(2, 5, 3), new Point(5, 3), new Segment(5, 6, 4, 2), new Point(6, 6));
    private static readonly Sequence Ramp2To9 = S(new Point(2, 1), new Segment(2, 9, 2, 1), new Point(9, 11));
    private static readonly Sequence OpenFrom1To3 = S(new Segment(1, 3, 5, 2), new Point(3, 12));
    private static readonly Sequence OpenBothEnds0To2 = S(new Point(0, 0), new Segment(0, 2, 0, 3));
    private static readonly Sequence At4Alone = S(new Point(4, 7));
    private static readonly Sequence PlusInfinityFrom2 = S(new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2), Segment.PlusInfinite(2, 5), Point.PlusInfinite(5));
    private static readonly Sequence MinusInfinityFrom1 = S(new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), Segment.MinusInfinite(1, 3), Point.MinusInfinite(3));

    /// <summary>
    /// Sequences taken from curves, whose breakpoints do not coincide.
    /// </summary>
    private static readonly Sequence StairCut = new StairCurve(2, 3).Cut(0, 10, true, true);
    private static readonly Sequence RateLatencyCut = new RateLatencyServiceCurve(new Rational(3, 2), new Rational(7, 2)).Cut(0, 10, true, true);

    public static List<(string Name, Sequence Left, Sequence Right)> OperandCases() =>
    [
        ("same support, breakpoints that do not coincide", Ramp0To4, Steps0To6.Cut(0, 4, true, true)),
        ("the second starts later", Steps0To6, Ramp2To9),
        ("the first starts later", Ramp2To9, Steps0To6),
        ("an operand open at its start", Steps0To6, OpenFrom1To3),
        ("an operand open at its end", Ramp0To4, OpenBothEnds0To2),
        ("supports sharing a single time", Ramp0To4, At4Alone),
        ("cuts of a staircase and a rate-latency curve", StairCut, RateLatencyCut),
        ("the same sequence", Steps0To6, Steps0To6),
        ("+inf and a finite sequence", PlusInfinityFrom2, Ramp0To4),
        ("a finite sequence and -inf", Ramp0To4, MinusInfinityFrom1),
        ("+inf and a finite sequence, the first operand shorter", PlusInfinityFrom2, StairCut),
    ];

    public static IEnumerable<(string Name, Sequence Left, Sequence Right)> FiniteOperandCases() =>
        OperandCases().Where(pair => pair.Left.IsFinite && pair.Right.IsFinite);

    public static IEnumerable<(string Name, Sequence Left, Sequence Right)> InfiniteOperandCases() =>
        OperandCases().Where(pair => !pair.Left.IsFinite || !pair.Right.IsFinite);

    public static IEnumerable<object[]> FiniteOperands() => FiniteOperandCases().ToXUnitTestCases();

    public static IEnumerable<object[]> InfiniteOperands() => InfiniteOperandCases().ToXUnitTestCases();

    private static void AssertMatchesReference(Sequence expected, IEnumerable<Element> stream)
    {
        var elements = stream.ToList();
        Assert.True(elements.AreUninterruptedSequence(), "The stream has gaps or overlaps.");
        var actual = elements.MergeAsEnumerable()
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, actual), $"Expected {expected}, got {actual}");
    }

    [Theory]
    [MemberData(nameof(FiniteOperands))]
    [MemberData(nameof(InfiniteOperands))]
    public void AdditionMatchesTheBucketReference(string name, Sequence a, Sequence b)
    {
        Assert.False(string.IsNullOrEmpty(name));
        AssertMatchesReference(MaterializedPointwiseReference.Addition(a, b), a.Elements.Addition(b.Elements));
        AssertMatchesReference(MaterializedPointwiseReference.Addition(b, a), b.Elements.Addition(a.Elements));
    }

    [Theory]
    [MemberData(nameof(FiniteOperands))]
    [MemberData(nameof(InfiniteOperands))]
    public void SubtractionMatchesTheBucketReference(string name, Sequence a, Sequence b)
    {
        Assert.False(string.IsNullOrEmpty(name));
        AssertMatchesReference(MaterializedPointwiseReference.Subtraction(a, b), a.Elements.Subtraction(b.Elements));
        AssertMatchesReference(MaterializedPointwiseReference.Subtraction(b, a), b.Elements.Subtraction(a.Elements));
    }

    [Fact]
    public void TheResultHasAPointAtEveryBreakpointOfEitherOperand()
    {
        var times = Ramp0To4.Elements.Addition(Steps0To6.Cut(0, 4, true, true).Elements)
            .OfType<Point>()
            .Select(p => p.Time)
            .ToList();
        Assert.Equal<Rational>([0, 2, 4], times);
    }

    [Fact]
    public void TheResultEndsWhereTheShorterOperandDoes()
    {
        var sum = Ramp0To4.Elements.Addition(Steps0To6.Elements)
            .ToList();
        Assert.Equal(4, sum.Last().EndTime);
    }

    [Fact]
    public void SupportsSharingASingleTimeGiveThePointThere()
    {
        var sum = Ramp0To4.Elements.Addition(At4Alone.Elements)
            .ToList();
        Assert.Equal<Element>([new Point(4, 11)], sum);
    }

    [Fact]
    public void SupportsWithNoTimeInCommonAreRejected()
    {
        var left = S(new Point(0, 0), new Segment(0, 1, 0, 1));
        var rightOpen = S(new Segment(1, 2, 6, 1), new Point(2, 7));
        var rightClosed = S(new Point(1, 6), new Segment(1, 2, 6, 1), new Point(2, 7));
        var farRight = S(new Point(3, 0), new Segment(3, 4, 0, 1));

        Assert.Throws<ArgumentException>(() => left.Elements.Addition(rightOpen.Elements)
            .ToList());
        Assert.Throws<ArgumentException>(() => rightOpen.Elements.Addition(left.Elements)
            .ToList());
        Assert.Throws<ArgumentException>(() => left.Elements.Subtraction(rightClosed.Elements)
            .ToList());
        Assert.Throws<ArgumentException>(() => left.Elements.Addition(farRight.Elements)
            .ToList());
        Assert.Throws<ArgumentException>(() => farRight.Elements.Subtraction(left.Elements)
            .ToList());
    }

    [Fact]
    public void AnEmptyOperandIsRejected()
    {
        Assert.Throws<ArgumentException>(() => Ramp0To4.Elements.Addition([])
            .ToList());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().Subtraction(Ramp0To4.Elements)
            .ToList());
    }

    [Fact]
    public void OppositeInfinitiesAreUndeterminedInTheSum()
    {
        Assert.Throws<UndeterminedResultException>(() => PlusInfinityFrom2.Elements.Addition(MinusInfinityFrom1.Elements)
            .ToList());
        Assert.Throws<UndeterminedResultException>(() => Sequence.Addition(PlusInfinityFrom2, MinusInfinityFrom1));
    }

    [Fact]
    public void TheSameInfinityIsUndeterminedInTheDifference()
    {
        Assert.Throws<UndeterminedResultException>(() => PlusInfinityFrom2.Elements.Subtraction(PlusInfinityFrom2.Elements)
            .ToList());
        Assert.Throws<UndeterminedResultException>(() => Sequence.Subtraction(PlusInfinityFrom2, PlusInfinityFrom2));
    }

    private static IEnumerable<Element> CountingPulls(IEnumerable<Element> elements, Action onPull)
    {
        foreach (var element in elements)
        {
            onPull();
            yield return element;
        }
    }

    [Fact]
    public void TheOperandsAreReadOnlyAsTheResultIsConsumed()
    {
        var pulledA = 0;
        var pulledB = 0;
        var a = CountingPulls(new StairCurve(2, 3).CutAsEnumerable(0, 1_000_000, true, true), () => pulledA++);
        var b = CountingPulls(new RateLatencyServiceCurve(1, 2).CutAsEnumerable(0, 1_000_000, true, true), () => pulledB++);

        var firstFive = a.Addition(b)
            .Take(5)
            .ToList();

        Assert.Equal(5, firstFive.Count);
        Assert.InRange(pulledA, 1, 10);
        Assert.InRange(pulledB, 1, 10);
    }

    [Fact]
    public void TheFirstElementOverOperandsWithAHugeCommonPeriodArrivesAtOnce()
    {
        // pseudo-inverses of periods 7 and 40,000,003, whose least common multiple, 280,000,021, has tens of millions of breakpoints
        var f = new StairCurve(7, 1);
        var g = new StairCurve(40_000_003, 1);
        Rational end = 280_000_021;

        var firstThree = g.LowerPseudoInverse()
            .CutAsEnumerable(0, end, true, true)
            .Subtraction(f.LowerPseudoInverse()
                .CutAsEnumerable(0, end, true, true))
            .Take(3)
            .ToList();

        Assert.Equal(3, firstThree.Count);
    }

    public static IEnumerable<object[]> InvalidInputs() => ValidationCases.InvalidInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void AdditionRejectsMalformedOperands(Element[] elements)
    {
        Element[] other = [new Point(0, 1), Segment.Constant(0, 10, 1), new Point(10, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Addition(other)
            .ToList());
        Assert.Throws<ArgumentException>(() => other.Addition(ValidationCases.SingleUse(elements))
            .ToList());
    }

    [Fact]
    public void AdditionEnumeratesEachOperandOnce()
    {
        Assert.NotEmpty(ValidationCases.SingleUse(ValidationCases.ValidInput())
            .Addition(ValidationCases.SingleUse(ValidationCases.ValidInput()))
            .ToList());
    }

    [Fact]
    public void AdditionLeavesAnInvalidUnreadTailAlone()
    {
        Element[] elements = [new Point(0, 0), new Point(-1, 1)];
        Assert.Single(ValidationCases.SingleUse(elements).Addition(ValidationCases.ValidInput())
            .Take(1));
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void SubtractionRejectsMalformedOperands(Element[] elements)
    {
        Element[] other = [new Point(0, 1), Segment.Constant(0, 10, 1), new Point(10, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Subtraction(other)
            .ToList());
        Assert.Throws<ArgumentException>(() => other.Subtraction(ValidationCases.SingleUse(elements))
            .ToList());
    }

    [Fact]
    public void SubtractionEnumeratesEachOperandOnce()
    {
        Assert.NotEmpty(ValidationCases.SingleUse(ValidationCases.ValidInput())
            .Subtraction(ValidationCases.SingleUse(ValidationCases.ValidInput()))
            .ToList());
    }

    [Fact]
    public void SubtractionLeavesAnInvalidUnreadTailAlone()
    {
        Element[] elements = [new Point(0, 0), new Point(-1, 1)];
        Assert.Single(ValidationCases.SingleUse(elements).Subtraction(ValidationCases.ValidInput())
            .Take(1));
    }
}
