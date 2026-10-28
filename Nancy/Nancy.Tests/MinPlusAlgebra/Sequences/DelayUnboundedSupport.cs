using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class DelayUnboundedSupport
{
    private static Sequence Input() => new([Segment.Constant(Rational.MinusInfinity, 4, 3)]);

    public static IEnumerable<Rational> PositiveDelays => [1, 2];
    public static IEnumerable<object[]> PositiveDelayCases() => PositiveDelays.ToXUnitTestCases();

    public static IEnumerable<bool> PrefixOptions => [false, true];
    public static IEnumerable<object[]> PrefixOptionCases() => PrefixOptions.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PositiveDelayCases))]
    public void SequenceDelayRejectsAZeroPrefixOnLeftUnboundedSupport(Rational delay)
    {
        var sequence = Input();
        var exception = Assert.Throws<ArgumentException>("prependWithZero", () => sequence.Delay(delay));
        Assert.Contains("left-unbounded", exception.Message);
        Assert.Throws<ArgumentException>("prependWithZero", () => sequence.Delay(delay, prependWithZero: true));
    }

    [Theory]
    [MemberData(nameof(PositiveDelayCases))]
    public void StreamDelayRejectsAZeroPrefixOnFirstEnumeration(Rational delay)
    {
        var started = false;
        var disposed = false;
        IEnumerable<Element> Source()
        {
            started = true;
            try
            {
                yield return Segment.Constant(Rational.MinusInfinity, 4, 3);
                throw new InvalidOperationException("The rejection must not read another element.");
            }
            finally
            {
                disposed = true;
            }
        }

        var delayed = Source().Delay(delay);
        Assert.False(started);
        using var enumerator = delayed.GetEnumerator();
        Assert.False(started);
        var exception = Assert.Throws<ArgumentException>("prependWithZero", () => enumerator.MoveNext());
        Assert.Contains("left-unbounded", exception.Message);
        Assert.True(started);
        Assert.True(disposed);
    }

    [Theory]
    [MemberData(nameof(PositiveDelayCases))]
    public void SequenceDelayWithoutPrefixTranslatesLeftUnboundedSupport(Rational delay)
    {
        var result = Input().Delay(delay, prependWithZero: false);
        Assert.Equal(Segment.Constant(Rational.MinusInfinity, 4 + delay, 3), Assert.Single(result.Elements));
    }

    [Theory]
    [MemberData(nameof(PositiveDelayCases))]
    public void StreamDelayWithoutPrefixTranslatesLeftUnboundedSupport(Rational delay)
    {
        var result = Input().Elements.Delay(delay, prependWithZero: false)
            .ToList();
        Assert.Equal(Segment.Constant(Rational.MinusInfinity, 4 + delay, 3), Assert.Single(result));
    }

    [Theory]
    [MemberData(nameof(PrefixOptionCases))]
    public void SequenceZeroDelayPreservesLeftUnboundedInput(bool prependWithZero)
    {
        var sequence = Input();
        Assert.Same(sequence, sequence.Delay(0, prependWithZero));
    }

    [Theory]
    [MemberData(nameof(PrefixOptionCases))]
    public void StreamZeroDelayPreservesLeftUnboundedElements(bool prependWithZero)
    {
        var sequence = Input();
        var result = sequence.Elements.Delay(0, prependWithZero)
            .ToList();
        Assert.Same(Assert.Single(sequence.Elements), Assert.Single(result));
    }

    [Theory]
    [MemberData(nameof(PositiveDelayCases))]
    public void SequencePositiveHorizontalShiftRejectsLeftUnboundedSupport(Rational shift)
    {
        Assert.Throws<ArgumentException>("prependWithZero", () => Input().HorizontalShift(shift));
    }

    [Theory]
    [MemberData(nameof(PositiveDelayCases))]
    public void StreamPositiveHorizontalShiftRejectsLeftUnboundedSupport(Rational shift)
    {
        var shifted = Input().Elements.HorizontalShift(shift);
        using var enumerator = shifted.GetEnumerator();
        Assert.Throws<ArgumentException>("prependWithZero", () => enumerator.MoveNext());
    }

    [Fact]
    public void SequenceZeroHorizontalShiftPreservesLeftUnboundedInput()
    {
        var sequence = Input();
        Assert.Same(sequence, sequence.HorizontalShift(0));
    }

    [Fact]
    public void StreamZeroHorizontalShiftPreservesLeftUnboundedElements()
    {
        var sequence = Input();
        var result = sequence.Elements.HorizontalShift(0)
            .ToList();
        Assert.Same(Assert.Single(sequence.Elements), Assert.Single(result));
    }

    [Theory]
    [MemberData(nameof(PositiveDelayCases))]
    public void SequenceNegativeHorizontalShiftTranslatesLeftUnboundedSupport(Rational advance)
    {
        var result = Input().HorizontalShift(-advance);
        Assert.Equal(Segment.Constant(Rational.MinusInfinity, 4 - advance, 3), Assert.Single(result.Elements));
    }

    [Theory]
    [MemberData(nameof(PositiveDelayCases))]
    public void StreamNegativeHorizontalShiftTranslatesLeftUnboundedSupport(Rational advance)
    {
        var result = Input().Elements.HorizontalShift(-advance)
            .ToList();
        Assert.Equal(Segment.Constant(Rational.MinusInfinity, 4 - advance, 3), Assert.Single(result));
    }
}
