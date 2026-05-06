using Unipi.Nancy.Numerics;

namespace Nancy.Analyzers.Sample;

public class FloatingPointToRationalExamples
{
    public void RationalFromDouble()
    {
        double value = 0.1;
        var rational = new Rational(value);
    }
    
    public void RationalFromFloat()
    {
        float value = 0.1f;
        var rational = new Rational(value);
    }
    public void DoubleToRational()
    {
        double value = 0.1;
        Rational rational = value;
    }

    public void FloatToRational()
    {
        float value = 0.1f;
        Rational rational = value;
    }

    public void ExplicitCasts()
    {
        double doubleValue = 0.1;
        float floatValue = 0.1f;

        var rationalFromDouble = (Rational) doubleValue;
        var rationalFromFloat = (Rational) floatValue;
    }

    public void SpecificRationalTypes()
    {
        double doubleValue = 0.1;
        float floatValue = 0.1f;

        BigRational bigRational = doubleValue;
        LongRational longRational = floatValue;
    }

    public void MethodArguments()
    {
        AcceptRational(0.1);
        AcceptBigRational(0.1);
        AcceptLongRational(0.1f);
    }

    public void PreferredAlternatives()
    {
        Rational decimalRational = 0.1m;
        Rational directRational = new Rational(1, 10);
        BigRational decimalBigRational = 0.1m;
        LongRational directLongRational = new LongRational(1, 10);
    }

    private static void AcceptRational(Rational rational)
    {
    }

    private static void AcceptBigRational(BigRational rational)
    {
    }

    private static void AcceptLongRational(LongRational rational)
    {
    }
}
