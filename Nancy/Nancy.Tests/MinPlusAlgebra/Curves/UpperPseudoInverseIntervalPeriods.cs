using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks unbounded interval inverses at and around periodic boundaries, including excluded starts.
/// </summary>
public class UpperPseudoInverseIntervalPeriods
{
    public static IEnumerable<(string Name, Curve Curve, Rational Start)> CurvesAndStarts()
    {
        foreach (var (name, curve) in PseudoInversePeriods.Curves()
                     .Where(test => test.Curve.IsNonNegative))
        {
            foreach (var start in new[] { Rational.Zero, curve.PseudoPeriodStart,
                         curve.PseudoPeriodStart + curve.PseudoPeriodLength / 2,
                         curve.FirstPseudoPeriodEnd }.Distinct())
                yield return (name, curve, start);
        }
    }

    public static IEnumerable<object[]> Cases() => CurvesAndStarts().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void IncludedStartAgreesWithSupremum(string name, Curve curve, Rational start)
    {
        var inverse = curve.UpperPseudoInverseOverInterval(start);
        var sourceEnd = curve.FirstPseudoPeriodEnd + 6 * curve.PseudoPeriodLength;
        var source = curve.Cut(start, sourceEnd, isStartIncluded: true, isEndIncluded: true);
        var end = curve.ValueAt(sourceEnd) - curve.PseudoPeriodHeight;
        var actual = inverse.Cut(0, end, isEndIncluded: true);
        foreach (var height in Heights(source, actual, end))
            Assert.True(PseudoInverseDefinitions.Supremum(source.Elements, height) == actual.ValueAt(height),
                $"{name}, start {start}, height {height}");
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void ExcludedStartAgreesWithSupremum(string name, Curve curve, Rational start)
    {
        var inverse = curve.UpperPseudoInverseOverInterval(start, isStartIncluded: false);
        var sourceEnd = curve.FirstPseudoPeriodEnd + 6 * curve.PseudoPeriodLength;
        var source = curve.Cut(start, sourceEnd, isStartIncluded: false, isEndIncluded: true);
        var end = curve.ValueAt(sourceEnd) - curve.PseudoPeriodHeight;
        var actual = inverse.Cut(0, end, isEndIncluded: true);
        foreach (var height in Heights(source, actual, end))
            Assert.True(PseudoInverseDefinitions.Supremum(source.Elements, height) == actual.ValueAt(height),
                $"{name}, excluded start {start}, height {height}");
    }

    private static IEnumerable<Rational> Heights(Sequence source, Sequence inverse, Rational end)
        => source.Elements
            .SelectMany(element => element is Point point
                ? new[] { point.Value }
                : new[] { ((Segment)element).RightLimitAtStartTime, ((Segment)element).LeftLimitAtEndTime })
            .Concat(inverse.Elements.SelectMany(element => element is Point point
                ? new[] { point.Time }
                : new[] { element.StartTime + (element.EndTime - element.StartTime) / 3,
                    element.StartTime + 2 * (element.EndTime - element.StartTime) / 3 }))
            .Where(height => height >= 0 && height <= end)
            .Distinct();
}
