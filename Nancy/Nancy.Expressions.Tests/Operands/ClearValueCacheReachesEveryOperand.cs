using System.Collections.Generic;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

/// <summary>
/// Clearing a subtree reaches every operand, whatever the shape of the node and whichever tree the operand belongs to.
/// The operands checked are curves and sequences, since a rational node always keeps its own cache.
/// </summary>
public class ClearValueCacheReachesEveryOperand
{
    private static readonly ExpressionSettings NotCheap =
        new() { CacheSettings = new CacheSettings { CheapCacheElementThreshold = 0 } };

    private static readonly Sequence SeqA = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence SeqB = new([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);

    private static CurveExpression CompositeCurve(string name)
        => new ConcreteCurveExpression(new RateLatencyServiceCurve(2, 1), name + "_l")
            .Addition(new ConcreteCurveExpression(new RateLatencyServiceCurve(1, 2), name + "_r"), settings: NotCheap);

    private static SequenceExpression CompositeSequence(string name)
        => SeqA.ToExpression(name + "_a").Addition(SeqB.ToExpression(name + "_b"), settings: NotCheap);

    public static IEnumerable<object[]> Cases()
    {
        {
            var operand = CompositeCurve("cu");
            yield return new object[]
            {
                "curve unary", Expressions.SubAdditiveClosure(operand, settings: NotCheap), new IExpression[] { operand },
            };
        }
        {
            var left = CompositeCurve("cb_l");
            var right = CompositeCurve("cb_r");
            yield return new object[]
            {
                "curve binary", Expressions.Deconvolution(left, right, settings: NotCheap),
                new IExpression[] { left, right },
            };
        }
        {
            var a = CompositeCurve("cn_a");
            var b = CompositeCurve("cn_b");
            var c = CompositeCurve("cn_c");
            yield return new object[]
            {
                "curve n-ary", a.Convolution(b, settings: NotCheap).Convolution(c, settings: NotCheap),
                new IExpression[] { a, b, c },
            };
        }
        {
            var curve = CompositeCurve("cbr_c");
            var factor = Expressions.FromRational(new Rational(2), "factor");
            yield return new object[]
            {
                "curve binary with a rational operand", Expressions.VerticalShift(curve, factor, settings: NotCheap),
                new IExpression[] { curve },
            };
        }
        {
            var left = CompositeCurve("rb_l");
            var right = CompositeCurve("rb_r");
            yield return new object[]
            {
                "rational binary over curves", Expressions.HorizontalDeviation(left, right, settings: NotCheap),
                new IExpression[] { left, right },
            };
        }
        {
            var a = CompositeCurve("rn_a");
            var b = CompositeCurve("rn_b");
            var c = CompositeCurve("rn_c");
            var d = CompositeCurve("rn_d");
            var left = Expressions.HorizontalDeviation(a, b, settings: NotCheap);
            var right = Expressions.HorizontalDeviation(c, d, settings: NotCheap);
            yield return new object[]
            {
                "rational n-ary", Expressions.RationalAddition(left, right, settings: NotCheap),
                new IExpression[] { a, b, c, d },
            };
        }
        {
            var operand = CompositeSequence("su");
            yield return new object[]
            {
                "sequence unary", operand.Negate(settings: NotCheap), new IExpression[] { operand },
            };
        }
        {
            var left = CompositeSequence("sb_l");
            var right = CompositeSequence("sb_r");
            yield return new object[]
            {
                "sequence binary", left.Deconvolution(right, settings: NotCheap), new IExpression[] { left, right },
            };
        }
        {
            var a = CompositeSequence("sn_a");
            var b = CompositeSequence("sn_b");
            var c = CompositeSequence("sn_c");
            yield return new object[]
            {
                "sequence n-ary", a.Convolution(b, settings: NotCheap).Convolution(c, settings: NotCheap),
                new IExpression[] { a, b, c },
            };
        }
        {
            var curve = CompositeCurve("cut");
            yield return new object[]
            {
                "sequence over curve", curve.Cut(new Interval(0, 5, true, true), settings: NotCheap),
                new IExpression[] { curve },
            };
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void ClearingReachesEveryOperand(string name, IExpression root, IExpression[] operands)
    {
        foreach (var operand in operands)
            operand.ComputeWithoutResult();
        root.ComputeWithoutResult();

        root.ClearValueCache(CacheClearScope.Subtree);

        foreach (var operand in operands)
            Assert.False(operand.IsComputed, name);
    }
}
