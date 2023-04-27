using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Stair curve defined in [TLBB21] as $v(t) = a \cdot \left\lceil \frac{t}{b} \right\rceil$
/// </summary>
public class StairCurve : Curve
{
    /// <summary>
    /// Vertical height of each step.
    /// </summary>
    public Rational A { get; }

    /// <summary>
    /// Horizontal length of each step.
    /// </summary>
    public Rational B { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public StairCurve(Rational a, Rational b)
        : base(
            baseSequence: BuildSequence(a, b),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: b,
            pseudoPeriodHeight: a
        )
    {
        A = a;
        B = b;
    }

    private static Sequence BuildSequence(Rational a, Rational b)
    {
        if (a < 0)
            throw new ArgumentException("a must be >= 0");
        if (b <= 0)
            throw new ArgumentException("b must be > 0");

        if (a == 0)
            return Sequence.Zero(0, b);
        else
        {
            return new Sequence(new Element[]
            {
                Point.Origin(),
                Segment.Constant(0, b, a)
            });
        }
    }
}