using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Computes pointwise reference results using interval buckets, independently of the stream operators and their sequence consumers.
/// </summary>
internal static class MaterializedPointwiseReference
{
    internal static Sequence Addition(Sequence a, Sequence b) =>
        new(Buckets(a, b).Select(bucket => Element.Addition(bucket.Elements))
            .ToList()
            .Merge());

    internal static Sequence Subtraction(Sequence a, Sequence b) =>
        Addition(a, new Sequence(b.Elements.Select(e => e.Negate())));

    internal static Sequence Minimum(Sequence a, Sequence b) =>
        new(Buckets(a, b).SelectMany(bucket => bucket.LowerEnvelope())
            .ToList()
            .Merge());

    internal static Sequence Maximum(Sequence a, Sequence b) =>
        new(Buckets(a, b).SelectMany(bucket => bucket.UpperEnvelope())
            .ToList()
            .Merge());

    private static IEnumerable<IntervalBucket> Buckets(Sequence a, Sequence b)
    {
        var overlap = Interval.Intersection(a.Support, b.Support) ?? throw new ArgumentException();
        return IntervalBucket.ComputeIntervals(a.Cut(overlap), b.Cut(overlap));
    }
}
