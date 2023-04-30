using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.JsonTests;

public class ShortcutMethods
{
    [Theory]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/1.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/2.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/3.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/4.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/5.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/6.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/7.json")]
    [EmbeddedResourceData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/8.json")]
    public void FromJson(string json)
    {
        var curve = Curve.FromJson(json);
        var serialization = curve.ToString();

        var curve_2 = Curve.FromJson(serialization);

        Assert.Equal(curve, curve_2);
    }
}