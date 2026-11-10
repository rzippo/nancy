using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Sorts interval endpoints through independent chunk sorts and parallel merges.
/// </summary>
internal static class IntervalEndpointSorter
{
    private const int MinimumChunkLength = 4096;

    /// <summary>
    /// Sorts the array in place, using serial sorting for small arrays or a single processor.
    /// </summary>
    /// <param name="values">The endpoint array to sort in place.</param>
    /// <param name="parallel">Whether independent sorting and merging work may execute concurrently.</param>
    /// <remarks>Comparison failures are reported as their underlying exception in both execution modes.</remarks>
    internal static void Sort(Rational[] values, bool parallel = true)
    {
        int workers = Environment.ProcessorCount;
        int count = values.Length;
        if (!parallel || count <= MinimumChunkLength || workers == 1)
        {
            try
            {
                Array.Sort(values);
            }
            catch (InvalidOperationException exception) when (exception.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }
        else
        {
            var options = new ParallelOptions { MaxDegreeOfParallelism = workers };
            int width = Math.Max(MinimumChunkLength, 1 + (count - 1) / workers);
            int chunks = 1 + (count - 1) / width;
            try
            {
                Parallel.For(0, chunks, options, chunk =>
                {
                    int start = chunk * width;
                    try
                    {
                        Array.Sort(values, start, Math.Min(width, count - start));
                    }
                    catch (InvalidOperationException exception) when (exception.InnerException is not null)
                    {
                        ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                        throw;
                    }
                });
            }
            catch (AggregateException aggregate)
            {
                // Flatten failures from nested parallel loops and rethrow one worker failure, preserving its original stack.
                ExceptionDispatchInfo.Capture(aggregate.Flatten().InnerExceptions[0]).Throw();
                throw;
            }

            var buffer = new Rational[count];
            var source = values;
            var target = buffer;
            while (width < count)
            {
                int pairWidth = (int)Math.Min(2L * width, count);
                int pairs = 1 + (count - 1) / pairWidth;
                int tiles = Math.Min(workers, 1 + (pairWidth - 1) / MinimumChunkLength);
                int currentWidth = width;
                var currentSource = source;
                var currentTarget = target;

                // Each tile owns an output slice, including in the final merge of two runs.
                try
                {
                    Parallel.For(0, (long)pairs * tiles, options, task =>
                    {
                        int pair = (int)(task / tiles);
                        int tile = (int)(task % tiles);
                        int start = pair * pairWidth;
                        int middle = start + Math.Min(currentWidth, count - start);
                        int end = start + Math.Min(pairWidth, count - start);
                        int length = end - start;
                        int outputStart = (int)((long)length * tile / tiles);
                        int outputEnd = (int)((long)length * (tile + 1) / tiles);
                        int aCount = middle - start;
                        int bCount = end - middle;
                        int a = MergePrefixLength(outputStart, currentSource, start, aCount, middle, bCount);
                        int b = outputStart - a;
                        for (int output = outputStart; output < outputEnd; output++)
                        {
                            if (a < aCount && (b >= bCount || currentSource[start + a] <= currentSource[middle + b]))
                                currentTarget[start + output] = currentSource[start + a++];
                            else
                                currentTarget[start + output] = currentSource[middle + b++];
                        }
                    });
                }
                catch (AggregateException aggregate)
                {
                    // Flatten failures from nested parallel loops and rethrow one worker failure, preserving its original stack.
                    ExceptionDispatchInfo.Capture(aggregate.Flatten().InnerExceptions[0]).Throw();
                    throw;
                }

                // All slices are complete before the next stage reads their output.
                (source, target) = (target, source);
                width = pairWidth;
            }

            if (!ReferenceEquals(source, values))
                Array.Copy(source, values, count);
        }
    }

    /// <summary>
    /// Returns the number of entries from A in the first <paramref name="rank"/> entries of the stable merge of sorted runs A and B.
    /// Equal entries from A precede those from B.
    /// </summary>
    /// <remarks>The runs must lie in <paramref name="source"/>, and rank must be between zero and the sum of their lengths.</remarks>
    private static int MergePrefixLength(int rank, Rational[] source, int aStart, int aCount, int bStart, int bCount)
    {
        int low = Math.Max(0, rank - bCount);
        int high = Math.Min(rank, aCount);
        while (low <= high)
        {
            int a = low + (high - low) / 2;
            int b = rank - a;
            if (a > 0 && b < bCount && source[aStart + a - 1] > source[bStart + b])
                high = a - 1;
            else if (b > 0 && a < aCount && source[bStart + b - 1] >= source[aStart + a])
                low = a + 1;
            else
                return a;
        }
        throw new InvalidOperationException("No partition found for the sorted runs");
    }
}
