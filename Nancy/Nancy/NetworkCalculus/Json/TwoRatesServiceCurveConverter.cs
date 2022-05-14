using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="RateLatencyServiceCurve"/>.
/// </summary>
public class TwoRatesServiceCurveConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <summary>
    /// Code used in JSON output to distinguish this type 
    /// </summary>
    public const string TypeCode = "twoRatesServiceCurve";

    private static readonly string DelayName = "delay";
    private static readonly string TransientRateName = "transientRate";
    private static readonly string TransientEndName = "transientEnd";
    private static readonly string SteadyRateName = "steadyRate";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(TwoRatesServiceCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        Rational delay = jo[DelayName]!.ToObject<Rational>();
        Rational transientRate = jo[TransientRateName]!.ToObject<Rational>();
        Rational transientEnd = jo[TransientEndName]!.ToObject<Rational>();
        Rational steadyRate = jo[SteadyRateName]!.ToObject<Rational>();

        TwoRatesServiceCurve curve = new TwoRatesServiceCurve(
            delay: delay,
            transientRate: transientRate,
            transientEnd: transientEnd,
            steadyRate: steadyRate
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        TwoRatesServiceCurve curve = (TwoRatesServiceCurve) value;

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(TypeCode) },
            { DelayName, JToken.FromObject(curve.Delay) },
            { TransientRateName, JToken.FromObject(curve.TransientRate) },
            { TransientEndName, JToken.FromObject(curve.TransientEnd) },
            { SteadyRateName, JToken.FromObject(curve.SteadyRate) }
        };

        jo.WriteTo(writer);
    }
}