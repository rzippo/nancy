using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Provides lower-bounding methods.
/// </summary>
public static class CurveLowerBounds
{
    /// <exclude />
    /// <summary>
    /// Lower bounds the curve with a two-rates curve.
    /// </summary>
    /// <remarks>
    /// There may be infinite valid lower-bounds, varying on the trade-off between delay and long-term service.
    /// In this implementation, we chose to maximize the long-term service, sacrificing delay.
    /// </remarks>
    public static Curve TwoRatesLowerBound(this Curve curve)
    {
        if (curve is RateLatencyServiceCurve dr)
            return dr;

        if (curve.PseudoPeriodSlope.IsInfinite)
            throw new ArgumentException("Trying to lower bound a curve with infinite sustained rate");

        var minDelay = curve.FirstNonZeroTime;

        if (minDelay >= curve.PseudoPeriodStart )
        {
            return RateLatencyLowerBound(curve);
        }
        else
        {
            var sustainedRate = curve.PseudoPeriodSlope;
            var periodicCorners = curve
                .Extend(curve.SecondPseudoPeriodEnd)
                .Cut(curve.PseudoPeriodStart, curve.SecondPseudoPeriodEnd)
                .Elements
                .Where(e => e.IsFinite)
                .SelectMany(GetCorners);

            var maxPeriodStartValue = periodicCorners
                .Select(PeriodStartValue)
                .Min();

            if (maxPeriodStartValue <= 0)
                return RateLatencyLowerBound(curve);

            var periodStart = new Point( 
                time: curve.PseudoPeriodStart,
                value: Rational.Min(maxPeriodStartValue, curve.LeftLimitAt(curve.PseudoPeriodStart))
            );

            var minTransientRate = periodStart.Value / (periodStart.Time - minDelay);

            var temporaryCorners = curve
                .Cut(minDelay, periodStart.Time)
                .Elements
                .Where(e => e.IsFinite)
                .SelectMany(GetCorners)
                .Where(c => c.Time != minDelay && c.Time != periodStart.Time);

            var candidateTransientRate = temporaryCorners
                .Select(c => TransientRate(c, periodStart))
                .DefaultIfEmpty(Rational.MinusInfinity)
                .Max();

            var transientRate = Rational.Max(minTransientRate, candidateTransientRate);
            var delay = periodStart.Time - periodStart.Value / transientRate;

            if (transientRate == sustainedRate)
            {
                return new RateLatencyServiceCurve(rate: sustainedRate, latency: delay);
            }
            else
            {
                return new TwoRatesServiceCurve(
                    delay: delay,
                    transientRate: transientRate,
                    transientEnd: curve.PseudoPeriodStart,
                    steadyRate: sustainedRate
                );
            }
        }

        IEnumerable<Point> GetCorners(Element e)
        {
            switch (e)
            {
                case Point p:
                    return new[] { p };

                case Segment s:
                    return new[]
                    {
                        new Point(
                            time: s.StartTime,
                            value: s.RightLimitAtStartTime
                        ),
                        new Point(
                            time: s.EndTime,
                            value: s.LeftLimitAtEndTime
                        )
                    };

                default:
                    throw new InvalidCastException();
            }
        }

        Rational PeriodStartValue(Point corner)
            => corner.Value - (corner.Time - curve.PseudoPeriodStart) * curve.PseudoPeriodSlope;

        Rational TransientRate(Point corner, Point periodStart)
            => (periodStart.Value - corner.Value) / (periodStart.Time - corner.Time);
    }

    /// <summary>
    /// Lower bounds the curve with a rate-latency curve.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="alpha">Controls the type of bounding. If 0, the bounding optimizes for low delay. If 1, optimizes for high rate.</param>
    /// <remarks>
    /// There may be infinite valid lower-bounds, varying on the trade-off between delay and long-term service.
    /// The parameter <paramref name="alpha"/> controls this behavior, with the default setting being highest rate.
    /// </remarks>
    public static RateLatencyServiceCurve RateLatencyLowerBound(this Curve curve, decimal alpha = 1.0m)
    {
        if (curve is RateLatencyServiceCurve dr)
            return dr;
        if (curve.PseudoPeriodSlope.IsInfinite)
            throw new ArgumentException("Trying to lower bound a curve with infinite sustained rate");
        if (alpha < 0 || alpha > 1)
            throw new ArgumentException("Alpha parameter must be in [0, 1]");

        var maxRate = curve.PseudoPeriodSlope;
        var minDelay = curve.FirstNonZeroTime;

        var corners = curve.Extend(curve.SecondPseudoPeriodEnd)
            .Elements
            .Where(e => e.IsFinite)
            .SelectMany(GetCorners)
            .ToList();

        //The delay is selected through alpha in the interval [minDelay, maxDelay]
        //maxDelay is not an actual limitation, but it is the lowest that allows maxRate
        //Selecting an higher delay than maxDelay does not improve the rate
        var maxCornersDelay = corners
            .DefaultIfEmpty(Point.Origin())
            .Max(CornerDelay);

        var maxDelay = Rational.Max(minDelay, maxCornersDelay);
        var delay = minDelay + alpha * (maxDelay - minDelay);

        //Compute the rate as that of the segment from the delay point to the pivot corner
        var pivotCorner = corners
            .DefaultIfEmpty(Point.Origin())
            .Where(c => CornerDelay(c) == maxCornersDelay)
            .OrderBy(c => c.Time)
            .First();

        var rate = pivotCorner.Value.IsZero ?
            maxRate :
            pivotCorner.Value / (pivotCorner.Time - delay);                       

        return new RateLatencyServiceCurve(rate: rate, latency: delay);

        IEnumerable<Point> GetCorners(Element e)
        {
            switch(e)
            {
                case Point p:
                    return new[] { p };

                case Segment s:
                    return new[]
                    {
                        new Point(
                            time: s.StartTime,
                            value: s.RightLimitAtStartTime
                        ),
                        new Point(
                            time: s.EndTime,
                            value: s.LeftLimitAtEndTime
                        )
                    };

                default:
                    throw new InvalidCastException();
            }
        }

        Rational CornerDelay(Point corner)
            => corner.Time - corner.Value / maxRate;            
    }

    /// <summary>
    /// Lower bounds the given set of points with a rate-latency curve.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="alpha">Controls the type of bounding. If 0, the bounding optimizes for low delay. If 1, optimizes for high rate.</param>
    /// <remarks>
    /// There may be infinite valid lower-bounds, varying on the trade-off between delay and long-term service.
    /// The parameter <paramref name="alpha"/> controls this behavior, with the default setting being highest rate.
    /// </remarks>
    public static RateLatencyServiceCurve RateLatencyLowerBound(IReadOnlyList<Point> points, decimal alpha = 1.0m)
    {
        //Filter out infinite values and duplicates, order by time
        points = points.Where(p => p.IsFinite)
            .OrderBy(p => p.Time)
            .Distinct()
            .ToArray();

        if (points.All(p => p.IsZero))
            throw new ArgumentException("No non-zero points, cannot bound with finite rate-latency");

        var minDelay = points
            .Where(p => p.Value == Rational.Zero)
            .DefaultIfEmpty(Point.Origin())
            .Max(p => p.Time);

        // A candidate corner is a point which is at the 'bottom-right frontier' w.r.t. a range of rates
        var corners = new List<(Point p, Rational rate)>();
        foreach(var point in points)
        {
            // check that this is the only point at the given time
            // if not, check if this is the lowest
            if(points.Any(p => p.Time == point.Time && p.Value < point.Value))
                continue;

            // check that there are no later points with lower or equal value
            if(points.Any(p => p.Time > point.Time && p.Value <= point.Value))
                continue;

            // we compare this point to the forward ones, to find a rate w.r.t. this point is a corner
            foreach (var point2 in points.Where(p => p.Time > point.Time))
            {
                var localRate = (point2.Value - point.Value) / (point2.Time - point.Time);
                if (localRate.IsNegative || localRate.IsZero)
                    throw new InvalidOperationException("Something went wrong");
                if (IsFrontierPoint(point, localRate))
                {
                    corners.Add((point, localRate));
                    break;
                }
            }
        }

        if(!corners.Any())
            throw new InvalidOperationException("Something went wrong");

        // the pivot corner is the earliest non-zero corner if possible, else it is the delay point
        if (corners.Any(tuple => tuple.p.Value > 0))
        {
            var (pivotCorner, maxRate) = corners.First(tuple => tuple.p.Value > 0);

            var maxDelay = pivotCorner.Time - pivotCorner.Value / maxRate;

            var delay = minDelay + alpha * (maxDelay - minDelay);
            var rate = pivotCorner.Value / (pivotCorner.Time - delay);

            return new RateLatencyServiceCurve(rate, delay);
        }
        else
        {
            var (delayPoint, rate) = corners.First();
            return new RateLatencyServiceCurve(rate, delayPoint.Time);
        }

        bool IsFrontierPoint(Point candidate, Rational rate)
        {
            return points
                .All(p =>
                {
                    var frontierValueAtP = candidate.Value + (p.Time - candidate.Time) * rate;
                    return frontierValueAtP <= p.Value;
                });
        }
    }
}