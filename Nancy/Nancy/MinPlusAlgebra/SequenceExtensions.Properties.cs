using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

public static partial class SequenceExtensions
{
    /// <summary>
    /// True if there is no discontinuity within the sequence.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsContinuous(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsContinuous(this IEnumerable<Element> elements)
        => IsContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsContinuous(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsContinuous(new ElementReader(elements));
    }

    /// <summary>
    /// True if there is no discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsContinuous(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsContinuous((ReadOnlySpan<Element>) elements);
    }

    private static bool IsContinuous(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if(!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));

        Rational lastValue = reader.Current is Point p ?
            p.Value :
            ((Segment)reader.Current).LeftLimitAtEndTime;

        while (reader.MoveNext())
        {
            switch(reader.Current)
            {
                case Point point:
                    if (point.Value == lastValue)
                        break;
                    else
                        return false;

                case Segment segment:
                    if (segment.RightLimitAtStartTime == lastValue)
                    {
                        lastValue = segment.LeftLimitAtEndTime;
                        break;
                    }
                    else
                        return false;

                default:
                    throw new InvalidCastException();
            }
        }

        return true;
    }

    /// <summary>
    /// True if there is no left-discontinuity within the sequence.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsLeftContinuous(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsLeftContinuous(this IEnumerable<Element> elements)
        => IsLeftContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no left-discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsLeftContinuous(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsLeftContinuous(new ElementReader(elements));
    }

    /// <summary>
    /// True if there is no left-discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsLeftContinuous(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsLeftContinuous((ReadOnlySpan<Element>) elements);
    }

    private static bool IsLeftContinuous(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        Segment? previous = null;
        do
        {
            if (reader.Current is Point point && previous is { } left &&
                left.LeftLimitAtEndTime != point.Value)
                return false;
            previous = reader.Current as Segment;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>
    /// True if there is no right-discontinuity within the sequence.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsRightContinuous(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsRightContinuous(this IEnumerable<Element> elements)
        => IsRightContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no right-discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsRightContinuous(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsRightContinuous(new ElementReader(elements));
    }

    /// <summary>
    /// True if there is no right-discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsRightContinuous(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsRightContinuous((ReadOnlySpan<Element>) elements);
    }

    private static bool IsRightContinuous(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        Point? previous = null;
        do
        {
            if (reader.Current is Segment segment && previous is { } left &&
                left.Value != segment.RightLimitAtStartTime)
                return false;
            previous = reader.Current as Point;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if every defined value is zero.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsZero(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>True if every element is zero.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsZero(this IEnumerable<Element> elements)
        => IsZero(new ElementReader(elements));

    /// <summary>True if every defined value is zero.</summary>
    /// <returns>True if every element is zero.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsZero(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsZero(new ElementReader(elements));
    }

    /// <summary>True if every defined value is zero.</summary>
    /// <returns>True if every element is zero.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsZero(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsZero((ReadOnlySpan<Element>) elements);
    }

    private static bool IsZero(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsZero)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if every defined value is infinite, with either sign.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsInfinite(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>True if every element is infinite.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsInfinite(this IEnumerable<Element> elements)
        => IsInfinite(new ElementReader(elements));

    /// <summary>True if every defined value is infinite, with either sign.</summary>
    /// <returns>True if every element is infinite.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsInfinite(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsInfinite(new ElementReader(elements));
    }

    /// <summary>True if every defined value is infinite, with either sign.</summary>
    /// <returns>True if every element is infinite.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsInfinite(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsInfinite((ReadOnlySpan<Element>) elements);
    }

    private static bool IsInfinite(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsInfinite)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if some defined value is plus infinity.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="HasPlusInfinity(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>True if an element is plus infinite.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool HasPlusInfinity(this IEnumerable<Element> elements)
        => HasPlusInfinity(new ElementReader(elements));

    /// <summary>True if some defined value is plus infinity.</summary>
    /// <returns>True if an element is plus infinite.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool HasPlusInfinity(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return HasPlusInfinity(new ElementReader(elements));
    }

    /// <summary>True if some defined value is plus infinity.</summary>
    /// <returns>True if an element is plus infinite.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool HasPlusInfinity(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return HasPlusInfinity((ReadOnlySpan<Element>) elements);
    }

    private static bool HasPlusInfinity(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (reader.Current.IsPlusInfinite)
                return true;
        } while (reader.MoveNext());
        return false;
    }

    /// <summary>True if some defined value is minus infinity.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="HasMinusInfinity(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>True if an element is minus infinite.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool HasMinusInfinity(this IEnumerable<Element> elements)
        => HasMinusInfinity(new ElementReader(elements));

    /// <summary>True if some defined value is minus infinity.</summary>
    /// <returns>True if an element is minus infinite.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool HasMinusInfinity(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return HasMinusInfinity(new ElementReader(elements));
    }

    /// <summary>True if some defined value is minus infinity.</summary>
    /// <returns>True if an element is minus infinite.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool HasMinusInfinity(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return HasMinusInfinity((ReadOnlySpan<Element>) elements);
    }

    private static bool HasMinusInfinity(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (reader.Current.IsMinusInfinite)
                return true;
        } while (reader.MoveNext());
        return false;
    }

    /// <summary>True if every defined value is non-positive.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsNonPositive(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNonPositive(this IEnumerable<Element> elements)
        => IsNonPositive(new ElementReader(elements));

    /// <summary>True if every defined value is non-positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonPositive(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsNonPositive(new ElementReader(elements));
    }

    /// <summary>True if every defined value is non-positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonPositive(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsNonPositive((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNonPositive(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            var matches = element switch
            {
                Point point => point.Value <= 0,
                Segment segment => segment.RightLimitAtStartTime <= 0 && segment.LeftLimitAtEndTime <= 0,
                _ => throw new InvalidCastException()
            };
            if (!matches)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if every defined value is strictly positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>
    /// Segment endpoints are excluded; their limits need not satisfy a strict inequality.
    /// Arrays and lists are read as spans, through <see cref="IsPositive(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsPositive(this IEnumerable<Element> elements)
        => IsPositive(new ElementReader(elements));

    /// <summary>True if every defined value is strictly positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>Segment endpoints are excluded; their limits need not satisfy a strict inequality.</remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsPositive(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsPositive(new ElementReader(elements));
    }

    /// <summary>True if every defined value is strictly positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>Segment endpoints are excluded; their limits need not satisfy a strict inequality.</remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsPositive(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsPositive((ReadOnlySpan<Element>) elements);
    }

    private static bool IsPositive(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            var matches = element switch
            {
                Point point => point.Value > 0,
                Segment segment => segment.IsConstant
                    ? segment.RightLimitAtStartTime > 0
                    : segment.RightLimitAtStartTime >= 0 && segment.LeftLimitAtEndTime >= 0,
                _ => throw new InvalidCastException()
            };
            if (!matches)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if every defined value is strictly negative.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>
    /// Segment endpoints are excluded; their limits need not satisfy a strict inequality.
    /// Arrays and lists are read as spans, through <see cref="IsNegative(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNegative(this IEnumerable<Element> elements)
        => IsNegative(new ElementReader(elements));

    /// <summary>True if every defined value is strictly negative.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>Segment endpoints are excluded; their limits need not satisfy a strict inequality.</remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNegative(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsNegative(new ElementReader(elements));
    }

    /// <summary>True if every defined value is strictly negative.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>Segment endpoints are excluded; their limits need not satisfy a strict inequality.</remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNegative(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsNegative((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNegative(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            var matches = element switch
            {
                Point point => point.Value < 0,
                Segment segment => segment.IsConstant
                    ? segment.RightLimitAtStartTime < 0
                    : segment.RightLimitAtStartTime <= 0 && segment.LeftLimitAtEndTime <= 0,
                _ => throw new InvalidCastException()
            };
            if (!matches)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if all defined values equal one constant, possibly infinite.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsConstant(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether every element has the same constant value.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsConstant(this IEnumerable<Element> elements)
        => IsConstant(new ElementReader(elements));

    /// <summary>True if all defined values equal one constant, possibly infinite.</summary>
    /// <returns>Whether every element has the same constant value.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsConstant(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsConstant(new ElementReader(elements));
    }

    /// <summary>True if all defined values equal one constant, possibly infinite.</summary>
    /// <returns>Whether every element has the same constant value.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsConstant(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsConstant((ReadOnlySpan<Element>) elements);
    }

    private static bool IsConstant(ElementReader elements)
    {
        elements.ThrowIfStarted();
        Rational? value = null;
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            if (element is Segment { IsConstant: false })
                return false;
            var current = element is Point point ? point.Value : ((Segment) element).RightLimitAtStartTime;
            if (value is { } previous && previous != current)
                return false;
            value = current;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if finite coefficients $a$ and $b$ exist such that $f(t)=at+b$ throughout the support.</summary>
    /// <returns>Whether the represented function is affine.</returns>
    /// <remarks>
    /// A stream containing one point with a finite value is affine.
    /// Enumeration stops when the result is false; remaining elements are not read or validated.
    /// Arrays and lists are read as spans, through <see cref="IsAffine(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsAffine(this IEnumerable<Element> elements)
        => IsAffine(new ElementReader(elements));

    /// <summary>True if finite coefficients $a$ and $b$ exist such that $f(t)=at+b$ throughout the support.</summary>
    /// <returns>Whether the represented function is affine.</returns>
    /// <remarks>
    /// A span containing one point with a finite value is affine.
    /// Reading stops when the result is false; remaining elements are not read or validated.
    /// </remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsAffine(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsAffine(new ElementReader(elements));
    }

    /// <summary>True if finite coefficients $a$ and $b$ exist such that $f(t)=at+b$ throughout the support.</summary>
    /// <returns>Whether the represented function is affine.</returns>
    /// <remarks>
    /// An array containing one point with a finite value is affine.
    /// Reading stops when the result is false; remaining elements are not read or validated.
    /// </remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsAffine(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsAffine((ReadOnlySpan<Element>) elements);
    }

    private static bool IsAffine(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        Rational? previousEndValue = null;
        Rational? slope = null;
        while (reader.MoveNext())
        {
            var element = reader.Current;
            if (!element.IsFinite)
                return false;

            var startValue = element is Point point ? point.Value : ((Segment) element).RightLimitAtStartTime;
            if (previousEndValue is { } previous && previous != startValue)
                return false;

            if (element is Segment segment)
            {
                if (slope is { } previousSlope && previousSlope != segment.Slope)
                    return false;

                slope = segment.Slope;
                previousEndValue = segment.LeftLimitAtEndTime;
            }
            else
            {
                previousEndValue = startValue;
            }
        }
        if (previousEndValue is null)
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        return true;
    }

    /// <summary>
    /// True if the sequence is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsNonNegative(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether every point and segment value is non-negative.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNonNegative(this IEnumerable<Element> elements)
        => IsNonNegative(new ElementReader(elements));

    /// <summary>
    /// True if the sequence is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    /// <returns>Whether every point and segment value is non-negative.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonNegative(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsNonNegative(new ElementReader(elements));
    }

    /// <summary>
    /// True if the sequence is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    /// <returns>Whether every point and segment value is non-negative.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonNegative(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsNonNegative((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNonNegative(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            if (element is Point point)
            {
                    if (point.Value < 0) return false;
            }
            else if (element is Segment segment)
            {
                    if (segment.RightLimitAtStartTime < 0 || segment.LeftLimitAtEndTime < 0) return false;
            }
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsNonDecreasing(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the stream is non-decreasing.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNonDecreasing(this IEnumerable<Element> elements)
        => IsNonDecreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    /// <returns>Whether the elements are non-decreasing.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonDecreasing(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsNonDecreasing(new ElementReader(elements));
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    /// <returns>Whether the elements are non-decreasing.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonDecreasing(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsNonDecreasing((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNonDecreasing(ElementReader elements)
    {
        return CheckMonotonicity(elements, Monotonicity.NonDecreasing);
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) > f(s)$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsIncreasing(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the stream is strictly increasing.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsIncreasing(this IEnumerable<Element> elements)
        => IsIncreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) > f(s)$.
    /// </summary>
    /// <returns>Whether the elements are strictly increasing.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsIncreasing(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsIncreasing(new ElementReader(elements));
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) > f(s)$.
    /// </summary>
    /// <returns>Whether the elements are strictly increasing.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsIncreasing(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsIncreasing((ReadOnlySpan<Element>) elements);
    }

    private static bool IsIncreasing(ElementReader elements)
    {
        return CheckMonotonicity(elements, Monotonicity.StrictlyIncreasing);
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) \le f(s)$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsNonIncreasing(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the stream is non-increasing.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNonIncreasing(this IEnumerable<Element> elements)
        => IsNonIncreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) \le f(s)$.
    /// </summary>
    /// <returns>Whether the elements are non-increasing.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonIncreasing(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsNonIncreasing(new ElementReader(elements));
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) \le f(s)$.
    /// </summary>
    /// <returns>Whether the elements are non-increasing.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonIncreasing(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsNonIncreasing((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNonIncreasing(ElementReader elements)
    {
        return CheckMonotonicity(elements, Monotonicity.NonIncreasing);
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) &lt; f(s)$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsDecreasing(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the stream is strictly decreasing.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsDecreasing(this IEnumerable<Element> elements)
        => IsDecreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) &lt; f(s)$.
    /// </summary>
    /// <returns>Whether the elements are strictly decreasing.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsDecreasing(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsDecreasing(new ElementReader(elements));
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) &lt; f(s)$.
    /// </summary>
    /// <returns>Whether the elements are strictly decreasing.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsDecreasing(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsDecreasing((ReadOnlySpan<Element>) elements);
    }

    private static bool IsDecreasing(ElementReader elements)
    {
        return CheckMonotonicity(elements, Monotonicity.StrictlyDecreasing);
    }

    private enum Monotonicity
    {
        NonDecreasing,
        StrictlyIncreasing,
        NonIncreasing,
        StrictlyDecreasing
    }

    /// <summary>
    /// Checks slopes and adjacent boundary values in one traversal of uninterrupted elements in time order.
    /// </summary>
    /// <param name="elements">The nonempty element stream.</param>
    /// <param name="monotonicity">The required monotonicity.</param>
    /// <returns>Whether the requested monotonicity holds.</returns>
    /// <remarks>
    /// <see cref="ElementReader.MoveNext"/> checks adjoining times and alternating points and segments during traversal.
    /// An invalid unread tail is not checked when a monotonicity violation has already determined the result.
    /// </remarks>
    /// <exception cref="ArgumentException">If the stream is empty or a consumed element violates uninterrupted time order.</exception>
    private static bool CheckMonotonicity(ElementReader elements, Monotonicity monotonicity)
    {
        elements.ThrowIfStarted();
        Rational? previous = null;
        using var reader = elements;
        while (reader.MoveNext())
        {
            var element = reader.Current;
            var start = element is Point point ? point.Value : ((Segment) element).RightLimitAtStartTime;
            if (previous is { } value && (monotonicity switch
                {
                    Monotonicity.NonDecreasing or Monotonicity.StrictlyIncreasing => value > start,
                    Monotonicity.NonIncreasing or Monotonicity.StrictlyDecreasing => value < start,
                    _ => throw new ArgumentOutOfRangeException(nameof(monotonicity))
                }))
                return false;
            if (element is Segment segment)
            {
                if (monotonicity switch
                    {
                        Monotonicity.NonDecreasing => segment.Slope < 0,
                        Monotonicity.StrictlyIncreasing => segment.Slope <= 0,
                        Monotonicity.NonIncreasing => segment.Slope > 0,
                        Monotonicity.StrictlyDecreasing => segment.Slope >= 0,
                        _ => throw new ArgumentOutOfRangeException(nameof(monotonicity))
                    })
                    return false;
                previous = segment.LeftLimitAtEndTime;
            }
            else
                previous = start;
        }
        if (previous is null)
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        return true;
    }

    /// <summary>
    /// True if for any $t$, $\left|f(t)\right| &lt; +\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsFinite(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsFinite(this IEnumerable<Element> elements)
        => IsFinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $\left|f(t)\right| &lt; +\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsFinite(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsFinite(new ElementReader(elements));
    }

    /// <summary>
    /// True if for any $t$, $\left|f(t)\right| &lt; +\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsFinite(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsFinite((ReadOnlySpan<Element>) elements);
    }

    private static bool IsFinite(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsFinite)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>
    /// True if for any $t$, $f(t) = -\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsMinusInfinite(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsMinusInfinite(this IEnumerable<Element> elements)
        => IsMinusInfinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $f(t) = -\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsMinusInfinite(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsMinusInfinite(new ElementReader(elements));
    }

    /// <summary>
    /// True if for any $t$, $f(t) = -\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsMinusInfinite(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsMinusInfinite((ReadOnlySpan<Element>) elements);
    }

    private static bool IsMinusInfinite(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsMinusInfinite)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>
    /// True if for any $t$, $f(t) = +\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsPlusInfinite(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsPlusInfinite(this IEnumerable<Element> elements)
        => IsPlusInfinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $f(t) = +\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsPlusInfinite(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return IsPlusInfinite(new ElementReader(elements));
    }

    /// <summary>
    /// True if for any $t$, $f(t) = +\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsPlusInfinite(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return IsPlusInfinite((ReadOnlySpan<Element>) elements);
    }

    private static bool IsPlusInfinite(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsPlusInfinite)
                return false;
        } while (reader.MoveNext());
        return true;
    }
}
