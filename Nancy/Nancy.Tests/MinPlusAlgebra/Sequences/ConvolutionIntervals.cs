using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class ConvolutionIntervals
{
    public static IEnumerable<(Sequence Left, Sequence Right, Rational? End, bool IncludeEnd, bool IsZero)> Inputs()
    {
        for (int shapeF = 0; shapeF < 5; shapeF++)
        for (int shapeG = 0; shapeG < 5; shapeG++)
        for (int ends = 0; ends < 16; ends++)
        for (int cut = 0; cut < 3; cut++)
            yield return (Make(1, 3, shapeF, (ends & 1) != 0, (ends & 2) != 0),
                Make(2, 4, shapeG, (ends & 4) != 0, (ends & 8) != 0),
                cut == 0 ? null : (Rational)4, cut == 2, shapeF == 0 && shapeG == 0);
    }

    public static IEnumerable<object[]> Cases() => Inputs()
        .Select(c => new object[] { c.Left, c.Right, c.End!, c.IncludeEnd, c.IsZero });

    [Theory]
    [MemberData(nameof(Cases))]
    public void MinPlusConvolutionUsesTheSumSupportAndPreservesValidValues(Sequence f, Sequence g, Rational? end, bool includeEnd, bool isZero)
    {
        var direct = new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = false };
        var optimized = direct with { UseBySequenceConvolutionIsospeedOptimization = true };
        var expected = Sequence.Convolution(f, g, direct, cutEnd: end, isEndIncluded: includeEnd, useIsomorphism: true);
        var actual = Sequence.Convolution(f, g, optimized, cutEnd: end, isEndIncluded: includeEnd, useIsomorphism: true);
        AssertSupportAndValues(f, g, end, includeEnd, isZero, expected, actual);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void MaxPlusConvolutionUsesTheSumSupportAndPreservesValidValues(Sequence f, Sequence g, Rational? end, bool includeEnd, bool isZero)
    {
        var direct = new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = false };
        var optimized = direct with { UseBySequenceConvolutionIsospeedOptimization = true };
        var expected = Sequence.MaxPlusConvolution(f, g, direct, cutEnd: end, isEndIncluded: includeEnd, useIsomorphism: true);
        var actual = Sequence.MaxPlusConvolution(f, g, optimized, cutEnd: end, isEndIncluded: includeEnd, useIsomorphism: true);
        AssertSupportAndValues(f, g, end, includeEnd, isZero, expected, actual);
    }

    private static void AssertSupportAndValues(Sequence f, Sequence g, Rational? end, bool includeEnd, bool isZero, Sequence expected, Sequence actual)
    {
        var support = Interval.PairwiseSum(f.Support, g.Support)!.Value;
        if (end is not null)
            support = Interval.Intersection(support, Interval.UnboundedBelow(end.Value, includeEnd))!.Value;
        Assert.Equal(support, expected.Support);
        Assert.Equal(support, actual.Support);
        var valid = Interval.Intersection(support, Interval.UnboundedBelow(5, f.IsRightClosed && g.IsRightClosed))!.Value;
        Assert.True(Sequence.Equivalent(expected.Cut(valid), actual.Cut(valid)));
        if (isZero)
            Assert.True(Sequence.Equivalent(Sequence.Constant(0, support.Lower, support.Upper, support.IsLowerIncluded, support.IsUpperIncluded), actual));
    }

    public static IEnumerable<ComputationSettings> Policies()
    {
        var serial = new ComputationSettings
        {
            UseParallelism = false, MaxDegreeOfParallelism = 2,
            UseConvolutionPartitioning = false, ConvolutionPartitioningThreshold = 1,
            ConvolutionParallelizationThreshold = 0, UseBySequenceConvolutionIsospeedOptimization = false
        };
        yield return serial;
        yield return serial with { UseParallelism = true };
        yield return serial with { UseConvolutionPartitioning = true };
    }

    public static IEnumerable<object[]> ExecutionPolicies() => Policies()
        .Select(settings => new object[] { settings });

    public static IEnumerable<object[]> EndpointCases => [[false], [true]];

    [Theory]
    [MemberData(nameof(ExecutionPolicies))]
    public void MinPlusExecutionPoliciesRespectTheIntervalCut(ComputationSettings settings)
    {
        var f = Make(1, 3, 0, false, true);
        var g = Make(2, 4, 0, true, true);
        var result = Sequence.Convolution(f, g, settings, cutEnd: 4, isEndIncluded: false);
        Assert.True(Sequence.Equivalent(Sequence.Constant(0, 3, 4, false, false), result));
    }

    [Theory]
    [MemberData(nameof(EndpointCases))]
    public void MinPlusNeutralInfiniteOperandsRetainEndpointInclusion(bool leftClosed)
    {
        var value = Rational.PlusInfinity;
        var f = Sequence.Constant(value, 1, 3, leftClosed, true);
        var g = Sequence.Constant(0, 2, 4, true, true);
        var result = Sequence.Convolution(f, g);
        Assert.True(Sequence.Equivalent(Sequence.Constant(value, 3, 7, leftClosed, true), result));
    }

    [Theory]
    [MemberData(nameof(EndpointCases))]
    public void MinPlusUnboundedOperandsKeepInfinityAsAnOpenBound(bool leftClosed)
    {
        var f = Sequence.Constant(0, 1, Rational.PlusInfinity, leftClosed);
        var g = Sequence.Constant(0, 2, 4, true, true);
        var settings = new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = true };
        var result = Sequence.Convolution(f, g, settings, useIsomorphism: true);
        Assert.True(Sequence.Equivalent(Sequence.Constant(0, 3, Rational.PlusInfinity, leftClosed), result));
    }

    [Theory]
    [MemberData(nameof(ExecutionPolicies))]
    public void MaxPlusExecutionPoliciesRespectTheIntervalCut(ComputationSettings settings)
    {
        var f = Make(1, 3, 0, false, true);
        var g = Make(2, 4, 0, true, true);
        var result = Sequence.MaxPlusConvolution(f, g, settings, cutEnd: 4, isEndIncluded: false);
        Assert.True(Sequence.Equivalent(Sequence.Constant(0, 3, 4, false, false), result));
    }

    [Theory]
    [MemberData(nameof(EndpointCases))]
    public void MaxPlusNeutralInfiniteOperandsRetainEndpointInclusion(bool leftClosed)
    {
        var value = Rational.MinusInfinity;
        var f = Sequence.Constant(value, 1, 3, leftClosed, true);
        var g = Sequence.Constant(0, 2, 4, true, true);
        var result = Sequence.MaxPlusConvolution(f, g);
        Assert.True(Sequence.Equivalent(Sequence.Constant(value, 3, 7, leftClosed, true), result));
    }

    [Theory]
    [MemberData(nameof(EndpointCases))]
    public void MaxPlusUnboundedOperandsKeepInfinityAsAnOpenBound(bool leftClosed)
    {
        var f = Sequence.Constant(0, 1, Rational.PlusInfinity, leftClosed);
        var g = Sequence.Constant(0, 2, 4, true, true);
        var settings = new ComputationSettings { UseParallelism = false, UseBySequenceConvolutionIsospeedOptimization = true };
        var result = Sequence.MaxPlusConvolution(f, g, settings, useIsomorphism: true);
        Assert.True(Sequence.Equivalent(Sequence.Constant(0, 3, Rational.PlusInfinity, leftClosed), result));
    }

    private static Sequence Make(int a, int b, int shape, bool left, bool right)
    {
        int mid = a + 1, slope1 = shape == 2 ? 1 : 0, slope2 = shape == 1 ? 1 : 0;
        var elements = new List<Element>();
        if (left) elements.Add(new Point(a, 0));
        elements.Add(new Segment(a, mid, 0, slope1));
        elements.Add(new Point(mid, shape == 3 ? 1 : slope1));
        elements.Add(new Segment(mid, b, shape >= 3 ? 1 : slope1, slope2));
        if (right) elements.Add(new Point(b, shape >= 3 ? 1 : slope1 + slope2));
        return new Sequence(elements);
    }
}
