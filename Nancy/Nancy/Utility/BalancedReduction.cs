using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unipi.Nancy.Utility;

/// <summary>
/// Reduces adjacent pairs with the same grouping in serial and parallel execution.
/// </summary>
internal static class BalancedReduction
{
    /// <summary>
    /// Combines adjacent operands in rounds, carrying an unpaired last operand to the next round.
    /// </summary>
    /// <param name="source">A nonempty sequence whose indexed storage, if present, remains unchanged during reduction.</param>
    /// <param name="combine">The binary operation, safe for concurrent calls when parallel execution is enabled.</param>
    /// <param name="parallel">Whether independent pairs may execute concurrently.</param>
    /// <returns>The result of the fixed reduction tree.</returns>
    /// <remarks>
    /// Each round owns its result array, so workers never overwrite operands needed by another pair.
    /// Indexed sources are read directly; other sources are buffered once.
    /// Only operand references are copied when reducing reference types.
    /// Grouping does not depend on the number of workers or on the completion order of their calls.
    /// </remarks>
    internal static T Aggregate<T>(IEnumerable<T> source, Func<T, T, T> combine, bool parallel)
    {
        var operands = GetIndexedOperands(source);

        if (operands.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements");

        while (operands.Count > 1)
        {
            int pairCount = operands.Count / 2;
            var results = new T[pairCount + operands.Count % 2];
            if (parallel && pairCount > 1)
            {
                Parallel.For(0, pairCount, i =>
                    results[i] = combine(operands[2 * i], operands[2 * i + 1]));
            }
            else
            {
                for (int i = 0; i < pairCount; i++)
                    results[i] = combine(operands[2 * i], operands[2 * i + 1]);
            }

            if (operands.Count % 2 != 0)
                results[^1] = operands[^1];
            operands = results;
        }

        return operands[0];
    }

    /// <summary>
    /// Returns indexed input directly, or consumes other input once into an array.
    /// </summary>
    /// <param name="source">The operands to make available by index.</param>
    /// <returns>The existing indexed storage or a new array of operands.</returns>
    internal static IReadOnlyList<T> GetIndexedOperands<T>(IEnumerable<T> source)
    {
        if (source is IReadOnlyList<T> indexed)
            return indexed;
        else
            return source.ToArray();
    }

}
