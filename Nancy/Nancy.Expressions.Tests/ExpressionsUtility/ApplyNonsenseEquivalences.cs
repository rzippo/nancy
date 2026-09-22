using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Equivalences;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

/// <summary>
/// Exercises the equivalence machinery at shapes the real catalogue never reaches.
/// Each law here is deliberately false: it is a probe, not a theorem, and it must never be read as mathematics.
/// Only the structure of the result is asserted, since a false law has no value to preserve.
/// </summary>
public class ApplyNonsenseEquivalences
{
    private static readonly Curve A = new RateLatencyServiceCurve(1, 2);
    private static readonly Curve B = new RateLatencyServiceCurve(2, 4);

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static CurveExpression Be => Expressions.FromCurve(B, "b");

    // Not a theorem: the left side is a bare placeholder, which matches any curve node.
    private static Equivalence NonsenseLaw_BarePlaceholderBecomesNegated()
        => new(Expressions.Placeholder("f"), Expressions.Negate(Expressions.Placeholder("f")));

    // Not a theorem: the left side is three levels deep, past the two the real laws use.
    private static Equivalence NonsenseLaw_ThreeLevelsCollapse()
        => new(Expressions.Negate(Expressions.Negate(Expressions.Placeholder("f"))),
            Expressions.Placeholder("f"));

    // Not a theorem: a unary left side is replaced by an n-ary one.
    private static Equivalence NonsenseLaw_UnaryBecomesNAry()
        => new(Expressions.SubAdditiveClosure(Expressions.Placeholder("f")),
            Expressions.Convolution([Expressions.Placeholder("f"), Expressions.Placeholder("f")]));

    // Not a theorem: it reaches a pseudo-inverse node, which no real law mentions.
    private static Equivalence NonsenseLaw_PseudoInverseIsDropped()
        => new(Expressions.LowerPseudoInverse(Expressions.Placeholder("f")),
            Expressions.Placeholder("f"));

    // Not a theorem: the placeholder occurs twice, so its two bindings have to agree.
    private static Equivalence NonsenseLaw_RepeatedPlaceholder()
        => new(Expressions.Subtraction(Expressions.Placeholder("f"), Expressions.Placeholder("f")),
            Expressions.Placeholder("f"));

    // Not a theorem: the right side drops a placeholder the left side bound.
    private static Equivalence NonsenseLaw_DropsABoundPlaceholder()
        => new(Expressions.Subtraction(Expressions.Placeholder("f"), Expressions.Placeholder("g")),
            Expressions.Placeholder("f"));

    public static IEnumerable<object[]> Applications()
    {
        yield return Case("a bare placeholder matches the whole curve expression",
            () => Expressions.Subtraction(Ae, Be).ApplyEquivalence(NonsenseLaw_BarePlaceholderBecomesNegated()),
            () => Expressions.Negate(Expressions.Subtraction(Ae, Be)));

        yield return Case("a left side three levels deep matches",
            () => Expressions.Negate(Expressions.Negate(Ae)).ApplyEquivalence(NonsenseLaw_ThreeLevelsCollapse()),
            () => Ae);

        yield return Case("a unary left side is rewritten as an n-ary",
            () => Expressions.SubAdditiveClosure(Ae).ApplyEquivalence(NonsenseLaw_UnaryBecomesNAry()),
            () => Expressions.Convolution([Ae, Ae]));

        yield return Case("a left side over a pseudo-inverse matches",
            () => Expressions.LowerPseudoInverse(Ae).ApplyEquivalence(NonsenseLaw_PseudoInverseIsDropped()),
            () => Ae);

        yield return Case("a repeated placeholder binds consistently",
            () => Expressions.Subtraction(Ae, Ae).ApplyEquivalence(NonsenseLaw_RepeatedPlaceholder()),
            () => Ae);

        yield return Case("a repeated placeholder with different bindings matches nothing",
            () => Expressions.Subtraction(Ae, Be).ApplyEquivalence(NonsenseLaw_RepeatedPlaceholder()),
            () => Expressions.Subtraction(Ae, Be));

        yield return Case("a right side drops a placeholder the left side bound",
            () => Expressions.Subtraction(Ae, Be).ApplyEquivalence(NonsenseLaw_DropsABoundPlaceholder()),
            () => Ae);

        static object[] Case(string name, Func<IExpression> apply, Func<IExpression> expected)
            => [name, apply, expected];
    }

    [Theory]
    [MemberData(nameof(Applications))]
    public void TheLawRewritesTheStructureItReaches(
        string name,
        Func<IExpression> apply,
        Func<IExpression> expected)
    {
        _ = name;

        Assert.Equal(expected().ToUnicodeString(), apply().ToUnicodeString());
    }
}
