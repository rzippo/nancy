using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks the formal results of [ZNS26] on pseudo-inverses and on the convolutions of periodic parts, and the convolutions of Nancy with and without each of the optimizations they justify, on a few curves:
/// the pseudo-period parameters the results state, the bounds on the image, and the values at all times against the definition.
/// The curves are chosen out of seeded random ones by covering, with the fewest and smallest, each kind of tail, start at, below and above 0, continuity, plateau at the start and at the end of the period,
/// jump and fractional value they were drawn with, and as many of the pairs of those kinds as fit eight cases.
/// Where the inferred period of a pair is neither of the operands', that is a kind of its own.
/// </summary>
public class SuperIsospeedFormalResults
{
    private static readonly ComputationSettings WithoutIsomorphism = ComputationSettings.Default() with
    {
        UseConvolutionIsospeedOptimization = false,
        UseConvolutionSuperIsospeedOptimization = false,
        UseBySequenceConvolutionIsospeedOptimization = false
    };

    /// <summary>
    /// Whether $h(t + d) = h(t) + c$ for $t \ge T$, at the values and at the limits of the points of two grids.
    /// </summary>
    private static bool IsPseudoPeriodic(Curve h, Rational T, Rational d, Rational c)
    {
        foreach (var divisions in new[] { 12, 7 })
        for (var k = 0; k <= 3 * divisions; k++)
        {
            var t = T + k * d / divisions;
            if (h.ValueAt(t + d) != h.ValueAt(t) + c)
                return false;
            if (h.RightLimitAt(t + d) != h.RightLimitAt(t) + c)
                return false;
            // the left limit at T is before the pseudo-period starts
            if (t > T && h.LeftLimitAt(t + d) != h.LeftLimitAt(t) + c)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Non-decreasing curves with finite non-negative values, not ultimately constant, of any continuity.
    /// </summary>
    private static readonly Curve[] MixedCurves =
    [
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,new Rational(1, 3)) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: new Rational(1, 3)),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(3, 4),1,0), new Point(new Rational(3, 4),2), new Segment(new Rational(3, 4),new Rational(3, 2),2,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,2,1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,2,2,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 2),2,0), new Point(new Rational(1, 2),3), new Segment(new Rational(1, 2),1,3,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(3, 2),2,1), new Point(new Rational(3, 2),new Rational(7, 2)), new Segment(new Rational(3, 2),new Rational(9, 2),new Rational(9, 2),0) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 3, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,2,2,0), new Point(2,4), new Segment(2,new Rational(5, 2),4,0), new Point(new Rational(5, 2),4), new Segment(new Rational(5, 2),3,5,0) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),new Rational(3, 2),0,1), new Point(new Rational(3, 2),1), new Segment(new Rational(3, 2),new Rational(5, 2),1,1) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
    ];

    /// <summary>
    /// The same, left-continuous.
    /// </summary>
    private static readonly Curve[] LeftContinuousCurves =
    [
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 4),1,0), new Point(new Rational(1, 4),1), new Segment(new Rational(1, 4),new Rational(1, 2),2,new Rational(3, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(11, 8)),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),new Rational(3, 2),0,2) }), pseudoPeriodStart: new Rational(1, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,1), new Point(2,3), new Segment(2,3,3,1) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 2),1,0), new Point(new Rational(1, 2),1), new Segment(new Rational(1, 2),3,3,0), new Point(3,3), new Segment(3,new Rational(11, 2),4,0) }), pseudoPeriodStart: 3, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),1,new Rational(3, 2)), new Point(new Rational(3, 2),new Rational(13, 4)), new Segment(new Rational(3, 2),3,new Rational(21, 4),0), new Point(3,new Rational(21, 4)), new Segment(3,new Rational(9, 2),new Rational(25, 4),0) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 3, pseudoPeriodHeight: 3)
    ];

    /// <summary>
    /// The same, right-continuous.
    /// </summary>
    private static readonly Curve[] RightContinuousCurves =
    [
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(1, 2),2,0), new Point(new Rational(1, 2),2), new Segment(new Rational(1, 2),1,2,new Rational(1, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: new Rational(9, 4)),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(5, 4),1,new Rational(1, 3)), new Point(new Rational(5, 4),new Rational(17, 12)), new Segment(new Rational(5, 4),new Rational(5, 2),new Rational(17, 12),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(17, 12)),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(3, 2),1,0), new Point(new Rational(3, 2),1), new Segment(new Rational(3, 2),new Rational(5, 2),1,1) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(5, 2),1,0), new Point(new Rational(5, 2),1), new Segment(new Rational(5, 2),new Rational(7, 2),1,0) }), pseudoPeriodStart: new Rational(5, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 4),0,new Rational(1, 2)), new Point(new Rational(1, 4),new Rational(1, 8)), new Segment(new Rational(1, 4),new Rational(1, 2),new Rational(1, 8),1) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(3, 8)),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 4),0,1), new Point(new Rational(1, 4),new Rational(5, 4)), new Segment(new Rational(1, 4),new Rational(1, 2),new Rational(5, 4),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(9, 4))
    ];

    /// <summary>
    /// Pairs of such curves, of any continuity.
    /// </summary>
    private static readonly (Curve f, Curve g)[] MixedPairs =
    [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,new Rational(1, 2)), new Point(1,new Rational(1, 2)), new Segment(1,2,new Rational(3, 2),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: new Rational(3, 2)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,1,new Rational(3, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: new Rational(3, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1), new Point(1,1), new Segment(1,2,1,1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,2,1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(5, 4),0,2), new Point(new Rational(5, 4),new Rational(7, 2)), new Segment(new Rational(5, 4),new Rational(5, 2),new Rational(7, 2),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(9, 2)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(3, 4),3,new Rational(1, 3)), new Point(new Rational(3, 4),new Rational(17, 4)), new Segment(new Rational(3, 4),new Rational(3, 2),new Rational(17, 4),new Rational(1, 3)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: new Rational(7, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,0,0), new Point(2,0), new Segment(2,3,1,1), new Point(3,3), new Segment(3,new Rational(9, 2),3,0) }), pseudoPeriodStart: 3, pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,2), new Point(1,3), new Segment(1,2,3,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1), new Point(1,2), new Segment(1,2,2,2), new Point(2,4), new Segment(2,3,4,2) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,2), new Point(new Rational(3, 2),5), new Segment(new Rational(3, 2),new Rational(9, 4),5,new Rational(1, 3)), new Point(new Rational(9, 4),new Rational(25, 4)), new Segment(new Rational(9, 4),3,new Rational(25, 4),new Rational(1, 3)) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: new Rational(3, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,new Rational(3, 2)), new Point(1,new Rational(5, 2)), new Segment(1,new Rational(5, 2),new Rational(7, 2),new Rational(3, 2)), new Point(new Rational(5, 2),new Rational(27, 4)), new Segment(new Rational(5, 2),new Rational(7, 2),new Rational(27, 4),2), new Point(new Rational(7, 2),new Rational(35, 4)), new Segment(new Rational(7, 2),new Rational(9, 2),new Rational(35, 4),0) }), pseudoPeriodStart: new Rational(5, 2), pseudoPeriodLength: 2, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,1,2,2), new Point(1,4), new Segment(1,2,4,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 2),1,new Rational(1, 3)), new Point(new Rational(1, 2),new Rational(7, 6)), new Segment(new Rational(1, 2),new Rational(5, 2),new Rational(13, 6),0), new Point(new Rational(5, 2),new Rational(13, 6)), new Segment(new Rational(5, 2),5,new Rational(13, 6),2), new Point(5,new Rational(43, 6)), new Segment(5,6,new Rational(43, 6),2) }), pseudoPeriodStart: 5, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,0), new Point(new Rational(3, 2),1), new Segment(new Rational(3, 2),3,1,2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 3, pseudoPeriodHeight: 4)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,2), new Point(1,3), new Segment(1,2,3,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,1), new Point(2,4), new Segment(2,new Rational(7, 2),4,new Rational(1, 3)), new Point(new Rational(7, 2),new Rational(9, 2)), new Segment(new Rational(7, 2),new Rational(11, 2),new Rational(11, 2),new Rational(1, 3)), new Point(new Rational(11, 2),new Rational(49, 6)), new Segment(new Rational(11, 2),7,new Rational(49, 6),2), new Point(7,new Rational(73, 6)), new Segment(7,new Rational(17, 2),new Rational(73, 6),0) }), pseudoPeriodStart: new Rational(11, 2), pseudoPeriodLength: 3, pseudoPeriodHeight: 5)
        )
    ];

    /// <summary>
    /// Pairs of left-continuous such curves.
    /// </summary>
    private static readonly (Curve f, Curve g)[] LeftContinuousPairs =
    [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,new Rational(1, 2)), new Point(1,new Rational(3, 2)), new Segment(1,2,new Rational(3, 2),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: new Rational(3, 2)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),1,0), new Point(new Rational(1, 2),1), new Segment(new Rational(1, 2),1,2,2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 3)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,new Rational(9, 4),1,1), new Point(new Rational(9, 4),new Rational(9, 4)), new Segment(new Rational(9, 4),new Rational(7, 2),new Rational(13, 4),new Rational(1, 2)) }), pseudoPeriodStart: 1, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(23, 8)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,2,0), new Point(2,2), new Segment(2,3,3,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 2, pseudoPeriodHeight: 4),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,2,2,2), new Point(2,6), new Segment(2,3,6,2) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(3, 2),2,0), new Point(new Rational(3, 2),2), new Segment(new Rational(3, 2),3,2,2), new Point(3,5), new Segment(3,new Rational(9, 2),5,0) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 3, pseudoPeriodHeight: 3),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,new Rational(1, 2)), new Point(new Rational(1, 2),new Rational(1, 4)), new Segment(new Rational(1, 2),new Rational(3, 2),new Rational(1, 4),2) }), pseudoPeriodStart: new Rational(1, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 4),0,0), new Point(new Rational(3, 4),0), new Segment(new Rational(3, 4),new Rational(3, 2),1,new Rational(1, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: new Rational(11, 8)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(3, 2),3,2), new Point(new Rational(3, 2),6), new Segment(new Rational(3, 2),2,6,0), new Point(2,6), new Segment(2,new Rational(5, 2),6,new Rational(1, 2)) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: new Rational(1, 4))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,new Rational(1, 3)), new Point(1,new Rational(1, 3)), new Segment(1,2,new Rational(1, 3),new Rational(1, 2)), new Point(2,new Rational(5, 6)), new Segment(2,4,new Rational(11, 6),0), new Point(4,new Rational(11, 6)), new Segment(4,new Rational(11, 2),new Rational(11, 6),new Rational(1, 3)), new Point(new Rational(11, 2),new Rational(7, 3)), new Segment(new Rational(11, 2),7,new Rational(10, 3),0) }), pseudoPeriodStart: 4, pseudoPeriodLength: 3, pseudoPeriodHeight: new Rational(3, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(1, 2),2,new Rational(1, 2)), new Point(new Rational(1, 2),new Rational(9, 4)), new Segment(new Rational(1, 2),new Rational(5, 2),new Rational(13, 4),new Rational(3, 2)), new Point(new Rational(5, 2),new Rational(25, 4)), new Segment(new Rational(5, 2),4,new Rational(33, 4),0), new Point(4,new Rational(33, 4)), new Segment(4,5,new Rational(33, 4),2) }), pseudoPeriodStart: 4, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(5, 4),0,0), new Point(new Rational(5, 4),0), new Segment(new Rational(5, 4),new Rational(5, 2),0,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(5, 4))
        )
    ];

    /// <summary>
    /// Pairs of right-continuous such curves.
    /// </summary>
    private static readonly (Curve f, Curve g)[] RightContinuousPairs =
    [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,0), new Point(new Rational(1, 2),0), new Segment(new Rational(1, 2),new Rational(3, 2),0,2) }), pseudoPeriodStart: new Rational(1, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,1), new Point(new Rational(3, 2),new Rational(5, 2)), new Segment(new Rational(3, 2),3,new Rational(5, 2),1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 3, pseudoPeriodHeight: 4),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(1, 2),2,0), new Point(new Rational(1, 2),3), new Segment(new Rational(1, 2),1,3,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,1), new Point(new Rational(3, 2),new Rational(3, 2)), new Segment(new Rational(3, 2),3,new Rational(3, 2),new Rational(3, 2)), new Point(3,new Rational(23, 4)), new Segment(3,4,new Rational(23, 4),2) }), pseudoPeriodStart: 3, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(3, 4),1,new Rational(1, 3)), new Point(new Rational(3, 4),new Rational(5, 4)), new Segment(new Rational(3, 4),new Rational(3, 2),new Rational(5, 4),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: new Rational(5, 4))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 2),1,0), new Point(new Rational(1, 2),1), new Segment(new Rational(1, 2),1,1,new Rational(1, 2)), new Point(1,new Rational(9, 4)), new Segment(1,new Rational(5, 2),new Rational(9, 4),1), new Point(new Rational(5, 2),new Rational(27, 4)), new Segment(new Rational(5, 2),new Rational(7, 2),new Rational(27, 4),1) }), pseudoPeriodStart: new Rational(5, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,0,new Rational(3, 2)), new Point(2,5), new Segment(2,4,5,new Rational(3, 2)), new Point(4,8), new Segment(4,new Rational(9, 2),8,new Rational(3, 2)), new Point(new Rational(9, 2),new Rational(35, 4)), new Segment(new Rational(9, 2),5,new Rational(35, 4),0) }), pseudoPeriodStart: 4, pseudoPeriodLength: 1, pseudoPeriodHeight: new Rational(3, 4)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,3), new Segment(0,new Rational(1, 2),3,2), new Point(new Rational(1, 2),5), new Segment(new Rational(1, 2),2,5,new Rational(1, 3)), new Point(2,new Rational(11, 2)), new Segment(2,3,new Rational(11, 2),2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 3, pseudoPeriodHeight: new Rational(9, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(3, 2),2,0), new Point(new Rational(3, 2),3), new Segment(new Rational(3, 2),new Rational(5, 2),3,0), new Point(new Rational(5, 2),5), new Segment(new Rational(5, 2),new Rational(15, 4),5,1), new Point(new Rational(15, 4),new Rational(25, 4)), new Segment(new Rational(15, 4),4,new Rational(25, 4),0) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(13, 4)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 2),1,2), new Point(new Rational(1, 2),2), new Segment(new Rational(1, 2),1,2,new Rational(1, 2)), new Point(1,new Rational(9, 4)), new Segment(1,2,new Rational(9, 4),2), new Point(2,new Rational(25, 4)), new Segment(2,new Rational(5, 2),new Rational(25, 4),2), new Point(new Rational(5, 2),new Rational(33, 4)), new Segment(new Rational(5, 2),3,new Rational(33, 4),2) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 4)
        )
    ];

    /// <summary>
    /// Pairs of left-continuous such curves for the checks against the definition, which cost more: the period that [ZNS26] Theorem 35 infers is neither of the operands'.
    /// </summary>
    private static readonly (Curve f, Curve g)[] SmallLeftContinuousPairs =
    [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 4),0,2), new Point(new Rational(3, 4),new Rational(3, 2)), new Segment(new Rational(3, 4),new Rational(3, 2),new Rational(3, 2),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: new Rational(3, 2)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,2,3,1), new Point(2,5), new Segment(2,3,5,1) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(5, 4),0,0), new Point(new Rational(5, 4),0), new Segment(new Rational(5, 4),new Rational(5, 2),0,0), new Point(new Rational(5, 2),0), new Segment(new Rational(5, 2),new Rational(15, 4),0,new Rational(3, 2)) }), pseudoPeriodStart: new Rational(5, 4), pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(15, 8))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 4),1,new Rational(1, 3)), new Point(new Rational(3, 4),new Rational(5, 4)), new Segment(new Rational(3, 4),new Rational(3, 2),new Rational(9, 4),new Rational(3, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: new Rational(27, 8)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,new Rational(3, 2)), new Point(new Rational(3, 2),new Rational(9, 4)), new Segment(new Rational(3, 2),new Rational(7, 2),new Rational(17, 4),0), new Point(new Rational(7, 2),new Rational(17, 4)), new Segment(new Rational(7, 2),new Rational(11, 2),new Rational(25, 4),new Rational(3, 2)), new Point(new Rational(11, 2),new Rational(37, 4)), new Segment(new Rational(11, 2),new Rational(13, 2),new Rational(37, 4),1) }), pseudoPeriodStart: new Rational(11, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,1), new Point(new Rational(1, 2),new Rational(1, 2)), new Segment(new Rational(1, 2),new Rational(3, 2),new Rational(5, 2),0), new Point(new Rational(3, 2),new Rational(5, 2)), new Segment(new Rational(3, 2),3,new Rational(9, 2),new Rational(1, 2)), new Point(3,new Rational(21, 4)), new Segment(3,new Rational(9, 2),new Rational(25, 4),0), new Point(new Rational(9, 2),new Rational(25, 4)), new Segment(new Rational(9, 2),6,new Rational(25, 4),new Rational(1, 2)) }), pseudoPeriodStart: 3, pseudoPeriodLength: 3, pseudoPeriodHeight: new Rational(7, 4)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,3,new Rational(3, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: new Rational(7, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,new Rational(1, 2)), new Point(new Rational(3, 2),new Rational(3, 4)), new Segment(new Rational(3, 2),new Rational(5, 2),new Rational(3, 4),2) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,1), new Point(new Rational(1, 2),new Rational(1, 2)), new Segment(new Rational(1, 2),new Rational(5, 2),new Rational(1, 2),new Rational(1, 2)), new Point(new Rational(5, 2),new Rational(3, 2)), new Segment(new Rational(5, 2),new Rational(9, 2),new Rational(5, 2),1), new Point(new Rational(9, 2),new Rational(9, 2)), new Segment(new Rational(9, 2),new Rational(23, 4),new Rational(11, 2),0), new Point(new Rational(23, 4),new Rational(11, 2)), new Segment(new Rational(23, 4),new Rational(33, 4),new Rational(13, 2),0) }), pseudoPeriodStart: new Rational(23, 4), pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,1,2,1), new Point(1,3), new Segment(1,new Rational(3, 2),3,new Rational(3, 2)), new Point(new Rational(3, 2),new Rational(15, 4)), new Segment(new Rational(3, 2),new Rational(11, 4),new Rational(15, 4),new Rational(1, 2)), new Point(new Rational(11, 4),new Rational(35, 8)), new Segment(new Rational(11, 4),4,new Rational(51, 8),0), new Point(4,new Rational(51, 8)), new Segment(4,new Rational(21, 4),new Rational(59, 8),new Rational(1, 2)) }), pseudoPeriodStart: new Rational(11, 4), pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(29, 8)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,2), new Point(2,5), new Segment(2,3,5,2) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        )
    ];

    /// <summary>
    /// The same, right-continuous.
    /// </summary>
    private static readonly (Curve f, Curve g)[] SmallRightContinuousPairs =
    [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(5, 4),0,0), new Point(new Rational(5, 4),1), new Segment(new Rational(5, 4),new Rational(5, 2),1,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,2) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,3), new Segment(1,2,3,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: 3),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(3, 2),1,0), new Point(new Rational(3, 2),2), new Segment(new Rational(3, 2),3,2,new Rational(1, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: 3, pseudoPeriodHeight: new Rational(15, 4))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(5, 4),0,2), new Point(new Rational(5, 4),new Rational(5, 2)), new Segment(new Rational(5, 4),new Rational(5, 2),new Rational(5, 2),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(5, 2)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,new Rational(1, 3)), new Point(1,new Rational(7, 3)), new Segment(1,2,new Rational(7, 3),2), new Point(2,new Rational(16, 3)), new Segment(2,3,new Rational(16, 3),1) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,3), new Segment(0,2,3,new Rational(1, 3)), new Point(2,new Rational(14, 3)), new Segment(2,new Rational(13, 4),new Rational(14, 3),0), new Point(new Rational(13, 4),new Rational(14, 3)), new Segment(new Rational(13, 4),new Rational(9, 2),new Rational(14, 3),new Rational(1, 2)) }), pseudoPeriodStart: 2, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(13, 8)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(3, 2),1,new Rational(3, 2)), new Point(new Rational(3, 2),new Rational(17, 4)), new Segment(new Rational(3, 2),3,new Rational(17, 4),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 3, pseudoPeriodHeight: new Rational(17, 4))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 2),1,0), new Point(new Rational(1, 2),1), new Segment(new Rational(1, 2),2,1,new Rational(1, 2)) }), pseudoPeriodStart: new Rational(1, 2), pseudoPeriodLength: new Rational(3, 2), pseudoPeriodHeight: new Rational(3, 4)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,0), new Point(new Rational(3, 2),2), new Segment(new Rational(3, 2),new Rational(5, 2),2,new Rational(1, 3)), new Point(new Rational(5, 2),new Rational(16, 3)), new Segment(new Rational(5, 2),new Rational(7, 2),new Rational(16, 3),2), new Point(new Rational(7, 2),new Rational(25, 3)), new Segment(new Rational(7, 2),new Rational(9, 2),new Rational(25, 3),2) }), pseudoPeriodStart: new Rational(7, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(3, 2),1,new Rational(1, 2)), new Point(new Rational(3, 2),new Rational(7, 4)), new Segment(new Rational(3, 2),2,new Rational(7, 4),0), new Point(2,new Rational(15, 4)), new Segment(2,new Rational(7, 2),new Rational(15, 4),0), new Point(new Rational(7, 2),new Rational(23, 4)), new Segment(new Rational(7, 2),new Rational(9, 2),new Rational(23, 4),0) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 3, pseudoPeriodHeight: 4),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(5, 4),0,0), new Point(new Rational(5, 4),0), new Segment(new Rational(5, 4),new Rational(5, 2),0,new Rational(3, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(15, 8))
        )
    ];

    /// <summary>
    /// Triples of such curves, of any continuity.
    /// </summary>
    private static readonly (Curve f, Curve g, Curve h)[] Triples =
    [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1), new Point(1,1), new Segment(1,2,1,1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,2), new Point(1,2), new Segment(1,2,2,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 4),0,new Rational(1, 3)), new Point(new Rational(1, 4),new Rational(13, 12)), new Segment(new Rational(1, 4),new Rational(1, 2),new Rational(25, 12),new Rational(1, 3)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(19, 6))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,2), new Point(1,3), new Segment(1,2,3,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,2,1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,new Rational(3, 2)), new Point(new Rational(3, 2),new Rational(13, 4)), new Segment(new Rational(3, 2),new Rational(7, 4),new Rational(17, 4),new Rational(1, 3)), new Point(new Rational(7, 4),new Rational(16, 3)), new Segment(new Rational(7, 4),2,new Rational(19, 3),0) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(49, 12))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,2,1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(5, 4),1,new Rational(3, 2)), new Point(new Rational(5, 4),new Rational(31, 8)), new Segment(new Rational(5, 4),new Rational(5, 2),new Rational(31, 8),new Rational(1, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(9, 2)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(1, 2),3,new Rational(3, 2)), new Point(new Rational(1, 2),new Rational(15, 4)), new Segment(new Rational(1, 2),new Rational(5, 2),new Rational(15, 4),new Rational(1, 2)), new Point(new Rational(5, 2),new Rational(19, 4)), new Segment(new Rational(5, 2),new Rational(11, 2),new Rational(19, 4),0) }), pseudoPeriodStart: new Rational(5, 2), pseudoPeriodLength: 3, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,0), new Point(new Rational(3, 2),1), new Segment(new Rational(3, 2),3,2,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 3, pseudoPeriodHeight: 3),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,new Rational(3, 2)), new Point(2,5), new Segment(2,3,5,0), new Point(3,6), new Segment(3,4,6,0) }), pseudoPeriodStart: 2, pseudoPeriodLength: 2, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,0), new Point(1,1), new Segment(1,2,1,new Rational(1, 3)), new Point(2,new Rational(4, 3)), new Segment(2,3,new Rational(4, 3),1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 2, pseudoPeriodHeight: new Rational(4, 3))
        )
    ];

    private static IEnumerable<object[]> Rows(IEnumerable<Curve> curves) => curves.Select(c => new object[] { c });

    private static IEnumerable<object[]> Rows(IEnumerable<(Curve f, Curve g)> pairs) => pairs.Select(p => new object[] { p.f, p.g });

    public static IEnumerable<object[]> TripleTestCases() => Triples.Select(t => new object[] { t.f, t.g, t.h });

    /// <summary>
    /// [ZNS26] Proposition 10 (1): the convolutions distribute over the minimum and the maximum, $f \otimes (g \wedge h) = (f \otimes g) \wedge (f \otimes h)$ and $f \overline{\otimes} (g \vee h) = (f \overline{\otimes} g) \vee (f \overline{\otimes} h)$.
    /// </summary>
    [Theory]
    [MemberData(nameof(TripleTestCases))]
    public void ConvolutionsDistributeOverTheMinimumAndTheMaximum(Curve f, Curve g, Curve h)
    {
        Assert.True(Curve.Equivalent(
            Curve.Convolution(f, Curve.Minimum(g, h)),
            Curve.Minimum(Curve.Convolution(f, g), Curve.Convolution(f, h))));
        Assert.True(Curve.Equivalent(
            Curve.MaxPlusConvolution(f, Curve.Maximum(g, h)),
            Curve.Maximum(Curve.MaxPlusConvolution(f, g), Curve.MaxPlusConvolution(f, h))));
    }

    /// <summary>
    /// [ZNS26] Proposition 10 (2) to (4): the convolutions of left-continuous operands are left-continuous, those of right-continuous operands are right-continuous,
    /// and those of non-decreasing operands are non-decreasing.
    /// </summary>
    [Theory]
    [MemberData(nameof(PairTestCases))]
    public void ConvolutionsKeepContinuityAndMonotonicity(Curve f, Curve g)
    {
        var (left_f, left_g) = (f.ToLeftContinuous(), g.ToLeftContinuous());
        Assert.True(Curve.Convolution(left_f, left_g).IsLeftContinuous);
        Assert.True(Curve.MaxPlusConvolution(left_f, left_g).IsLeftContinuous);

        var (right_f, right_g) = (f.ToRightContinuous(), g.ToRightContinuous());
        Assert.True(Curve.Convolution(right_f, right_g).IsRightContinuous);
        Assert.True(Curve.MaxPlusConvolution(right_f, right_g).IsRightContinuous);

        Assert.True(Curve.Convolution(f, g).IsNonDecreasing);
        Assert.True(Curve.MaxPlusConvolution(f, g).IsNonDecreasing);
    }

    public static IEnumerable<object[]> CurveTestCases() => Rows(MixedCurves);
    public static IEnumerable<object[]> LeftContinuousCurveTestCases() => Rows(LeftContinuousCurves);
    public static IEnumerable<object[]> RightContinuousCurveTestCases() => Rows(RightContinuousCurves);
    public static IEnumerable<object[]> PairTestCases() => Rows(MixedPairs);
    public static IEnumerable<object[]> LeftContinuousPairTestCases() => Rows(LeftContinuousPairs);
    public static IEnumerable<object[]> RightContinuousPairTestCases() => Rows(RightContinuousPairs);

    /// <summary>
    /// [ZNS26] Theorem 17: the lower and upper pseudo-inverses are pseudo-periodic with $T = f(T_f + d_f)$ and $T = f(T_f)$, period $c_f$ and height $d_f$.
    /// </summary>
    [Theory]
    [MemberData(nameof(CurveTestCases))]
    public void PseudoInverseParameters(Curve f)
    {
        var (T, d, c) = (f.PseudoPeriodStart, f.PseudoPeriodLength, f.PseudoPeriodHeight);
        Assert.True(IsPseudoPeriodic(f.LowerPseudoInverse(), f.ValueAt(T + d), c, d));
        Assert.True(IsPseudoPeriodic(f.UpperPseudoInverse(), f.ValueAt(T), c, d));
    }

    /// <summary>
    /// [ZNS26] Theorem 20: the same over $[a, +\infty[$, with $f(a + d_f)$ and $f(a)$ if $a > T_f$.
    /// </summary>
    [Theory]
    [MemberData(nameof(LeftContinuousCurveTestCases))]
    public void PseudoInverseOverIntervalParametersOfLeftContinuousCurves(Curve f)
        => AssertPseudoInverseOverIntervalParameters(f);

    [Theory]
    [MemberData(nameof(RightContinuousCurveTestCases))]
    public void PseudoInverseOverIntervalParametersOfRightContinuousCurves(Curve f)
        => AssertPseudoInverseOverIntervalParameters(f);

    private static void AssertPseudoInverseOverIntervalParameters(Curve f)
    {
        var (T, d, c) = (f.PseudoPeriodStart, f.PseudoPeriodLength, f.PseudoPeriodHeight);
        foreach (var a in new Rational[] { 0, T / 2, T, T + d / 2 })
        {
            var (lowerStart, upperStart) = a <= T ? (f.ValueAt(T + d), f.ValueAt(T)) : (f.ValueAt(a + d), f.ValueAt(a));
            Assert.True(IsPseudoPeriodic(f.LowerPseudoInverseOverInterval(a), lowerStart, c, d), $"lower, a = {a}");
            Assert.True(IsPseudoPeriodic(f.UpperPseudoInverseOverInterval(a), upperStart, c, d), $"upper, a = {a}");
        }
    }

    /// <summary>
    /// [ZNS26] Lemma 21: for right-continuous $f$, with $T^*$ and $T^{**}$ as defined there over $[a, +\infty[$, if $f$ is pseudo-periodic from $T^*$ and $T^{**} = T^* + d_f$,
    /// the lower pseudo-inverse over $[a, +\infty[$ is pseudo-periodic from $f(T_f)$.
    /// </summary>
    [Theory]
    [MemberData(nameof(RightContinuousCurveTestCases))]
    public void LowerPseudoInverseIsPseudoPeriodicFromTheValueAtTheStart(Curve f)
    {
        var (T, d, c) = (f.PseudoPeriodStart, f.PseudoPeriodLength, f.PseudoPeriodHeight);
        foreach (var a in new Rational[] { 0, T / 2, T })
        {
            var lpi = f.LowerPseudoInverseOverInterval(a);
            var tStar = lpi.ValueAt(f.ValueAt(T));
            var tStarStar = lpi.ValueAt(f.ValueAt(tStar + d));
            if (tStarStar != tStar + d || !IsPseudoPeriodic(f, tStar, d, c))
                continue;
            Assert.True(IsPseudoPeriodic(lpi, f.ValueAt(T), c, d), $"a = {a}");
        }
    }

    /// <summary>
    /// [ZNS26] Lemma 22 states that the lower pseudo-inverse over $[f(a), +\infty[$ of the upper one over $[a, +\infty[$ is pseudo-periodic from $f^{-1}_{\uparrow, I}(f(T_f))$,
    /// and writes that this is $T_f$. That holds from $a' = f^{-1}_{\uparrow, I}(f(T_f))$, which is $T_f$ unless $f$ has a plateau at $T_f$.
    /// </summary>
    [Theory]
    [MemberData(nameof(LeftContinuousCurveTestCases))]
    public void LowerOfUpperPseudoInverseIsPseudoPeriodicFromTheEndOfThePlateau(Curve f)
    {
        var (T, d, c) = (f.PseudoPeriodStart, f.PseudoPeriodLength, f.PseudoPeriodHeight);
        foreach (var a in new Rational[] { 0, T / 2, T })
        {
            var upi = f.UpperPseudoInverseOverInterval(a);
            var lpi = upi.LowerPseudoInverseOverInterval(f.ValueAt(a));
            var aPrime = upi.ValueAt(f.ValueAt(T));
            Assert.True(aPrime >= T);
            Assert.True(IsPseudoPeriodic(lpi, aPrime, d, c), $"a = {a}");
        }
    }

    /// <summary>
    /// [ZNS26] Propositions 23 and 25: for left-continuous $f$, the lower pseudo-inverse over $[f(a), +\infty[$ of the upper one over $[a, +\infty[$ is $f|^\wedge_{[a', +\infty[}$,
    /// and with the reconstruction operator of Definition 24, $f|^\wedge_{[a, +\infty[}$.
    /// </summary>
    [Theory]
    [MemberData(nameof(LeftContinuousCurveTestCases))]
    public void LowerOfUpperPseudoInverseIsTheRestriction(Curve f)
    {
        foreach (var a in new Rational[] { 0, f.PseudoPeriodStart / 2, f.PseudoPeriodStart })
        {
            var (_, restricted) = f.Decompose(a, minDecomposition: true);
            var upi = restricted.UpperPseudoInverseOverInterval(a);
            var f_a = f.ValueAt(a);
            var lpi = upi.LowerPseudoInverseOverInterval(f_a);

            var aPrime = f.UpperPseudoInverseOverInterval(a).ValueAt(f_a);
            Assert.True(Curve.Equivalent(f.Decompose(aPrime, minDecomposition: true).right, lpi), $"Proposition 23, a = {a}");

            if (aPrime == a)
                continue;
            // reconstruction: f(a') on [a, a'[
            var value = f.ValueAt(aPrime);
            var elements = new List<Element>();
            if (a > 0)
                elements.AddRange([ Point.PlusInfinite(0), Segment.PlusInfinite(0, a), new Point(a, value) ]);
            else
                elements.Add(new Point(0, value));
            elements.AddRange([ Segment.Constant(a, aPrime, value), Point.PlusInfinite(aPrime), Segment.PlusInfinite(aPrime, aPrime + 1) ]);
            var reconstructed = Curve.Minimum(lpi, new Curve(new Sequence(elements), aPrime, 1, 0));
            Assert.True(Curve.Equivalent(restricted, reconstructed), $"Proposition 25, a = {a}");
        }
    }

    /// <summary>
    /// [ZNS26] Lemma 26: for $x$ between $x_1$ and $x_2$ above $f(a)$, the upper pseudo-inverse over $[a, +\infty[$ is the supremum over the cut $[t_1, t_2]$ for the images of $x_1$ and $x_2$.
    /// </summary>
    [Theory]
    [MemberData(nameof(LeftContinuousCurveTestCases))]
    public void UpperPseudoInverseNeedsOnlyTheCutBetweenItsValues(Curve f)
    {
        foreach (var a in new Rational[] { 0, f.PseudoPeriodStart })
        {
            var upi = f.UpperPseudoInverseOverInterval(a);
            var f_a = f.ValueAt(a);
            var heights = new[] { f_a, f_a + new Rational(1, 3), f_a + 1, f_a + new Rational(5, 2), f_a + 4, f_a + 9 };
            for (var i = 0; i + 1 < heights.Length; i++)
            {
                var (x1, x2) = (heights[i], heights[i + 1]);
                var (t1, t2) = (upi.ValueAt(x1), upi.ValueAt(x2));
                var cut = f.Cut(t1, t2, true, true).Elements.ToList();
                for (var k = 0; k <= 12; k++)
                {
                    var x = x1 + (x2 - x1) * k / 12;
                    Assert.True(PseudoInverseDefinitions.Supremum(cut, x) == upi.ValueAt(x), $"a = {a}, x = {x}");
                }
            }
        }
    }

    /// <summary>
    /// [ZNS26] Lemma 27: the same for the lower pseudo-inverse, right-continuous $f$, and the infimum.
    /// </summary>
    [Theory]
    [MemberData(nameof(RightContinuousCurveTestCases))]
    public void LowerPseudoInverseNeedsOnlyTheCutBetweenItsValues(Curve f)
    {
        foreach (var a in new Rational[] { 0, f.PseudoPeriodStart })
        {
            var lpi = f.LowerPseudoInverseOverInterval(a);
            var f_a = f.ValueAt(a);
            var heights = new[] { f_a, f_a + new Rational(1, 3), f_a + 1, f_a + new Rational(5, 2), f_a + 4, f_a + 9 };
            for (var i = 0; i + 1 < heights.Length; i++)
            {
                var (x1, x2) = (heights[i], heights[i + 1]);
                var (t1, t2) = (lpi.ValueAt(x1), lpi.ValueAt(x2));
                var cut = f.Cut(t1, t2, true, true).Elements.ToList();
                for (var k = 0; k <= 12; k++)
                {
                    var x = x1 + (x2 - x1) * k / 12;
                    Assert.True(PseudoInverseDefinitions.Infimum(cut, x) == lpi.ValueAt(x), $"a = {a}, x = {x}");
                }
            }
        }
    }

    /// <summary>
    /// [ZNS26] Corollaries 12 and 14: the (min,+) convolution of the periodic parts $f^\wedge_p \otimes g^\wedge_p$, which is $+\infty$ before $T_f + T_g$ and $f(T_f) + g(T_g)$ there,
    /// is pseudo-periodic from $T_f + T_g + \mathrm{lcm}(d_f, d_g)$, with period $\mathrm{lcm}(d_f, d_g)$ and the height of the slower operand; for (max,+), $-\infty$ before, and the faster.
    /// </summary>
    [Theory]
    [MemberData(nameof(PairTestCases))]
    public void ConvolutionOfPeriodicPartsDirectParameters(Curve f, Curve g)
    {
        var (T_f, d_f, c_f) = (f.PseudoPeriodStart, f.PseudoPeriodLength, f.PseudoPeriodHeight);
        var (T_g, d_g, c_g) = (g.PseudoPeriodStart, g.PseudoPeriodLength, g.PseudoPeriodHeight);
        var (A, B) = (T_f + T_g, f.ValueAt(T_f) + g.ValueAt(T_g));
        var lcm = Rational.LeastCommonMultiple(d_f, d_g);

        var min = Curve.Convolution(f.Decompose(T_f).right, g.Decompose(T_g).right, WithoutIsomorphism);
        Assert.True(IsPseudoPeriodic(min, A + lcm, lcm, lcm * Rational.Min(c_f / d_f, c_g / d_g)));
        Assert.Equal(B, min.ValueAt(A));
        if (A > 0)
            Assert.True(Enumerable.Range(0, 12).All(k => min.ValueAt(A * k / 12).IsPlusInfinite));

        var max = Curve.MaxPlusConvolution(f.Decompose(T_f, minDecomposition: false).right, g.Decompose(T_g, minDecomposition: false).right, WithoutIsomorphism);
        Assert.True(IsPseudoPeriodic(max, A + lcm, lcm, lcm * Rational.Max(c_f / d_f, c_g / d_g)));
        Assert.Equal(B, max.ValueAt(A));
        if (A > 0)
            Assert.True(Enumerable.Range(0, 12).All(k => max.ValueAt(A * k / 12).IsMinusInfinite));
    }

    /// <summary>
    /// [ZNS26] Theorem 33, Proposition 37 and Theorem 35, for left-continuous operands: the (min,+) convolution of the periodic parts is pseudo-periodic with
    /// $T^{(2)} = \sup\{ t \ge T_f + T_g \mid h(t) \le f(T_f) + g(T_g) + \mathrm{lcm}(c_f, c_g) \}$, $d^{(2)} = \max(k_{c_g} d_g, k_{c_f} d_f)$ and $c^{(2)} = \mathrm{lcm}(c_f, c_g)$,
    /// takes values up to $f(T_f) + g(T_g) + 2 \mathrm{lcm}(c_f, c_g)$ before $T^{(2)} + d^{(2)}$,
    /// and $\gcd(d^{(1)}, d^{(2)})$ is also a period, from the later of the two starts.
    /// </summary>
    [Theory]
    [MemberData(nameof(LeftContinuousPairTestCases))]
    public void MinPlusConvolutionOfPeriodicPartsInverseParameters(Curve f, Curve g)
    {
        var (T_f, d_f, c_f) = (f.PseudoPeriodStart, f.PseudoPeriodLength, f.PseudoPeriodHeight);
        var (T_g, d_g, c_g) = (g.PseudoPeriodStart, g.PseudoPeriodLength, g.PseudoPeriodHeight);
        var (A, B) = (T_f + T_g, f.ValueAt(T_f) + g.ValueAt(T_g));
        var lcm_c = Rational.LeastCommonMultiple(c_f, c_g);
        var (k_cf, k_cg) = (lcm_c / c_f, lcm_c / c_g);

        var h = Curve.Convolution(f.Decompose(T_f).right, g.Decompose(T_g).right, WithoutIsomorphism);
        var T2 = h.UpperPseudoInverseOverInterval(A).ValueAt(B + lcm_c);
        var d2 = Rational.Max(k_cg * d_g, k_cf * d_f);
        Assert.True(IsPseudoPeriodic(h, T2, d2, lcm_c), "Theorem 33");
        Assert.True(h.LeftLimitAt(T2 + d2) <= B + 2 * lcm_c, "Proposition 37");

        var lcm_d = Rational.LeastCommonMultiple(d_f, d_g);
        var c1 = lcm_d * Rational.Min(c_f / d_f, c_g / d_g);
        Assert.Equal(lcm_c / d2, c1 / lcm_d);
        var d3 = Rational.GreatestCommonDivisor(lcm_d, d2);
        Assert.True(IsPseudoPeriodic(h, Rational.Max(A + lcm_d, T2), d3, d3 * (c1 / lcm_d)), "Theorem 35");
    }

    /// <summary>
    /// [ZNS26] Theorem 40 and Proposition 42, for right-continuous operands such that $T^{**} = T^* + d$ for both (Lemma 21): the (max,+) convolution of the periodic parts is pseudo-periodic with
    /// $T^{(2)} = \inf\{ t \ge T_f + T_g \mid h(t) \ge f(T_f) + g(T_g) + \mathrm{lcm}(c_f, c_g) \}$, $d^{(2)} = \min(k_{c_g} d_g, k_{c_f} d_f)$ and $c^{(2)} = \mathrm{lcm}(c_f, c_g)$,
    /// and takes values up to $f(T_f) + g(T_g) + 2 \mathrm{lcm}(c_f, c_g)$ before $T^{(2)} + d^{(2)}$.
    /// </summary>
    [Theory]
    [MemberData(nameof(RightContinuousPairTestCases))]
    public void MaxPlusConvolutionOfPeriodicPartsInverseParameters(Curve f, Curve g)
    {
        var (T_f, d_f, c_f) = (f.PseudoPeriodStart, f.PseudoPeriodLength, f.PseudoPeriodHeight);
        var (T_g, d_g, c_g) = (g.PseudoPeriodStart, g.PseudoPeriodLength, g.PseudoPeriodHeight);

        bool HasNoPlateauBefore(Curve c, Rational T, Rational d)
        {
            var lpi = c.LowerPseudoInverseOverInterval(T);
            var tStar = lpi.ValueAt(c.ValueAt(T));
            return lpi.ValueAt(c.ValueAt(tStar + d)) == tStar + d;
        }

        if (!HasNoPlateauBefore(f, T_f, d_f) || !HasNoPlateauBefore(g, T_g, d_g))
            return;

        var (A, B) = (T_f + T_g, f.ValueAt(T_f) + g.ValueAt(T_g));
        var lcm_c = Rational.LeastCommonMultiple(c_f, c_g);
        var (k_cf, k_cg) = (lcm_c / c_f, lcm_c / c_g);

        var h = Curve.MaxPlusConvolution(f.Decompose(T_f, minDecomposition: false).right, g.Decompose(T_g, minDecomposition: false).right, WithoutIsomorphism);
        var T2 = h.LowerPseudoInverseOverInterval(A).ValueAt(B + lcm_c);
        var d2 = Rational.Min(k_cg * d_g, k_cf * d_f);
        Assert.True(IsPseudoPeriodic(h, T2, d2, lcm_c), "Theorem 40");
        Assert.True(h.LeftLimitAt(T2 + d2) <= B + 2 * lcm_c, "Proposition 42");
    }

    /// <summary>
    /// The (max,+) case of the check Nancy makes of [ZNS26] Lemma 21: the start of the last plateau of the first period, which is cheap, is $T^{**}$ if there is a plateau, and the end of the period if not.
    /// </summary>
    [Theory]
    [MemberData(nameof(RightContinuousCurveTestCases))]
    public void LastPlateauStartIsTheSecondStartOfThePlateau(Curve f)
    {
        var (T, c) = (f.PseudoPeriodStart, f.PseudoPeriodHeight);
        var efficient = f.Cut(T, f.FirstPseudoPeriodEnd, isEndIncluded: true).LastPlateauStart;
        var theory = f.LowerPseudoInverseOverInterval(T).ValueAt(f.ValueAt(T) + c);
        Assert.Equal(efficient < f.FirstPseudoPeriodEnd, theory < f.FirstPseudoPeriodEnd);
        Assert.True(efficient >= f.FirstPseudoPeriodEnd || efficient == theory);
    }

    /// <summary>
    /// The settings: each of the three optimizations of [ZNS23a] and [ZNS26] on or off, which the convolutions give the same values under.
    /// </summary>
    public static IEnumerable<object[]> SettingsTestCases()
    {
        foreach (var isospeed in new[] { true, false })
        foreach (var superIsospeed in new[] { true, false })
        foreach (var bySequence in new[] { true, false })
            yield return
            [
                $"isospeed {isospeed}, super-isospeed {superIsospeed}, by-sequence {bySequence}",
                ComputationSettings.Default() with
                {
                    UseConvolutionIsospeedOptimization = isospeed,
                    UseConvolutionSuperIsospeedOptimization = superIsospeed,
                    UseBySequenceConvolutionIsospeedOptimization = bySequence
                }
            ];
    }

    /// <summary>
    /// $\inf$ or $\sup$ over $s$ of $f(s) + g(t - s)$, taken where the piecewise affine function can attain it: at the breakpoints of $f$ and of $g(t - \cdot)$, and at their one-sided limits.
    /// </summary>
    private static Rational ConvolutionByDefinition(Curve f, Curve g, Rational t, bool isMax)
    {
        var candidates = new SortedSet<Rational> { 0, t };
        foreach (var e in f.Cut(0, t, isEndIncluded: true).Elements) { candidates.Add(e.StartTime); candidates.Add(e.EndTime); }
        foreach (var e in g.Cut(0, t, isEndIncluded: true).Elements) { candidates.Add(t - e.StartTime); candidates.Add(t - e.EndTime); }

        var best = isMax ? Rational.MinusInfinity : Rational.PlusInfinity;
        void Take(Rational value)
        {
            if (isMax ? value > best : value < best)
                best = value;
        }

        foreach (var s in candidates.Where(s => s >= 0 && s <= t))
        {
            Take(f.ValueAt(s) + g.ValueAt(t - s));
            if (s > 0)
                Take(f.LeftLimitAt(s) + g.RightLimitAt(t - s));
            if (t - s > 0)
                Take(f.RightLimitAt(s) + g.LeftLimitAt(t - s));
        }

        return best;
    }

    private static void AssertConvolutionByDefinition(Curve f, Curve g, bool isMax, ComputationSettings settings, string settingsName)
    {
        var h = isMax ? Curve.MaxPlusConvolution(f, g, settings) : Curve.Convolution(f, g, settings);
        var end = h.PseudoPeriodStart + 3 * h.PseudoPeriodLength + 2;
        for (var k = 0; k <= (int)(end * 6); k++)
        foreach (var t in new[] { new Rational(k, 6), new Rational(k, 7) })
        {
            if (t > end)
                continue;
            var expected = ConvolutionByDefinition(f, g, t, isMax);
            Assert.True(expected == h.ValueAt(t), $"{settingsName}, {(isMax ? "(max,+)" : "(min,+)")}: at t = {t}, expected {expected}, got {h.ValueAt(t)}");
        }
    }

    public static IEnumerable<object[]> SmallRightContinuousPairTestCases() => Rows(SmallRightContinuousPairs);
    public static IEnumerable<object[]> SmallLeftContinuousPairTestCases() => Rows(SmallLeftContinuousPairs);

    /// <summary>
    /// Checks that the (min,+) and (max,+) convolutions of right-continuous curves match their definition, under each combination of the three convolution optimizations,
    /// at the times of a grid that covers the transient of the result and three of its pseudo-periods.
    /// </summary>
    [Theory]
    [MemberData(nameof(SettingsTestCases))]
    public void ConvolutionsOfRightContinuousCurvesAreTheDefinitionUnderEverySetting(string settingsName, ComputationSettings settings)
    {
        foreach (var pair in SmallRightContinuousPairTestCases())
        {
            var (f, g) = ((Curve) pair[0], (Curve) pair[1]);
            AssertConvolutionByDefinition(f, g, false, settings, settingsName);
            AssertConvolutionByDefinition(f, g, true, settings, settingsName);
        }
    }

    /// <summary>
    /// Checks that the (min,+) and (max,+) convolutions of left-continuous curves match their definition, under each combination of the three convolution optimizations,
    /// at the times of a grid that covers the transient of the result and three of its pseudo-periods.
    /// </summary>
    [Theory]
    [MemberData(nameof(SettingsTestCases))]
    public void ConvolutionsOfLeftContinuousCurvesAreTheDefinitionUnderEverySetting(string settingsName, ComputationSettings settings)
    {
        foreach (var pair in SmallLeftContinuousPairTestCases())
        {
            var (f, g) = ((Curve) pair[0], (Curve) pair[1]);
            AssertConvolutionByDefinition(f, g, false, settings, settingsName);
            AssertConvolutionByDefinition(f, g, true, settings, settingsName);
        }
    }
}
