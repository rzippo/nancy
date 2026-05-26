using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class NormalizationInvariant
{
    public static List<(LongRational value, string label)> LongRationals =
    [
        (new LongRational(), "default constructor"),
        (new LongRational(5), "int numerator only"),
        (new LongRational(4, 6), "int pair, simplifiable"),
        (new LongRational(-3, 4), "int pair, negative numerator"),
        (new LongRational(3, -4), "int pair, negative denominator"),
        (new LongRational(0, 5), "int pair, zero numerator"),
        (new LongRational(7, 3), "int pair, already simple"),
        (new LongRational(8, 12), "int pair, simplifiable"),
        (new LongRational(2L, 4L), "long pair, simplifiable"),
        (new LongRational(5L, 7L), "long pair, already simple"),
        (new LongRational(0.5m), "decimal 0.5"),
        (new LongRational(1.25m), "decimal 1.25"),
        (LongRational.Zero, "Zero constant"),
        (LongRational.One, "One constant"),
        (LongRational.MinusOne, "MinusOne constant"),
        (LongRational.PlusInfinity, "PlusInfinity constant"),
        (LongRational.MinusInfinity, "MinusInfinity constant"),
        (new LongRational(1, 2) + new LongRational(1, 3), "addition operator"),
        (new LongRational(1, 2) - new LongRational(1, 3), "subtraction operator"),
        (new LongRational(1, 2) * new LongRational(2, 3), "multiplication operator"),
        (new LongRational(1, 2) / new LongRational(3, 4), "division operator"),
        (-new LongRational(1, 2), "unary negation operator"),
        (LongRational.Add(new LongRational(1, 2), new LongRational(1, 3)), "Add"),
        (LongRational.Subtract(new LongRational(1, 2), new LongRational(1, 3)), "Subtract"),
        (LongRational.Multiply(new LongRational(1, 2), new LongRational(2, 3)), "Multiply"),
        (LongRational.Divide(new LongRational(1, 2), new LongRational(3, 4)), "Divide"),
        (LongRational.Negate(new LongRational(1, 2)), "Negate"),
        (LongRational.Abs(new LongRational(-1, 2)), "Abs"),
        (LongRational.Invert(new LongRational(2, 3)), "Invert"),
        (LongRational.Max(new LongRational(1, 2), new LongRational(1, 3)), "Max"),
        (LongRational.Min(new LongRational(1, 2), new LongRational(1, 3)), "Min"),
        // Newtonsoft.JsonConvert conversion test cases
        (JsonConvert.DeserializeObject<LongRational>("{\"num\":2,\"den\":4}"), "JsonConvert.DeserializeObject<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<LongRational>("{\"num\":3,\"den\":9}"), "JsonConvert.DeserializeObject<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<LongRational>("{\"num\":6,\"den\":4}"), "JsonConvert.DeserializeObject<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<LongRational>("{\"num\":-2,\"den\":4}"), "JsonConvert.DeserializeObject<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (JsonConvert.DeserializeObject<LongRational>("{\"num\":0,\"den\":5}"), "JsonConvert.DeserializeObject<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        // System.Text.Json conversion test cases
        (System.Text.Json.JsonSerializer.Deserialize<LongRational>("{\"num\":2,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<LongRational>("{\"num\":3,\"den\":9}"), "System.Text.Json.JsonSerializer.Deserialize<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<LongRational>("{\"num\":6,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<LongRational>("{\"num\":-2,\"den\":4}"), "System.Text.Json.JsonSerializer.Deserialize<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),
        (System.Text.Json.JsonSerializer.Deserialize<LongRational>("{\"num\":0,\"den\":5}"), "System.Text.Json.JsonSerializer.Deserialize<LongRational>(\"{\\\"num\\\":2,\\\"den\\\":4}\")"),

    ];

    public static IEnumerable<object[]> GetRationalTestCases()
        => LongRationals.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetRationalTestCases))]
    public void ValueIsNormalized(LongRational value, string label)
    {
        Assert.True(IsNormalized(value), $"Value from {label} is not normalized: {value}");
    }

    static bool IsNormalized(LongRational r)
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
}
