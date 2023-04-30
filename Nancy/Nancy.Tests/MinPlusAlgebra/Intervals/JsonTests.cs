using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Intervals;

public class JsonComputeIntervals
{
    private readonly ITestOutputHelper output;

    public JsonComputeIntervals(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static string[] CurveNames =
    {
        "Unipi.Nancy.Tests/MinPlusAlgebra/Intervals/JsonComputeIntervals/convolution_1.json",
        #if !SKIP_LONG_TESTS
        "Unipi.Nancy.Tests/MinPlusAlgebra/Intervals/JsonComputeIntervals/convolution_2.json",
        #endif
        "Unipi.Nancy.Tests/MinPlusAlgebra/Intervals/JsonComputeIntervals/deconvolution_1.json",
        "Unipi.Nancy.Tests/MinPlusAlgebra/Intervals/JsonComputeIntervals/deconvolution_2.json"
    };

    public static IEnumerable<object[]> GetJsonTestCases()
    {
        foreach (var curveName in CurveNames)
        {
            string json = EmbeddedResourceDataAttribute.ReadManifestData(curveName);
            var elements = JsonConvert.DeserializeObject<Element[]>(json, new GenericCurveConverter(), new RationalConverter())!;

            yield return new object[] { elements };
        }
    }

    [Theory]
    [MemberData(nameof(GetJsonTestCases))]
    public void FromJsonTest(Element[] elements)
    {
        // ConfigNLog();
        output.WriteLine($"Computing intervals for {elements.Length} elements");

        var settings = new ComputationSettings
        {
            UseParallelComputeIntervals = true,
            ParallelComputeIntervalsThreshold = 1_000
        };
        var intervals = Interval.ComputeIntervals(elements, settings);

        //Check that the intervals contain the proper elements
        foreach (var interval in intervals)
        {
            var intervalElements = interval.Elements;
            if (interval.IsPointInterval)
            {
                Assert.True(intervalElements
                    .All(e => e is Point p && 
                              p.Time == interval.Start)
                );
            }
            else
            {
                Assert.True(intervalElements
                    .All(e => e is Segment s && 
                              s.StartTime == interval.Start &&
                              s.EndTime == interval.End)
                );
            }
        }

        //output.WriteLine(JsonConvert.SerializeObject(intervals));

        Assert.True(intervals.AreInTimeSequence());

#pragma warning disable CS8321
        void ConfigNLog()
#pragma warning restore CS8321
        {
            var config = new NLog.Config.LoggingConfiguration();

            var fileTarget = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "json_compute_interval.${longdate:cached=true}.txt"
            };

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }
    }
}