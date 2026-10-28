using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;

/// <summary>
/// Checks the streamed inverse-gap window against source-based infima, including one-sided values between source heights.
/// </summary>
public class InverseGapWindow
{
    public static IEnumerable<(Curve F, Curve G)> Pairs()
    {
        Curve[] arrivals = [new StairCurve(2, 2), new StairCurve(3, 3),
            new StairCurve(2, 2).DelayBy(2), new RateLatencyServiceCurve(1, 2)];
        Curve[] services = [new StairCurve(3, 3), new StairCurve(3, 2),
            new StairCurve(3, 3).DelayBy(4), new RateLatencyServiceCurve(2, 3),
            new StairCurve(3, 3).VerticalShift(-4, exceptOrigin: false)];
        foreach (var f in arrivals)
        foreach (var g in services)
            yield return (f, g);
    }

    public static IEnumerable<object[]> Cases() => Pairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void GapAgreesWithInfimaOfSourceElements(Curve f, Curve g)
    {
        // These fixtures have inverse periods 2 or 3 and transients shorter than height 12.
        // Height 24 covers more than one common period beyond them; time 100 reaches every sampled height.
        Rational heightEnd = 24;
        var fSource = f.Cut(0, 100, isEndIncluded: true);
        var gSource = g.Cut(0, 100, isEndIncluded: true);
        var heights = fSource.Elements.Concat(gSource.Elements)
            .SelectMany(element => element is Point point
                ? new[] { point.Value }
                : new[] { ((Segment)element).RightLimitAtStartTime, ((Segment)element).LeftLimitAtEndTime })
            .Append(0)
            .Append(heightEnd)
            .Where(height => height >= 0 && height <= heightEnd)
            .Distinct()
            .OrderBy(height => height)
            .ToList();
        Rational Gap(Rational height) => PseudoInverseDefinitions.Infimum(gSource.Elements, height)
                                        - PseudoInverseDefinitions.Infimum(fSource.Elements, height);
        var expected = heights.Select(Gap).Max();
        foreach (var (left, right) in heights.Zip(heights.Skip(1)))
        {
            var first = Gap(left + (right - left) / 3);
            var second = Gap(left + 2 * (right - left) / 3);
            // Between consecutive source heights the gap is affine; extrapolate both limits instead of relying on samples alone.
            expected = Rational.Max(expected, Rational.Max(2 * first - second, 2 * second - first));
        }

        var actual = Curve.HorizontalDeviation(f, g, new ComputationSettings
        {
            UseParallelism = false,
            HorizontalDeviationStrategy = HorizontalDeviationStrategy.PseudoInverseGap
        });
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AffineOperandsReuseTheirReducedInverseTransient()
    {
        // Deliberately retain a long representation of two affine functions.
        var f = new Curve(new Sequence([Point.Origin(), new Segment(0, 1000, 0, 1),
            new Point(1000, 1000), new Segment(1000, 1001, 1000, 1)]), 1000, 1, 1);
        var g = new Curve(new Sequence([new Point(0, -3), new Segment(0, 1000, -3, 2),
            new Point(1000, 1997), new Segment(1000, 1001, 1997, 2)]), 1000, 1, 2);
        Assert.True(f.LowerPseudoInverse().PseudoPeriodStart < f.ValueAt(f.FirstPseudoPeriodEnd));
        Assert.True(g.LowerPseudoInverse().PseudoPeriodStart < g.ValueAt(g.FirstPseudoPeriodEnd));
        Assert.Equal(new Rational(3, 2), Curve.HorizontalDeviation(f, g, new ComputationSettings
        {
            UseParallelism = false,
            HorizontalDeviationStrategy = HorizontalDeviationStrategy.PseudoInverseGap
        }));
    }
}
