using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class ScalarAndShapeProperties
{
    public record Case(Sequence Sequence, bool NonPositive, bool Positive, bool Negative, bool Constant, bool Affine);

    public static List<Case> Cases =>
    [
        new(new Sequence([new Point(0, 0)]), true, false, false, true, true),
        new(new Sequence([new Point(0, 2)]), false, true, false, true, true),
        new(new Sequence([new Point(0, -2)]), true, false, true, true, true),
        new(new Sequence([Segment.Zero(0, 1)]), true, false, false, true, true),
        new(new Sequence([new Segment(0, 1, 0, 1)]), false, true, false, false, true),
        new(new Sequence([new Segment(0, 1, -1, 1)]), true, false, true, false, true),
        new(new Sequence([new Segment(0, 1, 1, -1)]), false, true, false, false, true),
        new(new Sequence([new Segment(0, 1, 0, -1)]), true, false, true, false, true),
        new(new Sequence([new Segment(0, 2, -1, 1)]), false, false, false, false, true),
        new(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), false, false, false, false, true),
        new(new Sequence([new Segment(0, 1, -1, 1), Point.Zero(1)]), true, false, false, false, true),
        new(new Sequence([Segment.PlusInfinite(0, 1)]), false, true, false, true, false),
        new(new Sequence([Segment.MinusInfinite(0, 1)]), true, false, true, true, false),
        new(new Sequence([new Point(0, 1), Segment.Constant(0, 1, 2)]), false, true, false, false, false),
        new(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 2)]), false, false, false, false, false),
        new(new Sequence([new Segment(0, Rational.PlusInfinity, 0, 1)]), false, true, false, false, true)
    ];

    public static IEnumerable<object[]> PropertyCases => Cases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PropertyCases))]
    public void IsNonPositiveMatchesDefinedValues(Case item)
    {
        Assert.Equal(item.NonPositive, item.Sequence.IsNonPositive);
        Assert.Equal(item.NonPositive, Once(item.Sequence.Elements).IsNonPositive());
    }

    [Fact]
    public void IsNonPositiveRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonPositive());

    [Theory]
    [MemberData(nameof(PropertyCases))]
    public void IsPositiveMatchesDefinedValues(Case item)
    {
        Assert.Equal(item.Positive, item.Sequence.IsPositive);
        Assert.Equal(item.Positive, Once(item.Sequence.Elements).IsPositive());
    }

    [Fact]
    public void IsPositiveRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsPositive());

    [Theory]
    [MemberData(nameof(PropertyCases))]
    public void IsNegativeMatchesDefinedValues(Case item)
    {
        Assert.Equal(item.Negative, item.Sequence.IsNegative);
        Assert.Equal(item.Negative, Once(item.Sequence.Elements).IsNegative());
    }

    [Fact]
    public void IsNegativeRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNegative());

    [Theory]
    [MemberData(nameof(PropertyCases))]
    public void IsConstantMatchesDefinedValues(Case item)
    {
        Assert.Equal(item.Constant, item.Sequence.IsConstant);
        Assert.Equal(item.Constant, Once(item.Sequence.Elements).IsConstant());
    }

    [Fact]
    public void IsConstantRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsConstant());

    [Theory]
    [MemberData(nameof(PropertyCases))]
    public void IsAffineMatchesDefinedValues(Case item)
    {
        Assert.Equal(item.Affine, item.Sequence.IsAffine);
        Assert.Equal(item.Affine, Once(item.Sequence.Elements).IsAffine());
    }

    [Fact]
    public void IsAffineRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsAffine());

    [Fact]
    public void IsNonPositiveStopsAtADecisiveElement()
    {
        Assert.False(DecisiveThenThrow(new Point(0, 1)).IsNonPositive());
    }

    [Fact]
    public void IsPositiveStopsAtADecisiveElement()
    {
        Assert.False(DecisiveThenThrow(Point.Origin()).IsPositive());
    }

    [Fact]
    public void IsNegativeStopsAtADecisiveElement()
    {
        Assert.False(DecisiveThenThrow(Point.Origin()).IsNegative());
    }

    [Fact]
    public void IsConstantStopsAtADecisiveElement()
    {
        Assert.False(DecisiveThenThrow(new Segment(0, 1, 0, 1)).IsConstant());
    }

    public static IEnumerable<Element[]> LeftUnboundedConstants =>
    [
        [Segment.Constant(Rational.MinusInfinity, Rational.PlusInfinity, 7)],
        [Segment.Constant(Rational.MinusInfinity, 0, 7), new Point(0, 7), Segment.Constant(0, 2, 7)]
    ];

    public static IEnumerable<object[]> LeftUnboundedCases => LeftUnboundedConstants.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(LeftUnboundedCases))]
    public void AffinityAcceptsLeftUnboundedConstants(Element[] elements)
    {
        Assert.True(Once(elements).IsAffine());
        Assert.True(new Sequence(elements).IsAffine);
    }

    [Fact]
    public void AffinityValidatesConsumedTimeOrder()
    {
        Element[] elements = [Point.Origin(), new Segment(2, 3, 0, 0)];
        Assert.Throws<ArgumentException>(() => elements.IsAffine());
    }

    [Fact]
    public void AffinityDoesNotReadPastAnInfiniteElement()
        => Assert.False(DecisiveThenThrow(Point.PlusInfinite(0)).IsAffine());

    [Fact]
    public void ScalarPredicatesDistinguishMixedAndUniformInfinities()
    {
        Element[] mixed = [Point.PlusInfinite(0), Segment.MinusInfinite(0, 1)];
        Assert.True(Once(mixed).IsInfinite());
        Assert.False(Once(mixed).IsConstant());
        Assert.True(Once(mixed).HasPlusInfinity());
        Assert.True(Once(mixed).HasMinusInfinity());
        Assert.False(Once(mixed).IsZero());
        Assert.False(Once(mixed.Append(new Point(1, 0))).IsInfinite());
    }

    [Fact]
    public void ScalarPredicatesRejectEmptyInput()
    {
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsZero());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsInfinite());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().HasPlusInfinity());
        Assert.Throws<ArgumentException>(() => Array.Empty<Element>().HasMinusInfinity());
    }

    [Fact]
    public void ZeroStopsAtANonzeroElement()
        => Assert.False(DecisiveThenThrow(new Point(0, 1)).IsZero());

    [Fact]
    public void InfinityStopsAtAFiniteElement()
        => Assert.False(DecisiveThenThrow(Point.Origin()).IsInfinite());

    [Fact]
    public void PlusInfinityStopsAtAWitness()
        => Assert.True(DecisiveThenThrow(Point.PlusInfinite(0)).HasPlusInfinity());

    [Fact]
    public void MinusInfinityStopsAtAWitness()
        => Assert.True(DecisiveThenThrow(Point.MinusInfinite(0)).HasMinusInfinity());

    [Fact]
    public void SupremumPairUsesOneTraversalAndTheEarliestTie()
    {
        Element[] elements = [new Point(1, 7), Segment.Constant(1, 3, -2), new Point(3, 7)];
        Assert.Equal(((Rational)7, (Rational)1), Once(elements).SupValueAndArg());
    }

    [Fact]
    public void InfimumPairUsesOneTraversalAndTheEarliestTie()
    {
        Element[] elements = [new Point(1, -2), Segment.Constant(1, 3, 7), new Point(3, -2)];
        Assert.Equal(((Rational)(-2), (Rational)1), Once(elements).InfValueAndArg());
    }

    [Fact]
    public void SequenceExtremaDistinguishLimitsFromAttainment()
    {
        var sequence = new Sequence([new Segment(2, 4, 1, 1)]);
        Assert.Equal(((Rational)3, (Rational)4), sequence.SupValueAndArg());
        Assert.Equal(((Rational)1, (Rational)2), sequence.InfValueAndArg());
        Assert.Equal(4, sequence.SupArg());
        Assert.Equal(2, sequence.InfArg());
        Assert.Null(sequence.MaxArg());
        Assert.Null(sequence.MinArg());
    }

    [Fact]
    public void InfiniteBoundsRetainTheirBoundaryLocation()
    {
        var sequence = new Sequence([Segment.PlusInfinite(2, 4)]);
        Assert.Equal((Rational.PlusInfinity, (Rational)2), sequence.SupValueAndArg());
        Assert.Equal((Rational.PlusInfinity, (Rational)2), sequence.InfValueAndArg());
        Assert.InRange(sequence.MaxArg()!.Value, (Rational) 2, (Rational) 4);
        Assert.InRange(sequence.MinArg()!.Value, (Rational) 2, (Rational) 4);
    }

    [Fact]
    public void SupremumPairRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().SupValueAndArg());

    [Fact]
    public void InfimumPairRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().InfValueAndArg());

    private static IEnumerable<Element> DecisiveThenThrow(Element first)
    {
        yield return first;
        throw new InvalidOperationException("The decisive prefix should stop traversal.");
    }

    private static IEnumerable<Element> Once(IEnumerable<Element> elements) => new SingleUse(elements);

    private sealed class SingleUse(IEnumerable<Element> elements) : IEnumerable<Element>
    {
        private bool used;
        public IEnumerator<Element> GetEnumerator()
        {
            if (used) throw new InvalidOperationException("The source was enumerated twice.");
            used = true;
            return elements.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
