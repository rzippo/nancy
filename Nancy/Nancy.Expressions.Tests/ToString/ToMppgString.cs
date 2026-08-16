using System.Collections.Generic;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

[TestSubject(typeof(Nancy.Expressions.Visitors.MppgFormatterVisitor))]
public class ToMppgString
{
    private static CurveExpression A => Expressions.FromCurve(new SigmaRhoArrivalCurve(1, 2), "a");

    private static CurveExpression B => Expressions.FromCurve(new RateLatencyServiceCurve(2, 5), "b");

    private static CurveExpression C => Expressions.FromCurve(new RateLatencyServiceCurve(3, 1), "c");

    private static RationalExpression X => Expressions.FromRational(3, "x");

    private static RationalExpression Y => Expressions.FromRational(new Rational(5, 2), "y");

    public static List<(IExpression expr, string mppg)> MppgFormattingExamples =
    [
        // leaves
        (A, "a"),
        (Expressions.FromCurve(new SigmaRhoArrivalCurve(1, 2), ""), "bucket(2, 1)"),
        (Expressions.FromCurve(new RateLatencyServiceCurve(2, 5), ""), "ratency(2, 5)"),
        (Expressions.FromRational(new Rational(5, 2)), "5/2"),

        // sum-level operators
        (Expressions.Addition(A, B), "a + b"),
        (Expressions.Addition(Expressions.Addition(A, B), C), "(a + b) + c"),
        (Expressions.Subtraction(A, B), "a - b"),
        (Expressions.Minimum(A, B), @"a /\ b"),
        (Expressions.Maximum(A, B), @"a \/ b"),

        // product-level operators
        (Expressions.Convolution(A, B), "a * b"),
        (Expressions.Convolution(Expressions.Convolution(A, B), C), "(a * b) * c"),
        (Expressions.Deconvolution(A, B), "a / b"),
        (Expressions.MaxPlusConvolution(A, B), "a *^ b"),
        (Expressions.MaxPlusDeconvolution(A, B), "a /^ b"),
        (Expressions.Composition(A, B), "a comp b"),

        // grouping, which the formatting style spells out even where precedence would not need it
        (Expressions.Convolution(Expressions.Addition(A, B), C), "(a + b) * c"),
        (Expressions.Addition(Expressions.Convolution(A, B), C), "(a * b) + c"),
        (Expressions.Addition(Expressions.Minimum(A, B), C), @"(a /\ b) + c"),
        (Expressions.Minimum(C, Expressions.Addition(A, B)), @"c /\ (a + b)"),
        (Expressions.Deconvolution(A, Expressions.Deconvolution(B, C)), "a / (b / c)"),
        (Expressions.Deconvolution(Expressions.Deconvolution(A, B), C), "(a / b) / c"),
        (Expressions.Composition(Expressions.Composition(A, B), C), "(a comp b) comp c"),
        (Expressions.Convolution(
            Expressions.Minimum(A, B).SubAdditiveClosure(),
            Expressions.Deconvolution(C, Expressions.Addition(A, B))),
            @"subaddclosure(a /\ b) * (c / (a + b))"),
        // the cases the playground's own reformat tests pin, up to the names
        (Expressions.Addition(Expressions.Subtraction(A, B), C), "(a - b) + c"),
        (Expressions.Addition(A, Expressions.Convolution(B, C)), "a + (b * c)"),
        // an n-ary node is flattened by the library and chained left-associatively when written
        (Expressions.Addition([A, B, C, A]), "((a + b) + c) + a"),
        (Expressions.Convolution([A, B, C]), "(a * b) * c"),
        (Expressions.Addition(Expressions.Convolution(A, B), Expressions.Convolution(C, A)), "(a * b) + (c * a)"),
        // a call delimits its arguments already, so they take no parentheses of their own
        (Expressions.Addition(A, B).SubAdditiveClosure(), "subaddclosure(a + b)"),
        (Expressions.HorizontalDeviation(A, B), "hDev(a, b)"),

        // unary operators
        (A.SubAdditiveClosure(), "subaddclosure(a)"),
        (A.SuperAdditiveClosure(), "superaddclosure(a)"),
        (Expressions.Addition(A, B).SubAdditiveClosure(), "subaddclosure(a + b)"),
        (A.ToUpperNonDecreasing(), "upclosure(a)"),
        (A.ToLowerNonDecreasing(), "lowclosure(a)"),
        (A.ToLeftContinuous(), "left-ext(a)"),
        (A.ToRightContinuous(), "right-ext(a)"),
        (A.LowerPseudoInverse(), "low_inv(a)"),
        (A.UpperPseudoInverse(), "up_inv(a)"),
        (A.Floor(), "floor(a)"),
        (A.Ceil(), "ceil(a)"),
        (A.Negate(), "-a"),
        (Expressions.Convolution(A, B).Negate(), "-(a * b)"),

        // the non-negative closures, which MPPG fuses into a single operator
        (A.ToNonNegative().ToUpperNonDecreasing(), "nnupclosure(a)"),
        (A.ToUpperNonDecreasing().ToNonNegative(), "nnupclosure(a)"),
        (A.ToNonNegative().ToLowerNonDecreasing(), "nnlowclosure(a)"),
        (A.ToLowerNonDecreasing().ToNonNegative(), "nnlowclosure(a)"),
        (A.ToNonNegative(), @"a \/ 0"),
        (Expressions.Addition(A, B).ToNonNegative(), @"(a + b) \/ 0"),
#pragma warning disable CS0618 // Type or member is obsolete
        (Expressions.Subtraction(A, B, nonNegative: true), @"(a - b) \/ 0"),
        (Expressions.Subtraction(A, B, nonNegative: false), "a - b"),
#pragma warning restore CS0618 // Type or member is obsolete

        // shifts and scaling, which MPPG spells through hShift and vShift
        (A.HorizontalShift(3), "hShift(a, 3)"),
        (A.HorizontalShift(-3), "hShift(a, -3)"),
        (A.DelayBy(4), "hShift(a, 4)"),
        (A.ForwardBy(4), "hShift(a, -4)"),
        (A.VerticalShift(3), "vShift(a, 3)"),
        (A.VerticalShift(-3), "vShift(a, -3)"),
        (A.Scale(3), "3 * a"),
        (A.Scale(new Rational(5, 2)), "(5/2) * a"),
        (Expressions.Addition(A, B).Scale(3), "3 * (a + b)"),

        // scalar operations
        (X + Y, "3 + 5/2"),
        (X - Y, "3 - 5/2"),
        (X * Y, "3 * (5/2)"),
        (X / Y, "3 / (5/2)"),
        ((X + Y) * X, "(3 + 5/2) * 3"),
        (X.Negate(), "-3"),
        (X.Invert(), "1/3"),
        ((Y - X).AbsoluteValue(), "abs(5/2 - 3)"),
        (Y.Floor(), "floor(5/2)"),
        (Y.Ceil(), "ceil(5/2)"),
        (new RationalModuloExpression(Expressions.FromRational(7), Expressions.FromRational(3)), "7 mod 3"),
        (new RationalPowerExpression(Expressions.FromRational(2), Expressions.FromRational(3)), "pow(2, 3)"),
        (Expressions.GreatestCommonDivisor(Expressions.FromRational(12), Expressions.FromRational(8)), "gcd(12, 8)"),
        (Expressions.LeastCommonMultiple(Expressions.FromRational(4), Expressions.FromRational(6)), "lcm(4, 6)"),
        (new RationalMinimumExpression([X, Y]), @"3 /\ 5/2"),
        (new RationalMaximumExpression([X, Y]), @"3 \/ 5/2"),
        (((X + Y) * (X - Y)).AbsoluteValue(), "abs((3 + 5/2) * (3 - 5/2))"),

        // scalar-returning operations on curves
        (A.ValueAt(3), "a(3)"),
        (A.LeftLimitAt(3), "a(3~-)"),
        (A.RightLimitAt(3), "a(3~+)"),
        (Expressions.HorizontalDeviation(A, B), "hDev(a, b)"),
        (Expressions.VerticalDeviation(A, B), "vDev(a, b)"),
        (Expressions.ZDeviation(A, B), "zDev(a, b)"),
    ];

    public static IEnumerable<object[]> MppgFormattingTestCases =
        MppgFormattingExamples.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MppgFormattingTestCases))]
    public void ToMppgString_MppgFormatting(IExpression expr, string mppg)
    {
        var exprToString = expr.ToMppgString();
        Assert.Equal(mppg, exprToString);
    }

    [Fact]
    public void ToMppgString_NamedSubExpression_IsNotExpandedPastDepth()
    {
        var inner = Expressions.Convolution(A, B, "k");
        var expr = Expressions.Addition(inner, C);

        Assert.Equal("k + c", expr.ToMppgString(depth: 1));
        Assert.Equal("(a * b) + c", expr.ToMppgString());
    }

    [Fact]
    public void ToMppgString_NameThatMppgCannotLex_FallsBackToTheValue()
    {
        var greek = Expressions.FromCurve(new RateLatencyServiceCurve(2, 5), "beta 1");
        var keyword = Expressions.FromCurve(new RateLatencyServiceCurve(2, 5), "floor");

        Assert.Equal("ratency(2, 5)", greek.ToMppgString());
        Assert.Equal("ratency(2, 5)", keyword.ToMppgString());
    }

    [Fact]
    public void ToMppgString_ShowRationalsAsName_UsesTheName()
    {
        var expr = A.VerticalShift(X);

        Assert.Equal("vShift(a, 3)", expr.ToMppgString());
        Assert.Equal("vShift(a, x)", expr.ToMppgString(showRationalsAsName: true));
    }
}
