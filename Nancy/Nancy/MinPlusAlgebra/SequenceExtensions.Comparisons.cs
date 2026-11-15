using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

public static partial class SequenceExtensions
{
    /// <summary>
    /// Checks whether the first nonempty, uninterrupted stream bounds the second from below over their overlap.
    /// </summary>
    /// <param name="a">The first stream in time order.</param>
    /// <param name="b">The second stream in time order.</param>
    /// <returns>Whether the bound holds at every shared point and throughout every shared segment.</returns>
    /// <exception cref="ArgumentException">If an operand is empty or the streams share no time.</exception>
    internal static bool LessOrEqualOnOverlap(IEnumerable<Element> a, IEnumerable<Element> b)
        => LessOrEqualOnOverlap(a, b, out _);

    /// <summary>
    /// Checks whether the first nonempty, uninterrupted stream bounds the second from below over their overlap, and whether the two are equal there.
    /// </summary>
    /// <param name="a">The first stream in time order.</param>
    /// <param name="b">The second stream in time order.</param>
    /// <param name="isEqual">Whether the streams are equal at every shared point and throughout every shared segment; meaningful only when the bound holds.</param>
    /// <returns>Whether the bound holds at every shared point and throughout every shared segment.</returns>
    /// <exception cref="ArgumentException">If an operand is empty or the streams share no time.</exception>
    internal static bool LessOrEqualOnOverlap(IEnumerable<Element> a, IEnumerable<Element> b, out bool isEqual)
    {
        isEqual = true;
        foreach (var (left, right) in OverlappingElements(a, b))
        {
            if (left is Point point)
            {
                var value = ((Point) right).Value;
                if (point.Value > value)
                    return false;
                isEqual = isEqual && point.Value == value;
            }
            else
            {
                var segmentA = (Segment) left;
                var segmentB = (Segment) right;
                var start = Rational.Max(segmentA.StartTime, segmentB.StartTime);
                var end = Rational.Min(segmentA.EndTime, segmentB.EndTime);
                var startA = segmentA.RightLimitAt(start);
                var startB = segmentB.RightLimitAt(start);
                if (end.IsPlusInfinite && startA.IsFinite && startB.IsFinite)
                {
                    // Equal limits at infinity can hide different slopes.
                    if (startA > startB || segmentA.Slope > segmentB.Slope)
                        return false;
                    isEqual = isEqual && startA == startB && segmentA.Slope == segmentB.Slope;
                }
                else
                {
                    var endA = segmentA.LeftLimitAt(end);
                    var endB = segmentB.LeftLimitAt(end);
                    if (startA > startB || endA > endB)
                        return false;
                    isEqual = isEqual && startA == startB && endA == endB;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Finds the first time at or immediately after which two uninterrupted streams differ over their overlap.
    /// </summary>
    /// <param name="a">The first nonempty stream in time order.</param>
    /// <param name="b">The second nonempty stream in time order.</param>
    /// <returns>The first differing time, or null when the streams agree throughout their overlap.</returns>
    internal static Rational? FirstInequivalenceOnOverlap(IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
        {
            if (left is Point point)
            {
                if (point.Value != ((Point) right).Value)
                    return point.Time;
            }
            else
            {
                var segmentA = (Segment) left;
                var segmentB = (Segment) right;
                var start = Rational.Max(segmentA.StartTime, segmentB.StartTime);
                var end = Rational.Min(segmentA.EndTime, segmentB.EndTime);
                var startA = segmentA.RightLimitAt(start);
                var startB = segmentB.RightLimitAt(start);
                if (end.IsPlusInfinite && startA.IsFinite && startB.IsFinite)
                {
                    // Equal limits at infinity can hide different slopes.
                    if (startA != startB || segmentA.Slope != segmentB.Slope)
                        return start;
                }
                else if (startA != startB || segmentA.LeftLimitAt(end) != segmentB.LeftLimitAt(end))
                    return start;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks a strict pointwise bound over the overlap of finite uninterrupted streams.
    /// </summary>
    /// <param name="a">The first nonempty stream in time order.</param>
    /// <param name="b">The second nonempty stream in time order.</param>
    /// <returns>Whether the first stream is strictly below the second at every shared time.</returns>
    internal static bool StrictlyLessOnOverlap(IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
        {
            if (left is Point point)
            {
                if (point.Value >= ((Point) right).Value)
                    return false;
            }
            else
            {
                var segmentA = (Segment) left;
                var segmentB = (Segment) right;
                var start = Rational.Max(segmentA.StartTime, segmentB.StartTime);
                var end = Rational.Min(segmentA.EndTime, segmentB.EndTime);
                var startA = segmentA.RightLimitAt(start);
                var startB = segmentB.RightLimitAt(start);
                if (end.IsPlusInfinite && startA.IsFinite && startB.IsFinite)
                {
                    // Equal limits at infinity can hide different slopes.
                    if (startA > startB || segmentA.Slope > segmentB.Slope
                        || (startA == startB && segmentA.Slope == segmentB.Slope))
                        return false;
                }
                else
                {
                    var endA = segmentA.LeftLimitAt(end);
                    var endB = segmentB.LeftLimitAt(end);
                    if (startA > startB || endA > endB || (startA == startB && endA == endB))
                        return false;
                }
            }
        }
        return true;
    }
}
