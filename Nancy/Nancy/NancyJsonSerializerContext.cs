using System.Text.Json.Serialization;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
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
[JsonSerializable(typeof(Rational))]
[JsonSerializable(typeof(Curve))]
[JsonSerializable(typeof(ConvexCurve))]
[JsonSerializable(typeof(ConcaveCurve))]
[JsonSerializable(typeof(SuperAdditiveCurve))]
[JsonSerializable(typeof(SubAdditiveCurve))]
[JsonSerializable(typeof(TwoRatesServiceCurve))]
[JsonSerializable(typeof(ConstantCurve))]
[JsonSerializable(typeof(DelayServiceCurve))]
[JsonSerializable(typeof(FlowControlCurve))]
[JsonSerializable(typeof(RaisedRateLatencyServiceCurve))]
[JsonSerializable(typeof(RateLatencyServiceCurve))]
[JsonSerializable(typeof(StepCurve))]
[JsonSerializable(typeof(StairCurve))]
[JsonSerializable(typeof(SigmaRhoArrivalCurve))]
partial class NancyJsonSerializerContext : JsonSerializerContext
{
}