using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unipi.Nancy.Numerics;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="Rational"/>.
/// </summary>
public class RationalConverter : JsonConverter
{
    private static readonly string NumeratorName = "num";
    private static readonly string DenominatorName = "den";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Rational));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var jt = JToken.Load(reader);
        if (jt.Type == JTokenType.Integer)
        {
            return new Rational(jt.ToObject<int>());
        }
#if BIG_RATIONAL
        if (jt.Type == JTokenType.String)
        {
            var value = BigInteger.Parse(jt.ToString());
            return new Rational(value);
        }
#endif
        var numTkn = jt[NumeratorName]!;
        var denTkn = jt[DenominatorName]!;

#if BIG_RATIONAL
        BigInteger num, den;
        if (numTkn.Type == JTokenType.Integer)
            num = numTkn.ToObject<long>();
        else
            num = BigInteger.Parse(numTkn.ToString());

        if (denTkn.Type == JTokenType.Integer)
            den = denTkn.ToObject<long>();
        else
            den = BigInteger.Parse(denTkn.ToString());
#else
        long num, den;
        if (numTkn.Type == JTokenType.Integer)
            num = numTkn.ToObject<long>();
        else
            num = long.Parse(numTkn.ToString());

        if (denTkn.Type == JTokenType.Integer)
            den = denTkn.ToObject<long>();
        else
            den = long.Parse(denTkn.ToString());
#endif
        return new Rational(num, den);
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        Rational rational = (Rational) value;

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