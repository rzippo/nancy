using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="TwoRatesServiceCurve"/>.
/// </summary>
public class TwoRatesServiceCurveNewtonsoftJsonConverter : JsonConverter
{
    private const string TypeName = "type";

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

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        Rational delay = jo[DelayName]!.ToObject<Rational>(serializer);
        Rational transientRate = jo[TransientRateName]!.ToObject<Rational>(serializer);
        Rational transientEnd = jo[TransientEndName]!.ToObject<Rational>(serializer);
        Rational steadyRate = jo[SteadyRateName]!.ToObject<Rational>(serializer);

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

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(TwoRatesServiceCurve.TypeCode, serializer) },
            { DelayName, JToken.FromObject(curve.Delay, serializer) },
            { TransientRateName, JToken.FromObject(curve.TransientRate, serializer) },
            { TransientEndName, JToken.FromObject(curve.TransientEnd, serializer) },
            { SteadyRateName, JToken.FromObject(curve.SteadyRate, serializer) }
        };

        jo.WriteTo(writer);
    }
}