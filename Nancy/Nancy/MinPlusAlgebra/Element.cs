using System;
using System.Collections.Generic;
using System.Linq;
using JsonSubTypes;
using Newtonsoft.Json;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Smallest unit of representation of a min-plus algebra function.
/// Can be either a <see cref="Point"/> or a <see cref="Segment"/>.
/// </summary>
/// <docs position="5"/>
[JsonConverter(typeof(JsonSubtypes), "type")]
[JsonSubtypes.KnownSubType(typeof(Point), Point.TypeCode)]
[JsonSubtypes.KnownSubType(typeof(Segment), Segment.TypeCode)]
public abstract class Element : IToCodeString
{
    #region Properties

    /// <summary>
    /// Left endpoint of the support of the element.
    /// If the element is a <see cref="Segment"/>, it is exclusive and strictly lower than <see cref="EndTime"/>.
    /// If the element is a <see cref="Point"/>, it is inclusive and equal to <see cref="EndTime"/>.
    /// </summary>
    public abstract Rational StartTime { get; }

    /// <summary>
    /// Right endpoint of the support of the element.
    /// If the element is a <see cref="Segment"/>, it is exclusive and strictly greater than <see cref="StartTime"/>.
    /// If the element is a <see cref="Point"/>, it is inclusive and equal to <see cref="StartTime"/>.
    /// </summary>
    public abstract Rational EndTime { get; }

    /// <summary>
    /// Length in time of the element.
    /// If the element is a <see cref="Segment"/>, it is strictly greater than 0.
    /// If the element is a <see cref="Point"/>, it is equal to 0.
    /// </summary>
    public Rational Length => EndTime - StartTime;

    /// <summary>
    /// True if the element has plus/minus infinite value.
    /// </summary>
    public abstract bool IsInfinite { get; }

    /// <summary>
    /// True if the element has plus infinite value.
    /// </summary>
    public abstract bool IsPlusInfinite { get; }

    /// <summary>
    /// True if the element has minus infinite value.
    /// </summary>
    public abstract bool IsMinusInfinite { get; }

    /// <summary>
    /// True if the element has finite value.
    /// </summary>
    public bool IsFinite => !IsInfinite;

    /// <summary>
    /// True if the element has 0 value.
    /// If the element is a <see cref="Segment"/>, this must be true for all its support.
    /// </summary>
    public abstract bool IsZero { get; }

    #endregion

    #region Serialization

    /// <summary>
    /// Type identification property for JSON (de)serialization.
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public abstract string Type { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Computes the value of the element at the given time.
    /// </summary>
    /// <param name="time">The time of sampling.</param>
    /// <returns>Value of the element at the given time or $+\infty$ if outside definition bounds.</returns>
    public abstract Rational ValueAt(Rational time);

    /// <summary>
    /// True if given time is between the element's definition bounds.
    /// </summary>
    public abstract bool IsDefinedFor(Rational time);

    #endregion Methods

    #region Json Methods

    /// <summary>
    /// Returns string serialization in Json format.
    /// </summary>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, new RationalConverter());
    }

    /// <summary>
    /// Deserializes an <see cref="Element"/>.
    /// </summary>
    public static Element FromJson(string json)
    {
        var element = JsonConvert.DeserializeObject<Element>(json, new RationalConverter());
        if (element == null)
            throw new InvalidOperationException("Invalid JSON format.");
        return element;
    }

    /// <summary>
    /// Returns a string containing C# code to create this Element.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    public abstract string ToCodeString(bool formatted = false, int indentation = 0);

    #endregion Json Methods

    #region Basic manipulations

    /// <summary>
    /// Scales the element by a multiplicative factor.
    /// </summary>
    public abstract Element Scale(Rational scaling);

    /// <summary>
    /// Scales the element by a multiplicative factor.
    /// </summary>
    public static Element operator *(Element element, Rational scaling)
        => element.Scale(scaling);

    /// <summary>
    /// Scales the element by a multiplicative factor.
    /// </summary>
    public static Element operator *(Rational scaling, Element element)
        => element.Scale(scaling);

    /// <summary>
    /// Translates forwards the support by the given time quantity.
    /// </summary>
    public abstract Element Delay(Rational delay);

    /// <summary>
    /// Translates backwards the support by the given time quantity.
    /// </summary>
    public abstract Element Anticipate(Rational time);

    /// <summary>
    /// Shifts the element vertically by an additive factor.
    /// </summary>
    public abstract Element VerticalShift(Rational shift);

    /// <summary>
    /// Shifts the element vertically by an additive factor.
    /// </summary>
    public static Element operator +(Element element, Rational shift)
        => element.VerticalShift(shift);

    /// <summary>
    /// Shifts the element vertically by an additive factor.
    /// </summary>
    public static Element operator +(Rational shift, Element element)
        => element.VerticalShift(shift);

    /// <summary>
    /// Returns the opposite element, $g(t) = -f(t)$.
    /// </summary>
    public abstract Element Negate();

    /// <summary>
    /// Returns the opposite element, $g(t) = -f(t)$.
    /// </summary>
    public static Element operator -(Element e)
        => e.Negate();

    /// <summary>
    /// Compute the inverse function, $f^{-1}(x) = inf { t : f(t) = x }$
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// If the element is non-bijective, i.e. a constant segment.
    /// </exception>
    public abstract Element Inverse();

    #endregion Basic manipulations

    #region Addition and Subtraction operators

    /// <summary>
    /// Sums two elements over their overlapping part.
    /// </summary>
    /// <param name="element">Second operand.</param>
    /// <exception cref="ArgumentException">Thown if the two elements do not overlap.</exception>
    /// <returns>The element resulting from the sum.</returns>
    public abstract Element Addition(Element element);

    /// <summary>
    /// Sums two elements over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thown if the two elements do not overlap.</exception>
    /// <returns>The element resulting from the sum.</returns>
    public static Element Addition(Element a, Element b)
        => a.Addition(b);

    /// <summary>
    /// Sums two elements over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thown if the two elements do not overlap.</exception>
    /// <returns>The element resulting from the sum.</returns>
    public static Element operator +(Element a, Element b)
        => Addition(a, b);

    /// <summary>
    /// Sums a set of elements over their overlapping part.
    /// </summary>
    /// <param name="elements">The elements to be summed.</param>
    /// <exception cref="ArgumentException">Thrown if the elements do not overlap.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the set of elements is empty.</exception>
    /// <returns>The result of the overall sum.</returns>
    public static Element Addition(IEnumerable<Element> elements)
    {
        return elements.Aggregate(Addition);
    }

    /// <summary>
    /// Subtracts two elements over their overlapping part.
    /// </summary>
    /// <param name="element">Second operand.</param>
    /// <exception cref="ArgumentException">Thown if the two elements do not overlap.</exception>
    /// <returns>The element resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public abstract Element Subtraction(Element element);

    /// <summary>
    /// Subtracts two elements over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thown if the two elements do not overlap.</exception>
    /// <returns>The element resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Element Subtraction(Element a, Element b)
        => a.Subtraction(b);

    /// <summary>
    /// Subtracts two elements over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thown if the two elements do not overlap.</exception>
    /// <returns>The element resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Element operator -(Element a, Element b)
        => Subtraction(a, b);

    /// <summary>
    /// Subtracts a set of elements over their overlapping part.
    /// </summary>
    /// <param name="elements">The elements to be summed.</param>
    /// <exception cref="ArgumentException">Thrown if the elements do not overlap.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the set of elements is empty.</exception>
    /// <returns>The result of the overall subtraction.</returns>
    /// <remarks>
    /// The operation does not enforce non-negative values.
    /// As subtraction is not commutative, beware of the order of the operands.
    /// </remarks>
    public static Element Subtraction(IEnumerable<Element> elements)
    {
        return elements.Aggregate(Subtraction);
    }

    #endregion Addition and Subtraction operators

    #region Minimum operator

    /// <summary>
    /// Computes the minimum of two elements over their overlapping part.
    /// The result is either a point, a segment or a segment-point-segment sequence.
    /// </summary>
    /// <param name="element">Second operand.</param>
    /// <returns>The set of segments resulting from the minimum.</returns>
    public abstract List<Element> Minimum(Element element);

    /// <summary>
    /// Computes the minimum of two elements over their overlapping part.
    /// The result is either a point, a segment or a segment-point-segment sequence.
    /// </summary>
    /// <returns>The set of segments resulting from the minimum.</returns>
    public static List<Element> Minimum(Element a, Element b)
        => a.Minimum(b);

    /// <summary>
    /// Computes the minimum of a set of elements over their overlapping part.
    /// </summary>
    /// <param name="elements">Elements of which the minimum has to be computed.</param>
    /// <exception cref="ArgumentException">Thrown if the elements do not overlap.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the set of elements is empty.</exception>
    /// <returns>The result of the overall minimum.</returns>
    public static List<Element> Minimum(IReadOnlyList<Element> elements)
    {
        if (!elements.Any())
            throw new InvalidOperationException("The enumerable is empty.");
        if (elements.Count() == 1)
            return elements.ToList();
        if (elements.Count() == 2)
            return elements.ElementAt(0).Minimum(elements.ElementAt(1));

        var reference = elements.MinBy(e => e.Length);

        switch(reference)
        {
            case Point referencePoint:
            {
                var points = elements
                    .Select(e => SampleToReference(e, referencePoint.Time))
                    .ToList();

                return new List<Element> { 
                    Point.Minimum(points)
                };
            }

            case Segment referenceSegment:
            {
                var segments = elements
                    .Select(e => CutToReference(e, referenceSegment))
                    .ToList();

                return Segment.Minimum(segments);
            }

            default:
                throw new InvalidCastException();
        }

        Point SampleToReference(Element element, Rational time)
        {
            switch (element)
            {
                case Point p:
                {
                    if (p.Time == time)
                        return p;
                    else
                        throw new ArgumentException("Non-overlap between two points.");
                }

                case Segment s:
                {
                    if (s.IsDefinedFor(time))
                        return s.Sample(time);
                    else
                        throw new ArgumentException("Non-overlap between segment and point.");
                }

                default:
                    throw new InvalidCastException();
            }
        }

        Segment CutToReference(Element element, Segment reference)
        {
            switch (element)
            {
                case Segment s:
                {
                    var overlap = Segment.GetOverlap(s, reference);
                    if (overlap != null)
                    {
                        var (start, end) = overlap ?? default;
                        return s.Cut(start, end);
                    }
                    else
                        throw new ArgumentException("Non-overlap between two segments.");
                }

                case Point:
                    throw new InvalidOperationException("Cannot cut a Point to a Segment.");

                default:
                    throw new InvalidCastException();
            }
        }
    }

    #endregion Minimum operator

    #region Maximum operator

    /// <summary>
    /// Computes the maximum of two elements over their overlapping part.
    /// The result is either a point, a segment or a segment-point-segment sequence.
    /// </summary>
    /// <param name="element">Second operand.</param>
    /// <returns>The set of segments resulting from the maximum.</returns>
    public abstract List<Element> Maximum(Element element);

    /// <summary>
    /// Computes the maximum of two elements over their overlapping part.
    /// The result is either a point, a segment or a segment-point-segment sequence.
    /// </summary>
    /// <returns>The set of segments resulting from the maximum.</returns>
    public static List<Element> Maximum(Element a, Element b)
        => a.Maximum(b);

    /// <summary>
    /// Computes the maximum of a set of elements over their overlapping part.
    /// </summary>
    /// <param name="elements">Elements of which the maximum has to be computed.</param>
    /// <exception cref="ArgumentException">Thrown if the elements do not overlap.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the set of elements is empty.</exception>
    /// <returns>The result of the overall maximum.</returns>
    public static List<Element> Maximum(IReadOnlyList<Element> elements)
    {
        if (!elements.Any())
            throw new InvalidOperationException("The enumerable is empty.");
        if (elements.Count() == 1)
            return elements.ToList();
        if (elements.Count() == 2)
            return elements.ElementAt(0).Minimum(elements.ElementAt(1));

        var reference = elements.MinBy(e => e.Length);

        switch(reference)
        {
            case Point referencePoint:
            {
                var points = elements
                    .Select(e => SampleToReference(e, referencePoint.Time))
                    .ToList();

                return new List<Element> { 
                    Point.Maximum(points)
                };
            }

            case Segment referenceSegment:
            {
                var segments = elements
                    .Select(e => CutToReference(e, referenceSegment))
                    .ToList();

                return Segment.Maximum(segments);
            }

            default:
                throw new InvalidCastException();
        }

        Point SampleToReference(Element element, Rational time)
        {
            switch (element)
            {
                case Point p:
                {
                    if (p.Time == time)
                        return p;
                    else
                        throw new ArgumentException("Non-overlap between two points.");
                }

                case Segment s:
                {
                    if (s.IsDefinedFor(time))
                        return s.Sample(time);
                    else
                        throw new ArgumentException("Non-overlap between segment and point.");
                }

                default:
                    throw new InvalidCastException();
            }
        }

        Segment CutToReference(Element element, Segment reference)
        {
            switch (element)
            {
                case Segment s:
                {
                    var overlap = Segment.GetOverlap(s, reference);
                    if (overlap != null)
                    {
                        var (start, end) = overlap ?? default;
                        return s.Cut(start, end);
                    }
                    else
                        throw new ArgumentException("Non-overlap between two segments.");
                }

                case Point:
                    throw new InvalidOperationException("Cannot cut a Point to a Segment.");

                default:
                    throw new InvalidCastException();
            }
        }
    }

    #endregion Maximum operator

    #region Convolution operator 

    /// <summary>
    /// Computes the convolution between two Elements.
    /// </summary>
    /// <param name="element">Second operand.</param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <param name="cutCeiling">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The set of segments resulting from the convolution.</returns>
    /// <remarks>Described in [BT07] Section 3.2.1</remarks>
    public abstract IEnumerable<Element> Convolution(Element element, Rational? cutEnd = null, Rational? cutCeiling = null);

    /// <summary>
    /// Computes the convolution between two Elements.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <param name="cutCeiling">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The set of segments resulting from the convolution.</returns>
    /// <remarks>Described in [BT07] Section 3.2.1</remarks>
    public static IEnumerable<Element> Convolution(Element a, Element b, Rational? cutEnd = null, Rational? cutCeiling = null)
        => a.Convolution(b, cutEnd, cutCeiling);

    #endregion Convolution operator

    #region Deconvolution operator

    /// <summary>
    /// Computes the deconvolution between two Elements.
    /// </summary>
    /// <returns>The set of segments resulting from the deconvolution.</returns>
    /// <remarks>Described in [BT07] Section 3.2.2</remarks>
    public abstract IEnumerable<Element> Deconvolution(Element element);

    /// <summary>
    /// Computes the deconvolution between two Elements.
    /// </summary>
    /// <returns>The set of segments resulting from the deconvolution.</returns>
    /// <remarks>Described in [BT07] Section 3.2.2</remarks>
    public static IEnumerable<Element> Deconvolution(Element a, Element b)
        => a.Deconvolution(b);

    #endregion Deconvolution operator

    #region Max-Plus Convolution operator

    /// <summary>
    /// Computes the max-plus convolution between two Elements.
    /// </summary>
    /// <param name="element">Second operand.</param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The set of segments resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1</remarks>
    public abstract IEnumerable<Element> MaxPlusConvolution(Element element, Rational? cutEnd = null);

    /// <summary>
    /// Computes the max-plus convolution between two Elements.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The set of segments resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1</remarks>
    public static IEnumerable<Element> MaxPlusConvolution(Element a, Element b, Rational? cutEnd = null)
        => a.MaxPlusConvolution(b, cutEnd);

    #endregion Max-Plus Convolution operator

    #region Sub-additive closure

    /// <summary>
    /// Computes the sub-additive closure of the element.
    /// </summary>
    /// <param name="settings"></param>
    public abstract SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null);

    /// <summary>
    /// Computes the sub-additive closure of the pseudo-periodic element.
    /// </summary>
    /// <param name="pseudoPeriodLength">Lenght of the pseudo-period</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period</param>
    /// <param name="settings"></param>
    /// <exception cref="ArgumentException">Thrown if the period is not greater than 0</exception>
    public abstract SubAdditiveCurve SubAdditiveClosure(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight,
        ComputationSettings? settings = null
    );

    #endregion Sub-additive closure
}