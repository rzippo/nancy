using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Pseudo-inverses from height 0 of a sequence whose every value is negative.
/// At height 0, no instant has $f(t) \ge 0$, so the lower one is $\inf \emptyset = +\infty$,
/// and every instant has $f(t) \le 0$, so the upper one is the end of the support.
/// </summary>
public class PseudoInversesFromZeroOfNegativeSequences
{
    public static IEnumerable<(string Name, Sequence Source, Rational SupportEnd)> Sources()
    {
        yield return ("rising, closed end", new Sequence([
            new Point(0, -2), new Segment(0, 1, -2, 1), new Point(1, -1)
        ]), 1);
        yield return ("rising to 0 at an open end", new Sequence([
            new Point(0, -1), new Segment(0, 1, -1, 1)
        ]), 1);
        yield return ("constant", new Sequence([
            new Point(0, -1), Segment.Constant(0, 2, -1), new Point(2, -1)
        ]), 2);
    }

    public static IEnumerable<object[]> Cases() => Sources().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void LowerIsPlusInfiniteAtZero(string name, Sequence source, Rational supportEnd)
    {
        var expected = new Sequence([Point.PlusInfinite(0)]);
        Assert.True(Sequence.Equivalent(expected, source.LowerPseudoInverse(startFromZero: true)), name);
        Assert.True(Sequence.Equivalent(expected, source.Elements.LowerPseudoInverse(startFromZero: true).ToSequence()), name);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void UpperIsTheSupportEndAtZero(string name, Sequence source, Rational supportEnd)
    {
        var expected = new Sequence([new Point(0, supportEnd)]);
        Assert.True(Sequence.Equivalent(expected, source.UpperPseudoInverse(startFromZero: true)), name);
        Assert.True(Sequence.Equivalent(expected, source.Elements.UpperPseudoInverse(startFromZero: true).ToSequence()), name);
    }
}
