using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class BooleanCacheLifecycle
{
    [Fact]
    public void AFailedPropertyRetainsItsFailureUntilReset()
    {
        var elements = new ControlledElement();
        var sequence = new Sequence([elements]);
        elements.FailReads = true;
        var failure = Assert.Throws<InvalidOperationException>(() => sequence.IsFinite);
        elements.FailReads = false;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => sequence.IsFinite));
        sequence.ResetCachedProperties();
        Assert.True(sequence.IsFinite);
    }

    [Fact]
    public async Task ConcurrentReadersShareOnePropertyInitialization()
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var elements = new ControlledElement();
        var sequence = new Sequence([elements]);
        elements.Entered = entered;
        elements.Release = release;
        var secondResult = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new Thread(() =>
        {
            try
            {
                secondResult.SetResult(sequence.IsFinite);
            }
            catch (Exception e)
            {
                secondResult.SetException(e);
            }
        });
        var first = Task.Run(() => sequence.IsFinite, TestContext.Current.CancellationToken);
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
            var holder = sequence._isFinite;
            Assert.NotNull(holder);
            second.Start();

            // The second reader must reach a wait before the first initializer is released.
            // ExecutionAndPublication waits on the holder; PublicationOnly enters the factory and waits on release.
            Assert.True(SpinWait.SpinUntil(
                () => (second.ThreadState & ThreadState.WaitSleepJoin) != 0,
                TimeSpan.FromSeconds(10)));
            Assert.Same(holder, sequence._isFinite);
            Assert.Equal(1, Volatile.Read(ref elements.PropertyEnumerations));
        }
        finally
        {
            release.Set();
            if ((second.ThreadState & ThreadState.Unstarted) == 0)
                Assert.True(second.Join(TimeSpan.FromSeconds(10)));
            Assert.True(await first);
        }
        Assert.True(await secondResult.Task);
        Assert.Equal(1, elements.PropertyEnumerations);
    }

    /// A point at time 0 whose infinity check, which IsFinite reads, can be made to fail or to wait.
    private sealed class ControlledElement : Element
    {
        public bool FailReads;
        public ManualResetEventSlim? Entered;
        public ManualResetEventSlim? Release;
        public int PropertyEnumerations;

        public override bool IsInfinite
        {
            get
            {
                if (FailReads)
                    throw new InvalidOperationException("Controlled read failure.");
                if (Entered is not null)
                {
                    Interlocked.Increment(ref PropertyEnumerations);
                    Entered.Set();
                    if (!Release!.Wait(TimeSpan.FromSeconds(10)))
                        throw new TimeoutException();
                }
                return false;
            }
        }

        public override string Type => "controlled";
        public override Rational StartTime => 0;
        public override Rational EndTime => 0;
        public override bool IsPlusInfinite => false;
        public override bool IsMinusInfinite => false;
        public override bool IsZero => false;
        public override Rational ValueAt(Rational time) => 1;
        public override bool IsDefinedFor(Rational time) => time == 0;
        public override bool AttainsValue(Rational value) => value == 1;
        public override string ToCodeString(bool formatted = false, int indentation = 0) => throw new NotSupportedException();
        public override string ToMppgString() => throw new NotSupportedException();
        public override int GetStableHashCode() => 0;
        public override Element Scale(Rational scaling) => throw new NotSupportedException();
        public override Element Delay(Rational delay) => throw new NotSupportedException();
        public override Element Forward(Rational time) => throw new NotSupportedException();
        public override Element VerticalShift(Rational shift) => throw new NotSupportedException();
        public override Element Negate() => throw new NotSupportedException();
        public override Element Inverse() => throw new NotSupportedException();
        public override IEnumerable<Element> Floor() => throw new NotSupportedException();
        public override IEnumerable<Element> Ceil() => throw new NotSupportedException();
        public override Element Addition(Element element) => throw new NotSupportedException();
        public override Element Subtraction(Element element) => throw new NotSupportedException();
        public override List<Element> Minimum(Element element) => throw new NotSupportedException();
        public override List<Element> Maximum(Element element) => throw new NotSupportedException();
        public override IEnumerable<Element> Convolution(Element element, Rational? cutEnd = null, Rational? cutCeiling = null) => throw new NotSupportedException();
        public override IEnumerable<Element> Deconvolution(Element element) => throw new NotSupportedException();
        public override IEnumerable<Element> MaxPlusConvolution(Element element, Rational? cutEnd = null) => throw new NotSupportedException();
        public override SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null) => throw new NotSupportedException();
        public override SubAdditiveCurve SubAdditiveClosure(Rational pseudoPeriodLength, Rational pseudoPeriodHeight, ComputationSettings? settings = null) => throw new NotSupportedException();
    }
}
