using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Nancy.Analyzers.Sample;

public class CurveSubtractionExamples
{
    public void StaticDefaultSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2);
        var b = new SigmaRhoArrivalCurve(2, 1);
        var c = Curve.Subtraction(a, b);
    }
    public void MemberDefaultSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2);
        var b = new SigmaRhoArrivalCurve(2, 1);
        var c = a.Subtraction(b);
    }
    public void OperatorSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2);
        var b = new SigmaRhoArrivalCurve(2, 1);
        var c = a - b;
    }
    public void StaticNonNegativeSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2);
        var b = new SigmaRhoArrivalCurve(2, 1);
        var c = Curve.Subtraction(a, b, true);
    }
    public void MemberNonNegativeSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2);
        var b = new SigmaRhoArrivalCurve(2, 1);
        var c = a.Subtraction(b, true);
    }
}