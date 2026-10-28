using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class BooleanCacheLifecycle
{
    [Fact]
    public void AFailedPropertyRetainsItsFailureUntilReset()
    {
        var elements = new ControlledElements();
        var sequence = new Sequence(elements);
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
        var elements = new ControlledElements();
        var sequence = new Sequence(elements);
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

    private sealed class ControlledElements : IReadOnlyList<Element>
    {
        public bool FailReads;
        public ManualResetEventSlim? Entered;
        public ManualResetEventSlim? Release;
        public int PropertyEnumerations;
        public int Count => 1;
        public Element this[int index] => new Point(0, 1);

        public IEnumerator<Element> GetEnumerator()
        {
            if (FailReads)
                throw new InvalidOperationException("Controlled enumeration failure.");
            if (Entered is not null)
            {
                Interlocked.Increment(ref PropertyEnumerations);
                Entered.Set();
                if (!Release!.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException();
            }
            yield return new Point(0, 1);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
