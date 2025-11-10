namespace Nancy.Analyzers.Sample;

public class DivisionNonRationalExamples
{
    public void IntOverInt()
    {
        int a = 1;
        int b = 2;
        var c = a / b;
    }
    
    public void LongOverLong()
    {
        long a = 1;
        long b = 2;
        var c = a / b;
    }
    
    public void DecimalOverInt()
    {
        decimal a = 1.0m;
        long b = 2;
        var c = a / b;
    }
    
    public void DecimalOverDecimal()
    {
        decimal a = 1.0m;
        decimal b = 2m;
        var c = a / b;
    }
}