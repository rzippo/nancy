using System;
using System.Threading;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// The caches of Boolean properties: the shared, evaluated values, and how a property is read through its cache.
/// </summary>
internal static class KnownProperties
{
    internal static readonly Lazy<bool> True = new(value: true);
    internal static readonly Lazy<bool> False = new(value: false);

    /// <summary>
    /// Returns the cached value of a property, computing and publishing it on the first read.
    /// </summary>
    /// <param name="cache">The cache field of the property.</param>
    /// <param name="owner">The object the property belongs to.</param>
    /// <param name="compute">Computes the property of <paramref name="owner"/>; a static lambda, so that reading a cached value allocates nothing.</param>
    /// <remarks>
    /// Concurrent first reads publish a single cache, whose computation runs once.
    /// An exception thrown by the computation is cached with it, and rethrown on every read.
    /// </remarks>
    internal static bool Get<T>(ref Lazy<bool>? cache, T owner, Func<T, bool> compute)
    {
        var current = Volatile.Read(ref cache);
        if (current is null)
        {
            var candidate = new Lazy<bool>(() => compute(owner), LazyThreadSafetyMode.ExecutionAndPublication);
            current = Interlocked.CompareExchange(ref cache, candidate, null) ?? candidate;
        }
        return current.Value;
    }

    /// <summary>
    /// Returns the cached value of a property if it was already computed, without computing it.
    /// </summary>
    /// <param name="cache">The cache field of the property.</param>
    internal static bool? GetIfKnown(ref Lazy<bool>? cache)
        => Volatile.Read(ref cache) is { IsValueCreated: true } current ? current.Value : null;

    /// <summary>
    /// Sets <paramref name="target"/> to <paramref name="value"/> if <paramref name="source"/> already holds it.
    /// </summary>
    /// <param name="source">The cache field of the property on the object it is known for.</param>
    /// <param name="target">The cache field of the same property on another object.</param>
    /// <param name="value">The value that carries over.</param>
    /// <remarks>
    /// The write is plain: <paramref name="target"/> belongs to an object not yet published.
    /// </remarks>
    internal static void CopyIfKnown(ref Lazy<bool>? source, ref Lazy<bool>? target, bool value)
    {
        if (GetIfKnown(ref source) == value)
            target = value ? True : False;
    }
}
