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

    private static bool[] ReadProperties(Sequence s) =>
        [s.IsLeftContinuous, s.IsRightContinuous, s.IsNonDecreasing, s.IsIncreasing];

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void EveryCachedPropertyIsForgotten(Sequence sequence)
    {
        ReadProperties(sequence);
        sequence.ResetCachedProperties();

        Assert.Null(sequence._isLeftContinuous);
        Assert.Null(sequence._isRightContinuous);
        Assert.Null(sequence._isNonDecreasing);
        Assert.Null(sequence._isIncreasing);
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void EveryPropertyReportsWhatItDidBefore(Sequence sequence)
    {
        var before = ReadProperties(sequence);
        var copy = new Sequence(sequence.Elements);

        sequence.ResetCachedProperties();

        Assert.Equal(before, ReadProperties(sequence));
        Assert.True(Sequence.Equivalent(copy, sequence));
    }
}
