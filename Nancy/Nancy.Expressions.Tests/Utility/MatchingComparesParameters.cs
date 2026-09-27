using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Utility;

/// <summary>
/// A node matches another of its operator only when the parameters it holds besides its operands agree as well: a cut's interval, a concatenation's flags, a subtraction's non-negative flag.
/// The same pairs with equal parameters are the controls, which do match.
/// </summary>
public class MatchingComparesParameters
{
    private static readonly Curve C = new RateLatencyServiceCurve(1, 2);
    private static readonly Curve D = new RateLatencyServiceCurve(2, 1);
    private static readonly Sequence SequenceA = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence SequenceB = new([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);

    private static CurveExpression Ce => Expressions.FromCurve(C, "c");
    private static CurveExpression De => Expressions.FromCurve(D, "d");
    private static SequenceExpression Sa => SequenceA.ToExpression("a");
    private static SequenceExpression Sb => SequenceB.ToExpression("b");

    private static SequenceExpression Cut(Rational upper) => Ce.Cut(new Interval(0, upper, true, true));

#pragma warning disable CS0618
    private static CurveExpression NonNegativeDifference => Ce.Subtraction(De, true);
#pragma warning restore CS0618

    public static IEnumerable<object[]> Substitutions()
    {
        yield return Case("a cut over another interval", () => Cut(3).Addition(Sa), Cut(5), Sb, false);
        yield return Case("a cut over the same interval", () => Cut(3).Addition(Sa), Cut(3), Sb, true);
        yield return Case("a concatenation with other flags", () => Sa.Concat(Sb, preserveDelay: true), Sa.Concat(Sb), Sa, false);
        yield return Case("a concatenation with the same flags", () => Sa.Concat(Sb, preserveDelay: true), Sa.Concat(Sb, preserveDelay: true), Sa, true);
        yield return Case("a subtraction with another non-negative flag", () => NonNegativeDifference, Ce.Subtraction(De), Ce, false);
        yield return Case("a subtraction with the same non-negative flag", () => NonNegativeDifference, NonNegativeDifference, Ce, true);

        static object[] Case(string name, Func<IExpression> host, IExpression pattern, IExpression replacement, bool matches)
            => [name, host, pattern, replacement, matches];
    }

    [Theory]
    [MemberData(nameof(Substitutions))]
    public void ASubstitutionMatchesOnlyEqualParameters(string name, Func<IExpression> host, IExpression pattern, IExpression replacement, bool matches)
    {
        _ = name;
        var result = host() switch
        {
            CurveExpression curve => curve.ReplaceByValueWithResult((CurveExpression)pattern, (CurveExpression)replacement),
            SequenceExpression sequence when pattern is SequenceExpression p => sequence.ReplaceByValueWithResult(p, (SequenceExpression)replacement),
            _ => throw new ArgumentException("Unexpected case.")
        };

        Assert.Equal(matches, result.Matched);
    }

    [Fact]
    public void AnEquivalenceOverASubtractionDoesNotApplyToItsNonNegativePart()
    {
        var equivalence = new Equivalence(
            Expressions.Subtraction(Expressions.Placeholder("f"), Expressions.Placeholder("g")),
            Expressions.Placeholder("f"));

        Assert.False(NonNegativeDifference.ApplyEquivalenceWithResult(equivalence).Matched);
        Assert.True(Ce.Subtraction(De).ApplyEquivalenceWithResult(equivalence).Matched);
    }
}
