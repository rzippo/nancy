using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class NormalizationInvariant
{
    public static List<(Rational value, string label)> Rationals =
    [
        (new Rational(), "default constructor"),
        (new Rational(5), "int numerator only"),
        (new Rational(4, 6), "int pair, simplifiable"),
        (new Rational(-3, 4), "int pair, negative numerator"),
        (new Rational(3, -4), "int pair, negative denominator"),
        (new Rational(0, 5), "int pair, zero numerator"),
        (new Rational(7, 3), "int pair, already simple"),
        (new Rational(8, 12), "int pair, simplifiable"),
        (new Rational(2L, 4L), "long pair, simplifiable"),
        (new Rational(5L, 7L), "long pair, already simple"),
        (new Rational(0.5m), "decimal 0.5"),
        (new Rational(1.25m), "decimal 1.25"),
        (Rational.Zero, "Zero constant"),
        (Rational.One, "One constant"),
        (Rational.MinusOne, "MinusOne constant"),
        (Rational.PlusInfinity, "PlusInfinity constant"),
        (Rational.MinusInfinity, "MinusInfinity constant"),
        (new Rational(1, 2) + new Rational(1, 3), "addition operator"),
        (new Rational(1, 2) - new Rational(1, 3), "subtraction operator"),
        (new Rational(1, 2) * new Rational(2, 3), "multiplication operator"),
        (new Rational(1, 2) / new Rational(3, 4), "division operator"),
        (-new Rational(1, 2), "unary negation operator"),
        (Rational.Add(new Rational(1, 2), new Rational(1, 3)), "Add"),
        (Rational.Subtract(new Rational(1, 2), new Rational(1, 3)), "Subtract"),
        (Rational.Multiply(new Rational(1, 2), new Rational(2, 3)), "Multiply"),
        (Rational.Divide(new Rational(1, 2), new Rational(3, 4)), "Divide"),
        (Rational.Negate(new Rational(1, 2)), "Negate"),
        (Rational.Abs(new Rational(-1, 2)), "Abs"),
        (Rational.Invert(new Rational(2, 3)), "Invert"),
        (Rational.Max(new Rational(1, 2), new Rational(1, 3)), "Max"),
        (Rational.Min(new Rational(1, 2), new Rational(1, 3)), "Min"),
        #if BIG_RATIONAL
        // big integer cases
        (new Rational(new BigInteger(2), new BigInteger(4)), "BigInteger pair, simplifiable"),
        (new Rational(new BigInteger(5)), "BigInteger numerator only"),
        (new Rational(new BigInteger(12), new BigInteger(8)), "BigInteger pair, simplifiable"),
        (new Rational(new BigInteger(-2), new BigInteger(-4)), "BigInteger pair, both negative"),
        #endif
        // Newtonsoft.JsonConvert conversion test cases
        (JsonConvert.DeserializeObject<Rational>("{\"num\":2,\"den\":4}"), "JsonConvert.DeserializeObject<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<Rational>("{\"num\":3,\"den\":9}"), "JsonConvert.DeserializeObject<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<Rational>("{\"num\":6,\"den\":4}"), "JsonConvert.DeserializeObject<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<Rational>("{\"num\":-2,\"den\":4}"), "JsonConvert.DeserializeObject<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<Rational>("{\"num\":0,\"den\":5}"), "JsonConvert.DeserializeObject<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        // System.Text.Json conversion test cases
        (System.Text.Json.JsonSerializer.Deserialize<Rational>("{\"num\":2,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<Rational>("{\"num\":3,\"den\":9}"), "System.Text.Json.JsonSerializer.Deserialize<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<Rational>("{\"num\":6,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<Rational>("{\"num\":-2,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<Rational>("{\"num\":0,\"den\":5}"), "System.Text.Json.JsonSerializer.Deserialize<Rational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
    ];

    public static IEnumerable<object[]> GetRationalTestCases()
        => Rationals.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetRationalTestCases))]
    public void ValueIsNormalized(Rational value, string label)
    {
        Assert.True(IsNormalized(value), $"Value from {label} is not normalized: {value}");
    }

#if BIG_RATIONAL
    static bool IsNormalized(Rational r)
    {
        var num = r.Numerator;
        var den = r.Denominator;

        if (den.IsZero) return true;
        if (den.Sign < 0) return false;
        if (num.IsZero) return den.IsOne;
        if (den.IsOne || num.IsOne || num == BigInteger.MinusOne) return true;

        return BigInteger.GreatestCommonDivisor(BigInteger.Abs(num), den).IsOne;
    }
#else
    static bool IsNormalized(Rational r)
    {
        var num = r.Numerator;
        var den = r.Denominator;

        if (den == 0) return true;
        if (den < 0) return false;
        if (num == 0) return den == 1;
        if (den == 1 || num == 1 || num == -1) return true;

        var gcd = BigInteger.GreatestCommonDivisor(
            BigInteger.Abs(new BigInteger(num)),
            new BigInteger(den)
        );
        return gcd.IsOne;
    }
#endif
}
