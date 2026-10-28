using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

public class Union
{
    public static IEnumerable<(Sequence Left, Sequence Right)> UnionOperandCases()
    {
        Sequence[] sequences =
        [
            new Sequence([new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2)]),
            new Sequence([new Segment(0, 2, 3, -1)]),
            new Sequence([new Point(2, 5)]),
            new Sequence([new Segment(2, 4, -1, 2)]),
            new Sequence([new Point(5, 1), Segment.Constant(5, 7, 1), new Point(7, 1)]),
            Sequence.PlusInfinite(1, 3), Sequence.MinusInfinite(1, 3)
        ];
        foreach (var a in sequences)
        foreach (var b in sequences)
            yield return (a, b);
    }

    public static IEnumerable<object[]> UnionCases() => UnionOperandCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UnionCases))]
    public void MinimumOverTheUnionMatchesTheSequenceOperator(Sequence a, Sequence b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var expected = Sequence.Minimum(a, b, false, settings);
        var actual = (a.Elements.Minimum(b.Elements, false))
            .MergeAsEnumerable()
            .ToSequence();
        Assert.True(actual.Elements.AreUninterruptedSequence());
        Assert.True(Sequence.Equivalent(expected, actual), $"Expected {expected}, actual {actual}");
        Assert.Equal(expected.Support, actual.Support);
    }

    [Theory]
    [MemberData(nameof(UnionCases))]
    public void MaximumOverTheUnionMatchesTheSequenceOperator(Sequence a, Sequence b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var expected = Sequence.Maximum(a, b, false, settings);
        var actual = (a.Elements.Maximum(b.Elements, false))
            .MergeAsEnumerable()
            .ToSequence();
        Assert.True(actual.Elements.AreUninterruptedSequence());
        Assert.True(Sequence.Equivalent(expected, actual), $"Expected {expected}, actual {actual}");
        Assert.Equal(expected.Support, actual.Support);
    }

    [Fact]
    public void MinimumOfMultipleOperandsIncludesTheirGaps()
    {
        Sequence[] operands = [Sequence.Constant(1, 0, 1), Sequence.Constant(2, 2, 3), Sequence.Constant(-1, 4, 5)];
        var settings = new ComputationSettings { UseParallelism = false };
        var expected = operands.Aggregate((a, b) => Sequence.Minimum(a, b, false, settings));
        var streams = operands.Select(s => s.Elements);
        var actual = (streams.Minimum())
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, actual), $"Expected {expected}, actual {actual}");
    }

    [Fact]
    public void MaximumOfMultipleOperandsIncludesTheirGaps()
    {
        Sequence[] operands = [Sequence.Constant(1, 0, 1), Sequence.Constant(2, 2, 3), Sequence.Constant(-1, 4, 5)];
        var settings = new ComputationSettings { UseParallelism = false };
        var expected = operands.Aggregate((a, b) => Sequence.Maximum(a, b, false, settings));
        var streams = operands.Select(s => s.Elements);
        var actual = (streams.Maximum())
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, actual), $"Expected {expected}, actual {actual}");
    }

    [Fact]
    public void MinimumRejectsAnEmptyOperandCollection()
        => Assert.Throws<ArgumentException>(() => Enumerable.Empty<IEnumerable<Element>>().Minimum()
            .ToList());

    [Fact]
    public void MaximumRejectsAnEmptyOperandCollection()
        => Assert.Throws<ArgumentException>(() => Enumerable.Empty<IEnumerable<Element>>().Maximum()
            .ToList());

    public static IEnumerable<object[]> InvalidInputs() => ValidationCases.InvalidInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void UnionEnvelopesValidateBothOperandsAndSingletonCollections(Element[] elements)
    {
        var other = ValidationCases.ValidInput();
        Assert.Throws<ArgumentException>(() => elements.Minimum(other, false)
            .ToList());
        Assert.Throws<ArgumentException>(() => other.Maximum(elements, false)
            .ToList());
        Assert.Throws<ArgumentException>(() => new[] { ValidationCases.SingleUse(elements) }.Minimum()
            .ToList());
        Assert.Throws<ArgumentException>(() => new[] { ValidationCases.SingleUse(elements) }.Maximum()
            .ToList());
    }

    [Fact]
    public void UnionSweepsConsumeEachOperandOnceAndLeaveUnreadTailsAlone()
    {
        Assert.NotEmpty(ValidationCases.SingleUse(ValidationCases.ValidInput())
            .Minimum(ValidationCases.SingleUse(ValidationCases.ValidInput()), false)
            .ToList());
        Element[] elements = [new Point(0, 0), new Point(-1, 1)];
        Assert.Single(elements.Minimum(ValidationCases.ValidInput(), false)
            .Take(1));
        Assert.Single(new[] { ValidationCases.SingleUse(elements) }.Maximum()
            .Take(1));
    }
}
