using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra;

public class CurveExtensionsTests
{
    public enum CurveAggregationOperation
    {
        Addition,
        Minimum,
        Maximum,
        Convolution,
        MaxPlusConvolution
    }

    public static List<List<Curve>> EmptyEquivalentCases =
    [
        []
    ];

    public static IEnumerable<object[]> GetEmptyEquivalentCases()
        => EmptyEquivalentCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetEmptyEquivalentCases))]
    public void Equivalent_EmptyEnumerable_Throws(List<Curve> curves)
    {
        Assert.Throws<ArgumentException>(() => curves.AsEnumerable().Equivalent());
    }

    public static List<(List<Curve> curves, bool expected)> EquivalentCases =
    [
        (
            new List<Curve>
            {
                new ConstantCurve(5),
                new ConstantCurve(5)
            },
            true
        ),
        (
            new List<Curve>
            {
                new ConstantCurve(5),
                new ConstantCurve(10)
            },
            false
        ),
        (
            new List<Curve>
            {
                new RateLatencyServiceCurve(5, 3),
                new RateLatencyServiceCurve(5, 3),
                new RateLatencyServiceCurve(5, 3)
            },
            true
        )
    ];

    public static IEnumerable<object[]> GetEquivalentCases()
        => EquivalentCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetEquivalentCases))]
    public void Equivalent_IEnumerable_ReturnsExpected(List<Curve> curves, bool expected)
    {
        Assert.Equal(expected, curves.AsEnumerable().Equivalent());
    }

    public static List<(CurveAggregationOperation operation, List<Curve> curves)> CurveAggregationCases =
    [
        (
            CurveAggregationOperation.Addition,
            new List<Curve>
            {
                new ConstantCurve(5),
                new ConstantCurve(10)
            }
        ),
        (
            CurveAggregationOperation.Minimum,
            new List<Curve>
            {
                new ConstantCurve(5),
                new ConstantCurve(10)
            }
        ),
        (
            CurveAggregationOperation.Maximum,
            new List<Curve>
            {
                new ConstantCurve(5),
                new ConstantCurve(10)
            }
        ),
        (
            CurveAggregationOperation.Convolution,
            new List<Curve>
            {
                new ConstantCurve(5),
                new ConstantCurve(10)
            }
        ),
        (
            CurveAggregationOperation.MaxPlusConvolution,
            new List<Curve>
            {
                new ConstantCurve(5),
                new ConstantCurve(10)
            }
        ),
        (
            CurveAggregationOperation.Addition,
            new List<Curve>
            {
                new RateLatencyServiceCurve(5, 3),
                new ConstantCurve(2),
                new ConstantCurve(4)
            }
        ),
        (
            CurveAggregationOperation.Minimum,
            new List<Curve>
            {
                new RateLatencyServiceCurve(5, 3),
                new ConstantCurve(10),
                new ConstantCurve(12)
            }
        ),
        (
            CurveAggregationOperation.Maximum,
            new List<Curve>
            {
                new RateLatencyServiceCurve(5, 3),
                new ConstantCurve(10),
                new ConstantCurve(12)
            }
        ),
        (
            CurveAggregationOperation.Convolution,
            new List<Curve>
            {
                new RateLatencyServiceCurve(5, 3),
                new RateLatencyServiceCurve(7, 2),
                new ConstantCurve(4)
            }
        ),
        (
            CurveAggregationOperation.MaxPlusConvolution,
            new List<Curve>
            {
                new ConstantCurve(5),
                new ConstantCurve(10),
                new ConstantCurve(15)
            }
        )
    ];

    public static IEnumerable<object[]> GetCurveAggregationCases()
        => CurveAggregationCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetCurveAggregationCases))]
    public void Aggregation_IEnumerable_MatchesStatic(CurveAggregationOperation operation, List<Curve> curves)
    {
        var enumerable = curves.AsEnumerable();

        var result = InvokeExtension(operation, enumerable);
        var expected = InvokeStatic(operation, enumerable);

        Assert.True(expected.Equivalent(result));
    }

    [Theory]
    [MemberData(nameof(GetCurveAggregationCases))]
    public void Aggregation_IReadOnlyCollection_MatchesStatic(CurveAggregationOperation operation, List<Curve> curves)
    {
        IReadOnlyCollection<Curve> collection = curves;

        var result = InvokeExtension(operation, collection);
        var expected = InvokeStatic(operation, collection);

        Assert.True(expected.Equivalent(result));
    }

    private static Curve InvokeExtension(CurveAggregationOperation operation, IEnumerable<Curve> curves) =>
        operation switch
        {
            CurveAggregationOperation.Addition => curves.Addition(),
            CurveAggregationOperation.Minimum => curves.Minimum(),
            CurveAggregationOperation.Maximum => curves.Maximum(),
            CurveAggregationOperation.Convolution => curves.Convolution(),
            CurveAggregationOperation.MaxPlusConvolution => curves.MaxPlusConvolution(),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    private static Curve InvokeExtension(CurveAggregationOperation operation, IReadOnlyCollection<Curve> curves) =>
        operation switch
        {
            CurveAggregationOperation.Addition => curves.Addition(),
            CurveAggregationOperation.Minimum => curves.Minimum(),
            CurveAggregationOperation.Maximum => curves.Maximum(),
            CurveAggregationOperation.Convolution => curves.Convolution(),
            CurveAggregationOperation.MaxPlusConvolution => curves.MaxPlusConvolution(),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    private static Curve InvokeStatic(CurveAggregationOperation operation, IEnumerable<Curve> curves) =>
        operation switch
        {
            CurveAggregationOperation.Addition => Curve.Addition(curves),
            CurveAggregationOperation.Minimum => Curve.Minimum(curves),
            CurveAggregationOperation.Maximum => Curve.Maximum(curves),
            CurveAggregationOperation.Convolution => Curve.Convolution(curves),
            CurveAggregationOperation.MaxPlusConvolution => Curve.MaxPlusConvolution(curves),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    private static Curve InvokeStatic(CurveAggregationOperation operation, IReadOnlyCollection<Curve> curves) =>
        operation switch
        {
            CurveAggregationOperation.Addition => Curve.Addition(curves),
            CurveAggregationOperation.Minimum => Curve.Minimum(curves),
            CurveAggregationOperation.Maximum => Curve.Maximum(curves),
            CurveAggregationOperation.Convolution => Curve.Convolution(curves),
            CurveAggregationOperation.MaxPlusConvolution => Curve.MaxPlusConvolution(curves),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
}
