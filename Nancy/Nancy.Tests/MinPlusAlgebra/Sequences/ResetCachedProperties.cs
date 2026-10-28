using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class ResetCachedProperties
{
    public static List<Sequence> Sequences =
    [
        new([Point.Origin(), new Segment(0, 2, 0, 1)]),
        new([Point.Origin(), new Segment(0, 2, 1, -1), new Point(2, 5)]),
        new([new Segment(0, 1, 0, 0), new Point(1, 3), Segment.PlusInfinite(1, 2)])
    ];

    public static IEnumerable<object[]> SequenceCases()
        => Sequences.ToXUnitTestCases();

    private static bool[] ReadProperties(Sequence value) =>
    [
        value.IsFinite,
        value.IsInfinite,
        value.IsPlusInfinite,
        value.IsMinusInfinite,
        value.HasPlusInfinity,
        value.HasMinusInfinity,
        value.IsZero,
        value.IsNormalized,
        value.IsContinuous,
        value.IsLeftContinuous,
        value.IsRightContinuous,
        value.IsNonNegative,
        value.IsNonPositive,
        value.IsPositive,
        value.IsNegative,
        value.IsConstant,
        value.IsAffine,
        value.IsNonDecreasing,
        value.IsIncreasing,
        value.IsNonIncreasing,
        value.IsDecreasing,
        value.IsLeftOpen,
        value.IsLeftClosed,
        value.IsRightOpen,
        value.IsRightClosed,
        value.IsPoint,
        value.StartsWithPlateau,
        value.EndsWithPlateau
    ];

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void EveryCachedPropertyIsForgotten(Sequence sequence)
    {
        sequence = new Sequence(sequence.Elements);
        ReadProperties(sequence);
        sequence.ResetCachedProperties();

        Assert.Null(sequence._isFinite);
        Assert.Null(sequence._isInfinite);
        Assert.Null(sequence._isPlusInfinite);
        Assert.Null(sequence._isMinusInfinite);
        Assert.Null(sequence._hasPlusInfinity);
        Assert.Null(sequence._hasMinusInfinity);
        Assert.Null(sequence._isZero);
        Assert.Null(sequence._isNormalized);
        Assert.Null(sequence._isContinuous);
        Assert.Null(sequence._isLeftContinuous);
        Assert.Null(sequence._isRightContinuous);
        Assert.Null(sequence._isNonNegative);
        Assert.Null(sequence._isNonPositive);
        Assert.Null(sequence._isPositive);
        Assert.Null(sequence._isNegative);
        Assert.Null(sequence._isConstant);
        Assert.Null(sequence._isAffine);
        Assert.Null(sequence._isNonDecreasing);
        Assert.Null(sequence._isIncreasing);
        Assert.Null(sequence._isNonIncreasing);
        Assert.Null(sequence._isDecreasing);
        Assert.Null(sequence._isPoint);
        Assert.Null(sequence._startsWithPlateau);
        Assert.Null(sequence._endsWithPlateau);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void EveryPropertyReportsWhatItDidBefore(Sequence sequence)
    {
        sequence = new Sequence(sequence.Elements);
        var before = ReadProperties(sequence);
        Assert.Equal(before, ReadProperties(sequence));
        var copy = new Sequence(sequence.Elements);

        sequence.ResetCachedProperties();

        Assert.Equal(before, ReadProperties(sequence));
        Assert.True(Sequence.Equivalent(copy, sequence));
    }
}
