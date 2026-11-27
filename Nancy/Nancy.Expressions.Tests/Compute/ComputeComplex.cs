using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// Tests for complex curve computations that combine multiple operators.
/// These tests verify that multi-step calculations produce the same results
/// whether computed through Nancy directly or through Nancy.Expressions.
/// </summary>
public class ComputeComplex
{
    [Fact]
    public void GuidolinPinaB1AT2pASAgreesWithNancy()
    {
        Curve Reference()
        {
            var P_T1 = new Rational(15);
            var P_T2 = new Rational(30);
            var P_T3 = new Rational(45);
            var P_T4 = new Rational(60);
            var P_T5 = new Rational(120);
            var C_T1 = new Rational(3);
            var C_T2 = new Rational(4);
            var C_T3 = new Rational(15);
            var C_T4 = new Rational(5);
            var C_T5 = new Rational(10);
            var C_T2p = new Rational(4);
            var C_T3p = new Rational(8);
            var C_T4p = new Rational(2);
            var C_T2pp = new Rational(2);
            var C_T3pp = new Rational(4);
            var C_T4pp = new Rational(1);
            var aT1 = new StairCurve(C_T1, P_T1).DelayBy(new Rational(0));
            var aT2 = new StairCurve(C_T2, P_T2).DelayBy(new Rational(0));
            var aT3 = new StairCurve(C_T3, P_T3).DelayBy(new Rational(0));
            var aT4 = new StairCurve(C_T4, P_T4).DelayBy(new Rational(0));
            var aT5 = new StairCurve(C_T5, P_T5).DelayBy(new Rational(0));
            var aT23 = aT2 + aT3;
            var beta_m = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1));
            var beta_M = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1));
            var sigma = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1));
            var b_r_T1 = beta_m;
            var b_r_T2 = (beta_m - aT1).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T3 = (beta_m - aT1 - aT2).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T4 = beta_m;
            var b_r_T5 = (beta_m - aT4).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T23 = (beta_m - aT1).ToNonNegative().ToUpperNonDecreasing();
            var delay_T1 = Curve.HorizontalDeviation(aT1, b_r_T1);
            var delay_T2 = Curve.HorizontalDeviation(aT2, b_r_T2);
            var delay_T3 = Curve.HorizontalDeviation(aT3, b_r_T3);
            var delay_T4 = Curve.HorizontalDeviation(aT4, b_r_T4);
            var delay_T5 = Curve.HorizontalDeviation(aT5, b_r_T5);
            var delay_T23 = Curve.HorizontalDeviation(aT23, b_r_T23);
            var aT2p_aS = Curve.Minimum(Curve.Minimum(( Curve.Deconvolution(( Curve.Convolution(aT2, beta_M) ), ( b_r_T2 - C_T2 )) ), ( sigma + C_T2 )), ( Curve.Deconvolution(aT2, new DelayServiceCurve(delay_T2)) ));
            return aT2p_aS;
        }

        Curve Expression()
        {
            var P_T1 = Expressions.FromRational(new Rational(15));
            var P_T2 = Expressions.FromRational(new Rational(30));
            var P_T3 = Expressions.FromRational(new Rational(45));
            var P_T4 = Expressions.FromRational(new Rational(60));
            var P_T5 = Expressions.FromRational(new Rational(120));
            var C_T1 = Expressions.FromRational(new Rational(3));
            var C_T2 = Expressions.FromRational(new Rational(4));
            var C_T3 = Expressions.FromRational(new Rational(15));
            var C_T4 = Expressions.FromRational(new Rational(5));
            var C_T5 = Expressions.FromRational(new Rational(10));
            var C_T2p = Expressions.FromRational(new Rational(4));
            var C_T3p = Expressions.FromRational(new Rational(8));
            var C_T4p = Expressions.FromRational(new Rational(2));
            var C_T2pp = Expressions.FromRational(new Rational(2));
            var C_T3pp = Expressions.FromRational(new Rational(4));
            var C_T4pp = Expressions.FromRational(new Rational(1));
            var aT1 = Expressions.FromCurve(new StairCurve((C_T1).Compute(), (P_T1).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT2 = Expressions.FromCurve(new StairCurve((C_T2).Compute(), (P_T2).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT3 = Expressions.FromCurve(new StairCurve((C_T3).Compute(), (P_T3).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT4 = Expressions.FromCurve(new StairCurve((C_T4).Compute(), (P_T4).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT5 = Expressions.FromCurve(new StairCurve((C_T5).Compute(), (P_T5).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT23 = aT2 + aT3;
            var beta_m = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1)), name: "affine");
            var beta_M = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1)), name: "affine");
            var sigma = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1)), name: "affine");
            var b_r_T1 = beta_m;
            var b_r_T2 = (beta_m - aT1).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T3 = (beta_m - aT1 - aT2).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T4 = beta_m;
            var b_r_T5 = (beta_m - aT4).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T23 = (beta_m - aT1).ToNonNegative().ToUpperNonDecreasing();
            var delay_T1 = Expressions.HorizontalDeviation(aT1, b_r_T1);
            var delay_T2 = Expressions.HorizontalDeviation(aT2, b_r_T2);
            var delay_T3 = Expressions.HorizontalDeviation(aT3, b_r_T3);
            var delay_T4 = Expressions.HorizontalDeviation(aT4, b_r_T4);
            var delay_T5 = Expressions.HorizontalDeviation(aT5, b_r_T5);
            var delay_T23 = Expressions.HorizontalDeviation(aT23, b_r_T23);
            var aT2p_aS = ( ( aT2.Convolution(beta_M) ).Deconvolution(( b_r_T2 - C_T2 )) ).Minimum(( sigma + C_T2 )).Minimum(( aT2.Deconvolution(Expressions.FromCurve(new DelayServiceCurve((delay_T2).Compute()), name: "delay")) ));
            return aT2p_aS.Compute();
        }

        Assert.True(Reference().Equivalent(Expression()));
    }

    [Fact]
    public void GuidolinPinaB1AT2pAgreesWithNancy()
    {
        Curve Reference()
        {
            var P_T1 = new Rational(15);
            var P_T2 = new Rational(30);
            var P_T3 = new Rational(45);
            var P_T4 = new Rational(60);
            var P_T5 = new Rational(120);
            var C_T1 = new Rational(3);
            var C_T2 = new Rational(4);
            var C_T3 = new Rational(15);
            var C_T4 = new Rational(5);
            var C_T5 = new Rational(10);
            var C_T2p = new Rational(4);
            var C_T3p = new Rational(8);
            var C_T4p = new Rational(2);
            var C_T2pp = new Rational(2);
            var C_T3pp = new Rational(4);
            var C_T4pp = new Rational(1);
            var aT1 = new StairCurve(C_T1, P_T1).DelayBy(new Rational(0));
            var aT2 = new StairCurve(C_T2, P_T2).DelayBy(new Rational(0));
            var aT3 = new StairCurve(C_T3, P_T3).DelayBy(new Rational(0));
            var aT4 = new StairCurve(C_T4, P_T4).DelayBy(new Rational(0));
            var aT5 = new StairCurve(C_T5, P_T5).DelayBy(new Rational(0));
            var aT23 = aT2 + aT3;
            var beta_m = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1));
            var beta_M = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1));
            var sigma = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1));
            var b_r_T1 = beta_m;
            var b_r_T2 = (beta_m - aT1).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T3 = (beta_m - aT1 - aT2).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T4 = beta_m;
            var b_r_T5 = (beta_m - aT4).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T23 = (beta_m - aT1).ToNonNegative().ToUpperNonDecreasing();
            var delay_T1 = Curve.HorizontalDeviation(aT1, b_r_T1);
            var delay_T2 = Curve.HorizontalDeviation(aT2, b_r_T2);
            var delay_T3 = Curve.HorizontalDeviation(aT3, b_r_T3);
            var delay_T4 = Curve.HorizontalDeviation(aT4, b_r_T4);
            var delay_T5 = Curve.HorizontalDeviation(aT5, b_r_T5);
            var delay_T23 = Curve.HorizontalDeviation(aT23, b_r_T23);
            var aT2p_aS = Curve.Minimum(Curve.Minimum(( Curve.Deconvolution(( Curve.Convolution(aT2, beta_M) ), ( b_r_T2 - C_T2 )) ), ( sigma + C_T2 )), ( Curve.Deconvolution(aT2, new DelayServiceCurve(delay_T2)) ));
            var aT3p_aS = Curve.Minimum(Curve.Minimum(( Curve.Deconvolution(( Curve.Convolution(aT3, beta_M) ), ( b_r_T2 - C_T3 )) ), ( sigma + C_T3 )), ( Curve.Deconvolution(aT3, new DelayServiceCurve(delay_T3)) ));
            var aT4p_aS = Curve.Minimum(Curve.Minimum(( Curve.Deconvolution(( Curve.Convolution(aT4, beta_M) ), ( b_r_T2 - C_T4 )) ), ( sigma + C_T4 )), ( Curve.Deconvolution(aT4, new DelayServiceCurve(delay_T4)) ));
            var aT23p_aS = Curve.Minimum(Curve.Minimum(( Curve.Deconvolution(( Curve.Convolution(aT23, beta_M) ), ( b_r_T23 - C_T2 - C_T3 )) ), ( sigma + C_T2 + C_T3 )), ( Curve.Deconvolution(aT23, new DelayServiceCurve(Rational.Min(delay_T2, delay_T3))) ));
            var s2 = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), C_T2p / C_T2) ]), 0, 1, C_T2p / C_T2);
            var s3 = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), C_T3p / C_T3) ]), 0, 1, C_T3p / C_T3);
            var s4 = new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), C_T4p / C_T4) ]), 0, 1, C_T4p / C_T4);
            var s23 = ( Curve.Convolution(s2, s3) );
            var aT2p = Curve.Composition(s2, aT2p_aS);
            return aT2p;
        }

        Curve Expression()
        {
            var P_T1 = Expressions.FromRational(new Rational(15));
            var P_T2 = Expressions.FromRational(new Rational(30));
            var P_T3 = Expressions.FromRational(new Rational(45));
            var P_T4 = Expressions.FromRational(new Rational(60));
            var P_T5 = Expressions.FromRational(new Rational(120));
            var C_T1 = Expressions.FromRational(new Rational(3));
            var C_T2 = Expressions.FromRational(new Rational(4));
            var C_T3 = Expressions.FromRational(new Rational(15));
            var C_T4 = Expressions.FromRational(new Rational(5));
            var C_T5 = Expressions.FromRational(new Rational(10));
            var C_T2p = Expressions.FromRational(new Rational(4));
            var C_T3p = Expressions.FromRational(new Rational(8));
            var C_T4p = Expressions.FromRational(new Rational(2));
            var C_T2pp = Expressions.FromRational(new Rational(2));
            var C_T3pp = Expressions.FromRational(new Rational(4));
            var C_T4pp = Expressions.FromRational(new Rational(1));
            var aT1 = Expressions.FromCurve(new StairCurve((C_T1).Compute(), (P_T1).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT2 = Expressions.FromCurve(new StairCurve((C_T2).Compute(), (P_T2).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT3 = Expressions.FromCurve(new StairCurve((C_T3).Compute(), (P_T3).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT4 = Expressions.FromCurve(new StairCurve((C_T4).Compute(), (P_T4).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT5 = Expressions.FromCurve(new StairCurve((C_T5).Compute(), (P_T5).Compute()).DelayBy(new Rational(0)), name: "stair");
            var aT23 = aT2 + aT3;
            var beta_m = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1)), name: "affine");
            var beta_M = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1)), name: "affine");
            var sigma = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), new Rational(1)) ]), 0, 1, new Rational(1)), name: "affine");
            var b_r_T1 = beta_m;
            var b_r_T2 = (beta_m - aT1).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T3 = (beta_m - aT1 - aT2).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T4 = beta_m;
            var b_r_T5 = (beta_m - aT4).ToNonNegative().ToUpperNonDecreasing();
            var b_r_T23 = (beta_m - aT1).ToNonNegative().ToUpperNonDecreasing();
            var delay_T1 = Expressions.HorizontalDeviation(aT1, b_r_T1);
            var delay_T2 = Expressions.HorizontalDeviation(aT2, b_r_T2);
            var delay_T3 = Expressions.HorizontalDeviation(aT3, b_r_T3);
            var delay_T4 = Expressions.HorizontalDeviation(aT4, b_r_T4);
            var delay_T5 = Expressions.HorizontalDeviation(aT5, b_r_T5);
            var delay_T23 = Expressions.HorizontalDeviation(aT23, b_r_T23);
            var aT2p_aS = ( ( aT2.Convolution(beta_M) ).Deconvolution(( b_r_T2 - C_T2 )) ).Minimum(( sigma + C_T2 )).Minimum(( aT2.Deconvolution(Expressions.FromCurve(new DelayServiceCurve((delay_T2).Compute()), name: "delay")) ));
            var aT3p_aS = ( ( aT3.Convolution(beta_M) ).Deconvolution(( b_r_T2 - C_T3 )) ).Minimum(( sigma + C_T3 )).Minimum(( aT3.Deconvolution(Expressions.FromCurve(new DelayServiceCurve((delay_T3).Compute()), name: "delay")) ));
            var aT4p_aS = ( ( aT4.Convolution(beta_M) ).Deconvolution(( b_r_T2 - C_T4 )) ).Minimum(( sigma + C_T4 )).Minimum(( aT4.Deconvolution(Expressions.FromCurve(new DelayServiceCurve((delay_T4).Compute()), name: "delay")) ));
            var aT23p_aS = ( ( aT23.Convolution(beta_M) ).Deconvolution(( b_r_T23 - C_T2 - C_T3 )) ).Minimum(( sigma + C_T2 + C_T3 )).Minimum(( aT23.Deconvolution(Expressions.FromCurve(new DelayServiceCurve((RationalExpression.Min(delay_T2, delay_T3)).Compute()), name: "delay")) ));
            var s2 = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), (C_T2p / C_T2).Compute()) ]), 0, 1, (C_T2p / C_T2).Compute()), name: "affine");
            var s3 = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), (C_T3p / C_T3).Compute()) ]), 0, 1, (C_T3p / C_T3).Compute()), name: "affine");
            var s4 = Expressions.FromCurve(new Curve(new Sequence([new Point(0, new Rational(0)), new Segment(0, 1, new Rational(0), (C_T4p / C_T4).Compute()) ]), 0, 1, (C_T4p / C_T4).Compute()), name: "affine");
            var s23 = ( s2.Convolution(s3) );
            var aT2p = s2.Composition(aT2p_aS);
            return aT2p.Compute();
        }

        Assert.True(Reference().Equivalent(Expression()));
    }

    [Theory]
    [MemberData(nameof(CurvePairData))]
    public void ToNonNegativeThenToUpperNonDecreasingAgreesWithNancy(Curve a, Curve b)
    {
        Curve Reference()
        {
            var intermediate = a.ToNonNegative();
            return intermediate.ToUpperNonDecreasing();
        }

        Curve Expression()
        {
            var aExp = Expressions.FromCurve(a);
            var bExp = Expressions.FromCurve(b);
            var intermediate = aExp.ToNonNegative();
            var result = intermediate.ToUpperNonDecreasing();
            return result.Compute();
        }

        Assert.True(Reference().Equivalent(Expression()));
    }

    [Theory]
    [MemberData(nameof(CurvePairData))]
    public void MinimumThenToNonNegativeAgreesWithNancy(Curve a, Curve b)
    {
        Curve Reference()
        {
            var intermediate = Curve.Minimum(a, b);
            return intermediate.ToNonNegative();
        }

        Curve Expression()
        {
            var intermediate = Expressions.Minimum(a, b);
            var result = intermediate.ToNonNegative();
            return result.Compute();
        }

        Assert.True(Reference().Equivalent(Expression()));
    }

    [Theory]
    [MemberData(nameof(CurvePairData))]
    public void DeconvolutionThenToUpperNonDecreasingAgreesWithNancy(Curve a, Curve b)
    {
        Curve Reference()
        {
            var intermediate = Curve.Deconvolution(a, b);
            return intermediate.ToUpperNonDecreasing();
        }

        Curve Expression()
        {
            var intermediate = Expressions.Deconvolution(a, b);
            var result = intermediate.ToUpperNonDecreasing();
            return result.Compute();
        }

        Assert.True(Reference().Equivalent(Expression()));
    }

    [Theory]
    [MemberData(nameof(CurvePairData))]
    public void ToNonNegativeThenMinimumAgreesWithNancy(Curve a, Curve b)
    {
        Curve Reference()
        {
            var intermediate = a.ToNonNegative();
            return Curve.Minimum(intermediate, b);
        }

        Curve Expression()
        {
            var aExp = Expressions.FromCurve(a);
            var bExp = Expressions.FromCurve(b);
            var intermediate = aExp.ToNonNegative();
            var result = Expressions.Minimum(intermediate, bExp);
            return result.Compute();
        }

        Assert.True(Reference().Equivalent(Expression()));
    }

    [Theory]
    [MemberData(nameof(CurvePairData))]
    public void MinimumThenToNonNegativeThenToUpperNonDecreasingAgreesWithNancy(Curve a, Curve b)
    {
        Curve Reference()
        {
            var min = Curve.Minimum(a, b);
            var nonNeg = min.ToNonNegative();
            return nonNeg.ToUpperNonDecreasing();
        }

        Curve Expression()
        {
            var aExp = Expressions.FromCurve(a);
            var bExp = Expressions.FromCurve(b);
            var min = Expressions.Minimum(aExp, bExp);
            var nonNeg = min.ToNonNegative();
            var result = nonNeg.ToUpperNonDecreasing();
            return result.Compute();
        }

        Assert.True(Reference().Equivalent(Expression()));
    }

    [Theory]
    [MemberData(nameof(RationalPairData))]
    public void MaximumThenDivisionAgreesWithNancy(Rational a, Rational b)
    {
        Rational Reference()
        {
            var maximum = Rational.Max(a, b);
            return maximum / new Rational(2);
        }

        Rational Expression()
        {
            var aExp = Expressions.FromRational(a);
            var bExp = Expressions.FromRational(b);
            var maximum = aExp.Max(bExp);
            var result = maximum / 2;
            return result.Compute();
        }

        Assert.Equal(Reference(), Expression());
    }

    [Theory]
    [MemberData(nameof(RationalPairData))]
    public void MinimumThenDivisionAgreesWithNancy(Rational a, Rational b)
    {
        Rational Reference()
        {
            var minimum = Rational.Min(a, b);
            return minimum / new Rational(3);
        }

        Rational Expression()
        {
            var aExp = Expressions.FromRational(a);
            var bExp = Expressions.FromRational(b);
            var minimum = aExp.Min(bExp);
            var result = minimum / 3;
            return result.Compute();
        }

        Assert.Equal(Reference(), Expression());
    }

    [Theory]
    [MemberData(nameof(RationalPairData))]
    public void DivisionThenMaximumAgreesWithNancy(Rational a, Rational b)
    {
        Rational Reference()
        {
            var divided = a / b;
            return Rational.Max(divided, new Rational(0));
        }

        Rational Expression()
        {
            var aExp = Expressions.FromRational(a);
            var bExp = Expressions.FromRational(b);
            var divided = aExp / bExp;
            var result = divided.Max(0);
            return result.Compute();
        }

        Assert.Equal(Reference(), Expression());
    }

    [Theory]
    [MemberData(nameof(RationalPairData))]
    public void MaximumThenGreatestCommonDivisorAgreesWithNancy(Rational a, Rational b)
    {
        Rational Reference()
        {
            var maximum = Rational.Max(a, b);
            return Rational.GreatestCommonDivisor(maximum, new Rational(6));
        }

        Rational Expression()
        {
            var aExp = Expressions.FromRational(a);
            var bExp = Expressions.FromRational(b);
            var maximum = aExp.Max(bExp);
            var result = maximum.GreatestCommonDivisor(6);
            return result.Compute();
        }

        Assert.Equal(Reference(), Expression());
    }

    [Theory]
    [MemberData(nameof(RationalPairData))]
    public void MinimumThenDivisionThenMaximumAgreesWithNancy(Rational a, Rational b)
    {
        Rational Reference()
        {
            var minimum = Rational.Min(a, b);
            var divided = minimum / new Rational(2);
            return Rational.Max(divided, new Rational(1));
        }

        Rational Expression()
        {
            var aExp = Expressions.FromRational(a);
            var bExp = Expressions.FromRational(b);
            var minimum = aExp.Min(bExp);
            var divided = minimum / 2;
            var result = divided.Max(1);
            return result.Compute();
        }

        Assert.Equal(Reference(), Expression());
    }

    [Theory]
    [MemberData(nameof(RationalPairData))]
    public void LeastCommonMultipleThenMaximumAgreesWithNancy(Rational a, Rational b)
    {
        Rational Reference()
        {
            var lcm = Rational.LeastCommonMultiple(a, b);
            return Rational.Max(lcm, new Rational(10));
        }

        Rational Expression()
        {
            var aExp = Expressions.FromRational(a);
            var bExp = Expressions.FromRational(b);
            var lcm = aExp.LeastCommonMultiple(bExp);
            var result = lcm.Max(10);
            return result.Compute();
        }

        Assert.Equal(Reference(), Expression());
    }

    public static List<(Curve a, Curve b)> CurvePairs =
    [
        // Simple sigma-rho and rate-latency
        (
            new SigmaRhoArrivalCurve(sigma: 10, rho: 5),
            new RateLatencyServiceCurve(rate: 2, latency: 3)
        ),
        // Different sigma-rho values
        (
            new SigmaRhoArrivalCurve(sigma: 20, rho: 10),
            new RateLatencyServiceCurve(rate: 5, latency: 2)
        ),
        // Custom curves with various behaviors
        (
            new Curve(
                baseSequence: new Sequence([
                    new Point(0, -2),
                    new Segment(0, 2, -2, 1),
                    new Point(2, 0),
                    new Segment(2, 4, 0, 1)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1
            ),
            new RateLatencyServiceCurve(rate: 3, latency: 1)
        ),
    ];

    public static IEnumerable<object[]> CurvePairData => CurvePairs.ToXUnitTestCases();

    public static List<(Rational a, Rational b)> RationalPairs =
    [
        (new Rational(4), new Rational(2)),
        (new Rational(6), new Rational(4)),
        (new Rational(10), new Rational(15)),
        (new Rational(3, 4), new Rational(1, 2)),
        (new Rational(5, 6), new Rational(2, 3)),
        (new Rational(12), new Rational(8)),
    ];

    public static IEnumerable<object[]> RationalPairData => RationalPairs.ToXUnitTestCases();

}
