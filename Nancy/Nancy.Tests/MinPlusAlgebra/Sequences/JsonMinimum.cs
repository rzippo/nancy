using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class JsonMinimum
{
    private readonly ITestOutputHelper output;

    public JsonMinimum(ITestOutputHelper output)
    {
        this.output = output;
    }

    class TestCase
    {
        public Sequence a { get; }
        public Sequence b { get; }
        public bool cutToOverlap { get; }

        public TestCase(Sequence a, Sequence b, bool cutToOverlap)
        {
            this.a = a;
            this.b = b;
            this.cutToOverlap = cutToOverlap;
        }
    }

    public static string[] TestCaseNames =
    {
        "Unipi.Nancy.Tests/MinPlusAlgebra/Sequences/JsonMinimumTests/1.json",
        "Unipi.Nancy.Tests/MinPlusAlgebra/Sequences/JsonMinimumTests/2.json"
    };

    public static IEnumerable<object[]> GetTestCases()
    {
        foreach (var curveName in TestCaseNames)
        {
            string json = EmbeddedResourceDataAttribute.ReadManifestData(curveName);
            var testCase = JsonConvert.DeserializeObject<TestCase>(json, new RationalConverter())!;

            yield return new object[] { testCase.a, testCase.b, testCase.cutToOverlap };
        }
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void MinimumTest(Sequence a, Sequence b, bool cutToOverlap)
    {
        var minimum = Sequence.Minimum(a, b, cutToOverlap);
    }
}