using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class CurveUpperBounds
{
    private readonly ITestOutputHelper output;

    public CurveUpperBounds(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Theory]
    [InlineData("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":100,\"rho\":5 }")]
    [InlineData("{\"type\":\"rateLatencyServiceCurve\",\"latency\":10,\"rate\":10 }")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/1.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/2.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/3.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/4.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/5.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/6.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/7.json")]
    public void SigmaRhoTest(string curveJson)
    {
        var curve = JsonConvert.DeserializeObject<Curve>(curveJson, new GenericCurveConverter(), new RationalConverter())!;

        var sigmaRhoCurve = curve.SigmaRhoUpperBound();

        output.WriteLine(JsonConvert.SerializeObject(sigmaRhoCurve, new GenericCurveConverter(), new RationalConverter()));

        Assert.True(sigmaRhoCurve.IsContinuousExceptOrigin);
        Assert.True(sigmaRhoCurve.IsLeftContinuous);
        Assert.True(sigmaRhoCurve.IsFinite);
        Assert.True(sigmaRhoCurve.IsUltimatelyPlain);

        Assert.True(sigmaRhoCurve >= curve);
    }
}