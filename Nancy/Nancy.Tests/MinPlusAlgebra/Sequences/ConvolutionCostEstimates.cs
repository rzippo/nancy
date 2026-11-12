using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class ConvolutionCostEstimates
{
    public static IEnumerable<object[]> SegmentCounts => [[23_170], [23_171]];

    [Theory]
    [MemberData(nameof(SegmentCounts))]
    public void MinPlusConvolutionMatchesDirectResultWithLargeCost(int segments)
    {
        var sequence = SequenceWithPlateau(segments);
        Assert.Equal(2 * segments + 3, sequence.Count);
        var result = Sequence.Convolution(sequence, sequence,
            new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = true },
            cutEnd: 2 * (segments + 1), isEndIncluded: true, useIsomorphism: true);
        var expected = Sequence.Convolution(sequence.Optimize(), sequence.Optimize(),
            new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = false },
            cutEnd: 2 * (segments + 1), isEndIncluded: true);
        Assert.True(Sequence.Equivalent(expected, result), result.ToCodeString());
    }

    [Theory]
    [MemberData(nameof(SegmentCounts))]
    public void MaxPlusConvolutionMatchesDirectResultWithLargeCost(int segments)
    {
        var sequence = SequenceWithPlateau(segments);
        Assert.Equal(2 * segments + 3, sequence.Count);
        var result = Sequence.MaxPlusConvolution(sequence, sequence,
            new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = true },
            cutEnd: 2 * (segments + 1), isEndIncluded: true, useIsomorphism: true);
        var expected = Sequence.MaxPlusConvolution(sequence.Optimize(), sequence.Optimize(),
            new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = false },
            cutEnd: 2 * (segments + 1), isEndIncluded: true);
        Assert.True(Sequence.Equivalent(expected, result), result.ToCodeString());
    }

    private static Sequence SequenceWithPlateau(int segments)
    {
        var elements = new List<Element>(2 * segments + 1) { Point.Origin() };
        for (int i = 0; i < segments; i++)
        {
            elements.Add(Segment.Constant(i, i + 1, 0));
            elements.Add(new Point(i + 1, 0));
        }
        elements.Add(new Segment(segments, segments + 1, 0, 1));
        elements.Add(new Point(segments + 1, 1));
        return new Sequence(elements);
    }
}
