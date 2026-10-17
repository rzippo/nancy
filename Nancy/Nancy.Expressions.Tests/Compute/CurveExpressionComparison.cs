using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// The comparison operators of curve expressions compare the computed curves, also when only the part after both transients tells them apart.
/// </summary>
public class CurveExpressionComparison
{
    // A curve of unit slope between jumps, given by its pieces: the value at the start of each, and the right limit there.
    private static Curve Pieces(
        Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight,
        params (Rational Start, Rational End, Rational Value, Rational Right)[] pieces)
        => new Curve(
            baseSequence: new Sequence(pieces.SelectMany(p => new Element[]
            {
                new Point(p.Start, p.Value),
                new Segment(p.Start, p.End, p.Right, 1)
            })),
            pseudoPeriodStart: pseudoPeriodStart,
            pseudoPeriodLength: pseudoPeriodLength,
            pseudoPeriodHeight: pseudoPeriodHeight);

    // f(t) = t + g(t), with g = 0 on [0,2), then 1 on [2,3), 0 on [3,4), 1 on [4,5) and so on: period 2 from T = 1.
    private static Curve Steps(Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight)
        => Pieces(pseudoPeriodStart, pseudoPeriodLength, pseudoPeriodHeight,
            (0, 2, 0, 0), (2, 3, 3, 3));

    // The same function written over the base sequence [0,5), so that it can carry period 4 from T = 1 or period 2 from T = 3.
    private static Curve StepsOverFiveUnits(Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight, Rational valueAtFour)
        => Pieces(pseudoPeriodStart, pseudoPeriodLength, pseudoPeriodHeight,
            (0, 2, 0, 0), (2, 3, 3, 3), (3, 4, 3, 3), (4, 5, valueAtFour, 5));

    // In each pair below, what tells the two curves apart happens only after both transients end.
    // Cases: the first curve, the second, whether the first is below the second, whether the second is below the first.
    public static IEnumerable<object[]> LatePairs()
    {
        // Period 3 from T = 2: t + 3 on [0,2), then t + 2, t + 1, t + 2 on the three unit stretches of a period.
        var upper = Pieces(2, 3, 3, (0, 2, 3, 3), (2, 3, 4, 4), (3, 4, 4, 4), (4, 5, 6, 6));
        // As upper, but t + 1/2 on the middle stretch of a period.
        var crossing = Pieces(2, 3, 3, (0, 2, 3, 3), (2, 3, 4, 4), (3, 4, new Rational(7, 2), new Rational(7, 2)), (4, 5, 6, 6));

        // Steps (period 2 from T = 1) sits below upper everywhere and meets it only on [6,7], in the third period of upper
        yield return Case(Steps(1, 2, 2), upper, true, false);
        // Steps is above crossing only on [6,7], so the two cross at t = 6, after both transients and before t = 8
        yield return Case(Steps(1, 2, 2), crossing, false, false);
        // the same function over period 2 from T = 1 and over period 4 from T = 1
        yield return Case(Steps(1, 2, 2), StepsOverFiveUnits(1, 4, 4, 5), true, true);
        // the same function with the first period unrolled into the transient, period 2 from T = 3
        yield return Case(Steps(1, 2, 2), StepsOverFiveUnits(3, 2, 2, 5), true, true);
        // equal except at t = 4, where the period-4 encoding takes 4 instead of 5, and so again at t = 8, 12, ...
        yield return Case(StepsOverFiveUnits(1, 4, 4, 4), Steps(1, 2, 2), true, false);

        static object[] Case(Curve a, Curve b, bool aBelowB, bool bBelowA)
            => [a, b, aBelowB, bBelowA];
    }

    [Theory]
    [MemberData(nameof(LatePairs))]
    public void ExpressionsOfCurvesDifferingOnlyAfterTheirTransientsAreOrderedAccordingly(
        Curve a, Curve b, bool aBelowB, bool bBelowA)
    {
        var expressionA = a.ToExpression("a");
        var expressionB = b.ToExpression("b");

        Assert.Equal(aBelowB, expressionA <= expressionB);
        Assert.Equal(bBelowA, expressionA >= expressionB);
        Assert.Equal(bBelowA, expressionB <= expressionA);
        Assert.Equal(aBelowB, expressionB >= expressionA);
    }
}
