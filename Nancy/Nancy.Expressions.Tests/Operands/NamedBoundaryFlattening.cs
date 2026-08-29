using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class NamedBoundaryFlattening
{
    public delegate CurveExpression CurveOp(CurveExpression left, CurveExpression right);
    public delegate RationalExpression RationalOp(RationalExpression left, RationalExpression right);

    public static IEnumerable<object[]> CurveOperators()
    {
        yield return new object[] { (CurveOp)((l, r) => l.Addition(r)) };
        yield return new object[] { (CurveOp)((l, r) => l.Minimum(r)) };
        yield return new object[] { (CurveOp)((l, r) => l.Maximum(r)) };
        yield return new object[] { (CurveOp)((l, r) => l.Convolution(r)) };
        yield return new object[] { (CurveOp)((l, r) => l.MaxPlusConvolution(r)) };
    }

    public static IEnumerable<object[]> RationalOperators()
    {
        yield return new object[] { (RationalOp)((l, r) => l.Addition(r)) };
        yield return new object[] { (RationalOp)((l, r) => l.Product(r)) };
        yield return new object[] { (RationalOp)((l, r) => l.Min(r)) };
        yield return new object[] { (RationalOp)((l, r) => l.Max(r)) };
        yield return new object[] { (RationalOp)((l, r) => l.GreatestCommonDivisor(r)) };
        yield return new object[] { (RationalOp)((l, r) => l.LeastCommonMultiple(r)) };
    }

    // A reassignment chain, x := x <op> a; x := x <op> b; x := x <op> c, must never grow past two operands.
    // The stored x is an opaque value the caller has bound, and its own structure stays its own.
    [Theory]
    [MemberData(nameof(CurveOperators))]
    public void ReassigningANamedCurveVariableDoesNotGrowPastTwoOperands(CurveOp op)
    {
        CurveExpression x = Expressions.FromCurve(new ConstantCurve(1), "seed").WithName("x");
        foreach (var value in new[] { 2, 3, 4 })
        {
            x = op(x, Expressions.FromCurve(new ConstantCurve(value))).WithName("x");
            Assert.Equal(2, ((CurveNAryExpression)x).Operands.Count);
        }
    }

    [Theory]
    [MemberData(nameof(RationalOperators))]
    public void ReassigningANamedRationalVariableDoesNotGrowPastTwoOperands(RationalOp op)
    {
        RationalExpression x = Expressions.FromRational(1, "seed").WithName("x");
        foreach (var value in new Rational[] { 2, 3, 4 })
        {
            x = op(x, Expressions.FromRational(value)).WithName("x");
            Assert.Equal(2, ((RationalNAryExpression)x).Operands.Count);
        }
    }

    // Combining two already-named results must not reach into either one's own operand list.
    [Theory]
    [MemberData(nameof(CurveOperators))]
    public void CombiningTwoNamedCurveResultsDoesNotMergeEitherHistory(CurveOp op)
    {
        var a = Expressions.FromCurve(new ConstantCurve(1), "a");
        var b = Expressions.FromCurve(new ConstantCurve(2), "b");
        var c = Expressions.FromCurve(new ConstantCurve(3), "c");
        var d = Expressions.FromCurve(new ConstantCurve(4), "d");
        var p1 = op(a, b).WithName("p1");
        var p2 = op(c, d).WithName("p2");

        var total = (CurveNAryExpression)op(p1, p2);

        Assert.Equal(2, total.Operands.Count);
        Assert.Same(p1, total.Operands.ElementAt(0));
        Assert.Same(p2, total.Operands.ElementAt(1));
    }

    [Theory]
    [MemberData(nameof(RationalOperators))]
    public void CombiningTwoNamedRationalResultsDoesNotMergeEitherHistory(RationalOp op)
    {
        var a = Expressions.FromRational(1, "a");
        var b = Expressions.FromRational(2, "b");
        var c = Expressions.FromRational(3, "c");
        var d = Expressions.FromRational(4, "d");
        var p1 = op(a, b).WithName("p1");
        var p2 = op(c, d).WithName("p2");

        var total = (RationalNAryExpression)op(p1, p2);

        Assert.Equal(2, total.Operands.Count);
        Assert.Same(p1, total.Operands.ElementAt(0));
        Assert.Same(p2, total.Operands.ElementAt(1));
    }

    // Regression guard: a single statement combining ten anonymous terms is unaffected by the fix, none of its intermediate terms being independently named.
    [Theory]
    [MemberData(nameof(CurveOperators))]
    public void AWideCombinationOfAnonymousCurveTermsStillFlattens(CurveOp op)
    {
        CurveExpression acc = Expressions.FromCurve(new ConstantCurve(0));
        for (var i = 1; i <= 9; i++)
            acc = op(acc, Expressions.FromCurve(new ConstantCurve(i)));

        Assert.Equal(10, ((CurveNAryExpression)acc).Operands.Count);
    }

    [Theory]
    [MemberData(nameof(RationalOperators))]
    public void AWideCombinationOfAnonymousRationalTermsStillFlattens(RationalOp op)
    {
        RationalExpression acc = Expressions.FromRational(0);
        for (var i = 1; i <= 9; i++)
            acc = op(acc, Expressions.FromRational(i));

        Assert.Equal(10, ((RationalNAryExpression)acc).Operands.Count);
    }

    // Associativity makes the fix structure-only.
    // Two representative operators, checked that the reassignment chain computes the same value as a direct wide combination.
    [Fact]
    public void CurveAdditionReassignmentChainComputesTheSameValueAsADirectWideSum()
    {
        CurveExpression x = Expressions.FromCurve(new ConstantCurve(1)).WithName("x");
        x = x.Addition(Expressions.FromCurve(new ConstantCurve(2))).WithName("x");
        x = x.Addition(Expressions.FromCurve(new ConstantCurve(3))).WithName("x");

        var direct = Curve.Addition(new List<Curve> { new ConstantCurve(1), new ConstantCurve(2), new ConstantCurve(3) });
        Assert.Equal(direct, x.Compute());
    }

    [Fact]
    public void RationalAdditionReassignmentChainComputesTheSameValueAsADirectWideSum()
    {
        RationalExpression x = Expressions.FromRational(1).WithName("x");
        x = x.Addition(Expressions.FromRational(2)).WithName("x");
        x = x.Addition(Expressions.FromRational(3)).WithName("x");

        Assert.Equal(new Rational(6), x.Compute());
    }
}
