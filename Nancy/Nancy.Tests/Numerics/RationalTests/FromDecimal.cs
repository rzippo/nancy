using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class FromDecimal
{
    public static IEnumerable<object[]> GetDecimals()
    {
        var decimalTuples = new []
        {
            (128.3m, 1283, 10),
            (0.5m, 1, 2)
        };

        foreach (var tuple in decimalTuples)
            yield return new object[] { tuple.Item1, tuple.Item2, tuple.Item3 };
    }

    [Theory]
    [MemberData(nameof(GetDecimals))]
    public void DecimalCtor(decimal d, int num, int den)
    {
        var r = new Rational(d);

        Assert.Equal(num, r.Numerator);
        Assert.Equal(den, r.Denominator);
    }
}