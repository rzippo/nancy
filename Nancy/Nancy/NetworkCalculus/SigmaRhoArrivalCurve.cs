using NLog;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A (sigma, rho) traffic model, also know as token-bucket or leaky-bucket.
/// Sub-additive
/// </summary>
public class SigmaRhoArrivalCurve : ConcaveCurve
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Maximum burst of the traffic
    /// </summary>
    public Rational Sigma { get; }

    /// <summary>
    /// Maximum rate of the traffic
    /// </summary>
    public Rational Rho { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public SigmaRhoArrivalCurve(Rational sigma, Rational rho)
        : base(
            baseSequence: new Sequence(new Element[]
            {
                Point.Origin(), 
                new Segment
                (
                    startTime : 0,
                    rightLimitAtStartTime : sigma,
                    slope : rho,
                    endTime : DefaultPeriodLength
                ),
                new Point(time: DefaultPeriodLength, value: sigma + rho * DefaultPeriodLength), 
                new Segment
                (
                    startTime : DefaultPeriodLength,
                    rightLimitAtStartTime : sigma + rho * DefaultPeriodLength,
                    slope : rho,
                    endTime : 2* DefaultPeriodLength
                )
            }),
            pseudoPeriodStart: DefaultPeriodLength,
            pseudoPeriodLength: DefaultPeriodLength,
            pseudoPeriodHeight: rho * DefaultPeriodLength
        )
    {
        Sigma = sigma;
        Rho = rho;
    }

    //These parameters have no meaning model-wise, they only influence efficiency of Extend()
    private static readonly Rational DefaultPeriodLength = 1;

    #region Optimized Overrides

    /// <inheritdoc cref="Curve.Scale(Rational)"/>
    public override Curve Scale(Rational scaling)
    {
        logger.Trace("Optimized SR Scale");
        return new SigmaRhoArrivalCurve(sigma: scaling * Sigma, rho: scaling * Rho);
    }

    #endregion
}