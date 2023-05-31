using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveDecompose
{
    public static List<Curve> ContinuousExamples = new List<Curve>()
    {
        // a
        new Curve(
            baseSequence: new Sequence(new List<Element>()
            {
                Point.Origin(),
                Segment.Zero(0,1),
                new Point(1, 0),
                new Segment(1, 3, 0, 0.5m),
                new Point(3, 1),
                new Segment(3, 4, 1, 1),
                new Point(4, 2),
                Segment.Constant(4, 5, 2)
            }),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        ),
        // b
        new Curve(
            baseSequence: new Sequence(new List<Element>()
            {
                Point.Origin(),
                new Segment(0,1, 0, 1),
                new Point(1, 1),
                Segment.Constant(1, 3, 1),
                new Point(3, 1),
                new Segment(3,4, 1, 1)
            }),
            pseudoPeriodStart: 2,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        ),
        // b with T delayed
        new Curve(
            baseSequence: new Sequence(new List<Element>()
            {
                Point.Origin(),
                new Segment(0,1, 0, 1),
                new Point(1, 1),
                Segment.Constant(1, 3, 1),
                new Point(3, 1),
                new Segment(3,4, 1, 1),
                new Point(4, 2),
                Segment.Constant(4, 5, 2),
            }),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        )
    };

    public static IEnumerable<object[]> DecompositionEquivalenceTestCases()
    {
        var testcases = ContinuousExamples;

        foreach (var c in testcases)
        {
            yield return new object[] { c };
        }
    }

    [Theory]
    [MemberData(nameof(DecompositionEquivalenceTestCases))]
    public void MinPlusDecompositionEquivalence(Curve c)
    {
        var (c_t, c_p) = c.Decompose();
        var recomposed = Curve.Minimum(c_t, c_p);
        Assert.True(Curve.Equivalent(c, recomposed));
    }

    [Theory]
    [MemberData(nameof(DecompositionEquivalenceTestCases))]
    public void MaxPlusDecompositionEquivalence(Curve c)
    {
        var (c_t, c_p) = c.Decompose(minDecomposition: false);
        var recomposed = Curve.Maximum(c_t, c_p);
        Assert.True(Curve.Equivalent(c, recomposed));
    }
}