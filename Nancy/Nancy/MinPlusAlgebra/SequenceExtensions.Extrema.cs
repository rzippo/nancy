using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

public static partial class SequenceExtensions
{
    /// <summary>
    /// If the sequence is upper-bounded around the breakpoint $t_c$, i.e., exists $x$ such that $f(t) \le x$ for any $t \in [t_c - \varepsilon, t_c + \varepsilon]$, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>

    public static Rational BreakpointSupValue(this (Segment? left, Point center, Segment? right) breakpoint)
    {
        return breakpoint.GetBreakpointBoundaryValues().Max();
    }

    /// <summary>Finds the supremum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>
    /// The time may be an excluded endpoint or plus infinity; it need not attain the bound.
    /// Arrays and lists are read as spans, through <see cref="SupValueAndArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) SupValueAndArg(this IEnumerable<Element> elements)
        => SupValueAndArg(new ElementReader(elements));

    /// <summary>Finds the supremum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>The time may be an excluded endpoint or plus infinity; it need not attain the bound.</remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) SupValueAndArg(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return SupValueAndArg(new ElementReader(elements));
    }

    /// <summary>Finds the supremum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>The time may be an excluded endpoint or plus infinity; it need not attain the bound.</remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) SupValueAndArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return SupValueAndArg((ReadOnlySpan<Element>) elements);
    }

    private static (Rational Value, Rational Time) SupValueAndArg(ElementReader elements)
        => FindBoundaryExtremum(elements, BoundKind.Supremum);

    /// <summary>Finds the infimum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>
    /// The time may be an excluded endpoint or plus infinity; it need not attain the bound.
    /// Arrays and lists are read as spans, through <see cref="InfValueAndArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) InfValueAndArg(this IEnumerable<Element> elements)
        => InfValueAndArg(new ElementReader(elements));

    /// <summary>Finds the infimum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>The time may be an excluded endpoint or plus infinity; it need not attain the bound.</remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) InfValueAndArg(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return InfValueAndArg(new ElementReader(elements));
    }

    /// <summary>Finds the infimum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>The time may be an excluded endpoint or plus infinity; it need not attain the bound.</remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) InfValueAndArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return InfValueAndArg((ReadOnlySpan<Element>) elements);
    }

    private static (Rational Value, Rational Time) InfValueAndArg(ElementReader elements)
        => FindBoundaryExtremum(elements, BoundKind.Infimum);

    /// <summary>
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for every $t$ in the support, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="SupValue(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational SupValue(this IEnumerable<Element> elements)
        => SupValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for every $t$ in the support, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational SupValue(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return SupValue(new ElementReader(elements));
    }

    /// <summary>
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for every $t$ in the support, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational SupValue(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return SupValue((ReadOnlySpan<Element>) elements);
    }

    private static Rational SupValue(ElementReader elements)
    {
        return FindBoundaryExtremum(elements, BoundKind.Supremum).Value;
    }

    /// <summary>Returns the earliest boundary location of the supremum.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="SupArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational SupArg(this IEnumerable<Element> elements)
        => SupArg(new ElementReader(elements));

    /// <summary>Returns the earliest boundary location of the supremum.</summary>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational SupArg(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return SupArg(new ElementReader(elements));
    }

    /// <summary>Returns the earliest boundary location of the supremum.</summary>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational SupArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return SupArg((ReadOnlySpan<Element>) elements);
    }

    private static Rational SupArg(ElementReader elements)
    {
        return FindBoundaryExtremum(elements, BoundKind.Supremum).Time;
    }

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="MaxValue(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The attained maximum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MaxValue(this IEnumerable<Element> elements)
        => MaxValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>The attained maximum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MaxValue(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return MaxValue(new ElementReader(elements));
    }

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>The attained maximum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MaxValue(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return MaxValue((ReadOnlySpan<Element>) elements);
    }

    private static Rational? MaxValue(ElementReader elements)
    {
        var (value, attainingElement) = FindBoundAndAttainment(elements, BoundKind.Supremum);
        return attainingElement is null ? null : value;
    }

    /// <inheritdoc cref="MaxValue(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MaxValue(this IReadOnlyCollection<Element> elements)
    {
        return MaxValue((IEnumerable<Element>) elements);
    }

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="MaxArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>An argument in the support attaining the maximum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MaxArg(this IEnumerable<Element> elements)
        => MaxArg(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>An argument in the support attaining the maximum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MaxArg(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return MaxArg(new ElementReader(elements));
    }

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>An argument in the support attaining the maximum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MaxArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return MaxArg((ReadOnlySpan<Element>) elements);
    }

    private static Rational? MaxArg(ElementReader elements)
    {
        var (_, attainingElement) = FindBoundAndAttainment(elements, BoundKind.Supremum);
        return attainingElement switch
        {
            Point point => point.Time,
            Segment segment => InteriorTime(segment),
            _ => null
        };
    }

    /// <inheritdoc cref="MaxArg(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MaxArg(this IReadOnlyCollection<Element> elements)
    {
        return MaxArg((IEnumerable<Element>) elements);
    }

    /// If the sequence is upper-bounded around the breakpoint $t_c$, i.e., exists $x$ such that $f(t) \ge x$ for any $t \in [t_c - \varepsilon, t_c + \varepsilon]$, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    public static Rational BreakpointInfValue(this (Segment? left, Point center, Segment? right) breakpoint)
    {
        return breakpoint.GetBreakpointBoundaryValues().Min();
    }

    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for every $t$ in the support, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="InfValue(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational InfValue(this IEnumerable<Element> elements)
        => InfValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for every $t$ in the support, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational InfValue(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return InfValue(new ElementReader(elements));
    }

    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for every $t$ in the support, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational InfValue(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return InfValue((ReadOnlySpan<Element>) elements);
    }

    private static Rational InfValue(ElementReader elements)
    {
        return FindBoundaryExtremum(elements, BoundKind.Infimum).Value;
    }

    /// <summary>Returns the earliest boundary location of the infimum.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="InfArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational InfArg(this IEnumerable<Element> elements)
        => InfArg(new ElementReader(elements));

    /// <summary>Returns the earliest boundary location of the infimum.</summary>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational InfArg(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return InfArg(new ElementReader(elements));
    }

    /// <summary>Returns the earliest boundary location of the infimum.</summary>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational InfArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return InfArg((ReadOnlySpan<Element>) elements);
    }

    private static Rational InfArg(ElementReader elements)
    {
        return FindBoundaryExtremum(elements, BoundKind.Infimum).Time;
    }

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="MinValue(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The attained minimum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MinValue(this IEnumerable<Element> elements)
        => MinValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>The attained minimum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MinValue(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return MinValue(new ElementReader(elements));
    }

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>The attained minimum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MinValue(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return MinValue((ReadOnlySpan<Element>) elements);
    }

    private static Rational? MinValue(ElementReader elements)
    {
        var (value, attainingElement) = FindBoundAndAttainment(elements, BoundKind.Infimum);
        return attainingElement is null ? null : value;
    }

    /// <inheritdoc cref="MinValue(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MinValue(this IReadOnlyCollection<Element> elements)
    {
        return MinValue((IEnumerable<Element>) elements);
    }
    
    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="MinArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>An argument in the support attaining the minimum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MinArg(this IEnumerable<Element> elements)
        => MinArg(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>An argument in the support attaining the minimum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MinArg(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element span is empty.", nameof(elements));
        return MinArg(new ElementReader(elements));
    }

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>An argument in the support attaining the minimum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MinArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (elements.Length == 0)
            throw new ArgumentException("The element array is empty.", nameof(elements));
        return MinArg((ReadOnlySpan<Element>) elements);
    }

    private static Rational? MinArg(ElementReader elements)
    {
        var (_, attainingElement) = FindBoundAndAttainment(elements, BoundKind.Infimum);
        return attainingElement switch
        {
            Point point => point.Time,
            Segment segment => InteriorTime(segment),
            _ => null
        };
    }

    /// <inheritdoc cref="MinArg(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MinArg(this IReadOnlyCollection<Element> elements)
    {
        return MinArg((IEnumerable<Element>) elements);
    }

    private enum BoundKind
    {
        Infimum,
        Supremum
    }

    /// <summary>
    /// Chooses a finite time strictly inside the segment's open support.
    /// </summary>
    /// <param name="segment">The segment to sample.</param>
    /// <returns>An interior time, without requiring a particular representative.</returns>
    /// <remarks>
    /// For a constant segment, every interior time attains its value, whereas its endpoints are excluded.
    /// </remarks>
    private static Rational InteriorTime(Segment segment)
    {
        if (segment.StartTime.IsFinite)
        {
            if (segment.EndTime.IsFinite)
            {
                // The midpoint lies strictly between the two finite endpoints.
                return segment.StartTime + (segment.EndTime - segment.StartTime) / 2;
            }
            else
            {
                // With no upper bound, moving one unit right stays inside the support.
                return segment.StartTime + 1;
            }
        }
        else
        {
            if (segment.EndTime.IsFinite)
            {
                // With no lower bound, moving one unit left stays inside the support.
                return segment.EndTime - 1;
            }
            else
            {
                // The support is the whole real line, so zero is an interior time.
                return 0;
            }
        }
    }

    private static (Rational Value, Rational Time) FindBoundaryExtremum(
        ElementReader elements, BoundKind kind)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        var first = reader.Current;
        var bestValue = first is Point p ? p.Value : ((Segment) first).RightLimitAtStartTime;
        var bestTime = first.StartTime;
        do
        {
            if (reader.Current is Point point)
                Consider(point.Value, point.Time);
            else
            {
                var segment = (Segment) reader.Current;
                Consider(segment.RightLimitAtStartTime, segment.StartTime);
                Consider(segment.LeftLimitAtEndTime, segment.EndTime);
            }
        } while (reader.MoveNext());
        return (bestValue, bestTime);

        void Consider(Rational value, Rational time)
        {
            var improves = kind == BoundKind.Supremum ? value > bestValue : value < bestValue;
            if (improves || (value == bestValue && time < bestTime))
            {
                bestValue = value;
                bestTime = time;
            }
        }
    }

    /// <summary>
    /// Finds the extreme boundary value and the first element attaining it, retaining only the current candidate.
    /// </summary>
    /// <param name="elements">The element stream.</param>
    /// <param name="boundKind">The bound to find.</param>
    /// <returns>The extreme value and its first attaining element, or a null element when the bound is unattained.</returns>
    /// <exception cref="ArgumentException">If the stream is empty or an encountered element violates uninterrupted time order.</exception>
    private static (Rational bound, Element? attainingElement) FindBoundAndAttainment(ElementReader elements, BoundKind boundKind)
    {
        elements.ThrowIfStarted();
        Rational? extreme = null;
        Element? attainingElement = null;
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            Rational candidate;
            if (element is Point point)
                candidate = point.Value;
            else if (element is Segment segment)
                candidate = boundKind == BoundKind.Supremum
                    ? Rational.Max(segment.RightLimitAtStartTime, segment.LeftLimitAtEndTime)
                    : Rational.Min(segment.RightLimitAtStartTime, segment.LeftLimitAtEndTime);
            else
                continue;

            if (extreme is null || (boundKind == BoundKind.Supremum ? candidate > extreme.Value : candidate < extreme.Value))
            {
                extreme = candidate;
                attainingElement = element.AttainsValue(candidate) ? element : null;
            }
            else if (candidate == extreme.Value && attainingElement is null && element.AttainsValue(candidate))
                attainingElement = element;
        } while (reader.MoveNext());
        return (extreme!.Value, attainingElement);
    }
}
