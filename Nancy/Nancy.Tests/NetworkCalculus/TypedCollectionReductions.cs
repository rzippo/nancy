using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class TypedCollectionReductions
{
    [Fact]
    public void ConcaveMinimumUsesTheSameVirtualOperationForBothOverloads()
    {
        var first = new ObservedConcave();
        ConcaveCurve[] operands = [first, new SigmaRhoArrivalCurve(3, 2), new SigmaRhoArrivalCurve(2, 3)];
        var settings = new ComputationSettings { UseParallelism = false };
        var collection = ConcaveCurveExtensions.Minimum((IReadOnlyCollection<ConcaveCurve>) operands, settings);
        Assert.Equal(1, first.MinimumCalls);
        var enumerable = ConcaveCurveExtensions.Minimum((IEnumerable<ConcaveCurve>) operands, settings);
        Assert.Equal(2, first.MinimumCalls);
        Assert.Same(settings, first.LastSettings);
        Assert.True(Curve.Equivalent(collection, enumerable, settings));
    }

    [Fact]
    public void ConvexMaximumUsesTheSameVirtualOperationForBothOverloads()
    {
        var first = new ObservedConvex();
        ConvexCurve[] operands = [first, new RateLatencyServiceCurve(3, 2), new RateLatencyServiceCurve(1, 3)];
        var settings = new ComputationSettings { UseParallelism = false };
        var collection = ConvexCurveExtensions.Maximum((IReadOnlyCollection<ConvexCurve>) operands, settings);
        Assert.Equal(1, first.MaximumCalls);
        var enumerable = ConvexCurveExtensions.Maximum((IEnumerable<ConvexCurve>) operands, settings);
        Assert.Equal(2, first.MaximumCalls);
        Assert.Same(settings, first.LastSettings);
        Assert.True(Curve.Equivalent(collection, enumerable, settings));
    }

    [Fact]
    public void ConvexConvolutionUsesTheSameVirtualOperationForBothOverloads()
    {
        var first = new ObservedConvex();
        ConvexCurve[] operands = [first, new RateLatencyServiceCurve(3, 2), new RateLatencyServiceCurve(1, 3)];
        var settings = new ComputationSettings { UseParallelism = false };
        var collection = ConvexCurveExtensions.Convolution((IReadOnlyCollection<ConvexCurve>) operands, settings);
        Assert.Equal(1, first.ConvolutionCalls);
        var enumerable = ConvexCurveExtensions.Convolution((IEnumerable<ConvexCurve>) operands, settings);
        Assert.Equal(2, first.ConvolutionCalls);
        Assert.Same(settings, first.LastSettings);
        Assert.True(Curve.Equivalent(collection, enumerable, settings));
    }

    private sealed class ObservedConcave() : ConcaveCurve(new SigmaRhoArrivalCurve(1, 4))
    {
        public int MinimumCalls { get; private set; }
        public ComputationSettings? LastSettings { get; private set; }
        public override Curve Minimum(Curve curve, ComputationSettings? settings = null)
        {
            MinimumCalls++;
            LastSettings = settings;
            return base.Minimum(curve, settings);
        }
    }

    private sealed class ObservedConvex() : ConvexCurve(new RateLatencyServiceCurve(2, 1))
    {
        public int MaximumCalls { get; private set; }
        public int ConvolutionCalls { get; private set; }
        public ComputationSettings? LastSettings { get; private set; }
        public override Curve Maximum(Curve curve, ComputationSettings? settings = null)
        {
            MaximumCalls++;
            LastSettings = settings;
            return base.Maximum(curve, settings);
        }
        public override Curve Convolution(Curve curve, ComputationSettings? settings = null)
        {
            ConvolutionCalls++;
            LastSettings = settings;
            return base.Convolution(curve, settings);
        }
    }
}
