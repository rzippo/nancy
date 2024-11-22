using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.MinPlusAlgebra.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="Curve"/> and its subclasses.
/// Should be safely usable for (de)serialization of any curve.
/// </summary>
public class GenericCurveSystemJsonConverter : JsonConverter<Curve>
{
    private const string TypeName = "type";

    // ugly hack?
    /// Proxy class for the serialization of <see cref="Curve"/>.
    internal record PlainCurve(string type, Sequence baseSequence, Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight);
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override Curve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var readerClone = reader;
        var success = JsonDocument.TryParseValue(ref readerClone, out JsonDocument? document);
        if (!success || document == null)
            throw new JsonException();
        var type = document.RootElement.GetProperty(TypeName).GetString();

        switch (type)
        {
            case RateLatencyServiceCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.RateLatencyServiceCurve)!;

            case ConvexCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.ConvexCurve)!;

            case DelayServiceCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.DelayServiceCurve)!;

            case SuperAdditiveCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.SuperAdditiveCurve)!;

            case ConstantCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.ConstantCurve)!;

            case SigmaRhoArrivalCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.SigmaRhoArrivalCurve)!;

            case ConcaveCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.ConcaveCurve)!;

            case FlowControlCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.FlowControlCurve)!;

            case SubAdditiveCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.SubAdditiveCurve)!;

            case StairCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.StairCurve)!;

            case StepCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.StepCurve)!;

            case RaisedRateLatencyServiceCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.RaisedRateLatencyServiceCurve)!;

            case TwoRatesServiceCurve.TypeCode:
                return JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.TwoRatesServiceCurve)!;

            case Curve.TypeCode:
            {
                var plain = JsonSerializer.Deserialize<PlainCurve>(ref reader, NancyJsonSerializerContext.Default.PlainCurve);
                if(plain == null || plain.baseSequence == null )
                    throw new JsonException();
                return new Curve(
                    plain.baseSequence, 
                    plain.pseudoPeriodStart, 
                    plain.pseudoPeriodLength, 
                    plain.pseudoPeriodHeight
                );
            }
            
            default:
                throw new JsonException();
        }
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        Curve value,
        JsonSerializerOptions options)
    {
        switch (value)
        {
            case RateLatencyServiceCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.RateLatencyServiceCurve);
                break;
            }
            
            case ConvexCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.ConvexCurve);
                break;
            }

            case DelayServiceCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.DelayServiceCurve);
                break;
            }

            case SuperAdditiveCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.SuperAdditiveCurve);
                break;
            }

            case ConstantCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.ConstantCurve);
                break;
            }

            case SigmaRhoArrivalCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.SigmaRhoArrivalCurve);
                break;
            }

            case ConcaveCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.ConcaveCurve);
                break;
            }

            case FlowControlCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.FlowControlCurve);
                break;
            }

            case SubAdditiveCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.SubAdditiveCurve);
                break;
            }

            case StairCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.StairCurve);
                break;
            }
            
            case StepCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.StepCurve);
                break;
            }
            
            case RaisedRateLatencyServiceCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.RaisedRateLatencyServiceCurve);
                break;
            }

            case TwoRatesServiceCurve c:
            {
                JsonSerializer.Serialize(writer, c, NancyJsonSerializerContext.Default.TwoRatesServiceCurve);
                break;
            }

            default:
            {
                var plain = new PlainCurve(
                    Curve.TypeCode,
                    value.BaseSequence,
                    value.PseudoPeriodStart,
                    value.PseudoPeriodLength,
                    value.PseudoPeriodHeight
                );
                JsonSerializer.Serialize(writer, plain, NancyJsonSerializerContext.Default.PlainCurve);
                break;
            }
        }
    }
}