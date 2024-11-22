using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unipi.Nancy.Numerics;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="Rational"/>.
/// </summary>
public class RationalSystemJsonConverter : JsonConverter<Rational>
{
    // ugly hack...
    #if BIG_RATIONAL
        internal record PlainRational(
            [property: JsonConverter(typeof(BigIntegerSystemJsonConverter))]
            BigInteger num, 
            [property: JsonConverter(typeof(BigIntegerSystemJsonConverter))]
            BigInteger den
        );
    #else
        internal record PlainRational(long num, long den);
    #endif

    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override Rational Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                if (reader.TryGetInt32(out int i))
                {
                    return new Rational(i);
                }
                else if (reader.TryGetInt64(out long l))
                {
                    return new Rational(l);
                }
                else if (reader.TryGetDecimal(out decimal d))
                {
                    return (Rational)d;
                }
                else
                    throw new JsonException($"Could not parse Rational from: {reader.GetString()}");
            }

            #if BIG_RATIONAL
            case JsonTokenType.String:
            {
                if (BigInteger.TryParse(reader.GetString(), out BigInteger bi))
                {
                    return new Rational(bi);
                }
                else
                    throw new JsonException($"Could not parse Rational from: {reader.GetString()}");
            }
            #endif

            default:
            {
                var plain = JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.PlainRational);
                if (plain == null)
                    throw new JsonException();
                return new Rational(plain.num, plain.den);
            }
        }
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        Rational value,
        JsonSerializerOptions options)
    {
        if (value.Denominator == 1)
        {
            #if BIG_RATIONAL
            new BigIntegerSystemJsonConverter().Write(writer, value.Numerator, options);
            #else
            writer.WriteNumberValue(value.Numerator);
            #endif
        }
        else
        {
            var plain = new PlainRational(value.Numerator, value.Denominator);
            JsonSerializer.Serialize(writer, plain, NancyJsonSerializerContext.Default.PlainRational);
        }
    }
}