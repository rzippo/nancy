using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class PairEnvelopes
{
    public static IEnumerable<(Sequence Left, Sequence Right)> OperandPairs()
    {
        var elements = new List<Element>();
        for (int i = 0; i < 32; i++)
        {
            elements.Add(new Point(i, i % 3 - 2));
            elements.Add(new Segment(i, i + 1, i % 4, i % 2 == 0 ? 1 : -1));
        }
        elements.Add(new Point(32, -3));
        var varying = new Sequence(elements);
        yield return (varying, Sequence.Constant(1, 0, 32, true, true));
        yield return (varying, new Sequence([
            new Point(new Rational(49, 2), 3), new Segment(new Rational(49, 2), new Rational(51, 2), 2, -1)
        ]));
        yield return (new Sequence([
            Segment.Constant(Rational.MinusInfinity, 0, 2), new Point(0, -3), Segment.Constant(0, Rational.PlusInfinity, 4)
        ]), Sequence.Constant(1, Rational.MinusInfinity, Rational.PlusInfinity, false, false));
        yield return (varying, new Sequence([new Point(32, 5), new Segment(32, 33, 7, -1)]));
    }

    public static IEnumerable<object[]> Cases() => OperandPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void MinimumMatchesTheBucketReference(Sequence left, Sequence right)
    {
        var expected = MaterializedPointwiseReference.Minimum(left, right);
        var serial = Sequence.Minimum(left, right, settings: new ComputationSettings { UseParallelism = false });
        var parallel = Sequence.Minimum(left, right, settings: new ComputationSettings
        {
            UseParallelLowerEnvelope = true,
            SequenceLowerEnvelopeParallelizationThreshold = 0
        });
        Assert.True(Sequence.Equivalent(expected, serial));
        Assert.True(Sequence.Equivalent(expected, parallel));
        Assert.Equal(expected.Count, parallel.Count);
        var union = IntervalBucket.ComputeIntervals(left, right)
            .SelectMany(bucket => bucket.LowerEnvelope())
            .MergeAsEnumerable()
            .ToList();
        Assert.Equal(union, Sequence.LowerEnvelope(left, right, new ComputationSettings { UseParallelism = false }));
        Assert.Equal(union, Sequence.LowerEnvelope(left, right, new ComputationSettings
        {
            UseParallelLowerEnvelope = true,
            SequenceLowerEnvelopeParallelizationThreshold = 0
        }));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void MaximumMatchesTheBucketReference(Sequence left, Sequence right)
    {
        var expected = MaterializedPointwiseReference.Maximum(left, right);
        var serial = Sequence.Maximum(left, right, settings: new ComputationSettings { UseParallelism = false });
        var parallel = Sequence.Maximum(left, right, settings: new ComputationSettings
        {
            UseParallelUpperEnvelope = true,
            SequenceUpperEnvelopeParallelizationThreshold = 0
        });
        Assert.True(Sequence.Equivalent(expected, serial));
        Assert.True(Sequence.Equivalent(expected, parallel));
        Assert.Equal(expected.Count, parallel.Count);
        var union = IntervalBucket.ComputeIntervals(left, right)
            .SelectMany(bucket => bucket.UpperEnvelope())
            .MergeAsEnumerable()
            .ToList();
        Assert.Equal(union, Sequence.UpperEnvelope(left, right, new ComputationSettings { UseParallelism = false }));
        Assert.Equal(union, Sequence.UpperEnvelope(left, right, new ComputationSettings
        {
            UseParallelUpperEnvelope = true,
            SequenceUpperEnvelopeParallelizationThreshold = 0
        }));
    }
}
