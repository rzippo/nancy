using System.Collections.Generic;
using Unipi.Nancy.NetworkCalculus;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class FifoResidualSc
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FifoResidualSc(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void FifoResidualTest()
    {
        var beta = new RateLatencyServiceCurve(10, 5);

        var foi = new SigmaRhoArrivalCurve(5, 1).ToExpression("a_0"); 
        var contendingFlows = new List<CurveExpression>
        {
            new SigmaRhoArrivalCurve(2, 1).ToExpression("a_1"),
            new SigmaRhoArrivalCurve(3, 2).ToExpression("a_2"),
            new SigmaRhoArrivalCurve(5, 2).ToExpression("a_3"),
        };
        
        var delta_theta_0 = Expressions.FromCurve(new DelayServiceCurve(0), "d_0");
        
        var b_theta_0 = Expressions.Subtraction(
                beta, 
                Expressions.Addition(contendingFlows)
                .Convolution(delta_theta_0)
            )
            .ToNonNegative()
            .Minimum(delta_theta_0);
        var wcd_0 = Expressions.HorizontalDeviation(foi, b_theta_0);
        _testOutputHelper.WriteLine(wcd_0.ToString());
        Assert.Equal(13, wcd_0.Compute());

        var delta_theta_1 = Expressions.FromCurve(new DelayServiceCurve(1), "d_1");
        var b_theta_1 = b_theta_0.ReplaceByValue(delta_theta_0, delta_theta_1);
        var wcd_1 = Expressions.HorizontalDeviation(foi, b_theta_1);
        _testOutputHelper.WriteLine(wcd_1.ToString());
        Assert.Equal(12, wcd_1.Compute());
        
        var delta_theta_10 = Expressions.FromCurve(new DelayServiceCurve(10), "d_10");
        var b_theta_10 = b_theta_0.ReplaceByValue(delta_theta_0, delta_theta_10);
        var wcd_10 = Expressions.HorizontalDeviation(foi, b_theta_10);
        _testOutputHelper.WriteLine(wcd_10.ToString());
        Assert.Equal(10, wcd_10.Compute());
        
        var delta_theta_20 = Expressions.FromCurve(new DelayServiceCurve(20), "d_20");
        var b_theta_20 = b_theta_0.ReplaceByValue(delta_theta_0, delta_theta_20);
        var wcd_20 = Expressions.HorizontalDeviation(foi, b_theta_20);
        _testOutputHelper.WriteLine(wcd_20.ToString());
        Assert.Equal(20, wcd_20.Compute());
    }
}