using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class UnionSupport
{
    public static IEnumerable<(Sequence Left, Sequence Right)> OperandPairs()
    {
        Sequence[] operands =
        [
            Sequence.Constant(1, -2, 0, false, false),
            Sequence.Constant(2, -2, 0, true, false),
            Sequence.Constant(3, -1, 1, false, true),
            Sequence.Constant(4, 1, 3, false, false),
            new Sequence([new Point(0, 5)])
        ];
        foreach (var left in operands)
        foreach (var right in operands)
            yield return (left, right);
    }

    public static IEnumerable<object[]> Cases() => OperandPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void MinimumUsesOnlyDefinedOuterEndpoints(Sequence left, Sequence right)
    {
        var streamed = left.Elements.Minimum(right.Elements, cutToOverlap: false)
            .ToSequence();
        var materialized = Sequence.Minimum(left, right, cutToOverlap: false, settings: new ComputationSettings
        {
            UseParallelLowerEnvelope = true,
            SequenceLowerEnvelopeParallelizationThreshold = 0
        });
        var serial = Sequence.Minimum(left, right, cutToOverlap: false,
            settings: new ComputationSettings { UseParallelism = false });
        var many = new[] { left.Elements, right.Elements }.Minimum()
            .ToSequence();
        var fold = new[] { left, right }.LowerEnvelope()
            .ToSequence();
        foreach (var result in new[] { streamed, materialized, serial, many, fold })
        {
            CheckSupport(left, right, result);
            foreach (var time in SampleTimes(result))
            {
                var a = left.IsDefinedAt(time) ? left.ValueAt(time) : Rational.PlusInfinity;
                var b = right.IsDefinedAt(time) ? right.ValueAt(time) : Rational.PlusInfinity;
                Assert.Equal(Rational.Min(a, b), result.ValueAt(time));
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void MaximumUsesOnlyDefinedOuterEndpoints(Sequence left, Sequence right)
    {
        var streamed = left.Elements.Maximum(right.Elements, cutToOverlap: false)
            .ToSequence();
        var materialized = Sequence.Maximum(left, right, cutToOverlap: false, settings: new ComputationSettings
        {
            UseParallelUpperEnvelope = true,
            SequenceUpperEnvelopeParallelizationThreshold = 0
        });
        var serial = Sequence.Maximum(left, right, cutToOverlap: false,
            settings: new ComputationSettings { UseParallelism = false });
        var many = new[] { left.Elements, right.Elements }.Maximum()
            .ToSequence();
        var fold = new[] { left, right }.UpperEnvelope()
            .ToSequence();
        foreach (var result in new[] { streamed, materialized, serial, many, fold })
        {
            CheckSupport(left, right, result);
            foreach (var time in SampleTimes(result))
            {
                var a = left.IsDefinedAt(time) ? left.ValueAt(time) : Rational.MinusInfinity;
                var b = right.IsDefinedAt(time) ? right.ValueAt(time) : Rational.MinusInfinity;
                Assert.Equal(Rational.Max(a, b), result.ValueAt(time));
            }
        }
    }

    private static void CheckSupport(Sequence left, Sequence right, Sequence result)
    {
        var from = Rational.Min(left.DefinedFrom, right.DefinedFrom);
        var to = Rational.Max(left.DefinedUntil, right.DefinedUntil);
        Assert.Equal(from, result.DefinedFrom);
        Assert.Equal(to, result.DefinedUntil);
        Assert.Equal(left.IsDefinedAt(from) || right.IsDefinedAt(from), result.IsLeftClosed);
        Assert.Equal(left.IsDefinedAt(to) || right.IsDefinedAt(to), result.IsRightClosed);
    }

    private static IEnumerable<Rational> SampleTimes(Sequence result)
    {
        for (var time = result.DefinedFrom; time <= result.DefinedUntil; time += new Rational(1, 2))
            if (result.IsDefinedAt(time))
                yield return time;
    }
}
