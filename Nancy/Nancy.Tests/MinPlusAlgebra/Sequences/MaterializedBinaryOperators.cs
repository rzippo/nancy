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

    public static IEnumerable<(Curve Left, Curve Right)> CurvePairCases()
    {
        Curve[] curves =
        [
            new StairCurve(4, 3), new StairCurve(6, 5).DelayBy(2),
            new ConstantCurve(-2), new RateLatencyServiceCurve(2, 1),
            Curve.Minimum(new RateLatencyServiceCurve(3, 1), new ConstantCurve(8)),
            Curve.PlusInfinite(), Curve.MinusInfinite()
        ];
        foreach (var a in curves)
        foreach (var b in curves)
            yield return (a, b);
    }

    public static IEnumerable<object[]> CurvePairs() => CurvePairCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CurvePairs))]
    public void CurveAdditionMatchesTheMaterializedBucketReference(Curve a, Curve b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var start = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart);
        var period = Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);
        Sequence expected;
        try
        {
            _ = a.PseudoPeriodSlope + b.PseudoPeriodSlope;
            expected = MaterializedPointwiseReference.Addition(a.Extend(start + period, settings), b.Extend(start + period, settings));
        }
        catch (UndeterminedResultException)
        {
            Assert.Throws<UndeterminedResultException>(() => Curve.Addition(a, b, settings));
            return;
        }
        var actual = Curve.Addition(a, b, settings)
            .Cut(0, start + period, settings: settings);
        Assert.True(Sequence.Equivalent(expected, actual));
    }

    [Theory]
    [MemberData(nameof(CurvePairs))]
    public void CurveMinimumMatchesItsValuesAndLimits(Curve a, Curve b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var actual = Curve.Minimum(a, b, settings);
        var end = 2 * actual.FirstPseudoPeriodEnd;
        var times = a.Cut(0, end, true, true, settings).Elements
            .Concat(b.Cut(0, end, true, true, settings).Elements)
            .Concat(actual.Cut(0, end, true, true, settings).Elements)
            .SelectMany(e => new[] { e.StartTime, e.EndTime })
            .Distinct()
            .OrderBy(t => t)
            .ToArray();
        Rational Combine(Rational x, Rational y) => Rational.Min(x, y);
        foreach (var time in times)
        {
            Assert.Equal(Combine(a.ValueAt(time), b.ValueAt(time)), actual.ValueAt(time));
            Assert.Equal(Combine(a.RightLimitAt(time), b.RightLimitAt(time)), actual.RightLimitAt(time));
            if (time > 0) Assert.Equal(Combine(a.LeftLimitAt(time), b.LeftLimitAt(time)), actual.LeftLimitAt(time));
        }
        for (var i = 1; i < times.Length; i++)
        {
            var middle = (times[i - 1] + times[i]) / 2;
            Assert.Equal(Combine(a.ValueAt(middle), b.ValueAt(middle)), actual.ValueAt(middle));
        }
    }

    [Theory]
    [MemberData(nameof(CurvePairs))]
    public void CurveMaximumMatchesItsValuesAndLimits(Curve a, Curve b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var actual = Curve.Maximum(a, b, settings);
        var end = 2 * actual.FirstPseudoPeriodEnd;
        var times = a.Cut(0, end, true, true, settings).Elements
            .Concat(b.Cut(0, end, true, true, settings).Elements)
            .Concat(actual.Cut(0, end, true, true, settings).Elements)
            .SelectMany(e => new[] { e.StartTime, e.EndTime })
            .Distinct()
            .OrderBy(t => t)
            .ToArray();
        Rational Combine(Rational x, Rational y) => Rational.Max(x, y);
        foreach (var time in times)
        {
            Assert.Equal(Combine(a.ValueAt(time), b.ValueAt(time)), actual.ValueAt(time));
            Assert.Equal(Combine(a.RightLimitAt(time), b.RightLimitAt(time)), actual.RightLimitAt(time));
            if (time > 0) Assert.Equal(Combine(a.LeftLimitAt(time), b.LeftLimitAt(time)), actual.LeftLimitAt(time));
        }
        for (var i = 1; i < times.Length; i++)
        {
            var middle = (times[i - 1] + times[i]) / 2;
            Assert.Equal(Combine(a.ValueAt(middle), b.ValueAt(middle)), actual.ValueAt(middle));
        }
    }
}
