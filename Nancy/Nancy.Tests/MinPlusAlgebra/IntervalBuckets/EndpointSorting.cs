using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.IntervalBuckets;

/// <summary>
/// Checks exact endpoint sorting across chunk boundaries, incomplete runs and repeated values.
/// </summary>
public class EndpointSorting
{
    public static IEnumerable<object[]> Inputs()
    {
        foreach (int count in new[] { 0, 1, 4096, 4097, 8193, 16385 })
        {
            var values = Enumerable.Range(0, count)
                .Select(i => new Rational((i * 7919) % 1009 - 504, 1 + i % 7))
                .ToArray();
            if (count > 1)
            {
                values[0] = Rational.PlusInfinity;
                values[count - 1] = Rational.MinusInfinity;
            }
            yield return new object[] { values };
        }

        var ordered = Enumerable.Range(-4096, 8193)
            .Select(i => (Rational)i)
            .ToArray();
        yield return new object[] { ordered };
        yield return new object[] { Enumerable.Reverse(ordered).ToArray() };
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void SortProducesTheOrderedMultiset(Rational[] input)
    {
        var expected = (Rational[])input.Clone();
        Array.Sort(expected);
        var actual = (Rational[])input.Clone();

        IntervalEndpointSorter.Sort(actual);

        Assert.Equal(expected, actual);
    }
}
