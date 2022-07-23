using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Provides upper-bounding methods
/// </summary>
public static class CurveUpperBounds
{
    /// <summary>
    /// Upper-bounds the given curve with a <see cref="SigmaRhoArrivalCurve"/>
    /// </summary>
    /// <param name="curve"></param>
    /// <returns>The upper-bound</returns>
    public static SigmaRhoArrivalCurve SigmaRhoUpperBound(this Curve curve)
    {
        if (curve is SigmaRhoArrivalCurve)
            return (SigmaRhoArrivalCurve)curve;

        var rate = curve.PseudoPeriodSlope;
        var burst = CornerPoints()
            .Select(p => p.Value - p.Time * rate)
            .Max();

        return new SigmaRhoArrivalCurve(sigma: burst, rho: rate);

        List<Point> CornerPoints()
        {
            var cornerPoints = new List<Point>();

            foreach (var element in curve.BaseSequence.Elements)
            {
                switch (element)
                {
                    case Point point:
                        cornerPoints.Add(point);
                        break;

                    case Segment segment:
                        cornerPoints.Add(new Point(
                            time: segment.StartTime,
                            value: segment.RightLimitAtStartTime
                        ));
                        cornerPoints.Add(new Point(
                            time: segment.EndTime,
                            value: segment.LeftLimitAtEndTime
                        ));
                        break;

                    default:
                        throw new InvalidCastException();
                }
            }

            return cornerPoints;
        }
    }
}