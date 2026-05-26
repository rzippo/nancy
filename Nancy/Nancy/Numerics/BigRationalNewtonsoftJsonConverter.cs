using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unipi.Nancy.Numerics;

/// <exclude />
/// <summary>
/// Custom Newtonsoft.Json JsonConverter for <see cref="BigRational"/>.
/// </summary>
public class BigRationalNewtonsoftJsonConverter : JsonConverter
{
    private static readonly string NumeratorName = "num";
    private static readonly string DenominatorName = "den";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(BigRational));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var jt = JToken.Load(reader);
        if (jt.Type == JTokenType.Integer)
        {
            return new BigRational(jt.ToObject<int>());
        }

        if (jt.Type == JTokenType.String)
        {
            var value = BigInteger.Parse(jt.ToString());
            return new BigRational(value);
        }

        var numTkn = jt[NumeratorName]!;
        var denTkn = jt[DenominatorName]!;

        BigInteger num, den;
        if (numTkn.Type == JTokenType.Integer)
            num = numTkn.ToObject<long>();
        else
            num = BigInteger.Parse(numTkn.ToString());

        if (denTkn.Type == JTokenType.Integer)
            den = denTkn.ToObject<long>();
        else
            den = BigInteger.Parse(denTkn.ToString());

        return new BigRational(num, den);
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        BigRational rational = (BigRational) value;

        if (rational.Denominator == 1)
        {
            writer.WriteValue(rational.Numerator);
        }
        else
        {
            JObject jo = new JObject
            {
                {NumeratorName, JToken.FromObject(rational.Numerator, serializer)},
                {DenominatorName, JToken.FromObject(rational.Denominator, serializer)},
            };
            jo.WriteTo(writer);
        }
    }
}