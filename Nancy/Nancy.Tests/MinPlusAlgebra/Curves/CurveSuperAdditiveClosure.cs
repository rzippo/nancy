using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveSuperAdditiveClosure
{
    private readonly ITestOutputHelper output;

    public CurveSuperAdditiveClosure(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void PureDelay(int delay)
    {
        var curve = new DelayServiceCurve(delay);

        var closure = curve.SuperAdditiveClosure();
        Assert.True(Curve.Equivalent(closure, curve));
    }

    // note: super-additive closure and super-additive property are not well defined enough for infinite parts as in the pure delay
    // we rely on the super class properties in the meantime 

    // [Theory]
    // [InlineData(0)]
    // //[InlineData(5)]
    // public void PureDelay_AsGeneric(int delay)
    // {
    //     var delayCurve = new DelayServiceCurve(delay);
    //     var curve = new Curve(delayCurve);  // ensures no optimization is involved
    //
    //     var closure = curve.SuperAdditiveClosure();
    //     Assert.True(Curve.Equivalent(closure, curve));
    // }

}