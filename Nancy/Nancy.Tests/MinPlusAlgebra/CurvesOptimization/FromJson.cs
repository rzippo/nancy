using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.CurvesOptimization;

public class FromJson
{
    public static string[] CurveNames =
    {
        "Unipi.Nancy.Tests/MinPlusAlgebra/CurvesOptimization/JsonTestCases/1.json",
        "Unipi.Nancy.Tests/MinPlusAlgebra/CurvesOptimization/JsonTestCases/2.json",
        "Unipi.Nancy.Tests/MinPlusAlgebra/CurvesOptimization/JsonTestCases/3.json",
        "Unipi.Nancy.Tests/MinPlusAlgebra/CurvesOptimization/JsonTestCases/4.json",
        "Unipi.Nancy.Tests/MinPlusAlgebra/CurvesOptimization/JsonTestCases/5.json"
    };

    public static IEnumerable<object[]> GetJsonTestCases()
    {
        foreach (var curveName in CurveNames)
        {
            string json = EmbeddedResourceDataAttribute.ReadManifestData(curveName);
            var curve = JsonConvert.DeserializeObject<Curve>(json, new GenericCurveConverter(), new RationalConverter())!;

            yield return new object[] { curve };
        }
    }

    [Theory]
    [MemberData(nameof(GetJsonTestCases))]
    public void SelfEquivalence(Curve curve)
    {
        var optimizedCurve = curve.Optimize();
        Assert.True(Curve.Equivalent(optimizedCurve, curve));
    }

    [Theory]
    [MemberData(nameof(GetJsonTestCases))]
    public void IterativeEquivalence(Curve curve)
    {
        Curve prev = curve, next = curve;

        for (int i = 0; i < 10; i++)
        {
            next = prev.Optimize();
            Assert.True(Curve.Equivalent(prev, next));
            prev = next;
        }

        Assert.True(Curve.Equivalent(curve, next));
    }

    [Theory]
    [MemberData(nameof(GetJsonTestCases))]
    public void SelfConvolutionEquivalence(Curve curve)
    {
        var optimized = curve.Optimize();

        var optimizedConv = Curve.Convolution(optimized, optimized, new ComputationSettings {UseRepresentationMinimization = true});
        var unoptimizedConv = Curve.Convolution(curve, curve, new ComputationSettings {UseRepresentationMinimization = true});

        Assert.Equal(curve.IsContinuousExceptOrigin, optimized.IsContinuousExceptOrigin);
        Assert.Equal(unoptimizedConv.IsContinuousExceptOrigin, optimizedConv.IsContinuousExceptOrigin);

        Assert.True(Curve.Equivalent(optimizedConv, unoptimizedConv));
    }

    [Theory]
    [MemberData(nameof(GetJsonTestCases))]
    public void SelfMinimumEquivalence(Curve curve)
    {
        var optimized = curve.Optimize();

        var optimizedMin = Curve.Minimum(optimized, optimized, new ComputationSettings{UseRepresentationMinimization = true});
        var unoptimizedMin = Curve.Minimum(curve, curve, new ComputationSettings{UseRepresentationMinimization = true});

        Assert.True(Curve.Equivalent(optimizedMin, unoptimizedMin));
    }

    public static IEnumerable<object[]> PairsTestCases()
    {
        var curves = CurveNames
            .Select(name =>
            {
                string json = EmbeddedResourceDataAttribute.ReadManifestData(name);
                var curve = JsonConvert.DeserializeObject<Curve>(json, new GenericCurveConverter(), new RationalConverter());
                return curve;
            })
            .ToList();

        for (int i = 0; i < curves.Count/2; i++)
        {
            object[] pair = curves.Skip(i * 2).Take(2).Cast<object>().ToArray();
            if (pair.Length < 2)
                yield break;
            else
                yield return pair;
        }
    }

    [Theory]
    [MemberData(nameof(PairsTestCases))]
    public void MinimumEquivalence(Curve a, Curve b)
    {
        var unoptimizedMin = Curve.Minimum(a, b, new ComputationSettings {UseRepresentationMinimization = true});

        var optA = a.Optimize();
        var optB = b.Optimize();
        var optimizedMin = Curve.Minimum(optA, optB, new ComputationSettings {UseRepresentationMinimization = true});

        Assert.True(Curve.Equivalent(optimizedMin, unoptimizedMin));

        var partialMinA = Curve.Minimum(optA, b, new ComputationSettings {UseRepresentationMinimization = true});
        var partialMinB = Curve.Minimum(a, optB, new ComputationSettings {UseRepresentationMinimization = true});
        Assert.True(Curve.Equivalent(partialMinA, unoptimizedMin));
        Assert.True(Curve.Equivalent(partialMinB, unoptimizedMin));
    }

    // Commented out because it costs too much time
    // Did not seem to find any issues that SelfConvolution does not find

    // [Theory]
    // [MemberData(nameof(PairsTestCases))]
    // public void ConvolutionEquivalence(Curve a, Curve b)
    // {
    //     var unoptimizedConv = Curve.Convolution(a, b, new ComputationSettings {AutoOptimize = true});
    //
    //     var optA = a.Optimize();
    //     var optB = b.Optimize();
    //     var optimizedConv = Curve.Convolution(optA, optB, new ComputationSettings {AutoOptimize = true});
    //     
    //     Assert.True(Curve.Equivalent(optimizedConv, unoptimizedConv));
    //     
    //     var partialConvA = Curve.Convolution(optA, b, new ComputationSettings {AutoOptimize = true});
    //     var partialConvB = Curve.Convolution(a, optB, new ComputationSettings {AutoOptimize = true});
    //     Assert.True(Curve.Equivalent(partialConvA, unoptimizedConv));
    //     Assert.True(Curve.Equivalent(partialConvB, unoptimizedConv));
    // }
}