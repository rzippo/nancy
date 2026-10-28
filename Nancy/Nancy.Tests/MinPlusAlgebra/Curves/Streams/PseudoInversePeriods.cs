using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;

/// <summary>
/// Checks inverse pseudo-period boundaries against materialized inverses and direct searches of the source.
/// </summary>
public class PseudoInversePeriods
{
    public static IEnumerable<(string Name, Curve Curve)> Curves()
    {
        foreach (var curve in PseudoInverses.PseudoInverseCases()
                     .Where(c => c.IsFinite && c.PseudoPeriodHeight > 0 && !c.IsUltimatelyAffine))
            yield return (curve.GetType().Name, curve);

        yield return ("negative prefix jumps across zero", new Curve(new Sequence([
            new Point(0, -1), Segment.Constant(0, 1, -1),
            new Point(1, 1), Segment.Constant(1, 2, 1)
        ]), 0, 2, 4));

        yield return ("plateau begins before the pseudo-period", new Curve(new Sequence([
            Point.Origin(), new Segment(0, 1, 0, 2),
            new Point(1, 2), Segment.Constant(1, 3, 2),
            new Point(3, 2), new Segment(3, 4, 2, 2)
        ]), 2, 2, 2));

        foreach (Rational shift in new[] { -3, 0, 3, 20 })
        {
            var curve = new Curve(new Sequence([
                new Point(0, -shift), new Segment(0, 2, -shift, 1),
                new Point(2, 2 - shift), Segment.Constant(2, 3, 2 - shift),
                new Point(3, 3 - shift), new Segment(3, 4, 4 - shift, 2),
                new Point(4, 6 - shift), new Segment(4, 5, 6 - shift, 1)
            ]), 2, 3, 5);
            yield return ($"plateaus and jumps, shift {shift}", curve);
            yield return ($"fractional plateaus and jumps, shift {shift}", curve.Scale(new Rational(1, 2)));
        }
    }

    public static IEnumerable<object[]> Cases() => Curves().ToXUnitTestCases();

    private static IEnumerable<Rational> Ends(Curve curve)
    {
        var start = curve.ValueAt(curve.FirstPseudoPeriodEnd);
        var length = curve.PseudoPeriodHeight;
        if (start < 0)
            start += (-start / length).Ceil() * length;
        yield return 0;
        // UPI can repeat from f(T), one period before LPI; include both sides of its second-period boundary.
        if (start - length / 2 >= 0)
            yield return start - length / 2;
        yield return start;
        yield return start + length / 2;
        yield return start + length;
        yield return start + 3 * length / 2;
        yield return start + 2 * length;
        yield return start + 6 * length;
    }

    private static Sequence SourceBeyond(Curve curve, Rational height)
    {
        var end = curve.FirstPseudoPeriodEnd;
        while (curve.ValueAt(end) <= height)
            end *= 2;
        return curve.Cut(0, end, isEndIncluded: true);
    }

    private static IEnumerable<Rational> Heights(Sequence sequence) => sequence.Elements
        .SelectMany(element => element is Point point
            ? new[] { point.Time }
            : new[] { element.StartTime + (element.EndTime - element.StartTime) / 3,
                element.StartTime + 2 * (element.EndTime - element.StartTime) / 3 });

    [Theory]
    [MemberData(nameof(Cases))]
    public void LowerInversePreservesValuesAcrossRepeatedPeriods(string name, Curve curve)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var inverse = curve.LowerPseudoInverse();
        foreach (var end in Ends(curve))
        {
            var actual = curve.LowerPseudoInverseAsEnumerable(end, settings)
                .ToSequence();
            Assert.True(Sequence.Equivalent(inverse.Cut(0, end, true, true, settings), actual), name);
            var source = SourceBeyond(curve, end);
            foreach (var height in Heights(actual))
                Assert.Equal(PseudoInverseDefinitions.Infimum(source.Elements, height), actual.ValueAt(height));
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void UpperInversePreservesValuesAcrossRepeatedPeriods(string name, Curve curve)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var inverse = curve.UpperPseudoInverse();
        foreach (var end in Ends(curve))
        {
            var actual = curve.UpperPseudoInverseAsEnumerable(end, settings)
                .ToSequence();
            Assert.True(Sequence.Equivalent(inverse.Cut(0, end, true, true, settings), actual), name);
            var source = SourceBeyond(curve, end);
            foreach (var height in Heights(actual))
                Assert.Equal(PseudoInverseDefinitions.Supremum(source.Elements, height), actual.ValueAt(height));
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void LowerInverseCanBeEnumeratedAgain(string name, Curve curve)
    {
        var end = Ends(curve).Last();
        var stream = curve.LowerPseudoInverseAsEnumerable(end);
        var first = stream.ToSequence();
        Assert.True(Sequence.Equivalent(first, stream.ToSequence()), name);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void UpperInverseCanBeEnumeratedAgain(string name, Curve curve)
    {
        var end = Ends(curve).Last();
        var stream = curve.UpperPseudoInverseAsEnumerable(end);
        var first = stream.ToSequence();
        Assert.True(Sequence.Equivalent(first, stream.ToSequence()), name);
    }
}
