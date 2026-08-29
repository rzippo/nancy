using System.Collections.Generic;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class IfKnownAccessors
{
    public delegate bool? Peek(CurveExpression e);
    public delegate bool Force(CurveExpression e);

    public static IEnumerable<object[]> Predicates()
    {
        yield return new object[] { (Peek)(e => e.SubAdditiveIfKnown), (Force)(e => e.IsSubAdditive) };
        yield return new object[] { (Peek)(e => e.SuperAdditiveIfKnown), (Force)(e => e.IsSuperAdditive) };
        yield return new object[] { (Peek)(e => e.LeftContinuousIfKnown), (Force)(e => e.IsLeftContinuous) };
        yield return new object[] { (Peek)(e => e.RightContinuousIfKnown), (Force)(e => e.IsRightContinuous) };
        yield return new object[] { (Peek)(e => e.NonNegativeIfKnown), (Force)(e => e.IsNonNegative) };
        yield return new object[] { (Peek)(e => e.NonDecreasingIfKnown), (Force)(e => e.IsNonDecreasing) };
        yield return new object[] { (Peek)(e => e.IncreasingIfKnown), (Force)(e => e.IsIncreasing) };
        yield return new object[] { (Peek)(e => e.ConcaveIfKnown), (Force)(e => e.IsConcave) };
        yield return new object[] { (Peek)(e => e.ConvexIfKnown), (Force)(e => e.IsConvex) };
        yield return new object[] { (Peek)(e => e.PassingThroughOriginIfKnown), (Force)(e => e.IsPassingThroughOrigin) };
        yield return new object[] { (Peek)(e => e.UltimatelyFiniteIfKnown), (Force)(e => e.IsUltimatelyFinite) };
        yield return new object[] { (Peek)(e => e.PlainIfKnown), (Force)(e => e.IsPlain) };
        yield return new object[] { (Peek)(e => e.UltimatelyPlainIfKnown), (Force)(e => e.IsUltimatelyPlain) };
        yield return new object[] { (Peek)(e => e.UltimatelyAffineIfKnown), (Force)(e => e.IsUltimatelyAffine) };
        yield return new object[] { (Peek)(e => e.UltimatelyConstantIfKnown), (Force)(e => e.IsUltimatelyConstant) };
        yield return new object[] { (Peek)(e => e.WellDefinedIfKnown), (Force)(e => e.IsWellDefined) };
    }

    [Theory]
    [MemberData(nameof(Predicates))]
    public void IsNullUntilForcedThenMatchesTheForcedValue(Peek peek, Force force)
    {
        CurveExpression expression = Expressions.FromCurve(new ConstantCurve(1));

        Assert.Null(peek(expression));

        var forced = force(expression);

        Assert.Equal(forced, peek(expression));
    }
}
