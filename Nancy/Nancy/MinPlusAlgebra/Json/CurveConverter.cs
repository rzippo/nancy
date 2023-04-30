using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra.Json;


/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="Curve"/>.
/// </summary>
public class CurveConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <summary>
    /// Type identification constant for JSON (de)serialization.
    /// </summary>
    public const string TypeCode = "curve";

    private static readonly string BaseSequenceName = "sequence";
    private static readonly string PseudoPeriodStartTimeName = "periodStart";
    private static readonly string PseudoPeriodLengthName = "periodLength";
    private static readonly string PseudoPeriodHeightName = "periodHeight";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Curve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalConverter());

        Sequence? sequence = jo[BaseSequenceName]?.ToObject<Sequence>();
        if (sequence == null)
            throw new InvalidOperationException("Invalid JSON format.");
        Rational periodStart = jo[PseudoPeriodStartTimeName]!.ToObject<Rational>();
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

        serializer.Converters.Add(new RationalConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(TypeCode, serializer) },
            { BaseSequenceName, JToken.FromObject(curve.BaseSequence, serializer) },
            { PseudoPeriodStartTimeName, JToken.FromObject(curve.PseudoPeriodStart, serializer) },
            { PseudoPeriodLengthName, JToken.FromObject(curve.PseudoPeriodLength, serializer) },
            { PseudoPeriodHeightName, JToken.FromObject(curve.PseudoPeriodHeight, serializer) }
        };

        jo.WriteTo(writer);
    }
}