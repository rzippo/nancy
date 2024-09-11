using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;
using Unipi.Nancy.NetworkCalculus;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Convolution;

[TestSubject(typeof(Nancy.Expressions.Equivalences.SelfConvolutionSubAdditive))]
public class SelfConvolutionSubAdditive
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SelfConvolutionSubAdditive(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ApplyEquivalence_SelfConvolutionSubAdditive()
    {
        var f = new RateLatencyServiceCurve(1, 2).SubAdditiveClosure();

        var e = Expressions.Convolution(f, f);
        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.SelfConvolutionSubAdditive());

        Assert.False(e == eq);
        Assert.True(e.Equivalent(eq));
        Assert.Equal("f", eq.ToUnicodeString());
    }

    [Fact]
    public void ApplyEquivalence_SelfConvolutionSubAdditive2()
    {
        var beta1 = new RateLatencyServiceCurve(1, 2);
        var beta2 = new RateLatencyServiceCurve(3, 4);
        var beta3 = new RateLatencyServiceCurve(2, 1);
        var W2 = new ConstantCurve(3);
        var W3 = new ConstantCurve(2);

        var temp1 = Expressions.Convolution(beta1, beta2).Addition(W2).SubAdditiveClosure();
        var temp2 = Expressions.Convolution(beta2, beta3).Addition(W3).SubAdditiveClosure();
        var beta_eq1 = Expressions.Convolution(beta1, temp1).Convolution(temp2);
        var beta_eq2 = Expressions.Convolution(beta2, temp2);
        var beta_eq = Expressions.Convolution(beta_eq1, beta_eq2).Convolution(beta3);

        var selfConv = new Nancy.Expressions.Equivalences.SelfConvolutionSubAdditive();
        var equivalentExpression = beta_eq;
        do
        {
            _testOutputHelper.WriteLine(equivalentExpression.ToString());
            beta_eq = equivalentExpression;
            equivalentExpression = beta_eq.ApplyEquivalence(selfConv);
        } while (equivalentExpression != beta_eq);
    }

    [Fact]
    public void ApplyEquivalence_SelfConvolutionSubAdditive2_4()
    {
        var beta1 = new RateLatencyServiceCurve(1, 2);
        var beta2 = new RateLatencyServiceCurve(3, 4);
        var beta3 = new RateLatencyServiceCurve(2, 1);
        var beta4 = new RateLatencyServiceCurve(6, 7);
        var beta5 = new RateLatencyServiceCurve(7, 73);
        var beta6 = new RateLatencyServiceCurve(8, 27);
        var beta7 = new RateLatencyServiceCurve(9, 71);
        var beta8 = new RateLatencyServiceCurve(10, 23);
        var beta9 = new RateLatencyServiceCurve(11, 72);
        var beta10 = new RateLatencyServiceCurve(12, 54);
        var W2 = new ConstantCurve(3);
        var W3 = new ConstantCurve(2);
        var W4 = new ConstantCurve(22);
        var W5 = new ConstantCurve(24);
        var W6 = new ConstantCurve(25);
        var W7 = new ConstantCurve(12);
        var W8 = new ConstantCurve(22);
        var W9 = new ConstantCurve(14);
        var W10 = new ConstantCurve(4);

        var temp1 = Expressions.Convolution(beta1, beta2).Addition(W2).SubAdditiveClosure();
        var temp2 = Expressions.Convolution(beta2, beta3).Addition(W3).SubAdditiveClosure();
        var temp3 = Expressions.Convolution(beta3, beta4).Addition(W4).SubAdditiveClosure();
        var temp4 = Expressions.Convolution(beta4, beta5).Addition(W5).SubAdditiveClosure();
        var temp5 = Expressions.Convolution(beta5, beta6).Addition(W6).SubAdditiveClosure();
        var temp6 = Expressions.Convolution(beta6, beta7).Addition(W7).SubAdditiveClosure();
        var temp7 = Expressions.Convolution(beta7, beta8).Addition(W8).SubAdditiveClosure();
        var temp8 = Expressions.Convolution(beta8, beta9).Addition(W9).SubAdditiveClosure();
        var temp9 = Expressions.Convolution(beta9, beta10).Addition(W10).SubAdditiveClosure();
        var beta_eq1 = Expressions.Convolution(beta1, temp1).Convolution(temp2).Convolution(temp3).Convolution(temp4).Convolution(temp5).Convolution(temp6).Convolution(temp7).Convolution(temp8).Convolution(temp9);
        var beta_eq2 = Expressions.Convolution(beta2, temp2).Convolution(temp3).Convolution(temp4).Convolution(temp5).Convolution(temp6).Convolution(temp7).Convolution(temp8).Convolution(temp9);
        var beta_eq3 = Expressions.Convolution(beta3, temp3).Convolution(temp4).Convolution(temp5).Convolution(temp6).Convolution(temp7).Convolution(temp8).Convolution(temp9);
        var beta_eq4 = Expressions.Convolution(beta4, temp4).Convolution(temp5).Convolution(temp6).Convolution(temp7).Convolution(temp8).Convolution(temp9);
        var beta_eq5 = Expressions.Convolution(beta5, temp5).Convolution(temp6).Convolution(temp7).Convolution(temp8).Convolution(temp9);
        var beta_eq6 = Expressions.Convolution(beta6, temp6).Convolution(temp7).Convolution(temp8).Convolution(temp9);
        var beta_eq7 = Expressions.Convolution(beta7, temp7).Convolution(temp8).Convolution(temp9);
        var beta_eq8 = Expressions.Convolution(beta8, temp8).Convolution(temp9);
        var beta_eq9 = Expressions.Convolution(beta9, temp9);
        var beta_eq = Expressions.Convolution(beta_eq1, beta_eq2)
            .Convolution(beta_eq3)
            .Convolution(beta_eq4)
            .Convolution(beta_eq5)
            .Convolution(beta_eq6)
            .Convolution(beta_eq7)
            .Convolution(beta_eq8).Convolution(beta_eq9).Convolution(beta10);

        var selfConv = new Nancy.Expressions.Equivalences.SelfConvolutionSubAdditive();
        var equivalentExpression = beta_eq;
        do
        {
            _testOutputHelper.WriteLine(equivalentExpression.ToString());
            beta_eq = equivalentExpression;
            equivalentExpression = beta_eq.ApplyEquivalence(selfConv);
        } while (equivalentExpression != beta_eq);
    }


    [Fact]
    public void ApplyEquivalence_SelfConvolutionSubAdditive3()
    {
        int N = 10;
        Curve[] beta = new Curve[N];
        Curve[] W = new Curve[N];

        Random rnd = new Random();
        for (var i = 0; i < N; i++)
        {
            beta[i] = new RateLatencyServiceCurve(rnd.Next(1, 20), rnd.Next(1, 20)); 
        }

        for (int i = 1; i < N; i++)
        {
            W[i] = new ConstantCurve(rnd.Next(1, 20)); 
        }

        var temp = new CurveExpression[N - 1];
        for (int j = 0; j < N - 1; j++)
        {
            temp[j] = Expressions.Convolution(beta[j], beta[j + 1], "beta" + (j + 1), "beta" + (j + 2))
                .Addition(W[j + 1], "W" + (j + 2)).SubAdditiveClosure();
        }

        var beta_eq = new CurveExpression[N];
        for (int i = 0; i < N - 1; i++)
        {
            beta_eq[i] = Expressions.FromCurve(beta[i], "beta" + (i + 1));
            for (int j = i; j < N - 1; j++)
            {
                beta_eq[i] = beta_eq[i].Convolution(temp[j]);
            }
        }

        beta_eq[N - 1] = Expressions.FromCurve(beta[N - 1], "beta" + N);
        CurveExpression resultExpression = beta_eq[0];
        for (int i = 1; i < N; i++)
        {
            resultExpression = Expressions.Convolution(resultExpression, beta_eq[i]);
        }

        var selfConv = new Nancy.Expressions.Equivalences.SelfConvolutionSubAdditive();
        var equivalentExpression = resultExpression;
        do
        {
            _testOutputHelper.WriteLine(equivalentExpression.ToString());
            resultExpression = equivalentExpression;
            equivalentExpression = resultExpression.ApplyEquivalence(selfConv);
        } while (equivalentExpression != resultExpression);
    }
}