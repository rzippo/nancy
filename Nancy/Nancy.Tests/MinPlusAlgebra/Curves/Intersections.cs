using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Covers the times at which two curves take the same value, and the times at which one passes the other.
/// </summary>
public class Intersections
{
    #region Curves used below

    /// <summary>
    /// Touches 0 from below at $t = 2$ and turns back down, so it meets the axis without passing it.
    /// </summary>
    private static readonly Curve TouchingV = new (
        new Sequence(
        [
            new Point(0, -2),
            new Segment(0, 2, -2, 1),
            new Point(2, 0),
            new Segment(2, 4, 0, -1),
            new Point(4, -2),
        ]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 4,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// Jumps from below 0 to above it at $t = 2$, without ever being 0.
    /// </summary>
    private static readonly Curve JumpOverZero = new (
        new Sequence(
        [
            new Point(0, -1),
            new Segment(0, 2, -1, 0),
            new Point(2, 1),
            new Segment(2, 4, 1, 0),
        ]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 4,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// Rises to 0 at $t = 1$ and falls back, meeting the axis once per period forever.
    /// </summary>
    private static readonly Curve Sawtooth = new (
        new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 1, 0, 1),
            new Point(1, 1),
            new Segment(1, 2, 1, -1),
        ]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 2,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// Rises from the origin and then saturates to $+\infty$, with an infinite pseudo-period height.
    /// </summary>
    private static readonly Curve SaturatingWithInfiniteDrift = new (
        new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 2, 0, 1),
            new Point(2, 2),
            new Segment(2, 3, 2, 1),
        ]),
        pseudoPeriodStart: 2,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: Rational.PlusInfinity);

    /// <summary>
    /// $0$ up to 2 included, $+\infty$ right after, with a zero pseudo-period height.
    /// </summary>
    private static readonly Curve SaturatingAfterTwo = new (
        new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 2, 0, 0),
            new Point(2, 0),
            Segment.PlusInfinite(2, 4),
        ]),
        pseudoPeriodStart: 3,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// $0$ up to 2 included, wiggling up and down until 6, $+\infty$ from 6 on, with a zero pseudo-period height.
    /// </summary>
    private static readonly Curve WigglingThenSaturated = new (
        new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 2, 0, 0),
            new Point(2, 0),
            new Segment(2, 4, 0, 1),
            new Point(4, 2),
            new Segment(4, 6, 2, -1),
            new Point(6, Rational.PlusInfinity),
            Segment.PlusInfinite(6, 10),
        ]),
        pseudoPeriodStart: 6,
        pseudoPeriodLength: 4,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// $+\infty$ at every integer, $f(t) = t$ in between, drifting by the given height per period.
    /// </summary>
    private static Curve InfiniteAtIntegers(Rational height)
        => new(
            new Sequence(
            [
                new Point(0, Rational.PlusInfinity),
                new Segment(0, 1, 0, 1),
            ]),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: height);

    /// <summary>
    /// $f(t) = t$ on the first half of each unit period, $+\infty$ on the second half, drifting by 1 per period.
    /// </summary>
    private static readonly Curve SaturatingHalfwayThroughEachPeriod = new (
        new Sequence(
        [
            new Point(0, 0),
            new Segment(0, new Rational(1, 2), 0, 1),
            new Point(new Rational(1, 2), new Rational(1, 2)),
            Segment.PlusInfinite(new Rational(1, 2), 1),
        ]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 1);

    /// <summary>
    /// $f(t) = t + 1/10$ throughout, drifting by 2 per period.
    /// </summary>
    private static readonly Curve DriftingTwiceAsFast = new (
        new Sequence(
        [
            new Point(0, new Rational(1, 10)),
            new Segment(0, 1, new Rational(1, 10), 1),
        ]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 2);

    /// <summary>
    /// Rests at 3 up to 2, dips to 1 at 3 and is back at 2 by 4, repeating with period 2 and no drift.
    /// </summary>
    private static readonly Curve DippingOncePerPeriod = new (
        new Sequence(
        [
            new Point(0, 3),
            new Segment(0, 2, 3, 0),
            new Point(2, 3),
            new Segment(2, 3, 3, -2),
            new Point(3, 1),
            new Segment(3, 4, 1, 1),
        ]),
        pseudoPeriodStart: 2,
        pseudoPeriodLength: 2,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// Equals 1 on $[2, 3)$ and on $(5, 6)$ of each period of length 4 starting at 2, so that the
    /// stretches of equality with the constant 1, i.e. $(5, 7)$, $(9, 11)$ and so on, run across period boundaries.
    /// </summary>
    private static readonly Curve StretchCrossingPeriodBoundaries = new (
        new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 2, 0, 0),
            new Point(2, 1),
            new Segment(2, 3, 1, 0),
            new Point(3, 0),
            new Segment(3, 5, 0, 0),
            new Point(5, 1),
            new Segment(5, 6, 1, 0),
        ]),
        pseudoPeriodStart: 2,
        pseudoPeriodLength: 4,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// Rises through 0 inside $(0, 1)$ and later jumps down over it at $t = 2$, so that a crossing
    /// inside an interval and one at a breakpoint fall in the same window.
    /// </summary>
    private static readonly Curve CrossingInsideThenJumping = new (
        new Sequence(
        [
            new Point(0, -1),
            new Segment(0, 1, -1, 2),
            new Point(1, 1),
            new Segment(1, 2, 1, 0),
            new Point(2, -1),
            new Segment(2, 4, -1, 0),
        ]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 4,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// $f(t) = t - 2$, so it meets the axis at $t = 2$ and passes it there, with $t = 2$ a breakpoint
    /// of its own representation: on both sides of it the limits of the two curves coincide, and only
    /// the slopes say which is above.
    /// </summary>
    private static readonly Curve CrossingAtItsOwnBreakpoint = new (
        new Sequence(
        [
            new Point(0, -2),
            new Segment(0, 2, -2, 1),
            new Point(2, 0),
            new Segment(2, 4, 0, 1),
        ]),
        pseudoPeriodStart: 2,
        pseudoPeriodLength: 2,
        pseudoPeriodHeight: 2);

    /// <summary>
    /// Constantly 1 from the origin on.
    /// </summary>
    private static readonly Curve AlwaysOne = new (
        new Sequence([new Point(0, 1), new Segment(0, 1, 1, 0)]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 0);

    /// <summary>
    /// $f(t) = t + 2$, which stays above <see cref="AlwaysOne"/> at every $t \ge 0$: solving for where
    /// the two segments of a period meet gives $t = -1$, outside the interval it was solved on.
    /// </summary>
    private static readonly Curve LineAboveOne = new (
        new Sequence([new Point(0, 2), new Segment(0, 1, 2, 1)]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 1);

    /// <summary>
    /// $f(t) = 2t$, which overtakes <see cref="LineFromFive"/> at $t = 5$.
    /// </summary>
    private static readonly Curve LineOfSlopeTwo = new (
        new Sequence([new Point(0, 0), new Segment(0, 1, 0, 2)]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 2);

    /// <summary>
    /// $f(t) = t + 5$.
    /// </summary>
    private static readonly Curve LineFromFive = new (
        new Sequence([new Point(0, 5), new Segment(0, 1, 5, 1)]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 1);

    #endregion

    #region Equality

    [Fact]
    public void ALineMeetsAConstantAtTheOriginAndWhereItCatchesUp()
    {
        // a constant curve is 0 at the origin, so the two are equal there as well as at t = 5
        var pattern = new RateLatencyServiceCurve(1, 0).GetIntersections(new ConstantCurve(5));

        Assert.False(pattern.IsInfinite);
        Assert.Equal(2, pattern.Count);
        Assert.Equal(Interval.Closed(0, 0), pattern.First);
        Assert.Equal(Interval.Closed(5, 5), pattern.Last);
    }

    [Fact]
    public void TheOriginCanBeExcluded()
    {
        var pattern = new RateLatencyServiceCurve(1, 0)
            .GetIntersections(new ConstantCurve(5), from: 0, isStartInclusive: false);

        Assert.Equal(Interval.Closed(5, 5), pattern.First);
        Assert.Equal(1, pattern.Count);
    }

    [Fact]
    public void CurvesThatNeverMeetReportNothing()
    {
        // raised so that they differ at the origin too, where both constants would otherwise be 0
        var pattern = new ConstantCurve(3).GetIntersections(new ConstantCurve(3) + 2);

        Assert.True(pattern.IsEmpty);
        Assert.Null(pattern.First);
        Assert.Null(pattern.Last);
        Assert.Equal(0, pattern.Count);
    }

    [Fact]
    public void MeetingIsSymmetric()
    {
        var f = new RateLatencyServiceCurve(1, 0);
        var g = new ConstantCurve(5);

        Assert.Equal(f.GetIntersections(g).Take(10), g.GetIntersections(f).Take(10));
    }

    #endregion

    #region Stretches rather than instants

    [Fact]
    public void IdenticalCurvesMeetEverywhereAsOneStretch()
    {
        // the whole point of merging: equality that runs across period boundaries is one
        // interval, not one per period
        var f = new StairCurve(1, 3);

        var pattern = f.GetIntersections(f);

        Assert.False(pattern.IsInfinite);
        var only = Assert.Single(pattern.Take(5));
        Assert.Equal(0, only.Lower);
        Assert.True(only.IsUnboundedAbove);
    }

    [Fact]
    public void CurvesSaturatingTogetherMeetOnTheSharedStretchAndTheSharedTail()
    {
        // both are 0 up to 3, both +infinity after 5, and differ in between
        var pattern = new DelayServiceCurve(3).GetIntersections(new DelayServiceCurve(5));

        Assert.False(pattern.IsInfinite);
        var found = pattern.Take(5);
        Assert.Equal(2, found.Count);
        Assert.Equal(Interval.Closed(0, 3), found[0]);
        Assert.Equal(5, found[1].Lower);
        Assert.False(found[1].IsLowerIncluded);
        Assert.True(found[1].IsUnboundedAbove);
    }

    [Fact]
    public void EqualityAtAnInfinityCounts()
    {
        // the case that a difference-based implementation cannot answer, since it meets
        // infinity minus infinity
        var pattern = new DelayServiceCurve(3).GetIntersections(new DelayServiceCurve(5));

        Assert.False(pattern.IsEmpty);
    }

    [Fact]
    public void AnInstantOfDifferenceKeepsTwoStretchesApart()
    {
        // identical but for one instant, so the two stretches touch without joining
        var f = new Curve(
            new Sequence([
                new Point(0, 0), new Segment(0, 4, 0, 1),
                new Point(4, 4), new Segment(4, 5, 4, 1)]),
            pseudoPeriodStart: 4, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);
        var g = new Curve(
            new Sequence([
                new Point(0, 0), new Segment(0, 2, 0, 1),
                new Point(2, 9), new Segment(2, 4, 2, 1),
                new Point(4, 4), new Segment(4, 5, 4, 1)]),
            pseudoPeriodStart: 4, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);

        var found = f.GetIntersections(g, to: 4).Take(10);

        Assert.Equal(2, found.Count);
        Assert.False(found[0].Contains(2));
        Assert.False(found[1].Contains(2));
    }

    [Fact]
    public void SharingAStretchIsOverlapping()
    {
        var f = new DelayServiceCurve(3);

        Assert.True(f.OverlapsWith(new DelayServiceCurve(5)));
        Assert.True(f.IntersectsWith(new DelayServiceCurve(5)));

        // meeting only at instants is intersecting without overlapping
        var line = new RateLatencyServiceCurve(1, 0);
        Assert.True(line.IntersectsWith(new ConstantCurve(5)));
        Assert.False(line.OverlapsWith(new ConstantCurve(5)));
    }

    [Fact]
    public void AStretchCrossingAPeriodBoundaryRepeatsWhole()
    {
        // equality holds on (5, 7), (9, 11) and so on, each stretch running across a period boundary:
        // each is reported whole, with no redundant fragment of the next occurrence
        var found = StretchCrossingPeriodBoundaries.GetIntersections(new ConstantCurve(1)).Take(5);

        Assert.Equal(
            [
                Interval.Closed(0, 0),
                Interval.ClosedOpen(2, 3),
                Interval.ClosedOpen(5, 7),
                Interval.ClosedOpen(9, 11),
                Interval.ClosedOpen(13, 15),
            ],
            found);
    }

    #endregion

    #region Meeting forever

    [Fact]
    public void MeetingOncePerPeriodIsInfinite()
    {
        var pattern = Sawtooth.GetIntersections(Curve.Zero());

        Assert.True(pattern.IsInfinite);
        Assert.Null(pattern.Count);
        Assert.Equal(
            [Interval.Closed(0, 0), Interval.Closed(2, 2), Interval.Closed(4, 4)],
            pattern.Take(3));
    }

    [Fact]
    public void ThereIsNoLastWhenThereAreInfinitelyMany()
    {
        var pattern = Sawtooth.GetIntersections(Curve.Zero());

        Assert.Throws<InvalidOperationException>(() => pattern.Last);
    }

    [Fact]
    public void AFirstExistsWhenThereAreInfinitelyMany()
    {
        Assert.Equal(Interval.Closed(0, 0), Sawtooth.GetIntersections(Curve.Zero()).First);
    }

    [Fact]
    public void BoundingTheWindowMakesAnInfinitePatternFinite()
    {
        var pattern = Sawtooth.GetIntersections(Curve.Zero(), to: 5);

        Assert.False(pattern.IsInfinite);
        Assert.Equal(3, pattern.Count);
        Assert.Equal(Interval.Closed(4, 4), pattern.Last);
    }

    #endregion

    #region Past the horizon

    [Fact]
    public void ACurveMeetsItselfEverywhereEvenWhenSaturating()
    {
        // an infinite pseudo-period height pins the search horizon to the first period:
        // the query must still see the equality that lies past it
        var f = SaturatingWithInfiniteDrift;

        var only = Assert.Single(f.GetIntersections(f).Take(5));
        Assert.Equal(0, only.Lower);
        Assert.True(only.IsUnboundedAbove);
    }

    [Fact]
    public void AQueryStartingPastTheHorizonFindsTheCurveItself()
    {
        var f = SaturatingWithInfiniteDrift;

        var pattern = f.GetIntersections(f, from: 50);

        Assert.False(pattern.IsEmpty);
        var only = Assert.Single(pattern.Take(5));
        Assert.Equal(50, only.Lower);
        Assert.True(only.IsUnboundedAbove);
    }

    public static IEnumerable<object[]> CurvesAndStarts
    {
        get
        {
            Curve[] curves =
            [
                Sawtooth,
                new StairCurve(1, 3),
                new DelayServiceCurve(3),
                SaturatingWithInfiniteDrift,
                SaturatingHalfwayThroughEachPeriod,
            ];
            foreach (var f in curves)
            foreach (var t in new[] { 0, 5, 50 })
                yield return [f, t];
        }
    }

    [Theory]
    [MemberData(nameof(CurvesAndStarts))]
    public void SelfIntersectionsAreNeverEmpty(Curve f, int from)
    {
        // a curve is equal to itself everywhere, whatever the period heights and the window
        var pattern = f.GetIntersections(f, from: from);

        Assert.False(pattern.IsEmpty);
        var only = Assert.Single(pattern.Take(5));
        Assert.Equal(from, only.Lower);
        Assert.True(only.IsUnboundedAbove);
    }

    [Fact]
    public void TheBoundedAndUnboundedQueriesAgreeOnTheSameRegion()
    {
        // the bounded query always read the region directly; the unbounded one used to skip it
        var f = SaturatingWithInfiniteDrift;

        var bounded = f.GetIntersections(f, from: 50, to: 200);
        Assert.Equal(Interval.ClosedOpen(50, 200), Assert.Single(bounded.Take(5)));

        var unbounded = f.GetIntersections(f, from: 50);
        Assert.Equal(Interval.UnboundedAbove(50), Assert.Single(unbounded.Take(5)));
    }

    [Fact]
    public void ASaturatingCurveMeetsAPeriodicOneWhereBothAreInfinite()
    {
        // the finite side of the comparison is +infinity past its delay, so the two meet there forever
        var pattern = SaturatingWithInfiniteDrift.GetIntersections(new DelayServiceCurve(2));

        Assert.False(pattern.IsInfinite);
        Assert.Equal(
            [Interval.Closed(0, 0), Interval.UnboundedAbove(3)],
            pattern.Take(5));
    }

    [Fact]
    public void ASaturatingCurveMetFromPastItsStartIsOneTail()
    {
        var pattern = SaturatingWithInfiniteDrift.GetIntersections(new DelayServiceCurve(2), from: 50);

        Assert.Equal(Interval.UnboundedAbove(50), Assert.Single(pattern.Take(5)));
    }

    [Fact]
    public void InfiniteDriftNormalizationKeepsFiniteHitsFinite()
    {
        // the constructor moves the period start past the finite period before filling
        // the next period with +infinity, so a finite hit in that first period must not repeat.
        var hit = new Rational(5, 2);
        var pattern = SaturatingWithInfiniteDrift.GetIntersections(new ConstantCurve(hit));

        Assert.Equal(3, SaturatingWithInfiniteDrift.PseudoPeriodStart);
        Assert.False(pattern.IsInfinite);
        Assert.Equal(2, pattern.Count);
        Assert.Equal(
            [Interval.Closed(0, 0), Interval.Closed(hit, hit)],
            pattern.Take(5));
    }

    #endregion

    #region Unbounded tails

    [Fact]
    public void AnUnboundedTailIsOneMergedInterval()
    {
        // the two agree on [0, 2], disagree on (2, 6), and are both +infinity from 6 on:
        // the tail comes back whole, not chopped at the periods it spans or repeating
        var pattern = SaturatingAfterTwo.GetIntersections(WigglingThenSaturated);

        Assert.False(pattern.IsInfinite);
        Assert.Equal(
            [Interval.Closed(0, 2), Interval.UnboundedAbove(6)],
            pattern.Take(5));
    }

    [Fact]
    public void AnUnboundedTailStartsAtAPeriodBoundary()
    {
        // periodicity forces a tail of equality to cover whole periods, so it can only begin
        // where the repeating part begins: this is what makes the two-period read sound
        var pattern = SaturatingAfterTwo.GetIntersections(WigglingThenSaturated);

        var tail = pattern.Last!.Value;
        Assert.True(tail.IsUnboundedAbove);
        Assert.Equal(pattern.PeriodStart, tail.Lower);
    }

    #endregion

    #region Meetings at an infinity

    [Fact]
    public void MeetingsAtAnInfinityRepeatWhenTheFinitePartsDriftApart()
    {
        // both are +infinity at every integer, but the second drifts twice as fast:
        // they agree over [0, 1] and at every integer from 1 on.
        // Reading the drift through the difference used to throw, since the two infinities meet
        var pattern = InfiniteAtIntegers(1).GetIntersections(InfiniteAtIntegers(2));

        Assert.True(pattern.IsInfinite);
        Assert.Equal(
            [Interval.Closed(0, 1), Interval.Closed(2, 2), Interval.Closed(3, 3), Interval.Closed(4, 4)],
            pattern.Take(4));
    }

    [Fact]
    public void SignFlipsAtAnInfinityRepeatWhenTheFinitePartsDriftApart()
    {
        // the first jumps to +infinity at every half integer and falls back at every integer,
        // passing the second — which stays finite and drifts twice as fast — both ways forever
        var pattern = SaturatingHalfwayThroughEachPeriod.GetCrossings(DriftingTwiceAsFast);

        Assert.True(pattern.IsInfinite);
        var found = pattern.Take(6);
        Assert.Equal(
            new Rational[] { new(1, 2), 1, new(3, 2), 2, new(5, 2), 3 },
            found.Select(c => c.Time).ToArray());
        Assert.Equal(
            [true, false, true, false, true, false],
            found.Select(c => c.IsUpward).ToArray());
        Assert.All(found, c => Assert.False(c.IsContact));
    }

    #endregion

    #region Crossings

    [Fact]
    public void ALineOvertakingAConstantCrossesIt()
    {
        var crossing = Assert.Single(
            new RateLatencyServiceCurve(1, 0).GetCrossings(new ConstantCurve(5)).Take(5));

        Assert.Equal(5, crossing.Time);
        Assert.True(crossing.IsUpward);
        Assert.True(crossing.IsContact);
    }

    [Fact]
    public void CrossingDirectionFollowsTheReceiver()
    {
        var f = new RateLatencyServiceCurve(1, 0);
        var g = new ConstantCurve(5);

        Assert.True(f.GetCrossings(g).First!.Value.IsUpward);
        Assert.False(g.GetCrossings(f).First!.Value.IsUpward);
    }

    [Fact]
    public void TouchingWithoutPassingIsAnIntersectionAndNotACrossing()
    {
        // the case a sign-change search misses: they meet and part on the same side
        Assert.True(TouchingV.IntersectsWith(Curve.Zero()));
        Assert.False(TouchingV.CrossesWith(Curve.Zero()));
    }

    [Fact]
    public void JumpingOverIsACrossingAndNotAnIntersection()
    {
        // the case an equality search misses: they swap order without ever being equal
        Assert.True(JumpOverZero.CrossesWith(Curve.Zero()));
        Assert.False(JumpOverZero.IntersectsWith(Curve.Zero()));

        var crossing = JumpOverZero.GetCrossings(Curve.Zero()).First!.Value;
        Assert.Equal(2, crossing.Time);
        Assert.True(crossing.IsUpward);
        Assert.False(crossing.IsContact);
    }

    [Fact]
    public void ACrossingAtAPeriodBoundaryRepeats()
    {
        // a crosses the constant 2 at 5/2 within its first period, and then at each period start
        // from 4 on: it lands back at 2 from below and restarts at 3.
        // The boundary crossings were missed when the repeating block started at the period start,
        // since a crossing there is decided by signs that are periodic only one period later
        var pattern = DippingOncePerPeriod.GetCrossings(new ConstantCurve(2));

        Assert.True(pattern.IsInfinite);
        var found = pattern.Take(5);
        Assert.Equal(
            new Rational[] { new(5, 2), 4, new(9, 2), 6, new(13, 2) },
            found.Select(c => c.Time).ToArray());
        Assert.Equal(
            [false, true, false, true, false],
            found.Select(c => c.IsUpward).ToArray());
        Assert.Equal(
            [true, false, true, false, true],
            found.Select(c => c.IsContact).ToArray());
    }

    [Fact]
    public void ACrossingAtTheWindowStartReadsTheCurvesNotTheWindow()
    {
        // the two meet at 5 with different slopes, so the first passes the second there:
        // the sign to the left of the window decides, and it must be read from the curves
        var line = new Curve(
            new Sequence([new Point(0, 0), new Segment(0, 1, 0, 2)]),
            pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2);
        var shifted = new Curve(
            new Sequence([new Point(0, 5), new Segment(0, 1, 5, 1)]),
            pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);

        var pattern = line.GetCrossings(shifted, from: 5, to: 10);

        var crossing = Assert.Single(pattern.Take(5));
        Assert.Equal(5, crossing.Time);
        Assert.True(crossing.IsUpward);
        Assert.True(crossing.IsContact);
    }

    [Fact]
    public void ACrossingAtTheWindowStartCanBeExcluded()
    {
        var line = new Curve(
            new Sequence([new Point(0, 0), new Segment(0, 1, 0, 2)]),
            pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 2);
        var shifted = new Curve(
            new Sequence([new Point(0, 5), new Segment(0, 1, 5, 1)]),
            pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);

        Assert.True(line.GetCrossings(shifted, from: 5, to: 10, isStartInclusive: false).IsEmpty);
    }

    [Fact]
    public void ASinglePointWindowSeesTheCrossingThere()
    {
        var crossing = Assert.Single(
            LineOfSlopeTwo.GetCrossings(LineFromFive, from: 5, to: 5, isEndInclusive: true).Take(5));

        Assert.Equal(5, crossing.Time);
        Assert.True(crossing.IsContact);
    }

    [Fact]
    public void CrossingsComeBackInTimeOrder()
    {
        // the crossing at 1/2 lies inside an interval and the one at 2 is a jump at a breakpoint:
        // the two are found by separate walks, and First, Last and Before all read the order
        var pattern = CrossingInsideThenJumping.GetCrossings(Curve.Zero(), from: 0, to: 3);

        Assert.Equal([new Rational(1, 2), 2], pattern.Transient.Select(c => c.Time));
        Assert.Equal(new Rational(1, 2), pattern.First?.Time);
        Assert.Equal(2, pattern.Last?.Time);
        Assert.Equal([new Rational(1, 2)], pattern.Before(1).Select(c => c.Time));
    }

    [Fact]
    public void ACrossingAtASharedBreakpointIsSeen()
    {
        // the two are equal at t = 2 and their limits coincide on both sides of it, so only the
        // slopes distinguish above from below: comparing values alone reports no crossing at all
        var crossing = Assert.Single(
            CrossingAtItsOwnBreakpoint.GetCrossings(Curve.Zero(), from: 0, to: 4).Take(5));

        Assert.Equal(2, crossing.Time);
        Assert.True(crossing.IsUpward);
        Assert.True(crossing.IsContact);
    }

    [Fact]
    public void ACrossingAtTheWindowEdgeReadsTheSlopesWhenTheValuesTie()
    {
        var f = CrossingAtItsOwnBreakpoint;

        // the window starts where the two touch, so the sign to its left comes from outside it,
        // where the values are equal and only the slopes separate the curves
        var atStart = Assert.Single(f.GetCrossings(Curve.Zero(), from: 2, to: 4).Take(5));
        Assert.Equal(2, atStart.Time);

        // and the same at the far edge, read from what lies to the right of it
        var atEnd = Assert.Single(
            f.GetCrossings(Curve.Zero(), from: 0, to: 2, isEndInclusive: true).Take(5));
        Assert.Equal(2, atEnd.Time);
    }

    [Fact]
    public void SegmentsThatWouldOnlyMeetIfExtendedDoNotMeet()
    {
        // the two diverge over every period, and where they would meet is t = -1: a solution to
        // the equation, but outside the interval it was solved on and no time at all
        Assert.True(AlwaysOne.GetIntersections(LineAboveOne, to: 4).IsEmpty);
        Assert.True(AlwaysOne.GetCrossings(LineAboveOne, to: 4).IsEmpty);
    }

    #endregion

    #region The window

    [Fact]
    public void TheWindowCanBeGivenAsAnInterval()
    {
        var f = Sawtooth;
        var byParts = f.GetIntersections(Curve.Zero(), from: 1, to: 6, isStartInclusive: true, isEndInclusive: false);
        var byInterval = f.GetIntersections(Curve.Zero(), Interval.ClosedOpen(1, 6));

        Assert.Equal(byParts.Take(10), byInterval.Take(10));
    }

    [Theory]
    [InlineData(true, 3)]
    [InlineData(false, 2)]
    public void TheStartBoundIsHonoured(bool isStartInclusive, int expected)
    {
        // meetings at 0, 2, 4 within [0, 5): excluding the start drops the one at 0
        var pattern = Sawtooth.GetIntersections(
            Curve.Zero(), from: 0, to: 5, isStartInclusive: isStartInclusive);

        Assert.Equal(expected, pattern.Count);
    }

    [Theory]
    [InlineData(true, 3)]
    [InlineData(false, 2)]
    public void TheEndBoundIsHonoured(bool isEndInclusive, int expected)
    {
        // meetings at 0, 2, 4 up to 4: including the end keeps the one at 4
        var pattern = Sawtooth.GetIntersections(
            Curve.Zero(), from: 0, to: 4, isEndInclusive: isEndInclusive);

        Assert.Equal(expected, pattern.Count);
    }

    [Fact]
    public void AnEmptyWindowFindsNothing()
    {
        Assert.True(Sawtooth.GetIntersections(Curve.Zero(), from: 3, to: 3).IsEmpty);
    }

    [Fact]
    public void ANegativeBoundIsRejected()
    {
        Assert.Throws<ArgumentException>(() => Sawtooth.GetIntersections(Curve.Zero(), from: -5));
        Assert.Throws<ArgumentException>(() => Sawtooth.GetIntersections(Curve.Zero(), to: -5));
        Assert.Throws<ArgumentException>(() => Sawtooth.GetIntersections(Curve.Zero(), new Interval(-5, 10)));

        Assert.Throws<ArgumentException>(() => Sawtooth.GetCrossings(Curve.Zero(), from: -5));
        Assert.Throws<ArgumentException>(() => Sawtooth.GetCrossings(Curve.Zero(), to: -5));
        Assert.Throws<ArgumentException>(() => Sawtooth.GetCrossings(Curve.Zero(), new Interval(-5, 10)));
    }

    [Fact]
    public void AWindowUnboundedBelowIsRejected()
    {
        var window = Interval.UnboundedBelow(6);

        Assert.Throws<ArgumentException>(() => Sawtooth.GetIntersections(Curve.Zero(), window));
        Assert.Throws<ArgumentException>(() => Sawtooth.GetCrossings(Curve.Zero(), window));
    }

    [Fact]
    public void AWindowUnboundedAboveSearchesOn()
    {
        var byParts = Sawtooth.GetIntersections(Curve.Zero(), from: 3);
        var byWindow = Sawtooth.GetIntersections(Curve.Zero(), Interval.UnboundedAbove(3));

        Assert.Equal(byParts.Take(10), byWindow.Take(10));
    }

    [Fact]
    public void AWindowClosedAtPlusInfinityIsTreatedAsUnbounded()
    {
        // Interval can model a closed +infinity, but time windows ignore that closure.
        var closedAtInfinity = Interval.Closed(3, Rational.PlusInfinity);

        var intersectionsByParts = SaturatingAfterTwo.GetIntersections(WigglingThenSaturated, from: 3);
        var intersectionsByWindow = SaturatingAfterTwo.GetIntersections(WigglingThenSaturated, closedAtInfinity);
        var tail = Assert.Single(intersectionsByWindow.Take(10));

        Assert.Equal(intersectionsByParts.Take(10), intersectionsByWindow.Take(10));
        Assert.True(tail.IsUnboundedAbove);
        Assert.False(tail.IsUpperIncluded);

        var level = new ConstantCurve(new Rational(1, 2));
        var crossingsByParts = Sawtooth.GetCrossings(level, from: 3);
        var crossingsByWindow = Sawtooth.GetCrossings(level, closedAtInfinity);

        Assert.Equal(crossingsByParts.Take(6), crossingsByWindow.Take(6));
    }

    [Fact]
    public void ASinglePointWindowSeesTheMeetingThere()
    {
        var only = Assert.Single(
            LineOfSlopeTwo.GetIntersections(LineFromFive, from: 5, to: 5, isEndInclusive: true).Take(5));

        Assert.Equal(Interval.Closed(5, 5), only);
    }

    [Fact]
    public void AQueryStartingAtInfinityFindsNothing()
    {
        Assert.True(Sawtooth.GetIntersections(Curve.Zero(), from: Rational.PlusInfinity).IsEmpty);
        Assert.True(Sawtooth.GetCrossings(Curve.Zero(), from: Rational.PlusInfinity).IsEmpty);
    }

    [Fact]
    public void AWindowEndingBeforeItStartsFindsNothing()
    {
        Assert.True(Sawtooth.GetIntersections(Curve.Zero(), from: 5, to: 3).IsEmpty);
        Assert.True(Sawtooth.GetCrossings(Curve.Zero(), from: 5, to: 3).IsEmpty);
    }

    #endregion

    #region Pattern operations

    [Fact]
    public void EveryOperationTerminatesOnAnInfinitePattern()
    {
        var pattern = Sawtooth.GetIntersections(Curve.Zero());

        Assert.True(pattern.IsInfinite);
        Assert.Equal(4, pattern.Take(4).Count);
        Assert.Equal(3, pattern.Before(5).Count);
        Assert.Equal(Interval.Closed(6, 6), pattern.ElementAt(3));
        Assert.Equal(Interval.Closed(0, 0), pattern.First);
    }

    [Fact]
    public void BeforePlusInfinityRejectsAnInfiniteIntersectionPattern()
    {
        // asking for everything before +infinity would enumerate forever.
        var pattern = Sawtooth.GetIntersections(Curve.Zero());

        Assert.Throws<InvalidOperationException>(() => pattern.Before(Rational.PlusInfinity));
    }

    [Fact]
    public void EnumerateRefusesAnInfinitePatternUnlessAsked()
    {
        var pattern = Sawtooth.GetIntersections(Curve.Zero());

        Assert.Throws<InvalidOperationException>(() => pattern.Enumerate().ToList());
        Assert.Equal(3, pattern.Enumerate(throwIfInfinite: false).Take(3).Count());
    }

    [Fact]
    public void EnumerateWalksAFinitePatternByDefault()
    {
        var pattern = new RateLatencyServiceCurve(1, 0).GetIntersections(new ConstantCurve(5));

        Assert.Equal(2, pattern.Enumerate().Count());
    }

    [Fact]
    public void ElementAtIsNullPastTheEndOfAFinitePattern()
    {
        var pattern = new RateLatencyServiceCurve(1, 0).GetIntersections(new ConstantCurve(5));

        Assert.NotNull(pattern.ElementAt(1));
        Assert.Null(pattern.ElementAt(2));
        Assert.Null(pattern.ElementAt(-1));
    }

    #endregion

    /// <summary>
    /// The crossing pattern carries the same read-out operations as the intersection one, over the same shared walk,
    /// so the guarantee that each terminates on an infinite pattern has to hold for both.
    /// </summary>
    [Fact]
    public void EveryCrossingOperationTerminatesOnAnInfinitePattern()
    {
        // the sawtooth passes the level 1/2 twice per period, forever
        var pattern = Sawtooth.GetCrossings(new ConstantCurve(new Rational(1, 2)));

        Assert.True(pattern.IsInfinite);
        Assert.Null(pattern.Count);
        Assert.NotNull(pattern.First);
        Assert.Equal(4, pattern.Take(4).Count);
        Assert.Equal(4, pattern.Before(4).Count);
        Assert.NotNull(pattern.ElementAt(5));
        Assert.Throws<InvalidOperationException>(() => pattern.Last);
    }

    [Fact]
    public void BeforePlusInfinityRejectsAnInfiniteCrossingPattern()
    {
        // asking for everything before +infinity would enumerate forever.
        var pattern = Sawtooth.GetCrossings(new ConstantCurve(new Rational(1, 2)));

        Assert.Throws<InvalidOperationException>(() => pattern.Before(Rational.PlusInfinity));
    }

    [Fact]
    public void CrossingsRepeatOnePeriodApart()
    {
        var pattern = Sawtooth.GetCrossings(new ConstantCurve(new Rational(1, 2)));

        var first = pattern.ElementAt(0)!.Value;
        var aPeriodLater = pattern.ElementAt(pattern.Repeating.Count)!.Value;

        Assert.Equal(first.Time + pattern.PeriodLength, aPeriodLater.Time);
        Assert.Equal(first.IsUpward, aPeriodLater.IsUpward);
        Assert.Equal(first.IsContact, aPeriodLater.IsContact);
    }

    [Fact]
    public void EnumerateRefusesAnInfiniteCrossingPatternUnlessAsked()
    {
        var pattern = Sawtooth.GetCrossings(new ConstantCurve(new Rational(1, 2)));

        Assert.Throws<InvalidOperationException>(() => pattern.Enumerate().First());
        Assert.Equal(3, pattern.Enumerate(throwIfInfinite: false).Take(3).Count());
    }

    [Fact]
    public void AFiniteCrossingPatternIsWalkedByDefault()
    {
        var pattern = new RateLatencyServiceCurve(1, 0).GetCrossings(new ConstantCurve(5));

        Assert.False(pattern.IsInfinite);
        Assert.Equal(pattern.Count, pattern.Enumerate().Count());
        Assert.Null(pattern.ElementAt(pattern.Count!.Value));
    }

    #region Normal form

    public static IEnumerable<object[]> Pairs()
    {
        yield return [new RateLatencyServiceCurve(1, 0), new ConstantCurve(5)];
        yield return [new DelayServiceCurve(3), new DelayServiceCurve(5)];
        yield return [new DelayServiceCurve(3), new RateLatencyServiceCurve(1, 0)];
        yield return [new StairCurve(1, 3), new StairCurve(1, 3)];
        yield return [Sawtooth, Curve.Zero()];
        yield return [TouchingV, Curve.Zero()];
        yield return [SaturatingWithInfiniteDrift, SaturatingWithInfiniteDrift];
        yield return [SaturatingWithInfiniteDrift, new DelayServiceCurve(2)];
        yield return [SaturatingAfterTwo, WigglingThenSaturated];
        yield return [InfiniteAtIntegers(1), InfiniteAtIntegers(2)];
        yield return [StretchCrossingPeriodBoundaries, new ConstantCurve(1)];
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void NoIntervalIncludesAnInfiniteEndpoint(Curve f, Curve g)
    {
        foreach (var interval in f.GetIntersections(g).Take(8))
        {
            Assert.False(
                interval.IsUnboundedAbove && interval.IsUpperIncluded,
                $"{interval} includes an infinite upper endpoint");
        }
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void IntervalsAreOrderedAndDisjoint(Curve f, Curve g)
    {
        var found = f.GetIntersections(g).Take(8);

        for (var i = 1; i < found.Count; i++)
        {
            var previous = found[i - 1];
            var current = found[i];
            Assert.True(
                previous.Upper <= current.Lower,
                $"{previous} and {current} are out of order or overlapping");
            Assert.False(
                previous.Upper == current.Lower &&
                (previous.IsUpperIncluded || current.IsLowerIncluded),
                $"{previous} and {current} are contiguous and should have been joined");
        }
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void TheStoredIntervalsAreAlreadyMaximal(Curve f, Curve g)
    {
        // the walking read-outs merge as they go, but Count reads Transient directly: two stored
        // intervals that could be joined would make Count disagree with what Take returns
        var pattern = f.GetIntersections(g);

        AssertMaximal(pattern.Transient);
        AssertMaximal(pattern.Repeating);

        if (pattern.Count is { } count)
            Assert.Equal(count, pattern.Take(count + 1).Count);

        void AssertMaximal(IReadOnlyList<Interval> intervals)
        {
            for (var i = 1; i < intervals.Count; i++)
            {
                Assert.False(
                    Interval.Union(intervals[i - 1], intervals[i]).HasValue,
                    $"{intervals[i - 1]} and {intervals[i]} are stored apart but join into one");
            }
        }
    }

    [Fact]
    public void ATailReachingBackIntoTheTransientIsOneInterval()
    {
        // the equality runs from 0 and the unbounded tail starts at the period boundary, which the
        // constructor normalizes to 3 rather than the 2 the curve is declared with: stored apart
        // the two would make Count say 2 where the walk yields a single interval
        var pattern = SaturatingWithInfiniteDrift.GetIntersections(SaturatingWithInfiniteDrift);

        Assert.Equal(3, SaturatingWithInfiniteDrift.PseudoPeriodStart);
        Assert.Equal(1, pattern.Count);
        var only = Assert.Single(pattern.Transient);
        Assert.Equal(0, only.Lower);
        Assert.True(only.IsUnboundedAbove);
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void SettingsDoNotChangeTheAnswer(Curve f, Curve g)
    {
        var settings = new ComputationSettings { UseParallelism = false };

        Assert.Equal(f.GetIntersections(g).Take(8), f.GetIntersections(g, settings: settings).Take(8));
    }

    #endregion
}
