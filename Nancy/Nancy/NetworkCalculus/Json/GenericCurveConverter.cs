using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.MinPlusAlgebra.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="Curve"/> and its subclasses.
/// Should be safely usable for (de)serialization of any curve.
/// </summary>
public class GenericCurveConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return typeof(Curve).IsAssignableFrom(objectType);
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        var settings = new JsonSerializerSettings() {
            Converters = new JsonConverter[] {
                new CurveConverter(),
                new ConstantCurveConverter(),
                new StepCurveConverter(),
                new SigmaRhoArrivalCurveConverter(),
                new RateLatencyServiceCurveConverter(),
                new TwoRatesServiceCurveConverter(),
                new DelayServiceCurveConverter(),
                new RaisedRateLatencyServiceCurveConverter(),
                new FlowControlCurveConverter(),
                new SubAdditiveCurveConverter(),
                new StairCurveConverter()
            }
        };
        var curveSerializer = JsonSerializer.Create(settings);

        // todo: can we move this check to each individual converter, skipping this switch?
        var typename = jo[TypeName]?.ToObject<string>(serializer);
        if (typename == null)
            throw new InvalidOperationException("Invalid JSON format.");
        Curve? result;
        switch(typename)
        {
            case CurveConverter.TypeCode:
                result = jo.ToObject<Curve>(curveSerializer);
                break;

            case ConstantCurveConverter.TypeCode:
                result = jo.ToObject<ConstantCurve>(curveSerializer);
                break;

            case StepCurveConverter.TypeCode:
                result = jo.ToObject<StepCurve>(curveSerializer);
                break;

            case SigmaRhoArrivalCurveConverter.TypeCode:
                result = jo.ToObject<SigmaRhoArrivalCurve>(curveSerializer);
                break;

            case RateLatencyServiceCurveConverter.TypeCode:
                result = jo.ToObject<RateLatencyServiceCurve>(curveSerializer);
                break;

            case TwoRatesServiceCurveConverter.TypeCode:
                result = jo.ToObject<TwoRatesServiceCurve>(curveSerializer);
                break;

            case DelayServiceCurveConverter.TypeCode:
                result = jo.ToObject<DelayServiceCurve>(curveSerializer);
                break;

            case RaisedRateLatencyServiceCurveConverter.TypeCode:
                result = jo.ToObject<RaisedRateLatencyServiceCurve>(curveSerializer);
                break;

            case FlowControlCurveConverter.TypeCode:
                result = jo.ToObject<FlowControlCurve>(curveSerializer);
                break;

            case SubAdditiveCurveConverter.TypeCode:
                result = jo.ToObject<SubAdditiveCurve>(curveSerializer);
                break;

            case StairCurveConverter.TypeCode:
                result = jo.ToObject<StairCurve>(curveSerializer);
                break;

            default:
                throw new JsonSerializationException("Invalid Curve format");
        }

        if (result == null)
            throw new InvalidOperationException("Invalid JSON format.");
        return result;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var settings = new JsonSerializerSettings() {
            Converters = new JsonConverter[] {
                new CurveConverter(),
                new ConstantCurveConverter(),
                new StepCurveConverter(),
                new SigmaRhoArrivalCurveConverter(),
                new RateLatencyServiceCurveConverter(),
                new TwoRatesServiceCurveConverter(),
                new DelayServiceCurveConverter(),
                new RaisedRateLatencyServiceCurveConverter(),
                new FlowControlCurveConverter(),
                new SubAdditiveCurveConverter(),
                new StairCurveConverter()
            }
        };
        var curveSerializer = JsonSerializer.Create(settings);

        JObject jo = JObject.FromObject(value, curveSerializer);
        jo.WriteTo(writer);
    }
}