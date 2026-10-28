using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class EnvelopeSupport
{
    [Fact]
    public void MinimumPreservesAnOpenOuterEndpoint()
    {
        var left = new Sequence([Segment.Constant(0, 1, 2)]);
        var right = Sequence.Constant(3, 2, 3);
        var result = Sequence.Minimum(left, right, cutToOverlap: false);
        Assert.True(result.IsLeftOpen);
        Assert.Equal(0, result.DefinedFrom);
        Assert.Equal(Rational.PlusInfinity, result.ValueAt(1));
    }

    [Fact]
    public void MaximumPreservesAnOpenOuterEndpoint()
    {
        var left = new Sequence([Segment.Constant(0, 1, 2)]);
        var right = Sequence.Constant(3, 2, 3);
        var result = Sequence.Maximum(left, right, cutToOverlap: false);
        Assert.True(result.IsLeftOpen);
        Assert.Equal(0, result.DefinedFrom);
        Assert.Equal(Rational.MinusInfinity, result.ValueAt(1));
    }

    public static IEnumerable<IReadOnlyList<Sequence>> DisjointOperands()
    {
        foreach (var count in new[] { 2, 3, 4 })
            yield return Enumerable.Range(0, count)
                .Select(i => Sequence.Constant(i + 1, 2 * i, 2 * i + 1))
                .ToArray();
    }

    public static IEnumerable<object[]> DisjointCases() => DisjointOperands().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DisjointCases))]
    public void LowerFoldsFillInternalGapsWithPositiveInfinity(IReadOnlyList<Sequence> operands)
    {
        var result = operands.LowerEnvelope(new ComputationSettings { UseParallelism = false })
            .ToSequence();
        Assert.Equal(Rational.PlusInfinity, result.ValueAt(new Rational(3, 2)));
    }

    [Theory]
    [MemberData(nameof(DisjointCases))]
    public void UpperFoldsFillInternalGapsWithNegativeInfinity(IReadOnlyList<Sequence> operands)
    {
        var result = operands.UpperEnvelope(new ComputationSettings { UseParallelism = false })
            .ToSequence();
        Assert.Equal(Rational.MinusInfinity, result.ValueAt(new Rational(3, 2)));
    }
}
