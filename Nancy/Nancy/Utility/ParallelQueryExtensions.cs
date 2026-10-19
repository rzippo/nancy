using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Unipi.Nancy.Utility;

/// <summary>
/// Extension methods on parallel queries.
/// </summary>
internal static class ParallelQueryExtensions
{
    /// <summary>
    /// Enumerates the query so that an exception thrown by one of its delegates reaches the consumer as itself,
    /// where PLINQ wraps it in an <see cref="AggregateException"/>.
    /// </summary>
    /// <param name="query">The query to enumerate.</param>
    /// <returns>The elements of <paramref name="query"/>, in its order.</returns>
    /// <remarks>
    /// The first exception inside the wrapping is thrown, however deeply nested queries have wrapped it.
    /// </remarks>
    internal static IEnumerable<T> WithoutAggregateException<T>(this ParallelQuery<T> query)
    {
        using var enumerator = query.GetEnumerator();
        while (true)
        {
            bool hasNext;
            try
            {
                hasNext = enumerator.MoveNext();
            }
            catch (AggregateException aggregate)
            {
                ExceptionDispatchInfo.Capture(aggregate.Flatten().InnerExceptions[0]).Throw();
                throw;
            }

            if (!hasNext)
                yield break;
            yield return enumerator.Current;
        }
    }
}
