using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.MinPlusAlgebra.Json;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class CurveJson
{
    private readonly ITestOutputHelper output;

    public CurveJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void CurveSerialization()
    {
        Curve curve = new Curve(
            baseSequence: new Sequence(new Element[]
            {
                Point.Origin(),
                new Segment
                (
                    rightLimitAtStartTime : 0,
                    slope : 0,
                    startTime : 0,
                    endTime : 3
                ),
                new Point(time: 3, value: 5),
                new Segment
                (
                    rightLimitAtStartTime : 5,
                    slope : 0,
                    startTime : 3,
                    endTime : 5
                )
            }),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 3
        );

        var settings = new JsonSerializerSettings {
            Converters = new JsonConverter[] { new CurveConverter() }
        };

        string serialization = JsonConvert.SerializeObject(curve, settings);
        output.WriteLine(serialization);
        Curve deserialized = JsonConvert.DeserializeObject<Curve>(serialization, settings)!;

        Assert.Equal(curve, deserialized);
    }
}