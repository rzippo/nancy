using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra;

public class EmptyInputContracts
{
    [Fact]
    public void DelayServiceCurveConvolutionNamesTheEmptyCollection()
    {
        Check(Assert.Throws<ArgumentException>(() => DelayServiceCurve.Convolution(Array.Empty<DelayServiceCurve>())), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void RateLatencyServiceCurveConvolutionNamesTheEmptyCollection()
    {
        Check(Assert.Throws<ArgumentException>(() => RateLatencyServiceCurve.Convolution(Array.Empty<RateLatencyServiceCurve>())), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void SubAdditiveCurveConvolutionNamesTheEmptyCollection()
    {
        Check(Assert.Throws<ArgumentException>(() => SubAdditiveCurve.Convolution(Array.Empty<SubAdditiveCurve>())), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void FlowControlCurveConvolutionNamesTheEmptyCollection()
    {
        Check(Assert.Throws<ArgumentException>(() => FlowControlCurve.Convolution(Array.Empty<FlowControlCurve>())), "curves", "The collection of curves is empty.");
    }

    private static void Check(ArgumentException error, string parameter, string description)
    {
        Assert.Equal(parameter, error.ParamName);
        Assert.StartsWith(description, error.Message);
    }

    [Fact]
    public void CurveAdditionRejectsBothEmptyOverloads()
    {
        Curve[] curves = [];
        Check(Assert.Throws<ArgumentException>(() => Curve.Addition((IReadOnlyCollection<Curve>) curves)), "curves", "The collection of curves is empty.");
        Check(Assert.Throws<ArgumentException>(() => Curve.Addition((IEnumerable<Curve>) curves)), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void CurveMinimumRejectsBothEmptyOverloads()
    {
        Curve[] curves = [];
        Check(Assert.Throws<ArgumentException>(() => Curve.Minimum((IReadOnlyCollection<Curve>) curves)), "curves", "The collection of curves is empty.");
        Check(Assert.Throws<ArgumentException>(() => Curve.Minimum((IEnumerable<Curve>) curves)), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void CurveMaximumRejectsBothEmptyOverloads()
    {
        Curve[] curves = [];
        Check(Assert.Throws<ArgumentException>(() => Curve.Maximum((IReadOnlyCollection<Curve>) curves)), "curves", "The collection of curves is empty.");
        Check(Assert.Throws<ArgumentException>(() => Curve.Maximum((IEnumerable<Curve>) curves)), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void CurveMaxPlusConvolutionRejectsBothEmptyOverloads()
    {
        Curve[] curves = [];
        Check(Assert.Throws<ArgumentException>(() => Curve.MaxPlusConvolution((IReadOnlyCollection<Curve>) curves)), "curves", "The collection of curves is empty.");
        Check(Assert.Throws<ArgumentException>(() => Curve.MaxPlusConvolution((IEnumerable<Curve>) curves)), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void ConcaveMinimumRejectsBothEmptyOverloads()
    {
        ConcaveCurve[] curves = [];
        Check(Assert.Throws<ArgumentException>(() => ConcaveCurveExtensions.Minimum((IReadOnlyCollection<ConcaveCurve>) curves)), "curves", "The collection of curves is empty.");
        Check(Assert.Throws<ArgumentException>(() => ConcaveCurveExtensions.Minimum((IEnumerable<ConcaveCurve>) curves)), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void ConcaveConvolutionRejectsBothEmptyOverloads()
    {
        ConcaveCurve[] curves = [];
        Check(Assert.Throws<ArgumentException>(() => ConcaveCurveExtensions.Convolution((IReadOnlyCollection<ConcaveCurve>) curves)), "curves", "The collection of curves is empty.");
        Check(Assert.Throws<ArgumentException>(() => ConcaveCurveExtensions.Convolution((IEnumerable<ConcaveCurve>) curves)), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void ConvexMaximumRejectsBothEmptyOverloads()
    {
        ConvexCurve[] curves = [];
        Check(Assert.Throws<ArgumentException>(() => ConvexCurveExtensions.Maximum((IReadOnlyCollection<ConvexCurve>) curves)), "curves", "The collection of curves is empty.");
        Check(Assert.Throws<ArgumentException>(() => ConvexCurveExtensions.Maximum((IEnumerable<ConvexCurve>) curves)), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void ConvexConvolutionRejectsBothEmptyOverloads()
    {
        ConvexCurve[] curves = [];
        Check(Assert.Throws<ArgumentException>(() => ConvexCurveExtensions.Convolution((IReadOnlyCollection<ConvexCurve>) curves)), "curves", "The collection of curves is empty.");
        Check(Assert.Throws<ArgumentException>(() => ConvexCurveExtensions.Convolution((IEnumerable<ConvexCurve>) curves)), "curves", "The collection of curves is empty.");
    }

    [Fact]
    public void ElementMinimumNamesTheEmptySet()
    {
        Check(Assert.Throws<ArgumentException>(() => Element.Minimum(Array.Empty<Element>())), "elements", "The set of elements is empty.");
    }

    [Fact]
    public void ElementMaximumNamesTheEmptySet()
    {
        Check(Assert.Throws<ArgumentException>(() => Element.Maximum(Array.Empty<Element>())), "elements", "The set of elements is empty.");
    }

    [Fact]
    public void SegmentMinimumNamesTheEmptySet()
    {
        Check(Assert.Throws<ArgumentException>(() => Segment.Minimum(Array.Empty<Segment>())), "segments", "The set of segments is empty.");
    }

    [Fact]
    public void SegmentMaximumNamesTheEmptySet()
    {
        Check(Assert.Throws<ArgumentException>(() => Segment.Maximum(Array.Empty<Segment>())), "segments", "The set of segments is empty.");
    }

    [Fact]
    public void IsContinuousNamesThePublicInputShape()
    {
        Check(Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsContinuous()), "elements", "The element array is empty.");
        Check(Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.IsContinuous()), "elements", "The element span is empty.");
        Check(Assert.Throws<ArgumentException>(() => ((IEnumerable<Element>) Array.Empty<Element>()).IsContinuous()), "elements", "The element stream is empty.");
        Check(Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().Where(_ => true).IsContinuous()), "elements", "The element stream is empty.");
    }

    [Fact]
    public void SupValueAndArgNamesThePublicInputShape()
    {
        Check(Assert.Throws<ArgumentException>(() => Array.Empty<Element>().SupValueAndArg()), "elements", "The element array is empty.");
        Check(Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.SupValueAndArg()), "elements", "The element span is empty.");
        Check(Assert.Throws<ArgumentException>(() => ((IEnumerable<Element>) Array.Empty<Element>()).SupValueAndArg()), "elements", "The element stream is empty.");
        Check(Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().Where(_ => true).SupValueAndArg()), "elements", "The element stream is empty.");
    }

    [Fact]
    public void AreInTimeOrderNamesThePublicInputShape()
    {
        Check(Assert.Throws<ArgumentException>(() => Array.Empty<Element>().AreInTimeOrder()), "elements", "The element array is empty.");
        Check(Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.AreInTimeOrder()), "elements", "The element span is empty.");
        Check(Assert.Throws<ArgumentException>(() => ((IEnumerable<Element>) Array.Empty<Element>()).AreInTimeOrder()), "elements", "The element collection is empty.");
        Check(Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().Where(_ => true).AreInTimeOrder()), "elements", "The element collection is empty.");
    }

    [Fact]
    public void LowerEnvelopeNamesTheEmptySet()
    {
        Check(Assert.Throws<ArgumentException>(() => Array.Empty<Element>().LowerEnvelope()), "elements", "The set of elements is empty.");
        Check(Assert.Throws<ArgumentException>(() => Array.Empty<Sequence>().LowerEnvelope()), "sequences", "The set of sequences is empty.");
    }

    [Fact]
    public void UpperEnvelopeNamesTheEmptySet()
    {
        Check(Assert.Throws<ArgumentException>(() => Array.Empty<Element>().UpperEnvelope()), "elements", "The set of elements is empty.");
        Check(Assert.Throws<ArgumentException>(() => Array.Empty<Sequence>().UpperEnvelope()), "sequences", "The set of sequences is empty.");
    }

    [Fact]
    public void IntervalConstructionNamesTheEmptySet()
    {
        Check(Assert.Throws<ArgumentException>(() => IntervalBucket.ComputeIntervals(Array.Empty<Element>())), "elements", "The set of elements is empty.");
    }
}
