#nullable enable

using System;
using System.Threading;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class ConcurrentCachePublication
{
    private sealed class Counter
    {
        public int Calls;
        public Action<int> OnCompute = _ => { };
        public void Compute() => OnCompute(Interlocked.Increment(ref Calls));
    }

    private sealed record CountedRational(Counter Counter) : RationalExpression
    {
        public override void Accept(IRationalExpressionVisitor visitor)
        {
            Counter.Compute();
            visitor.Visit(new RationalNumberExpression(new Rational(17, 23)));
        }
        public override T Accept<T>(IRationalExpressionVisitor<T> visitor) => throw new NotSupportedException();
    }

    private sealed record CountedCurve(Counter Counter) : CurveExpression
    {
        public override void Accept(ICurveExpressionVisitor visitor)
        {
            Counter.Compute();
            visitor.Visit(new ConcreteCurveExpression(Curve.Zero()));
        }
        public override T Accept<T>(ICurveExpressionVisitor<T> visitor) => throw new NotSupportedException();
    }

    private sealed record CountedSequence(Counter Counter) : SequenceExpression
    {
        public override void Accept(ISequenceExpressionVisitor visitor)
        {
            Counter.Compute();
            visitor.Visit(new ConcreteSequenceExpression(Curve.Zero().BaseSequence));
        }
        public override T Accept<T>(ISequenceExpressionVisitor<T> visitor) => throw new NotSupportedException();
    }

    private static (IExpression Node, Func<object> Read) Create(int kind, Counter counter)
    {
        switch (kind)
        {
            case 0:
                var rational = new CountedRational(counter);
                return (rational, () => rational.Value);
            case 1:
                var curve = new CountedCurve(counter);
                return (curve, () => curve.Value);
            default:
                var sequence = new CountedSequence(counter);
                return (sequence, () => sequence.Value);
        }
    }

    [Fact]
    public void ClearingOnlyTheFailedChildKeepsTheParentsFailure()
    {
        var expected = new InvalidOperationException("temporary evaluation failure");
        var counter = new Counter { OnCompute = _ => throw expected };
        var child = new CountedRational(counter);
        var parent = child.Addition(Expressions.FromRational(1));
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => parent.Compute()));
        counter.OnCompute = _ => { };
        child.ClearValueCache(CacheClearScope.SelfOnly, force: true);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => parent.Compute()));
        Assert.Equal(1, counter.Calls);
        parent.ClearValueCache(CacheClearScope.Subtree, force: true);
        Assert.Equal(new Rational(40, 23), parent.Compute());
        Assert.Equal(2, counter.Calls);
    }

    [Fact]
    public void ArithmeticFailureRetainsItsOriginalExceptionType()
    {
        var expression = Expressions.FromRational(0).Division(Expressions.FromRational(0));
        var error = Assert.Throws<UndeterminedResultException>(() => expression.Compute());
        Assert.Same(error, Assert.Throws<UndeterminedResultException>(() => expression.Compute()));
        expression.ClearValueCache(CacheClearScope.Subtree, force: true);
        Assert.NotSame(error, Assert.Throws<UndeterminedResultException>(() => expression.Compute()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FailureFromAnOldHolderLeavesTheReplacementUsable(int kind)
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var expected = new InvalidOperationException("old calculation failure");
        var counter = new Counter { OnCompute = n =>
        {
            if (n != 1) return;
            entered.Set();
            Assert.True(release.Wait(TimeSpan.FromSeconds(10)));
            throw expected;
        }};
        var (node, read) = Create(kind, counter);
        Exception? error = null;
        var oldReader = new Thread(() => error = Record.Exception(() => read())) { IsBackground = true };
        oldReader.Start();
        object? replacement = null;
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
            node.ClearValueCache(CacheClearScope.SelfOnly, force: true);
            replacement = read();
            Assert.True(node.IsComputed);
        }
        finally
        {
            release.Set();
            Assert.True(oldReader.Join(TimeSpan.FromSeconds(10)));
        }
        Assert.Same(expected, error);
        Assert.Equal(replacement, read());
        Assert.Equal(2, counter.Calls);
    }

    [Fact]
    public void RecursiveValueInitializationThrowsAndCanBeReset()
    {
        var counter = new Counter();
        var (node, read) = Create(0, counter);
        counter.OnCompute = _ => read();
        var error = Assert.Throws<InvalidOperationException>(() => read());
        Assert.Equal(1, counter.Calls);
        counter.OnCompute = _ => { };
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => read()));
        node.ClearValueCache(CacheClearScope.SelfOnly, force: true);
        Assert.Equal(new Rational(17, 23), read());
        Assert.Equal(2, counter.Calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ClearingDuringComputationCreatesAnIndependentCache(int kind)
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var counter = new Counter { OnCompute = n =>
        {
            if (n != 1) return;
            entered.Set();
            Assert.True(release.Wait(TimeSpan.FromSeconds(10)));
        }};
        var (node, read) = Create(kind, counter);
        object? oldValue = null;
        Exception? error = null;
        var oldReader = new Thread(() =>
        {
            try { oldValue = read(); }
            catch (Exception e) { error = e; }
        }) { IsBackground = true };
        oldReader.Start();
        object? freshValue = null;
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
            node.ClearValueCache(CacheClearScope.SelfOnly, force: true);
            Assert.False(node.IsComputed);
            freshValue = read();
            Assert.True(node.IsComputed);
            Assert.Equal(2, counter.Calls);
        }
        finally
        {
            release.Set();
            Assert.True(oldReader.Join(TimeSpan.FromSeconds(10)));
        }
        Assert.Null(error);
        Assert.Equal(freshValue, oldValue);
        Assert.Equal(freshValue, read());
        Assert.Equal(2, counter.Calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FactoryExceptionsRemainCachedUntilForcedClear(int kind)
    {
        var expected = new InvalidOperationException("evaluation failure");
        var counter = new Counter { OnCompute = _ => throw expected };
        var (node, read) = Create(kind, counter);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => read()));
        counter.OnCompute = _ => { };
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => read()));
        Assert.False(node.IsComputed);
        Assert.Equal(1, counter.Calls);
        node.ClearValueCache(CacheClearScope.SelfOnly, force: true);
        _ = read();
        Assert.True(node.IsComputed);
        Assert.Equal(2, counter.Calls);
    }
}
