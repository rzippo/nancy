using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class VerticalShift
{
    private readonly ITestOutputHelper _testOutputHelper;

    public VerticalShift(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve curve, Rational shift, bool exceptOrigin, Curve expected)> KnownCases =
    [
        (
            new StairCurve(1, 1),
            2, 
            true,
            new Curve(new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 3),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 4)
                ]),
                1, 1, 1
            )
        ),
        (
            new StairCurve(1, 1),
            2, 
            false,
            new Curve(new Sequence([
                    new Point(0, 2),
                    Segment.Constant(0, 1, 3)
                ]),
                0, 1, 1
            )
        )
    ];

    public static IEnumerable<object[]> EquivalenceTestCases =
        KnownCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(EquivalenceTestCases))]
    public void VerticalShiftEquivalence(Curve curve, Rational shift, bool exceptOrigin, Curve expected)
    {
        var shifted = curve.VerticalShift(shift, exceptOrigin);
        _testOutputHelper.WriteLine(curve.ToString());
        _testOutputHelper.WriteLine(expected.ToString());
        _testOutputHelper.WriteLine(shifted.ToString());
        Assert.True(Curve.Equivalent(expected, shifted));
    }

    /// <summary>$f(t) = t$.</summary>
    private static readonly Curve Identity = new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 1);

    /// <summary>
    /// Excepting the origin excepts it only: every later time is shifted, including the pseudo-period boundaries.
    /// </summary>
    public static List<(Curve operand, Rational shift, Rational time, Rational expected)> ExceptOriginValues =
    [
        (Identity, 2, 0, 0),
        (Identity, 2, new Rational(1, 2), new Rational(5, 2)),
        (Identity, 2, 1, 3),
        (Identity, 2, 2, 4),
        (Identity, 2, 10, 12),
    ];

    public static IEnumerable<object[]> ExceptOriginValuesTestCases()
        => ExceptOriginValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ExceptOriginValuesTestCases))]
    public void ExceptingTheOriginExceptsItOnly(Curve operand, Rational shift, Rational time, Rational expected)
    {
        Assert.Equal(expected, operand.VerticalShift(shift, exceptOrigin: true).ValueAt(time));
    }

    private static readonly ComputationSettings WithShortcut = ComputationSettings.Default() with { UseInfiniteVerticalShiftShortcut = true };
    private static readonly ComputationSettings WithoutShortcut = ComputationSettings.Default() with { UseInfiniteVerticalShiftShortcut = false };

    /// <summary>3 at 0, 1 after it.</summary>
    private static readonly Curve ThreeAtOrigin = new Curve(new Sequence([new Point(0, 3), Segment.Constant(0, 2, 1)]), 1, 1, 0);

    /// <summary>$-\infty$ at 0, 1 after it.</summary>
    private static readonly Curve MinusInfiniteAtOrigin = new Curve(new Sequence([Point.MinusInfinite(0), Segment.Constant(0, 2, 1)]), 1, 1, 0);

    /// <summary>0 up to 1, $-\infty$ after it.</summary>
    private static readonly Curve MinusInfiniteAfterOne = new Curve(
        new Sequence([Point.Origin(), Segment.Zero(0, 1), Point.MinusInfinite(1), Segment.MinusInfinite(1, 2)]), 1, 1, 0);

    /// <summary>$-\infty$ at every integer, the origin included as the start of the pseudo-period, 1 between them.</summary>
    private static readonly Curve MinusInfiniteAtEveryInteger = new Curve(new Sequence([Point.MinusInfinite(0), Segment.Constant(0, 1, 1)]), 0, 1, 0);

    /// <summary>
    /// Curves shifted by $+\infty$, chosen for the value at the origin, finite, $+\infty$ or $-\infty$, and for $-\infty$ after it, where the sum is undefined,
    /// with types whose shift is overridden.
    /// </summary>
    public static IEnumerable<object[]> InfiniteShiftTestCases()
    {
        var curves = new (string id, Curve curve)[]
        {
            ("3 at 0, 1 after", ThreeAtOrigin),
            ("-inf at 0, 1 after", MinusInfiniteAtOrigin),
            ("0, then -inf after 1", MinusInfiniteAfterOne),
            ("-inf at every integer, from the origin", MinusInfiniteAtEveryInteger),
            ("-inf everywhere", Curve.MinusInfinite()),
            ("+inf everywhere", Curve.PlusInfinite()),
            ("zero", Curve.Zero()),
            ("rate-latency", new RateLatencyServiceCurve(2, 1)),
            ("token bucket", new SigmaRhoArrivalCurve(1, 1)),
            ("constant", new ConstantCurve(3)),
            ("raised rate-latency", new RaisedRateLatencyServiceCurve(2, 1, 3, withZeroOrigin: false)),
        };
        foreach (var (id, curve) in curves)
        foreach (var exceptOrigin in new[] { false, true })
            yield return new object[] { id, curve, exceptOrigin };
    }

    /// <summary>
    /// The shortcut for a shift by $+\infty$, and the overrides of the shift, give the same results and exceptions as shifting each element of the curve as a plain one.
    /// </summary>
    [Theory]
    [MemberData(nameof(InfiniteShiftTestCases))]
    public void InfiniteShiftMatchesTheShiftOfEachElement(string id, Curve curve, bool exceptOrigin)
    {
        _testOutputHelper.WriteLine(id);
        Curve? expected = null;
        try
        {
            expected = new Curve(curve).VerticalShift(Rational.PlusInfinity, exceptOrigin, WithoutShortcut);
        }
        catch (UndeterminedResultException)
        {
        }

        if (expected is null)
            Assert.Throws<UndeterminedResultException>(() => curve.VerticalShift(Rational.PlusInfinity, exceptOrigin, WithShortcut));
        else
            Assert.True(Curve.Equivalent(expected, curve.VerticalShift(Rational.PlusInfinity, exceptOrigin, WithShortcut)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InfiniteShiftKeepsAnExceptedOrigin(bool useShortcut)
    {
        var settings = useShortcut ? WithShortcut : WithoutShortcut;
        var shifted = ThreeAtOrigin.VerticalShift(Rational.PlusInfinity, exceptOrigin: true, settings);
        Assert.Equal(3, shifted.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, shifted.ValueAt(1));

        // an excepted origin at -inf is not summed with +inf, so it is kept as well
        var shiftedMinusInfinite = MinusInfiniteAtOrigin.VerticalShift(Rational.PlusInfinity, exceptOrigin: true, settings);
        Assert.Equal(Rational.MinusInfinity, shiftedMinusInfinite.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, shiftedMinusInfinite.ValueAt(1));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InfiniteShiftOfMinusInfinityThrows(bool useShortcut)
    {
        var settings = useShortcut ? WithShortcut : WithoutShortcut;
        // -inf + inf is undefined, also where the origin is excepted, as the curve is -inf after it too
        Assert.Throws<UndeterminedResultException>(() => Curve.MinusInfinite().VerticalShift(Rational.PlusInfinity, false, settings));
        Assert.Throws<UndeterminedResultException>(() => Curve.MinusInfinite().VerticalShift(Rational.PlusInfinity, true, settings));
        Assert.Throws<UndeterminedResultException>(() => MinusInfiniteAtOrigin.VerticalShift(Rational.PlusInfinity, false, settings));
        Assert.Throws<UndeterminedResultException>(() => MinusInfiniteAfterOne.VerticalShift(Rational.PlusInfinity, true, settings));
        // the origin starts the pseudo-period, so its -inf repeats after it
        Assert.Throws<UndeterminedResultException>(() => MinusInfiniteAtEveryInteger.VerticalShift(Rational.PlusInfinity, true, settings));
    }
}
