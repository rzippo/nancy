using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra.Json;


/// <exclude />
/// <summary>
/// Custom Newtonsoft.Json JsonConverter for <see cref="Curve"/>.
/// </summary>
public class CurveNewtonsoftJsonConverter : JsonConverter
{
    public const string TypeName = "type";

    public static readonly string BaseSequenceName = "baseSequence";
    public static readonly string PseudoPeriodStartName = "pseudoPeriodStart";
    public static readonly string PseudoPeriodLengthName = "pseudoPeriodLength";
    public static readonly string PseudoPeriodHeightName = "pseudoPeriodHeight";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Curve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        Sequence? sequence = jo[BaseSequenceName]?.ToObject<Sequence>();
        if (sequence == null)
            throw new InvalidOperationException("Invalid JSON format.");
        Rational periodStart = jo[PseudoPeriodStartName]!.ToObject<Rational>();
        Rational periodLength = jo[PseudoPeriodLengthName]!.ToObject<Rational>();
        Rational periodHeight = jo[PseudoPeriodHeightName]!.ToObject<Rational>();

        Curve curve = new Curve(
            baseSequence: sequence,
            pseudoPeriodStart: periodStart,
            pseudoPeriodLength: periodLength,
            pseudoPeriodHeight: periodHeight
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        Curve curve = (Curve) value;

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(Curve.TypeCode, serializer) },
            { BaseSequenceName, JToken.FromObject(curve.BaseSequence, serializer) },
            { PseudoPeriodStartName, JToken.FromObject(curve.PseudoPeriodStart, serializer) },
            { PseudoPeriodLengthName, JToken.FromObject(curve.PseudoPeriodLength, serializer) },
            { PseudoPeriodHeightName, JToken.FromObject(curve.PseudoPeriodHeight, serializer) }
        };

        jo.WriteTo(writer);
    }
}