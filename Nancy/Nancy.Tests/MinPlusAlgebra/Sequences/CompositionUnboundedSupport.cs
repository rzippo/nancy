using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class CompositionUnboundedSupport
{
    public static IReadOnlyList<(Sequence Outer, Sequence Inner, Sequence Expected)> KnownCompositions { get; } =
    [
        (
            new Sequence([new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)]),
            new Sequence([new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)]),
            new Sequence([new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)])
        ),
        (
            new Sequence([new Segment(0, Rational.PlusInfinity, 5, 2)]),
            new Sequence([new Point(1, 2), new Segment(1, Rational.PlusInfinity, 2, 3)]),
            new Sequence([new Point(1, 9), new Segment(1, Rational.PlusInfinity, 9, 6)])
        ),
        (
            new Sequence([
                new Point(0, 100), new Segment(0, 2, 1, 3),
                new Point(2, 7), new Segment(2, Rational.PlusInfinity, 9, -1)
            ]),
            new Sequence([new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 2)]),
            new Sequence([
                new Point(0, 100), new Segment(0, 1, 1, 6),
                new Point(1, 7), new Segment(1, Rational.PlusInfinity, 9, -2)
            ])
        ),
        (
            new Sequence([
                new Point(0, 100), new Segment(0, 2, 1, 3),
                new Point(2, 7), new Segment(2, Rational.PlusInfinity, 9, -1)
            ]),
            new Sequence([
                new Point(0, 0), new Segment(0, 1, 0, 1),
                new Point(1, 3), new Segment(1, Rational.PlusInfinity, 4, 2)
            ]),
            new Sequence([
                new Point(0, 100), new Segment(0, 1, 1, 3),
                new Point(1, 8), new Segment(1, Rational.PlusInfinity, 7, -2)
            ])
        ),
        (
            new Sequence([
                new Point(0, 0), new Segment(0, 1, 0, 1),
                new Point(1, 7), Segment.PlusInfinite(1, Rational.PlusInfinity)
            ]),
            new Sequence([new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)]),
            new Sequence([
                new Point(0, 0), new Segment(0, 1, 0, 1),
                new Point(1, 7), Segment.PlusInfinite(1, Rational.PlusInfinity)
            ])
        ),
        (
            new Sequence([
                new Point(0, 0), new Segment(0, 1, 0, 1),
                new Point(1, 7), Segment.MinusInfinite(1, Rational.PlusInfinity)
            ]),
            new Sequence([new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)]),
            new Sequence([
                new Point(0, 0), new Segment(0, 1, 0, 1),
                new Point(1, 7), Segment.MinusInfinite(1, Rational.PlusInfinity)
            ])
        ),
        (
            new Sequence([new Point(1, 7), new Segment(1, Rational.PlusInfinity, 9, 2)]),
            new Sequence([new Point(0, 1), Segment.Constant(0, Rational.PlusInfinity, 1)]),
            new Sequence([new Point(0, 7), Segment.Constant(0, Rational.PlusInfinity, 7)])
        ),
        (
            new Sequence([new Segment(0, Rational.PlusInfinity, 3, -2)]),
            new Sequence([new Segment(0, Rational.PlusInfinity, 0, 2)]),
            new Sequence([new Segment(0, Rational.PlusInfinity, 3, -4)])
        ),
        (
            new Sequence([new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)]),
            new Sequence([new Point(-2, 0), new Segment(-2, Rational.PlusInfinity, 0, 1)]),
            new Sequence([new Point(-2, 0), new Segment(-2, Rational.PlusInfinity, 0, 1)])
        )
    ];

    public static IEnumerable<object[]> Cases() => KnownCompositions.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void StreamCompositionPreservesUnboundedSupports(Sequence outer, Sequence inner, Sequence expected)
    {
        Assert.True(inner.IsFinite);
        Assert.True(inner.IsNonNegative);
        Assert.True(inner.IsNonDecreasing);
        var actual = outer.Elements.Composition(inner.Elements)
            .MergeAsEnumerable()
            .ToSequence();
        Assert.Equal(expected.Elements, actual.Elements);
    }

    [Fact]
    public void AnUnboundedInnerSegmentProducesAFiniteRequestedPrefix()
    {
        var pulls = 0;
        IEnumerable<Element> Outer()
        {
            for (var time = 0; time < 10; time++)
            {
                pulls++;
                yield return new Point(time, time);
                pulls++;
                yield return new Segment(time, time + 1, time, 1);
            }
            throw new InvalidOperationException("The stream was read past the prefix the operation needs.");
        }

        Element[] inner = [new Point(0, 0), new Segment(0, Rational.PlusInfinity, 0, 1)];
        Element[] expected = [new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 1)];
        var actual = Outer().Composition(inner)
            .Take(expected.Length)
            .ToArray();

        Assert.Equal(expected, actual);
        Assert.Equal(expected.Length, pulls);
    }
}
