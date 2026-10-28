using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

public class Translations
{
    public static IEnumerable<(Sequence Input, Rational Advance)> Advances()
    {
        foreach (var input in new[]
        {
            Sequence.Constant(3, -4, -2, false, false),
            Sequence.Constant(3, -1, 2, true, true),
            new Sequence([new Point(0, 7)]),
            new Sequence([new Segment(1, 3, 2, 1)])
        })
        foreach (var advance in new Rational[] { 0, 1, 5 })
            yield return (input, advance);
    }

    public static IEnumerable<object[]> Cases() => Advances().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void ForwardTranslatesEveryElementAndKeepsEndpointInclusion(Sequence input, Rational advance)
    {
        var expected = input.Elements.Select(e => e.Forward(advance))
            .ToArray();
        Assert.Equal(expected, input.Elements.Forward(advance)
            .ToArray());
        var result = input.Forward(advance);
        Assert.Equal(expected, result.Elements);
        Assert.Equal(input.IsLeftClosed, result.IsLeftClosed);
        Assert.Equal(input.IsRightClosed, result.IsRightClosed);
        Assert.Equal(input.DefinedFrom - advance, result.DefinedFrom);
        Assert.Equal(input.DefinedUntil - advance, result.DefinedUntil);
    }

    [Fact]
    public void PositiveDelaySuppliesZeroAtAnOpenJoin()
    {
        Element[] input = [new Segment(-2, 1, 3, 1)];
        Element[] expected = [Segment.Zero(-2, 0), Point.Zero(0), new Segment(0, 3, 3, 1)];
        Assert.Equal(expected, input.Delay(2)
            .ToArray());
        Assert.Equal(expected, new Sequence(input).Delay(2).Elements);
    }

    [Fact]
    public void PositiveDelayPreservesTheValueAtAClosedJoin()
    {
        Element[] input = [new Point(-2, 9), new Segment(-2, 1, 3, 1)];
        Element[] expected = [Point.Zero(-2), Segment.Zero(-2, 0), new Point(0, 9), new Segment(0, 3, 3, 1)];
        Assert.Equal(expected, input.Delay(2)
            .ToArray());
    }

    [Fact]
    public void ZeroDelayYieldsOriginalElementsAndValidatesReachedTransitions()
    {
        Element[] input = [new Point(0, 7), Segment.Zero(0, 1), new Point(0, 2)];
        using var result = ValidationCases.SingleUse(input).Delay(0).GetEnumerator();
        Assert.True(result.MoveNext());
        Assert.Same(input[0], result.Current);
        Assert.True(result.MoveNext());
        Assert.Same(input[1], result.Current);
        Assert.Throws<ArgumentException>(() => result.MoveNext());
    }

    [Fact]
    public void ZeroAdvanceYieldsOriginalElementsAndValidatesReachedTransitions()
    {
        Element[] input = [new Point(0, 7), Segment.Zero(0, 1), new Point(0, 2)];
        using var result = ValidationCases.SingleUse(input).Forward(0).GetEnumerator();
        Assert.True(result.MoveNext());
        Assert.Same(input[0], result.Current);
        Assert.True(result.MoveNext());
        Assert.Same(input[1], result.Current);
        Assert.Throws<ArgumentException>(() => result.MoveNext());
    }

    [Fact]
    public void ZeroVerticalShiftYieldsOriginalElementsAndValidatesReachedTransitions()
    {
        Element[] input = [new Point(0, 7), Segment.Zero(0, 1), new Point(0, 2)];
        using var result = ValidationCases.SingleUse(input).VerticalShift(0).GetEnumerator();
        Assert.True(result.MoveNext());
        Assert.Same(input[0], result.Current);
        Assert.True(result.MoveNext());
        Assert.Same(input[1], result.Current);
        Assert.Throws<ArgumentException>(() => result.MoveNext());
    }
}
