using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Checks materialized binary operators against interval-bucket and direct pointwise references independent of their stream implementations.
/// </summary>
public class MaterializedBinaryOperators
{
    public static IEnumerable<(Sequence Left, Sequence Right)> SequencePairs() =>
        Envelopes.OperandCases().Concat(Envelopes.SupportPairCases())
            .Select(pair => (pair.Left, pair.Right));

    public static IEnumerable<object[]> Cases() => SequencePairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void SequenceAdditionMatchesTheBucketReference(Sequence a, Sequence b)
    {
        Sequence Calculate() => Sequence.Addition(a, b);
        Sequence expected;
        try { expected = MaterializedPointwiseReference.Addition(a, b); }
        catch (UndeterminedResultException)
        {
            Assert.Throws<UndeterminedResultException>(() => Calculate());
            return;
        }
        Assert.True(Sequence.Equivalent(expected, Calculate()));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void SequenceSubtractionMatchesTheBucketReference(Sequence a, Sequence b)
    {
        Sequence Calculate() => Sequence.Subtraction(a, b);
        Sequence expected;
        try { expected = MaterializedPointwiseReference.Subtraction(a, b); }
        catch (UndeterminedResultException)
        {
            Assert.Throws<UndeterminedResultException>(() => Calculate());
            return;
        }
        Assert.True(Sequence.Equivalent(expected, Calculate()));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void SequenceMinimumMatchesTheBucketReference(Sequence a, Sequence b)
    {
        Sequence Calculate() => Sequence.Minimum(a, b, settings: new ComputationSettings { UseParallelism = false });
        Sequence expected;
        try { expected = MaterializedPointwiseReference.Minimum(a, b); }
        catch (UndeterminedResultException)
        {
            Assert.Throws<UndeterminedResultException>(() => Calculate());
            return;
        }
        Assert.True(Sequence.Equivalent(expected, Calculate()));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void SequenceMaximumMatchesTheBucketReference(Sequence a, Sequence b)
    {
        Sequence Calculate() => Sequence.Maximum(a, b, settings: new ComputationSettings { UseParallelism = false });
        Sequence expected;
        try { expected = MaterializedPointwiseReference.Maximum(a, b); }
        catch (UndeterminedResultException)
        {
            Assert.Throws<UndeterminedResultException>(() => Calculate());
            return;
        }
        Assert.True(Sequence.Equivalent(expected, Calculate()));
    }
}
