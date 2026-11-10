using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Utility;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra;

public class ParallelFailureContracts
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public void ReductionPreservesTheWorkerException(int degreeOfParallelism)
    {
        var failure = new ArithmeticException("Reduction failure");
        [MethodImpl(MethodImplOptions.NoInlining)]
        int Combine(int left, int right)
        {
            if (left == 1)
                throw failure;
            return left + right;
        }

        Assert.Same(failure, Assert.Throws<ArithmeticException>(() =>
            BalancedReduction.Aggregate(new[] { 1, 2, 3, 4 }, Combine, degreeOfParallelism)));
        Assert.Contains(nameof(Combine), failure.StackTrace);
    }

    [Fact]
    public void EndpointCollectionPreservesTheInputException()
    {
        var failure = new ArithmeticException("Endpoint failure");
        var elements = new FailingEndpoints(failure);
        Assert.Same(failure, Assert.Throws<ArithmeticException>(() => IntervalBucket.ComputeIntervals(elements,
            new ComputationSettings { UseParallelism = false })));
        Assert.Same(failure, Assert.Throws<ArithmeticException>(() => IntervalBucket.ComputeIntervals(elements,
            new ComputationSettings { UseParallelComputeIntervals = true, ComputeIntervalsParallelizationThreshold = 0 })));
    }

    [Fact]
    public void IntervalInsertionPreservesTheInputException()
    {
        var failure = new ArithmeticException("Insertion failure");
        Assert.Same(failure, Assert.Throws<ArithmeticException>(() => IntervalBucket.ComputeIntervals(new FailingInsertion(failure),
            new ComputationSettings { UseParallelism = false })));
        Assert.Same(failure, Assert.Throws<ArithmeticException>(() => IntervalBucket.ComputeIntervals(new FailingInsertion(failure),
            new ComputationSettings { UseParallelComputeIntervals = true, ComputeIntervalsParallelizationThreshold = 0 })));
    }

    [Fact]
    public void ElementSortingPreservesTheInputException()
    {
        var failure = new ArithmeticException("Sort failure");
        Assert.Same(failure, Assert.Throws<ArithmeticException>(() => new FailingSort(failure).SortElements(
            new ComputationSettings { UseParallelism = false })));
        Assert.Same(failure, Assert.Throws<ArithmeticException>(() => new FailingSort(failure).SortElements(
            new ComputationSettings { UseParallelSortElements = true, SortElementsParallelizationThreshold = 0 })));
    }

    #if LONG_RATIONAL
    [Fact]
    public void DeconvolutionPreservesWorkerOverflow()
    {
        var left = new Sequence([new Point(0, long.MaxValue)]);
        var right = new Sequence([new Point(0, -1)]);
        Assert.Throws<OverflowException>(() => left.Deconvolution(right,
            settings: new ComputationSettings { UseParallelism = false }));
        Assert.Throws<OverflowException>(() => left.Deconvolution(right,
            settings: new ComputationSettings { UseParallelDeconvolution = true, DeconvolutionParallelizationThreshold = 0 }));
    }

    [Fact]
    public void ConvolutionPreservesWorkerOverflow()
    {
        var left = new Sequence([new Point(0, long.MaxValue), Segment.Zero(0, 1)]);
        var right = new Sequence([new Point(0, 1), Segment.Zero(0, 1)]);
        Assert.Throws<OverflowException>(() => left.Convolution(right,
            new ComputationSettings { UseParallelism = false, UseConvolutionPartitioning = false }));
        Assert.Throws<OverflowException>(() => left.Convolution(right,
            new ComputationSettings { UseParallelConvolution = true, ConvolutionParallelizationThreshold = 0, UseConvolutionPartitioning = false }));
        Assert.Throws<OverflowException>(() => left.Convolution(right,
            new ComputationSettings { UseParallelConvolution = true, ConvolutionPartitioningThreshold = 1, UseConvolutionPartitioning = true }));
    }

    [Fact]
    public void MaxPlusConvolutionPreservesWorkerOverflow()
    {
        var left = new Sequence([new Point(0, long.MaxValue), Segment.Zero(0, 1)]);
        var right = new Sequence([new Point(0, long.MaxValue), Segment.Zero(0, 1)]);
        Assert.Throws<OverflowException>(() => left.MaxPlusConvolution(right,
            new ComputationSettings { UseParallelism = false, UseConvolutionPartitioning = false }));
        Assert.Throws<OverflowException>(() => left.MaxPlusConvolution(right,
            new ComputationSettings { UseParallelConvolution = true, ConvolutionParallelizationThreshold = 0, UseConvolutionPartitioning = false }));
        Assert.Throws<OverflowException>(() => left.MaxPlusConvolution(right,
            new ComputationSettings { UseParallelConvolution = true, ConvolutionPartitioningThreshold = 1, UseConvolutionPartitioning = true }));
    }

    [Fact]
    public void LowerEnvelopePreservesWorkerOverflow()
    {
        Element[] elements = [new Segment(0, 1, long.MaxValue, long.MaxValue), new Point(new Rational(1, 2), 0)];
        Assert.Throws<OverflowException>(() => elements.LowerEnvelope(new ComputationSettings { UseParallelism = false }));
        Assert.Throws<OverflowException>(() => elements.LowerEnvelope(new ComputationSettings
        {
            UseParallelism = false, UseParallelLowerEnvelope = true, ElementsLowerEnvelopeParallelizationThreshold = 0
        }));
    }

    [Fact]
    public void UpperEnvelopePreservesWorkerOverflow()
    {
        Element[] elements = [new Segment(0, 1, long.MaxValue, long.MaxValue), new Point(new Rational(1, 2), 0)];
        Assert.Throws<OverflowException>(() => elements.UpperEnvelope(new ComputationSettings { UseParallelism = false }));
        Assert.Throws<OverflowException>(() => elements.UpperEnvelope(new ComputationSettings
        {
            UseParallelism = false, UseParallelUpperEnvelope = true, ElementsUpperEnvelopeParallelizationThreshold = 0
        }));
    }

    [Fact]
    public void EndpointSortingPreservesComparisonFailure()
    {
        var values = Enumerable.Repeat(new Rational(long.MaxValue, 2), 8192).ToArray();
        Assert.Throws<OverflowException>(() => IntervalEndpointSorter.Sort(values[..2]));
        Assert.Throws<OverflowException>(() => IntervalEndpointSorter.Sort((Rational[]) values.Clone(), 4, 4096));
    }
    [Fact]
    public void EndpointMergingPreservesComparisonFailure()
    {
        var values = Enumerable.Repeat(new Rational(long.MaxValue, 1), 4096)
            .Concat(Enumerable.Repeat(new Rational(1, long.MaxValue), 4096))
            .ToArray();
        Assert.Throws<OverflowException>(() => IntervalEndpointSorter.Sort([values[0], values[^1]]));
        Assert.Throws<OverflowException>(() => IntervalEndpointSorter.Sort(values, 4, 4096));
    }
    #endif

    private sealed class FailingEndpoints(Exception failure) : IReadOnlyList<Element>
    {
        public int Count => 2;
        public Element this[int index] => index == 0 ? new Point(0, 0) : throw failure;
        public IEnumerator<Element> GetEnumerator()
        {
            yield return this[0];
            yield return this[1];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class FailingInsertion(Exception failure) : IReadOnlyList<Element>
    {
        private int indexedReads;
        private bool endpointsRead;
        public int Count => 2;
        public Element this[int index]
        {
            get
            {
                if (Interlocked.Increment(ref indexedReads) == Count)
                    endpointsRead = true;
                return new Point(index, 0);
            }
        }
        public IEnumerator<Element> GetEnumerator()
        {
            if (endpointsRead) throw failure;
            yield return new Point(0, 0);
            yield return new Point(1, 0);
            endpointsRead = true;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class FailingSort(Exception failure) : IReadOnlyList<Element>
    {
        private int enumerations;
        public int Count => 2;
        public Element this[int index] => new Point(1 - index, 0);
        public IEnumerator<Element> GetEnumerator()
        {
            if (++enumerations > 1) throw failure;
            yield return this[0];
            yield return this[1];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
