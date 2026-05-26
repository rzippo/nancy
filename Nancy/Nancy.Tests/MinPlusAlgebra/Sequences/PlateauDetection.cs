using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class PlateauDetection
{
    public static IEnumerable<object[]> SinglePointSequences()
    {
        yield return new object[] { new Sequence([new Point(0, 0)]) };
        yield return new object[] { new Sequence([new Point(5, 3)]) };
        yield return new object[] { new Sequence([new Point(10, new Rational(7, 2))]) };
    }

    [Theory]
    [MemberData(nameof(SinglePointSequences))]
    public void SinglePoint_DoesNotStartWithPlateau(Sequence sequence)
    {
        Assert.False(sequence.StartsWithPlateau);
    }

    [Theory]
    [MemberData(nameof(SinglePointSequences))]
    public void SinglePoint_DoesNotEndWithPlateau(Sequence sequence)
    {
        Assert.False(sequence.EndsWithPlateau);
    }

    public static IEnumerable<object[]> StartsWithPlateauCases()
    {
        yield return new object[]
        {
            new Sequence([
                Point.Origin(),
                Segment.Zero(0, 5),
                new Point(5, 0),
                new Segment(5, 10, 0, 2)
            ]),
            true
        };
        yield return new object[]
        {
            new Sequence([
                Point.Origin(),
                new Segment(0, 5, 0, 2),
                new Point(5, 10),
                new Segment(5, 10, 10, 0)
            ]),
            false
        };
        yield return new object[]
        {
            new Sequence([
                new Point(0, 3),
                Segment.Constant(0, 5, 3),
                new Point(5, 3),
                new Segment(5, 10, 3, 1)
            ]),
            true
        };
        yield return new object[]
        {
            new Sequence([
                new Segment(0, 5, 3, 0),
                new Point(5, 3)
            ]),
            true
        };
        yield return new object[]
        {
            new Sequence([
                new Segment(0, 5, 3, 2),
                new Point(5, 13)
            ]),
            false
        };
        yield return new object[]
        {
            new Sequence([new Segment(0, 5, 3, 0)]),
            true
        };
        yield return new object[]
        {
            new Sequence([new Segment(0, 5, 3, 2)]),
            false
        };
    }

    public static IEnumerable<object[]> StartsWithPlateauTestCases()
        => StartsWithPlateauCases();

    [Theory]
    [MemberData(nameof(StartsWithPlateauTestCases))]
    public void StartsWithPlateau_Detection(Sequence sequence, bool expected)
    {
        Assert.Equal(expected, sequence.StartsWithPlateau);
    }

    public static IEnumerable<object[]> EndsWithPlateauCases()
    {
        yield return new object[]
        {
            new Sequence([
                Point.Origin(),
                new Segment(0, 5, 0, 2),
                new Point(5, 10),
                new Segment(5, 10, 10, 0),
                new Point(10, 10)
            ]),
            true
        };
        yield return new object[]
        {
            new Sequence([
                Point.Origin(),
                new Segment(0, 5, 0, 2),
                new Point(5, 10)
            ]),
            false
        };
        yield return new object[]
        {
            new Sequence([
                new Point(3, 3),
                new Segment(3, 8, 3, 0),
                new Point(8, 3)
            ]),
            true
        };
        yield return new object[]
        {
            new Sequence([
                new Point(3, 3),
                new Segment(3, 8, 3, 2),
                new Point(8, 13)
            ]),
            false
        };
        yield return new object[]
        {
            new Sequence([
                new Point(3, 3),
                new Segment(3, 8, 3, 0)
            ]),
            true
        };
        yield return new object[]
        {
            new Sequence([new Segment(0, 5, 3, 0)]),
            true
        };
        yield return new object[]
        {
            new Sequence([new Segment(0, 5, 3, 2)]),
            false
        };
    }

    public static IEnumerable<object[]> EndsWithPlateauTestCases()
        => EndsWithPlateauCases();

    [Theory]
    [MemberData(nameof(EndsWithPlateauTestCases))]
    public void EndsWithPlateau_Detection(Sequence sequence, bool expected)
    {
        Assert.Equal(expected, sequence.EndsWithPlateau);
    }

    [Fact]
    public void FirstPlateauEnd_OnSinglePoint()
    {
        var seq = new Sequence([new Point(3, 5)]);
        Assert.Equal(seq.DefinedFrom, seq.FirstPlateauEnd);
    }

    [Fact]
    public void LastPlateauStart_OnSinglePoint()
    {
        var seq = new Sequence([new Point(3, 5)]);
        Assert.Equal(seq.DefinedUntil, seq.LastPlateauStart);
    }

    public static IEnumerable<object[]> FirstPlateauEndCases()
    {
        yield return new object[]
        {
            new Sequence([
                Point.Origin(),
                Segment.Zero(0, 5),
                new Point(5, 0),
                new Segment(5, 10, 0, 2),
                new Point(10, 10)
            ]),
            new Rational(5)
        };
        yield return new object[]
        {
            new Sequence([
                Point.Origin(),
                new Segment(0, 5, 0, 2),
                new Point(5, 10),
                new Segment(5, 10, 10, 0),
                new Point(10, 10)
            ]),
            new Rational(0)
        };
    }

    public static IEnumerable<object[]> FirstPlateauEndTestCases()
        => FirstPlateauEndCases();

    [Theory]
    [MemberData(nameof(FirstPlateauEndTestCases))]
    public void FirstPlateauEnd_Detection(Sequence sequence, Rational expected)
    {
        Assert.Equal(expected, sequence.FirstPlateauEnd);
    }

    public static IEnumerable<object[]> LastPlateauStartCases()
    {
        yield return new object[]
        {
            new Sequence([
                Point.Origin(),
                new Segment(0, 5, 0, 2),
                new Point(5, 10),
                new Segment(5, 10, 10, 0),
                new Point(10, 10)
            ]),
            new Rational(5)
        };
        yield return new object[]
        {
            new Sequence([
                Point.Origin(),
                new Segment(0, 5, 0, 2),
                new Point(5, 10)
            ]),
            new Rational(5)
        };
    }

    public static IEnumerable<object[]> LastPlateauStartTestCases()
        => LastPlateauStartCases();

    [Theory]
    [MemberData(nameof(LastPlateauStartTestCases))]
    public void LastPlateauStart_Detection(Sequence sequence, Rational expected)
    {
        Assert.Equal(expected, sequence.LastPlateauStart);
    }
}
