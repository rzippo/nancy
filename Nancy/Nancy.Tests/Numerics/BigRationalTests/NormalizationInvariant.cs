using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class NormalizationInvariant
{
    public static List<(BigRational value, string label)> BigRationals =
    [
        (new BigRational(), "default constructor"),
        (new BigRational(5), "int numerator only"),
        (new BigRational(4, 6), "int pair, simplifiable"),
        (new BigRational(-3, 4), "int pair, negative numerator"),
        (new BigRational(3, -4), "int pair, negative denominator"),
        (new BigRational(0, 5), "int pair, zero numerator"),
        (new BigRational(7, 3), "int pair, already simple"),
        (new BigRational(8, 12), "int pair, simplifiable"),
        (new BigRational(2L, 4L), "long pair, simplifiable"),
        (new BigRational(5L, 7L), "long pair, already simple"),
        (new BigRational(0.5m), "decimal 0.5"),
        (new BigRational(1.25m), "decimal 1.25"),
        (BigRational.Zero, "Zero constant"),
        (BigRational.One, "One constant"),
        (BigRational.MinusOne, "MinusOne constant"),
        (BigRational.PlusInfinity, "PlusInfinity constant"),
        (BigRational.MinusInfinity, "MinusInfinity constant"),
        (new BigRational(1, 2) + new BigRational(1, 3), "addition operator"),
        (new BigRational(1, 2) - new BigRational(1, 3), "subtraction operator"),
        (new BigRational(1, 2) * new BigRational(2, 3), "multiplication operator"),
        (new BigRational(1, 2) / new BigRational(3, 4), "division operator"),
        (-new BigRational(1, 2), "unary negation operator"),
        (BigRational.Add(new BigRational(1, 2), new BigRational(1, 3)), "Add"),
        (BigRational.Subtract(new BigRational(1, 2), new BigRational(1, 3)), "Subtract"),
        (BigRational.Multiply(new BigRational(1, 2), new BigRational(2, 3)), "Multiply"),
        (BigRational.Divide(new BigRational(1, 2), new BigRational(3, 4)), "Divide"),
        (BigRational.Negate(new BigRational(1, 2)), "Negate"),
        (BigRational.Abs(new BigRational(-1, 2)), "Abs"),
        (BigRational.Invert(new BigRational(2, 3)), "Invert"),
        (BigRational.Max(new BigRational(1, 2), new BigRational(1, 3)), "Max"),
        (BigRational.Min(new BigRational(1, 2), new BigRational(1, 3)), "Min"),
        (new BigRational(new BigInteger(2), new BigInteger(4)), "BigInteger pair, simplifiable"),
        (new BigRational(new BigInteger(5)), "BigInteger numerator only"),
        (new BigRational(new BigInteger(12), new BigInteger(8)), "BigInteger pair, simplifiable"),
        (new BigRational(new BigInteger(-2), new BigInteger(-4)), "BigInteger pair, both negative"),
        // Newtonsoft.JsonConvert conversion test cases
        (JsonConvert.DeserializeObject<BigRational>("{\"num\":2,\"den\":4}"), "JsonConvert.DeserializeObject<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<BigRational>("{\"num\":3,\"den\":9}"), "JsonConvert.DeserializeObject<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<BigRational>("{\"num\":6,\"den\":4}"), "JsonConvert.DeserializeObject<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<BigRational>("{\"num\":-2,\"den\":4}"), "JsonConvert.DeserializeObject<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<BigRational>("{\"num\":0,\"den\":5}"), "JsonConvert.DeserializeObject<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        // System.Text.Json conversion test cases
        (System.Text.Json.JsonSerializer.Deserialize<BigRational>("{\"num\":2,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<BigRational>("{\"num\":3,\"den\":9}"), "System.Text.Json.JsonSerializer.Deserialize<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<BigRational>("{\"num\":6,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<BigRational>("{\"num\":-2,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<BigRational>("{\"num\":0,\"den\":5}"), "System.Text.Json.JsonSerializer.Deserialize<BigRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
    ];

    public static IEnumerable<object[]> GetBigRationalTestCases()
        => BigRationals.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBigRationalTestCases))]
    public void ValueIsNormalized(BigRational value, string label)
    {
        Assert.True(IsNormalized(value), $"Value from {label} is not normalized: {value}");
    }

    static bool IsNormalized(BigRational r)
    {
        var num = r.Numerator;
        var den = r.Denominator;

        if (den.IsZero) return true;
        if (den.Sign < 0) return false;
        if (num.IsZero) return den.IsOne;
        if (den.IsOne || num.IsOne || num == BigInteger.MinusOne) return true;

        return BigInteger.GreatestCommonDivisor(BigInteger.Abs(num), den).IsOne;
    }
}
