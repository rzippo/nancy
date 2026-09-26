using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Nodes;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.Utility;

/// <summary>
/// An expression built over placeholders is a parametrized expression, and <c>ReplaceByValue</c> is how it is instantiated.
/// Each placeholder is replaced by the expression standing in for it.
/// <see cref="Nancy.Expressions.Equivalences.EquivalenceApplier"/> instantiates the side of an
/// equivalence it substitutes in exactly this way, so every equivalence runs through this path.
/// </summary>
public class ParametrizedExpressions
{
    private static readonly Curve A = new SigmaRhoArrivalCurve(1, 1);
    private static readonly Curve B = new SigmaRhoArrivalCurve(2, 2);
    private static readonly Sequence SequenceA = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence SequenceB = new([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static CurveExpression Be => Expressions.FromCurve(B, "b");
    private static CurveExpression F => Expressions.Placeholder("f");
    private static CurveExpression G => Expressions.Placeholder("g");
    private static RationalExpression K => Expressions.RationalPlaceholder("k");
    private static RationalExpression Two => Expressions.FromRational(new Rational(2), "two");
    private static SequenceExpression Sa => SequenceA.ToExpression("a");
    private static SequenceExpression Sb => SequenceB.ToExpression("b");
    private static SequenceExpression S => Expressions.SequencePlaceholder("s");

    public static IEnumerable<object[]> Instantiations()
    {
        yield return Case("a curve placeholder under a curve binary",
            () => Expressions.Subtraction(F, Be).ReplaceByValue(F, Ae, true),
            () => Expressions.Subtraction(Ae, Be));

        yield return Case("a curve placeholder under a curve unary",
            () => Expressions.SubAdditiveClosure(F).ReplaceByValue(F, Ae, true),
            () => Expressions.SubAdditiveClosure(Ae));

        yield return Case("a curve placeholder under a curve n-ary",
            () => Expressions.Convolution([F, Be]).ReplaceByValue(F, Ae, true),
            () => Expressions.Convolution([Ae, Be]));

        yield return Case("the same placeholder in two positions",
            () => Expressions.Subtraction(F, F).ReplaceByValue(F, Ae, true),
            () => Expressions.Subtraction(Ae, Ae));

        yield return Case("a rational placeholder under a curve binary",
            () => Expressions.Scale(Ae, K).ReplaceByValue(K, Two, true),
            () => Expressions.Scale(Ae, Two));

        yield return Case("one placeholder of two, leaving the expression parametrized",
            () => Expressions.Subtraction(F, G).ReplaceByValue(F, Ae, true),
            () => Expressions.Subtraction(Ae, G));

        yield return Case("a placeholder replaced by an expression that is itself parametrized",
            () => Expressions.SubAdditiveClosure(F).ReplaceByValue(F, Expressions.Convolution([G, Be]), true),
            () => Expressions.SubAdditiveClosure(Expressions.Convolution([G, Be])));

        yield return Case("a placeholder that does not occur",
            () => Expressions.Subtraction(F, Be).ReplaceByValue(G, Ae, true),
            () => Expressions.Subtraction(F, Be));

        // A parametrized expression whose value is a Rational, such as a bound expressed over placeholders.
        yield return Case("a curve placeholder under a rational binary",
            () => Expressions.HorizontalDeviation(F, Be).ReplaceByValue(F, Ae, true),
            () => Expressions.HorizontalDeviation(Ae, Be));

        yield return Case("a curve placeholder under a rational unary",
            () => Expressions.MaxValue(F).ReplaceByValue(F, Ae, true),
            () => Expressions.MaxValue(Ae));

        yield return Case("a rational placeholder under a rational binary",
            () => Expressions.RationalSubtraction(K, Two).ReplaceByValue(K, Two, true),
            () => Expressions.RationalSubtraction(Two, Two));

        // A curve expression holding a rational node, the shape the side of an equivalence may take.
        yield return Case("a curve placeholder under a rational node inside a curve expression",
            () => Expressions.DelayBy(Ae, Expressions.HorizontalDeviation(F, Be))
                .ReplaceByValue(F, Ae, true),
            () => Expressions.DelayBy(Ae, Expressions.HorizontalDeviation(Ae, Be)));

        // A sequence placeholder, instantiated the only way a parametrized sequence expression can be.

        yield return Case("a sequence placeholder under a sequence unary",
            () => S.Negate().ReplaceByValue(S, Sa, true),
            () => Sa.Negate());

        yield return Case("a sequence placeholder under a sequence binary",
            () => S.Subtraction(Sb).ReplaceByValue(S, Sa, true),
            () => Sa.Subtraction(Sb));

        yield return Case("a sequence placeholder under a sequence n-ary",
            () => S.Addition(Sb).ReplaceByValue(S, Sa, true),
            () => Sa.Addition(Sb));

        static object[] Case(string name, Func<IExpression> instantiate, Func<IExpression> expected)
            => [name, instantiate, expected];
    }

    [Theory]
    [MemberData(nameof(Instantiations))]
    public void ThePlaceholderIsReplacedByTheExpressionStandingForIt(
        string name,
        Func<IExpression> instantiate,
        Func<IExpression> expected)
    {
        _ = name;

        Assert.Equal(expected().ToUnicodeString(), instantiate().ToUnicodeString());
    }

    /// <summary>
    /// A placeholder cannot be computed, so an expression that still holds one cannot either.
    /// Substituting every placeholder is what turns the parametrized expression into a value.
    /// </summary>
    [Fact]
    public void AFullyInstantiatedExpressionComputes()
    {
        var parametrized = Expressions.Convolution([F, G]);
        Assert.Throws<InvalidOperationException>(() => parametrized.Compute());

        var instantiated = parametrized
            .ReplaceByValue(F, Ae, true)
            .ReplaceByValue(G, Be, true);

        Assert.True(A.Convolution(B).Equivalent(instantiated.Compute()));
    }

    [Fact]
    public void APartlyInstantiatedExpressionCannotBeComputed()
    {
        var parametrized = Expressions.Convolution([F, G]);

        var instantiated = parametrized.ReplaceByValue(F, Ae, true);

        Assert.Throws<InvalidOperationException>(() => instantiated.Compute());
    }
}
