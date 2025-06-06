﻿using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Deconvolution;

[TestSubject(typeof(Nancy.Expressions.Equivalences.DeconvDistributivityWithMax))]
public class DeconvDistributivityWithMax
{
    [Fact]
    public void ApplyEquivalence_DeconvDistributivityWithMax()
    {
        Curve f = new RateLatencyServiceCurve(1, 2);
        Curve g = new RateLatencyServiceCurve(2, 4);
        Curve h = new RateLatencyServiceCurve(3, 5);

        var e = Expressions.Deconvolution(
            Expressions.Maximum(f, g),
            h
        );

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.DeconvDistributivityWithMax());

        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        var expected = "(f \u2298 h) \u2228 (g \u2298 h)";
        var regex = $"\\(?{Regex.Escape(expected)}\\)?";
        Assert.Matches(regex, eq.ToUnicodeString());
    }
}