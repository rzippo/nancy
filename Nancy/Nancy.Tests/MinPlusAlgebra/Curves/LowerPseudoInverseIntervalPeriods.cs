using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks unbounded interval inverses at and around periodic boundaries, including excluded starts.
/// </summary>
public class LowerPseudoInverseIntervalPeriods
{
    public static IEnumerable<object[]> Cases() => UpperPseudoInverseIntervalPeriods.CurvesAndStarts().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void IncludedStartAgreesWithInfimum(string name, Curve curve, Rational start)
    {
        var inverse = curve.LowerPseudoInverseOverInterval(start);
        var sourceEnd = curve.FirstPseudoPeriodEnd + 6 * curve.PseudoPeriodLength;
        var source = curve.Cut(start, sourceEnd, isStartIncluded: true, isEndIncluded: true);
        var end = curve.ValueAt(sourceEnd) - curve.PseudoPeriodHeight;
        var actual = inverse.Cut(0, end, isEndIncluded: true);
        foreach (var height in Heights(source, actual, end))
            Assert.True(PseudoInverseDefinitions.Infimum(source.Elements, height) == actual.ValueAt(height),
                $"{name}, start {start}, height {height}");
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void ExcludedStartAgreesWithInfimum(string name, Curve curve, Rational start)
    {
        var inverse = curve.LowerPseudoInverseOverInterval(start, isStartIncluded: false);
        var sourceEnd = curve.FirstPseudoPeriodEnd + 6 * curve.PseudoPeriodLength;
        var source = curve.Cut(start, sourceEnd, isStartIncluded: false, isEndIncluded: true);
        var end = curve.ValueAt(sourceEnd) - curve.PseudoPeriodHeight;
        var actual = inverse.Cut(0, end, isEndIncluded: true);
        foreach (var height in Heights(source, actual, end))
            Assert.True(PseudoInverseDefinitions.Infimum(source.Elements, height) == actual.ValueAt(height),
                $"{name}, excluded start {start}, height {height}");
    }

    /// <summary>
    /// The heights of the source and inside the pieces of the inverse, from the infimum of the source on:
    /// below it, the inverse over the interval is $+\infty$ by definition.
    /// </summary>
    private static IEnumerable<Rational> Heights(Sequence source, Sequence inverse, Rational end)
        => source.Elements
            .SelectMany(element => element is Point point
                ? new[] { point.Value }
                : new[] { ((Segment)element).RightLimitAtStartTime, ((Segment)element).LeftLimitAtEndTime })
            .Concat(inverse.Elements.SelectMany(element => element is Point point
                ? new[] { point.Time }
                : new[] { element.StartTime + (element.EndTime - element.StartTime) / 3,
                    element.StartTime + 2 * (element.EndTime - element.StartTime) / 3 }))
            .Where(height => height >= 0 && height >= source.InfValue() && height <= end)
            .Distinct();
}
