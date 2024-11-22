using System.Text.Json.Serialization;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RationalSystemJsonConverter.PlainRational))]
[JsonSerializable(typeof(TwoRatesServiceCurveSystemJsonConverter.PlainTwoRatesServiceCurve))]
[JsonSerializable(typeof(GenericCurveSystemJsonConverter.PlainCurve))]
[JsonSerializable(typeof(ConstantCurveSystemJsonConverter.PlainConstantCurve))]
[JsonSerializable(typeof(DelayServiceCurveSystemJsonConverter.PlainDelayServiceCurve))]
[JsonSerializable(typeof(FlowControlCurveSystemJsonConverter.PlainFlowControlCurve))]
[JsonSerializable(typeof(RaisedRateLatencyServiceCurveSystemJsonConverter.PlainRaisedRateLatencyServiceCurve))]
[JsonSerializable(typeof(RateLatencyServiceCurveSystemJsonConverter.PlainRateLatencyServiceCurve))]
[JsonSerializable(typeof(StepCurveSystemJsonConverter.PlainStepCurve))]
[JsonSerializable(typeof(StairCurveSystemJsonConverter.PlainStairCurve))]
[JsonSerializable(typeof(SigmaRhoArrivalCurveSystemJsonConverter.PlainSigmaRhoArrivalCurve))]
partial class NancyJsonSerializerContext : JsonSerializerContext
{
}