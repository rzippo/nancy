using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class SubAdditiveCurveTests
{
    public static List<Curve> NonRegularSubAdditiveConstructorCases =
    [
        new Curve(
            baseSequence: new Sequence(
                [
                    new Point(0, 5),
                    Segment.Constant(0, 1, 5)
                ]
            ),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        )
    ];

    public static IEnumerable<object[]> GetNonRegularSubAdditiveConstructorCases()
        => NonRegularSubAdditiveConstructorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetNonRegularSubAdditiveConstructorCases))]
    public void Constructor_WithTest_RejectsNonRegularSubAdditiveCurve(Curve curve)
    {
        Assert.Throws<InvalidOperationException>(() => new SubAdditiveCurve(curve));
        Assert.Throws<InvalidOperationException>(() => new SubAdditiveCurve(
            curve.BaseSequence,
            curve.PseudoPeriodStart,
            curve.PseudoPeriodLength,
            curve.PseudoPeriodHeight
        ));
    }

    [Theory]
    [MemberData(nameof(GetNonRegularSubAdditiveConstructorCases))]
    public void Constructor_WhenTestIsSkipped_UsesDeclaredSubAdditiveState(Curve curve)
    {
        var subAdditiveCurve = new SubAdditiveCurve(curve, doTest: false);

        Assert.True(subAdditiveCurve.IsSubAdditive);
        Assert.True(subAdditiveCurve.IsSubAdditiveCheck());
        Assert.False(subAdditiveCurve.IsRegularSubAdditiveCheck());
    }

    public static List<SubAdditiveCurve> SubAdditiveCurves =
    [
        new FlowControlCurve(latency: 3, rate: 5, height: 2),
        new ConstantCurve(7)
    ];

    public static IEnumerable<object[]> GetSubAdditiveCurves()
        => SubAdditiveCurves.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetSubAdditiveCurves))]
    public void SubAdditiveClosure_ReturnsSameInstance(SubAdditiveCurve curve)
    {
        Assert.Same(curve, curve.SubAdditiveClosure());
    }

    [Theory]
    [MemberData(nameof(GetSubAdditiveCurves))]
    public void RegularSubAdditiveChecks_ReturnTrueForKnownRegularCurves(SubAdditiveCurve curve)
    {
        Assert.True(curve.IsSubAdditiveCheck());
        Assert.True(curve.IsRegularSubAdditiveCheck());
    }

    public static List<(SubAdditiveCurve left, SubAdditiveCurve right)> ComparableSubAdditivePairs =
    [
        (
            new FlowControlCurve(latency: 3, rate: 5, height: 2),
            new FlowControlCurve(latency: 3, rate: 5, height: 5)
        ),
        (
            new ConstantCurve(4),
            new ConstantCurve(9)
        )
    ];

    public static IEnumerable<object[]> GetComparableSubAdditivePairs()
        => ComparableSubAdditivePairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetComparableSubAdditivePairs))]
    public void OperatorConvolution_MatchesTypedConvolution(SubAdditiveCurve left, SubAdditiveCurve right)
    {
        var fromOperator = left * right;
        var fromMethod = left.Convolution(right);

        Assert.True(fromMethod.Equivalent(fromOperator));
    }

    [Theory]
    [MemberData(nameof(GetComparableSubAdditivePairs))]
    public void EstimateConvolution_ReturnsZeroWhenOneOperandDominatesMinimum(
        SubAdditiveCurve left,
        SubAdditiveCurve right)
    {
        Assert.Equal(0, left.EstimateConvolution(right));
        Assert.Equal(0, left.EstimateConvolution(right, countElements: true));
    }

    [Theory]
    [MemberData(nameof(GetComparableSubAdditivePairs))]
    public void EstimateConvolution_AsCurveUsesSubAdditiveOptimization(
        SubAdditiveCurve left,
        SubAdditiveCurve right)
    {
        Assert.Equal(
            left.EstimateConvolution(right),
            left.EstimateConvolution((Curve)right)
        );
        Assert.Equal(
            left.EstimateConvolution(right, countElements: true),
            left.EstimateConvolution((Curve)right, countElements: true)
        );
    }

    [Theory]
    [MemberData(nameof(GetComparableSubAdditivePairs))]
    public void EstimateConvolution_AsCurveCanBypassSubAdditiveOptimizations(
        SubAdditiveCurve left,
        SubAdditiveCurve right)
    {
        var settings = new ComputationSettings
        {
            UseSubAdditiveConvolutionOptimizations = false
        };

        var genericLeft = new Curve(left);
        var genericRight = new Curve(right);

        Assert.Equal(
            genericLeft.EstimateConvolution(genericRight, settings: settings),
            left.EstimateConvolution((Curve)right, settings: settings)
        );
    }

    [Theory]
    [MemberData(nameof(GetComparableSubAdditivePairs))]
    public void Convolution_AsCurveCanBypassSubAdditiveOptimizations(
        SubAdditiveCurve left,
        SubAdditiveCurve right)
    {
        var settings = new ComputationSettings
        {
            UseSubAdditiveConvolutionOptimizations = false
        };

        var typed = left.Convolution(right, settings);
        var asCurve = left.Convolution((Curve)right, settings);

        Assert.True(typed.Equivalent(asCurve));
    }

    public static List<(SubAdditiveCurve left, SubAdditiveCurve right)> NonDominatingEstimatePairs =
    [
        (
            new FlowControlCurve(4, 12, 4),
            new FlowControlCurve(3, 11, 3)
        ),
        (
            new SubAdditiveCurve(
                baseSequence: new Sequence(
                    [
                        Point.Origin(),
                        new Segment(0, 3, 1, 3),
                        new Point(3, 10),
                        new Segment(3, 6, 10, 2)
                    ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 6
            ),
            new SubAdditiveCurve(
                baseSequence: new Sequence(
                    [
                        Point.Origin(),
                        new Segment(0, 2, 0, 4),
                        new Point(2, 8),
                        new Segment(2, 5, 8, new Rational(2, 3)),
                        new Point(5, 10),
                        new Segment(5, 10, 10, 2)
                    ]),
                pseudoPeriodStart: 5,
                pseudoPeriodLength: 5,
                pseudoPeriodHeight: 10
            )
        )
    ];

    public static IEnumerable<object[]> GetNonDominatingEstimatePairs()
        => NonDominatingEstimatePairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetNonDominatingEstimatePairs))]
    public void EstimateConvolution_ForNonDominatingPairsReturnsFiniteWorkEstimate(
        SubAdditiveCurve left,
        SubAdditiveCurve right)
    {
        var pairsEstimate = left.EstimateConvolution(right);
        var elementEstimate = left.EstimateConvolution(right, countElements: true);

        Assert.True(pairsEstimate > 0);
        Assert.True(elementEstimate >= pairsEstimate);
    }

    public static List<List<SubAdditiveCurve>> SubAdditiveConvolutionLists =
    [
        [
            new FlowControlCurve(latency: 3, rate: 5, height: 2)
        ],
        [
            new FlowControlCurve(latency: 3, rate: 5, height: 2),
            new FlowControlCurve(latency: 3, rate: 5, height: 5),
            new ConstantCurve(10)
        ]
    ];

    public static IEnumerable<object[]> GetSubAdditiveConvolutionLists()
        => SubAdditiveConvolutionLists.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetSubAdditiveConvolutionLists))]
    public void StaticConvolution_OrdersAndConvolvesSubAdditiveCurves(List<SubAdditiveCurve> curves)
    {
        var result = SubAdditiveCurve.Convolution(curves);
        var generic = Curve.Convolution(curves.Select(c => new Curve(c)));

        Assert.True(generic.Equivalent(result));
    }
}
