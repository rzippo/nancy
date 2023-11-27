using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.MinPlusAlgebra.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom Newtonsoft.Json JsonConverter for <see cref="Curve"/> and its subclasses.
/// Should be safely usable for (de)serialization of any curve.
/// </summary>
public class GenericCurveNewtonsoftJsonConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return typeof(Curve).IsAssignableFrom(objectType);
    }

    public static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() {
            Converters = new JsonConverter[] {
                new CurveNewtonsoftJsonConverter(),
                new RateLatencyServiceCurveNewtonsoftJsonConverter(),
                new ConvexCurveNewtonsoftJsonConverter(),
                new DelayServiceCurveNewtonsoftJsonConverter(),
                new SuperAdditiveCurveNewtonsoftJsonConverter(),
                new ConstantCurveNewtonsoftJsonConverter(),
                new SigmaRhoArrivalCurveNewtonsoftJsonConverter(),
                new ConcaveCurveNewtonsoftJsonConverter(),
                new SubAdditiveCurveNewtonsoftJsonConverter(),
                new FlowControlCurveNewtonsoftJsonConverter(),
                new StairCurveNewtonsoftJsonConverter(),
                new StepCurveNewtonsoftJsonConverter(),
                new RaisedRateLatencyServiceCurveNewtonsoftJsonConverter(),
                new TwoRatesServiceCurveNewtonsoftJsonConverter(),
            }
        }; 

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        var curveSerializer = JsonSerializer.Create(jsonSerializerSettings);

        // todo: can we move this check to each individual converter, skipping this switch?
        var typename = jo[TypeName]?.ToObject<string>(serializer);
        if (typename == null)
            throw new InvalidOperationException("Invalid JSON format.");
        Curve? result;
        switch(typename)
        {
            case Curve.TypeCode:
                result = jo.ToObject<Curve>(curveSerializer);
                break;

            case RateLatencyServiceCurve.TypeCode:
                result = jo.ToObject<RateLatencyServiceCurve>(curveSerializer);
                break;

            case ConvexCurve.TypeCode:
                result = jo.ToObject<ConvexCurve>(curveSerializer);
                break;
            
            case DelayServiceCurve.TypeCode:
                result = jo.ToObject<DelayServiceCurve>(curveSerializer);
                break;

            case SuperAdditiveCurve.TypeCode:
                result = jo.ToObject<SuperAdditiveCurve>(curveSerializer);
                break;

            case ConstantCurve.TypeCode:
                result = jo.ToObject<ConstantCurve>(curveSerializer);
                break;

            case SigmaRhoArrivalCurve.TypeCode:
                result = jo.ToObject<SigmaRhoArrivalCurve>(curveSerializer);
                break;

            case ConcaveCurve.TypeCode:
                result = jo.ToObject<ConcaveCurve>(curveSerializer);
                break;

            case FlowControlCurve.TypeCode:
                result = jo.ToObject<FlowControlCurve>(curveSerializer);
                break;

            case SubAdditiveCurve.TypeCode:
                result = jo.ToObject<SubAdditiveCurve>(curveSerializer);
                break;

            case StairCurve.TypeCode:
                result = jo.ToObject<StairCurve>(curveSerializer);
                break;

            case StepCurve.TypeCode:
                result = jo.ToObject<StepCurve>(curveSerializer);
                break;

            case RaisedRateLatencyServiceCurve.TypeCode:
                result = jo.ToObject<RaisedRateLatencyServiceCurve>(curveSerializer);
                break;
            
            case TwoRatesServiceCurve.TypeCode:
                result = jo.ToObject<TwoRatesServiceCurve>(curveSerializer);
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

        var curveSerializer = JsonSerializer.Create(jsonSerializerSettings);

        JObject jo = JObject.FromObject(value, curveSerializer);
        jo.WriteTo(writer);
    }
}