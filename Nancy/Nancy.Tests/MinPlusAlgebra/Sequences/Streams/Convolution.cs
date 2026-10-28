using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

/// <summary>
/// Checks incremental partial-envelope merging and serial deconvolution pair production against complete mathematical references.
/// </summary>
public class Convolution
{
    public static IEnumerable<(Sequence Left, Sequence Right)> ConvolutionPairs()
    {
        Sequence[] sequences =
        [
            new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 2), new Segment(1, 3, 2, 2), new Point(3, 6)]),
            new Sequence([new Segment(0, 2, 1, 2), new Point(2, 5), Segment.Constant(2, 4, 6)]),
            new Sequence([new Point(0, 0), Segment.Constant(0, 1, 0), new Point(1, 1), new Segment(1, 4, 1, 1)]),
            new Sequence([new Point(1, 2), new Segment(1, 3, 2, -1), new Point(3, 0)]),
            new Sequence([new Point(0, 0), Segment.PlusInfinite(0, 1), new Point(1, 2), new Segment(1, 3, 2, 1)])
        ];
        foreach (var a in sequences)
        foreach (var b in sequences)
            yield return (a, b);
    }

    public static IEnumerable<object[]> Pairs() => ConvolutionPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Pairs))]
    public void ConvolutionPartialEnvelopesEqualTheUnpartitionedResult(Sequence a, Sequence b)
    {
        var serial = new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = false, UseConvolutionPartitioning = false };
        var expected = Sequence.Convolution(a, b, serial);
        foreach (var chunk in new[] { 2, 7, 19 })
        {
            var settings = serial with { UseConvolutionPartitioning = true, ConvolutionPartitioningThreshold = chunk };
            var actual = Sequence.Convolution(a, b, settings);
            Assert.True(Sequence.Equivalent(expected, actual));
        }
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void MaxPlusConvolutionPartialEnvelopesEqualTheUnpartitionedResult(Sequence a, Sequence b)
    {
        var serial = new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = false, UseConvolutionPartitioning = false };
        var expected = Sequence.MaxPlusConvolution(a, b, serial);
        foreach (var chunk in new[] { 2, 7, 19 })
        {
            var settings = serial with { UseConvolutionPartitioning = true, ConvolutionPartitioningThreshold = chunk };
            var actual = Sequence.MaxPlusConvolution(a, b, settings);
            Assert.True(Sequence.Equivalent(expected, actual));
        }
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void DeconvolutionEqualsTheEligibleElementPairEnvelope(Sequence a, Sequence b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var elements = a.Elements.SelectMany(ea => b.Elements.Select(eb => (a: ea, b: eb)))
            .Where(p => !p.a.IsMinusInfinite && !p.b.IsPlusInfinite)
            .SelectMany(p => Element.Deconvolution(p.a, p.b))
            .ToList();
        var expected = elements.UpperEnvelope(settings)
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, Sequence.Deconvolution(a, b, settings: settings)));
    }
}
