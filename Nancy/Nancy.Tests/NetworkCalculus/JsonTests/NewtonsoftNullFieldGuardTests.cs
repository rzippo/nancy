using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.NetworkCalculus.Json;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.JsonTests;

public class NewtonsoftNullFieldGuardTests
{
    public static IEnumerable<object[]> NullFieldCases()
    {
        // RateLatencyServiceCurve: both fields.
        yield return Case<RateLatencyServiceCurve>(new RateLatencyServiceCurveNewtonsoftJsonConverter(),
            "{\"latency\":null,\"rate\":20}", "RateLatencyServiceCurve", "latency");
        yield return Case<RateLatencyServiceCurve>(new RateLatencyServiceCurveNewtonsoftJsonConverter(),
            "{\"latency\":10,\"rate\":null}", "RateLatencyServiceCurve", "rate");

        // ConstantCurve: single field.
        yield return Case<ConstantCurve>(new ConstantCurveNewtonsoftJsonConverter(),
            "{\"value\":null}", "ConstantCurve", "value");

        // DelayServiceCurve: single field.
        yield return Case<DelayServiceCurve>(new DelayServiceCurveNewtonsoftJsonConverter(),
            "{\"delay\":null}", "DelayServiceCurve", "delay");

        // FlowControlCurve: all three fields.
        yield return Case<FlowControlCurve>(new FlowControlCurveNewtonsoftJsonConverter(),
            "{\"latency\":null,\"rate\":2,\"height\":3}", "FlowControlCurve", "latency");
        yield return Case<FlowControlCurve>(new FlowControlCurveNewtonsoftJsonConverter(),
            "{\"latency\":1,\"rate\":null,\"height\":3}", "FlowControlCurve", "rate");
        yield return Case<FlowControlCurve>(new FlowControlCurveNewtonsoftJsonConverter(),
            "{\"latency\":1,\"rate\":2,\"height\":null}", "FlowControlCurve", "height");

        // SigmaRhoArrivalCurve: both fields.
        yield return Case<SigmaRhoArrivalCurve>(new SigmaRhoArrivalCurveNewtonsoftJsonConverter(),
            "{\"sigma\":null,\"rho\":2}", "SigmaRhoArrivalCurve", "sigma");
        yield return Case<SigmaRhoArrivalCurve>(new SigmaRhoArrivalCurveNewtonsoftJsonConverter(),
            "{\"sigma\":1,\"rho\":null}", "SigmaRhoArrivalCurve", "rho");

        // StairCurve: both fields.
        yield return Case<StairCurve>(new StairCurveNewtonsoftJsonConverter(),
            "{\"a\":null,\"b\":2}", "StairCurve", "a");
        yield return Case<StairCurve>(new StairCurveNewtonsoftJsonConverter(),
            "{\"a\":1,\"b\":null}", "StairCurve", "b");

        // StepCurve: both fields.
        yield return Case<StepCurve>(new StepCurveNewtonsoftJsonConverter(),
            "{\"value\":null,\"stepTime\":2}", "StepCurve", "value");
        yield return Case<StepCurve>(new StepCurveNewtonsoftJsonConverter(),
            "{\"value\":1,\"stepTime\":null}", "StepCurve", "stepTime");

        // RaisedRateLatencyServiceCurve: the three guarded fields; withZeroOrigin stays optional, not tested here.
        yield return Case<RaisedRateLatencyServiceCurve>(new RaisedRateLatencyServiceCurveNewtonsoftJsonConverter(),
            "{\"latency\":null,\"rate\":2,\"bufferShift\":3}", "RaisedRateLatencyServiceCurve", "latency");
        yield return Case<RaisedRateLatencyServiceCurve>(new RaisedRateLatencyServiceCurveNewtonsoftJsonConverter(),
            "{\"latency\":1,\"rate\":null,\"bufferShift\":3}", "RaisedRateLatencyServiceCurve", "rate");
        yield return Case<RaisedRateLatencyServiceCurve>(new RaisedRateLatencyServiceCurveNewtonsoftJsonConverter(),
            "{\"latency\":1,\"rate\":2,\"bufferShift\":null}", "RaisedRateLatencyServiceCurve", "bufferShift");

        // TwoRatesServiceCurve: all four fields.
        yield return Case<TwoRatesServiceCurve>(new TwoRatesServiceCurveNewtonsoftJsonConverter(),
            "{\"delay\":null,\"transientRate\":2,\"transientEnd\":3,\"steadyRate\":4}", "TwoRatesServiceCurve", "delay");
        yield return Case<TwoRatesServiceCurve>(new TwoRatesServiceCurveNewtonsoftJsonConverter(),
            "{\"delay\":1,\"transientRate\":null,\"transientEnd\":3,\"steadyRate\":4}", "TwoRatesServiceCurve", "transientRate");
        yield return Case<TwoRatesServiceCurve>(new TwoRatesServiceCurveNewtonsoftJsonConverter(),
            "{\"delay\":1,\"transientRate\":2,\"transientEnd\":null,\"steadyRate\":4}", "TwoRatesServiceCurve", "transientEnd");
        yield return Case<TwoRatesServiceCurve>(new TwoRatesServiceCurveNewtonsoftJsonConverter(),
            "{\"delay\":1,\"transientRate\":2,\"transientEnd\":3,\"steadyRate\":null}", "TwoRatesServiceCurve", "steadyRate");

        // Convex/Concave/SubAdditive/SuperAdditive share Curve's baseSequence and three period fields.
        // Exhaustive coverage of that shape already lives with CurveNewtonsoftJson, so one field each here pins that the guard is wired through on all four subtypes.
        const string validPeriod = "\"pseudoPeriodStart\":3,\"pseudoPeriodLength\":2,\"pseudoPeriodHeight\":3";
        yield return Case<ConvexCurve>(new ConvexCurveNewtonsoftJsonConverter(),
            $"{{\"baseSequence\":null,{validPeriod}}}", "ConvexCurve", "baseSequence");
        yield return Case<ConcaveCurve>(new ConcaveCurveNewtonsoftJsonConverter(),
            $"{{\"baseSequence\":null,{validPeriod}}}", "ConcaveCurve", "baseSequence");
        yield return Case<SubAdditiveCurve>(new SubAdditiveCurveNewtonsoftJsonConverter(),
            $"{{\"baseSequence\":null,{validPeriod}}}", "SubAdditiveCurve", "baseSequence");
        yield return Case<SuperAdditiveCurve>(new SuperAdditiveCurveNewtonsoftJsonConverter(),
            $"{{\"baseSequence\":null,{validPeriod}}}", "SuperAdditiveCurve", "baseSequence");
    }

    private static object[] Case<T>(JsonConverter converter, string json, string typeName, string field)
        => new object[] { converter, json, typeof(T), $"{typeName} cannot be deserialized: {field} cannot be null." };

    [Theory]
    [MemberData(nameof(NullFieldCases))]
    public void DeserializeNullFieldThrowsWithFieldName(JsonConverter converter, string json, Type targetType, string expectedMessage)
    {
        var ex = Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject(json, targetType, converter));
        Assert.Equal(expectedMessage, ex.Message);
    }
}
