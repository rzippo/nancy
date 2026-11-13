using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class PeriodicCut
{
    private static Curve JoinedPeriods() => new(new Sequence([
        Point.Origin(), Segment.Zero(0, 2),
        Point.Zero(2), new Segment(2, 3, 0, 1),
        new Point(3, 1), new Segment(3, 4, 1, -1),
        Point.Zero(4), new Segment(4, 5, 0, 1)
    ]), 2, 3, 1);

    public static IEnumerable<(Curve Curve, Sequence Expected)> ClosedCuts()
    {
        yield return (JoinedPeriods(), new Sequence([
            new Point(new Rational(21, 4), new Rational(5, 4)),
            new Segment(new Rational(21, 4), new Rational(23, 4), new Rational(5, 4), 1),
            new Point(new Rational(23, 4), new Rational(7, 4))
        ]));
        yield return (JoinedPeriods(), new Sequence([
            new Point(new Rational(11, 2), new Rational(3, 2)),
            new Segment(new Rational(11, 2), 6, new Rational(3, 2), 1),
            new Point(6, 2), new Segment(6, 7, 2, -1),
            new Point(7, 1), new Segment(7, 8, 1, 1), new Point(8, 2)
        ]));
        yield return (JoinedPeriods(), new Sequence([
            new Point(5, 1), new Segment(5, 6, 1, 1),
            new Point(6, 2), new Segment(6, new Rational(13, 2), 2, -1),
            new Point(new Rational(13, 2), new Rational(3, 2))
        ]));
        yield return (JoinedPeriods(), new Sequence([
            new Point(5, 1), new Segment(5, 6, 1, 1),
            new Point(6, 2), new Segment(6, 7, 2, -1),
            new Point(7, 1), new Segment(7, 8, 1, 1), new Point(8, 2)
        ]));
        yield return (JoinedPeriods(), new Sequence([
            new Point(new Rational(11, 2), new Rational(3, 2)),
            new Segment(new Rational(11, 2), 6, new Rational(3, 2), 1),
            new Point(6, 2), new Segment(6, 7, 2, -1),
            new Point(7, 1), new Segment(7, 9, 1, 1),
            new Point(9, 3), new Segment(9, 10, 3, -1),
            new Point(10, 2), new Segment(10, new Rational(23, 2), 2, 1),
            new Point(new Rational(23, 2), new Rational(7, 2))
        ]));
        yield return (JoinedPeriods(), new Sequence([
            Point.Zero(1), Segment.Zero(1, 2),
            Point.Zero(2), new Segment(2, 3, 0, 1),
            new Point(3, 1), new Segment(3, 4, 1, -1),
            Point.Zero(4), new Segment(4, new Rational(11, 2), 0, 1),
            new Point(new Rational(11, 2), new Rational(3, 2))
        ]));
        yield return (new Curve(new Sequence([
            new Point(0, 10), new Segment(0, 1, 0, 1),
            new Point(1, 20), new Segment(1, 2, 5, 0)
        ]), 0, 2, 1), new Sequence([
            new Point(3, 21), new Segment(3, 4, 6, 0), new Point(4, 12)
        ]));
        yield return (new Curve(new Sequence([
            Point.Origin(), Segment.PlusInfinite(0, 1),
            new Point(1, 5), Segment.PlusInfinite(1, 2)
        ]), 0, 2, 0), new Sequence([
            new Point(new Rational(5, 2), Rational.PlusInfinity),
            Segment.PlusInfinite(new Rational(5, 2), 3),
            new Point(3, 5), Segment.PlusInfinite(3, 4),
            Point.Zero(4), Segment.PlusInfinite(4, new Rational(9, 2)),
            new Point(new Rational(9, 2), Rational.PlusInfinity)
        ]));
    }

    public static IEnumerable<(Curve Curve, Sequence Expected, bool IncludeStart, bool IncludeEnd)> CutCases()
    {
        foreach (var (curve, closed) in ClosedCuts())
        foreach (bool includeStart in new[] { false, true })
        foreach (bool includeEnd in new[] { false, true })
        {
            var expected = closed.Elements.Skip(includeStart ? 0 : 1)
                .SkipLast(includeEnd ? 0 : 1)
                .ToSequence();
            yield return (curve, expected, includeStart, includeEnd);
        }
    }

    public static IEnumerable<object[]> Cuts() => CutCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cuts))]
    public void MaterializedCutsPreserveBoundaryPieces(Curve curve, Sequence expected, bool includeStart, bool includeEnd)
    {
        var serial = new ComputationSettings
        {
            UseParallelism = false,
            ExtendParallelizationThreshold = 0,
            ComputeExtensionSequencesParallelizationThreshold = 0
        };
        foreach (var settings in new[] {
            serial,
            serial with { UseParallelExtend = true },
            serial with { UseParallelComputeExtensionSequences = true },
            serial with { UseParallelExtend = true, UseParallelComputeExtensionSequences = true }
        })
        {
            var actual = curve.Cut(expected.DefinedFrom, expected.DefinedUntil, includeStart, includeEnd, settings);
            Assert.Equal(expected, actual);
        }
    }

    [Theory]
    [MemberData(nameof(Cuts))]
    public void StreamedCutsPreserveBoundaryPieces(Curve curve, Sequence expected, bool includeStart, bool includeEnd)
    {
        var serial = new ComputationSettings
        {
            UseParallelism = false,
            ComputeExtensionSequencesParallelizationThreshold = 0
        };
        foreach (var settings in new[] { serial, serial with { UseParallelComputeExtensionSequences = true } })
        {
            var actual = curve.CutAsEnumerable(expected.DefinedFrom, expected.DefinedUntil, includeStart, includeEnd, settings)
                .ToSequence();
            Assert.Equal(expected, actual);
        }
    }
}
