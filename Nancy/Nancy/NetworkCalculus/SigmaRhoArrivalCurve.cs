using System.Text;
using System.Text.Json.Serialization;
using NLog;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A (sigma, rho) traffic model, also known as leaky-bucket.
/// Sub-additive
/// </summary>
[JsonConverter(typeof(SigmaRhoArrivalCurveSystemJsonConverter))]
public class SigmaRhoArrivalCurve : ConcaveCurve
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    /// <summary>
    /// Type identification constant for JSON (de)serialization. 
    /// </summary>
    /// <exclude />
    public new const string TypeCode = "sigmaRhoArrivalCurve";

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

    /// <inheritdoc cref="Curve.ToCodeString"/>
    public override string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var newline = formatted ? "\n" : "";
        var space = formatted ? "\n" : " ";

        var sb = new StringBuilder();
        sb.Append($"{tabs(0)}new SigmaRhoArrivalCurve({newline}");
        sb.Append($"{tabs(1)}{Sigma.ToCodeString()},{space}");
        sb.Append($"{tabs(1)}{Rho.ToCodeString()}{newline}");
        sb.Append($"{tabs(0)})");

        return sb.ToString();
        
        string tabs(int n)
        {
            if (!formatted)
                return "";
            var sbt = new StringBuilder();
            for (int i = 0; i < indentation + n; i++)
                sbt.Append("\t");
            return sbt.ToString();
        }
    }

    /// <inheritdoc cref="Curve.ToMppgString"/>
    public override string ToMppgString()
    {
        return $"bucket({Rho.ToMppgString()}, {Sigma.ToMppgString()})";
    }

    #region Optimized Overrides

    /// <inheritdoc cref="Curve.Scale(Rational)"/>
    public override Curve Scale(Rational scaling)
    {
        #if DO_LOG
        logger.Trace("Optimized SR Scale");
        #endif
        return new SigmaRhoArrivalCurve(sigma: scaling * Sigma, rho: scaling * Rho);
    }

    /// <inheritdoc cref="Curve.VerticalShift(Rational, bool)"/>
    /// <param name="shift">The additive factor $k$.</param>
    /// <param name="exceptOrigin">
    /// If false, which is the default, the shift applies to any $t$, the origin included, and the result is a plain <see cref="Curve"/>.
    /// If true, the value at the origin is left at 0 and the result is the curve with $\sigma + k$ as burst.
    /// </param>
    public override Curve VerticalShift(Rational shift, bool exceptOrigin = false)
    {
        if (shift == 0)
            return this;

        // leaving the origin at 0 raises the burst, which is another curve of this kind,
        // as long as the raised burst is one this type accepts, i.e. finite and non-negative
        if (exceptOrigin && shift.IsFinite)
        {
            var raisedSigma = Sigma + shift;
            if (raisedSigma.IsFinite && !raisedSigma.IsNegative)
            {
                #if DO_LOG
                logger.Trace("Optimized SR VerticalShift");
                #endif
                return new SigmaRhoArrivalCurve(sigma: raisedSigma, rho: Rho);
            }
        }

        return base.VerticalShift(shift, exceptOrigin);
    }

    #endregion
}