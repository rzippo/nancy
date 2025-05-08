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

    #region Optimized Overrides

    /// <inheritdoc cref="Curve.Scale(Rational)"/>
    public override Curve Scale(Rational scaling)
    {
        #if DO_LOG
        logger.Trace("Optimized SR Scale");
        #endif
        return new SigmaRhoArrivalCurve(sigma: scaling * Sigma, rho: scaling * Rho);
    }

    #endregion
}