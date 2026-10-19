using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Addition
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Addition(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SigmaRho_RateLatency()
    {
        SigmaRhoArrivalCurve arrival = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        RateLatencyServiceCurve service = new RateLatencyServiceCurve(rate: 20, latency: 10);

        Curve sum = arrival + service;

        Assert.False(sum.IsContinuous);
        Assert.True(sum.IsContinuousExceptOrigin);
        Assert.True(sum.IsLeftContinuous);
        Assert.True(sum.IsFinite);
        Assert.True(sum.IsUltimatelyPlain);
        Assert.True(sum.IsPlain);
        Assert.True(sum.IsUltimatelyAffine);

        Assert.Equal(0, sum.ValueAt(0));
        Assert.Equal(150, sum.ValueAt(10));
        Assert.Equal(400, sum.ValueAt(20));
        //Assert.Equal(250, fun.ValueAt(30));

        Assert.Equal(5, sum.GetSegmentAfter(3).Slope);
        Assert.Equal(25, sum.GetSegmentAfter(13).Slope);
        Assert.Equal(25, sum.GetSegmentAfter(23).Slope);
    }

    [Fact]
    public void SigmaRho_RateLatency_Commutative()
    {
        SigmaRhoArrivalCurve arrival = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        RateLatencyServiceCurve service = new RateLatencyServiceCurve(rate: 20, latency: 10);

        Curve sum = service + arrival;

        Assert.False(sum.IsContinuous);
        Assert.True(sum.IsContinuousExceptOrigin);
        Assert.True(sum.IsLeftContinuous);
        Assert.True(sum.IsFinite);
        Assert.True(sum.IsUltimatelyPlain);
        Assert.True(sum.IsPlain);
        Assert.True(sum.IsUltimatelyAffine);

        Assert.Equal(0, sum.ValueAt(0));
        Assert.Equal(150, sum.ValueAt(10));
        Assert.Equal(400, sum.ValueAt(20));
        //Assert.Equal(250, fun.ValueAt(30));

        Assert.Equal(5, sum.GetSegmentAfter(3).Slope);
        Assert.Equal(25, sum.GetSegmentAfter(13).Slope);
        Assert.Equal(25, sum.GetSegmentAfter(23).Slope);
    }
    
    public static List<(Curve a, Curve b, Curve expected)> KnownCases =
    [
        (
            new StairCurve(1, 1),
            new ConstantCurve(2), 
            new Curve(new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 3),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 4)
                ]),
                1, 1, 1
            )
        ),
        (
            new StairCurve(1, 1),
            Curve.Zero(), 
            new StairCurve(1, 1)
        )
    ];

    public static IEnumerable<object[]> EquivalenceTestCases =
        KnownCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(EquivalenceTestCases))]
    public void AdditionEquivalence(Curve a, Curve b, Curve expected)
    {
        var aPb = a + b;
        var bPa = b + a;

        _testOutputHelper.WriteLine(a.ToCodeString());
        _testOutputHelper.WriteLine(b.ToCodeString());
        _testOutputHelper.WriteLine(expected.ToCodeString());
        _testOutputHelper.WriteLine(aPb.ToCodeString());
        _testOutputHelper.WriteLine(bPa.ToCodeString());

        Assert.True(Curve.Equivalent(expected, aPb));
        Assert.True(Curve.Equivalent(aPb, bPa));
    }

    /// <summary>
    /// The staircase of period 2, written with a period of 4: added to a staircase of period 2, its sum has a period of 4, which minimization shortens to 2.
    /// </summary>
    private static readonly Curve StaircaseWithDoubledPeriod =
        new Curve(new StairCurve(1, 2).Cut(0, 8), 4, 4, 2);

    public static IEnumerable<object[]> AdditionFormsAndMinimization()
    {
        var forms = new (string name, Func<Curve, Curve, ComputationSettings, Curve> add)[]
        {
            ("instance", (a, b, s) => a.Addition(b, s)),
            ("static", (a, b, s) => Curve.Addition(a, b, s)),
            ("enumerable of two", (a, b, s) => Curve.Addition((IEnumerable<Curve>) new List<Curve> { a, b }, s)),
            ("collection of two", (a, b, s) => Curve.Addition((IReadOnlyCollection<Curve>) new List<Curve> { a, b }, s)),
            ("collection of three", (a, b, s) => Curve.Addition((IReadOnlyCollection<Curve>) new List<Curve> { a, b, b }, s)),
        };
        foreach (var (name, add) in forms)
        foreach (var useMinimization in new[] { true, false })
            yield return new object[] { name, add, useMinimization };
    }

    /// <summary>
    /// Every form of the addition honours <see cref="ComputationSettings.UseRepresentationMinimization"/>.
    /// </summary>
    [Theory]
    [MemberData(nameof(AdditionFormsAndMinimization))]
    public void EveryFormHonoursRepresentationMinimization(
        string form, Func<Curve, Curve, ComputationSettings, Curve> add, bool useMinimization)
    {
        _testOutputHelper.WriteLine(form);
        var settings = new ComputationSettings { UseRepresentationMinimization = useMinimization };
        var staircase = new StairCurve(1, 2);

        var sum = add(staircase, StaircaseWithDoubledPeriod, settings);

        Assert.Equal(useMinimization ? 2 : 4, sum.PseudoPeriodLength);
    }
}
