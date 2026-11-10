using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unipi.Nancy.Utility;
using Xunit;

namespace Unipi.Nancy.Tests.Utility;

public class BalancedReductionTests
{
    public static IEnumerable<object[]> AdjacentTrees =>
    [
        new object[] { "AB", "(AB)" },
        new object[] { "ABCDE", "(((AB)(CD))E)" },
        new object[] { "ABCDEFGH", "(((AB)(CD))((EF)(GH)))" },
        new object[] { "ABCDEFGHI", "((((AB)(CD))((EF)(GH)))I)" }
    ];

    [Theory]
    [MemberData(nameof(AdjacentTrees))]
    public void ExecutionModesPreserveAdjacentGrouping(string leaves, string expected)
    {
        var input = leaves.Select(c => c.ToString()).ToArray();
        int serialCalls = 0;
        int parallelCalls = 0;
        string serial = BalancedReduction.Aggregate(input, (a, b) =>
        {
            serialCalls++;
            return $"({a}{b})";
        }, false);
        string parallel = BalancedReduction.Aggregate(input, (a, b) =>
        {
            Interlocked.Increment(ref parallelCalls);
            return $"({a}{b})";
        }, true);

        Assert.Equal(expected, serial);
        Assert.Equal(expected, parallel);
        Assert.Equal(input.Length - 1, serialCalls);
        Assert.Equal(input.Length - 1, parallelCalls);
        Assert.Equal(leaves, string.Concat(input));
    }

    [Fact]
    public void IndexedInputIsReadWithoutEnumeration()
    {
        var input = new IndexedInput();
        Assert.Equal("(((AB)(CD))E)", BalancedReduction.Aggregate(input, (a, b) => $"({a}{b})", false));
        Assert.Equal("(((AB)(CD))E)", BalancedReduction.Aggregate(input, (a, b) => $"({a}{b})", true));
    }

    private sealed class IndexedInput : IReadOnlyList<string>
    {
        private readonly string[] elements = ["A", "B", "C", "D", "E"];
        public int Count => elements.Length;
        public string this[int index] => elements[index];
        public IEnumerator<string> GetEnumerator() => throw new InvalidOperationException("Indexed input must not be enumerated.");
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void SingleOperandIsReturnedWithoutCombining()
    {
        var operand = new object();
        Assert.Same(operand, BalancedReduction.Aggregate(new[] { operand },
            (_, _) => throw new InvalidOperationException(), false));
        Assert.Same(operand, BalancedReduction.Aggregate(new[] { operand },
            (_, _) => throw new InvalidOperationException(), true));
    }

    [Fact]
    public void SourceIsConsumedOnceAndDisposed()
    {
        int enumerations = 0;
        int disposals = 0;
        IEnumerable<int> Source()
        {
            Assert.Equal(1, ++enumerations);
            try
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }
            finally
            {
                disposals++;
            }
        }

        Assert.Equal(6, BalancedReduction.Aggregate(Source(), (a, b) => a + b, true));
        Assert.Equal(1, disposals);
        Assert.Throws<InvalidOperationException>(() =>
            BalancedReduction.Aggregate(Array.Empty<int>(), (a, b) => a + b, false));
        Assert.Throws<InvalidOperationException>(() =>
            BalancedReduction.Aggregate(Array.Empty<int>(), (a, b) => a + b, true));
    }
}
