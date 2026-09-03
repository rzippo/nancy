using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class CheapCacheThresholdDeprecationShim
{
    [Fact]
    public void ObsoleteSegmentThresholdKeepsTheDefaultItHadWhenItWasTheOnlyOne()
    {
#pragma warning disable CS0618
        Assert.Equal(20, new CacheSettings().CheapCacheSegmentThreshold);
#pragma warning restore CS0618
    }

    [Fact]
    public void ObsoleteSegmentThresholdIsKeptAsTwiceAsManyElements()
    {
#pragma warning disable CS0618
        var settings = new CacheSettings { CheapCacheSegmentThreshold = 15 };
#pragma warning restore CS0618

        Assert.Equal(30, settings.CheapCacheElementThreshold);
    }

    [Fact]
    public void ObsoleteSegmentThresholdReadsBackAsHalfTheElementThreshold()
    {
        var settings = new CacheSettings { CheapCacheElementThreshold = 30 };

#pragma warning disable CS0618
        Assert.Equal(15, settings.CheapCacheSegmentThreshold);
#pragma warning restore CS0618
    }

    [Fact]
    public void ObsoleteSegmentThresholdSurvivesAWithExpression()
    {
#pragma warning disable CS0618
        var settings = new CacheSettings() with { CheapCacheSegmentThreshold = 15 };

        Assert.Equal(15, settings.CheapCacheSegmentThreshold);
#pragma warning restore CS0618
        Assert.Equal(30, settings.CheapCacheElementThreshold);
    }

    [Fact]
    public void ANodeIsStillJudgedCheapByAThresholdSetThroughTheObsoleteName()
    {
        var a = new ConcreteCurveExpression(new ConstantCurve(1), "a");
        var b = new ConcreteCurveExpression(new ConstantCurve(2), "b");
        // A negative threshold forces not-cheap whatever the actual size, the element count never being negative.
#pragma warning disable CS0618
        var alwaysNotCheap = new ExpressionSettings
            { CacheSettings = new CacheSettings { CheapCacheSegmentThreshold = -1 } };
#pragma warning restore CS0618
        var root = a.Addition(b, settings: alwaysNotCheap);
        root.ComputeWithoutResult();
        Assert.True(root.IsComputed);

        root.ClearValueCache(CacheClearScope.Subtree);

        Assert.False(root.IsComputed);
    }
}
