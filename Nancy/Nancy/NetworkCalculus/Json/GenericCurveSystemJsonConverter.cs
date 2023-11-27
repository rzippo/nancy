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
    public record PlainCurve(string type, Sequence baseSequence, Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight);
    
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
                return JsonSerializer.Deserialize<RateLatencyServiceCurve>(ref reader, options)!;

            case ConvexCurve.TypeCode:
                return JsonSerializer.Deserialize<ConvexCurve>(ref reader, options)!;

            case DelayServiceCurve.TypeCode:
                return JsonSerializer.Deserialize<DelayServiceCurve>(ref reader, options)!;

            case SuperAdditiveCurve.TypeCode:
                return JsonSerializer.Deserialize<SuperAdditiveCurve>(ref reader, options)!;

            case ConstantCurve.TypeCode:
                return JsonSerializer.Deserialize<ConstantCurve>(ref reader, options)!;

            case SigmaRhoArrivalCurve.TypeCode:
                return JsonSerializer.Deserialize<SigmaRhoArrivalCurve>(ref reader, options)!;

            case ConcaveCurve.TypeCode:
                return JsonSerializer.Deserialize<ConcaveCurve>(ref reader, options)!;

            case FlowControlCurve.TypeCode:
                return JsonSerializer.Deserialize<FlowControlCurve>(ref reader, options)!;

            case SubAdditiveCurve.TypeCode:
                return JsonSerializer.Deserialize<SubAdditiveCurve>(ref reader, options)!;

            case StairCurve.TypeCode:
                return JsonSerializer.Deserialize<StairCurve>(ref reader, options)!;

            case StepCurve.TypeCode:
                return JsonSerializer.Deserialize<StepCurve>(ref reader, options)!;

            case RaisedRateLatencyServiceCurve.TypeCode:
                return JsonSerializer.Deserialize<RaisedRateLatencyServiceCurve>(ref reader, options)!;

            case TwoRatesServiceCurve.TypeCode:
                return JsonSerializer.Deserialize<TwoRatesServiceCurve>(ref reader, options)!;

            case Curve.TypeCode:
            {
                var plain = JsonSerializer.Deserialize<PlainCurve>(ref reader, options)!;
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
                JsonSerializer.Serialize(writer, c, options);
                break;
            }
            
            case ConvexCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case DelayServiceCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case SuperAdditiveCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case ConstantCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case SigmaRhoArrivalCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case ConcaveCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case FlowControlCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case SubAdditiveCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case StairCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }
            
            case StepCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }
            
            case RaisedRateLatencyServiceCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
                break;
            }

            case TwoRatesServiceCurve c:
            {
                JsonSerializer.Serialize(writer, c, options);
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
                JsonSerializer.Serialize(writer, plain, options);
                break;
            }
        }
    }
}