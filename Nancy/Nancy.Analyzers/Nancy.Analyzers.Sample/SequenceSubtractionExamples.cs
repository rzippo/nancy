using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Nancy.Analyzers.Sample;

public class SequenceSubtractionExamples
{
    public void StaticDefaultSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2).Cut(0, 50);
        var b = new SigmaRhoArrivalCurve(2, 1).Cut(0, 50);
        var c = Sequence.Subtraction(a, b);
    }
    public void MemberDefaultSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2).Cut(0, 50);
        var b = new SigmaRhoArrivalCurve(2, 1).Cut(0, 50);
        var c = a.Subtraction(b);
    }
    public void OperatorSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2).Cut(0, 50);
        var b = new SigmaRhoArrivalCurve(2, 1).Cut(0, 50);
        var c = a - b;
    }
    public void StaticNonNegativeSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2).Cut(0, 50);
        var b = new SigmaRhoArrivalCurve(2, 1).Cut(0, 50);
        var c = Sequence.Subtraction(a, b, true);
    }
    public void MemberNonNegativeSubtraction()
    {
        var a = new RateLatencyServiceCurve(1, 2).Cut(0, 50);
        var b = new SigmaRhoArrivalCurve(2, 1).Cut(0, 50);
        var c = a.Subtraction(b, true);
    }
}